using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// adapted from: https://github.com/neokabuto/OpenTKTutorialContent/blob/master/OpenTKTutorial7/OpenTKTutorial7/Game.cs

namespace Blacksmith.Three
{
    public class GLViewer
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public GLControl Control;
        public Camera Camera;
        public List<Model> Models;
        public Color BackgroundColor = Color.DarkGray;
        public RenderMode RenderMode = RenderMode.SOLID;
        public int PointSize = 5;
        public TextRenderer TextRenderer;

        private Vector3[] vertdata;
        private Vector3[] coldata;
        private Vector2[] texcoorddata;
        private int[] indicedata;
        private int ibo_elements;

        private Dictionary<string, int> textures = new Dictionary<string, int>();
        private Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        private string activeShader = "normal";
        private Matrix4 view = Matrix4.Identity;
        private Vector3 initCamPos = new Vector3(0, 1.5f, 5);

        public GLViewer(GLControl control)
        {
            Control = control;
            Camera = new Camera();
            Models = new List<Model>();
            TextRenderer = new TextRenderer(Control.Width, Control.Height);

            Control.MouseDown += OnMouseDown;
            Control.Click += OnClick;
            Control.MouseCaptureChanged += OnCaptureChanged;
        }

        public void Init()
        {
            GL.GenBuffers(1, out ibo_elements);

            // load shader
            shaders.Add("default", new ShaderProgram("vs.glsl", "fs.glsl", true));
            shaders.Add("normal", new ShaderProgram("vs_normal.glsl", "fs_normal.glsl", true));
            //shaders.Add("textured", new ShaderProgram("vs_tex.glsl", "fs_tex.glsl", true));

            /*activeShader = "textured";
            textures.Add("<texture>", loadImage("<texture>"));*/

            // setup
            Camera.Position = initCamPos;
            GL.ClearColor(BackgroundColor);
            GL.PointSize(PointSize);
        }

        public void Render()
        {
            UpdateFrame();
            RenderFrame();
        }

        public void SetCameraResetPosition(Vector3 p) => initCamPos = p;

        private void RenderFrame()
        {
            GL.Viewport(Control.ClientRectangle);
            GL.ClearColor(BackgroundColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ShadeModel(ShadingModel.Flat);
            GL.Enable(EnableCap.DepthTest);

            // shader
            shaders[activeShader].EnableVertexAttribArrays();

            // set render mode
            BeginMode beginMode = BeginMode.Triangles;
            if (RenderMode == RenderMode.WIREFRAME)
            {
                beginMode = BeginMode.Lines;
            }
            else if (RenderMode == RenderMode.POINTS)
            {
                beginMode = BeginMode.Points;
                GL.PointSize(PointSize);
            }

            // draw all models
            int indiceat = 0;
            foreach (Model v in Models)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, v.TextureID);
                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref v.ModelViewProjectionMatrix);

                if (shaders[activeShader].GetUniform("maintexture") != -1)
                    GL.Uniform1(shaders[activeShader].GetUniform("maintexture"), v.TextureID);

                if (shaders[activeShader].GetUniform("view") != -1)
                    GL.UniformMatrix4(shaders[activeShader].GetUniform("view"), false, ref view);

                if (shaders[activeShader].GetUniform("model") != -1)
                    GL.UniformMatrix4(shaders[activeShader].GetUniform("model"), false, ref v.ModelMatrix);

                GL.DrawElements(beginMode, v.GetIndices().Length, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                indiceat += v.GetIndices().Length;
            }

            shaders[activeShader].DisableVertexAttribArrays();

            GL.Flush();
            GL.Disable(EnableCap.DepthTest);
            Control.SwapBuffers();
        }

        Vector3[] normdata;
        private void UpdateFrame()
        {
            ProcessInput();

            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texcoords = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();

            // Assemble vertex and index data for all models
            int vertcount = 0;
            foreach (Model v in Models)
            {
                verts.AddRange(v.GetVertices());
                inds.AddRange(v.GetIndices(vertcount));
                colors.AddRange(v.GetColorData());
                texcoords.AddRange(v.GetTextureCoords());
                normals.AddRange(v.GetNormals());
                vertcount += v.VertexCount;
            }

            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            coldata = colors.ToArray();
            texcoorddata = texcoords.ToArray();
            normdata = normals.ToArray();

            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));

            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            // Buffer vertex color if shader supports it
            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            // Buffer texture coordinates if shader supports it
            if (shaders[activeShader].GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("texcoord"));
                GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }

            if (shaders[activeShader].GetAttribute("vNormal") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vNormal"));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(normdata.Length * Vector3.SizeInBytes), normdata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vNormal"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            // Update model view matrices
            foreach (Model v in Models)
            {
                v.CalculateModelMatrix();
                v.ViewProjectionMatrix = Camera.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, Control.ClientSize.Width / (float)Control.ClientSize.Height, 1.0f, 40.0f);
                v.ModelViewProjectionMatrix = v.ModelMatrix * v.ViewProjectionMatrix;
            }

            GL.UseProgram(shaders[activeShader].ProgramID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Buffer index data
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);

        /*List<Vector3> verts = new List<Vector3>();
        List<int> inds = new List<int>();
        List<Vector3> colors = new List<Vector3>();
        List<Vector2> texcoords = new List<Vector2>();

        // assemble vertex and indice data for all volumes
        int vertcount = 0;
        foreach (Model v in Meshes)
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
        foreach (Model m in Meshes)
        {
            m.CalculateModelMatrix();
            m.ViewProjectionMatrix = Camera.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, Control.Width / (float)Control.Height, .1f, 200);
            m.ModelViewProjectionMatrix = m.ModelMatrix * m.ViewProjectionMatrix;
        }

        GL.UseProgram(shaders[activeShader].ProgramID);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        // buffer index data
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
        GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);*/
    }

        public void ResetCamera()
        {
            Camera.Position = initCamPos;
            Camera.Orientation = new Vector3((float)Math.PI, 0, 0);
        }
        
        private float movement = 1;
        private float rotation = 10;
        private void ProcessInput()
        {
            Rectangle r = Control.RectangleToScreen(Control.ClientRectangle);
            if (GetForegroundWindow() == Control.ParentForm.Handle)
            {
                if (r.Contains(Cursor.Position))
                {
                    Control.Focus();

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
                        ResetCamera();

                    if (Keyboard.GetState().IsKeyDown(Key.Left))
                        Camera.AddRotation(rotation, 0);

                    if (Keyboard.GetState().IsKeyDown(Key.Right))
                        Camera.AddRotation(-rotation, 0);

                    if (Keyboard.GetState().IsKeyDown(Key.Up))
                        Camera.AddRotation(0, rotation);

                    if (Keyboard.GetState().IsKeyDown(Key.Down))
                        Camera.AddRotation(0, -rotation);

                    /*if (Mouse.GetState().IsButtonDown(0))
                    {
                        Cursor.Clip = Control.ParentForm.RectangleToScreen(Control.ParentForm.ClientRectangle);
                        //Cursor.Position = Control.PointToScreen(new Point(Control.Width / 2, Control.Height / 2));
                        //Cursor.Hide();
                        Control.Capture = true;
                        Point p = Control.PointToScreen(Cursor.Position);
                        Camera.AddRotation(p.X * mouseSensitivity, p.Y * mouseSensitivity);
                    }*/
                }
                /*else
                {
                    Cursor.Clip = Rectangle.Empty;
                    //Cursor.Show();
                    Control.Capture = false;
                }*/
            }
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