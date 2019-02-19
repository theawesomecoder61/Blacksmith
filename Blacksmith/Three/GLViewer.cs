using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

// adapted from: https://github.com/neokabuto/OpenTKTutorialContent/blob/master/OpenTKTutorial7/OpenTKTutorial7/Game.cs

namespace Blacksmith.Three
{
    public class GLViewer
    {
        public GLControl Control;
        public Camera Camera;
        public List<Mesh> Meshes;
        public Color BackgroundColor = Color.DarkGray;
        public RenderMode RenderMode = RenderMode.SOLID;

        private Vector3[] vertdata;
        private Vector3[] coldata;
        private Vector2[] texcoorddata;
        private int[] indicedata;
        private int ibo_elements;
        private Vector2 lastMousePos = new Vector2();

        private Dictionary<string, int> textures = new Dictionary<string, int>();
        private Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        private string activeShader = "default";
        private readonly Vector3 initCamPos = new Vector3(0, 2, 5);

        public GLViewer(GLControl control)
        {
            Control = control;
            Camera = new Camera();
            Meshes = new List<Mesh>();

            Control.MouseDown += OnMouseDown;
            Control.Click += OnClick;
            Control.MouseCaptureChanged += OnCaptureChanged;
        }

        public void Init()
        {
            lastMousePos = new Vector2(0, 0);

            GL.GenBuffers(1, out ibo_elements);

            // load shader
            shaders.Add("default", new ShaderProgram(Application.ExecutablePath + "\\..\\Shaders\\vs.glsl", Application.ExecutablePath + "\\..\\Shaders\\fs.glsl", true));
            //shaders.Add("textured", new ShaderProgram(Application.ExecutablePath + "\\..\\Shaders\\vs_tex.glsl", Application.ExecutablePath + "\\..\\Shaders\\fs_tex.glsl", true));

            /*if using the textured shader
            activeShader = "textured";
            textures.Add("<texture>", loadImage("<texture>"));*/

            // setup
            Camera.Position = initCamPos;
            GL.ClearColor(BackgroundColor);
            GL.PointSize(5f);
        }

        public void Render()
        {
            UpdateFrame();
            RenderFrame();
        }

        private void RenderFrame()
        {
            GL.Viewport(0, 0, Control.Width, Control.Height);
            GL.ClearColor(BackgroundColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            shaders[activeShader].EnableVertexAttribArrays();

            int indiceat = 0;

            // draw all objects
            foreach (Mesh v in Meshes)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, v.TextureID);
                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref v.ModelViewProjectionMatrix);

                if (shaders[activeShader].GetUniform("maintexture") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetUniform("maintexture"), 0);
                }

                // set render mode
                BeginMode beginMode = BeginMode.Triangles;
                if (RenderMode == RenderMode.WIREFRAME)
                    beginMode = BeginMode.Lines;
                else if (RenderMode == RenderMode.POINTS)
                    beginMode = BeginMode.Points;

                GL.DrawElements(beginMode, v.GetIndices().Length, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                indiceat += v.GetIndices().Length;
            }

            shaders[activeShader].DisableVertexAttribArrays();

            GL.Flush();
            Control.SwapBuffers();
        }

        private void UpdateFrame()
        {
            ProcessInput();

            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texcoords = new List<Vector2>();

            // assemble vertex and indice data for all volumes
            int vertcount = 0;
            foreach (Mesh v in Meshes)
            {
                verts.AddRange(v.GetVertices());
                inds.AddRange(v.GetIndices(vertcount));
                colors.AddRange(v.GetColorData());
                texcoords.AddRange(v.GetTextureCoords());
                vertcount += v.GetVertices().Length;
            }

            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            coldata = colors.ToArray();
            texcoorddata = texcoords.ToArray();

            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            // buffer vertex color if shader supports it
            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            // buffer texture coordinates if shader supports it
            if (shaders[activeShader].GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("texcoord"));
                GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }

            // update model view matrices
            foreach (Mesh v in Meshes)
            {
                v.CalculateModelMatrix();
                v.ViewProjectionMatrix = Camera.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, Control.Width / (float)Control.Height, 1, 200);
                v.ModelViewProjectionMatrix = v.ModelMatrix * v.ViewProjectionMatrix;
            }

            GL.UseProgram(shaders[activeShader].ProgramID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // buffer index data
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);
        }

        private void OnFocusedChanged(EventArgs e)
        {
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }
        
        private void ProcessInput()
        {
            float movement = 1 / 1000f;
            float rotation = 10;

            if (!(Keyboard.GetState().IsKeyDown(Key.LShift) || Keyboard.GetState().IsKeyDown(Key.RShift)))
                return;

            if (Keyboard.GetState().IsKeyDown(Key.W))
                Camera.Move(0, movement, 0);

            if (Keyboard.GetState().IsKeyDown(Key.S))
                Camera.Move(0, -movement, 0);

            if (Keyboard.GetState().IsKeyDown(Key.A))
                Camera.Move(-movement, 0, 0);

            if (Keyboard.GetState().IsKeyDown(Key.D))
                Camera.Move(movement, 0, 0);

            if (Keyboard.GetState().IsKeyDown(Key.Q))
                Camera.Move(0, 0, movement);

            if (Keyboard.GetState().IsKeyDown(Key.E))
                Camera.Move(0, 0, -movement);

            if (Keyboard.GetState().IsKeyDown(Key.R))
                Camera.Position = initCamPos;

            if (Keyboard.GetState().IsKeyDown(Key.Escape))
                Control.Capture = false;

            if (Keyboard.GetState().IsKeyDown(Key.Left))
                Camera.AddRotation(rotation, 0);

            if (Keyboard.GetState().IsKeyDown(Key.Right))
                Camera.AddRotation(-rotation, 0);

            if (Keyboard.GetState().IsKeyDown(Key.Up))
                Camera.AddRotation(0, rotation);

            if (Keyboard.GetState().IsKeyDown(Key.Down))
                Camera.AddRotation(0, -rotation);
        }

        private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs args)
        {
            Control.Capture = false;
        }

        private void OnClick(object sender, EventArgs args)
        {
            Control.Capture = true;
        }

        private void OnCaptureChanged(object sender, EventArgs args)
        {
            if (!Control.Capture)
                Cursor.Clip = new Rectangle(0, 0, 0, 0);
        }

        private int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }
        
        private int loadImage(string filename)
        {
            try
            {
                Bitmap file = new Bitmap(filename);
                return loadImage(file);
            }
            catch (FileNotFoundException)
            {
                return -1;
            }
        }
    }
}