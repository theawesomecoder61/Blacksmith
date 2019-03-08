using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;
using PixelShader = SlimDX.Direct3D11.PixelShader;

namespace Blacksmith.Three
{
    public class ThreeViewer
    {
        public D3D11Control Control;

        private Device device;
        private DeviceContext context;
        private SwapChain swapChain;
        private Effect effect;
        private ShaderSignature inputSignature;
        private PixelShader pixelShader;
        private VertexShader vertexShader;

        private Camera camera;
        private Mesh Mesh;

        public ThreeViewer(D3D11Control control)
        {
            Control = control;
            Control.KeyDown += KeyDown;
            Control.KeyUp += KeyUp;
        }

        public void Init()
        {
            if (Control == null)
                return;            

            swapChain = Control.SwapChain;
            
            Device tDevice = Control.Device;
            //camera = new Camera(Vector3.Zero, Vector3.Zero, ref tDevice);
            device = tDevice;
            context = device.ImmediateContext;

            //

            string shaderFile = $"{Path.GetDirectoryName(Application.ExecutablePath)}\\triangle.fx";

            using (var bytecode = ShaderBytecode.CompileFromFile(shaderFile, "VShader", "vs_4_0", ShaderFlags.None, EffectFlags.None))
                //effect = new Effect(device, bytecode);
            {
                inputSignature = ShaderSignature.GetInputSignature(bytecode);
                vertexShader = new VertexShader(device, bytecode);
            }

            // load and compile the pixel shader
            using (var bytecode = ShaderBytecode.CompileFromFile(shaderFile, "PShader", "ps_4_0", ShaderFlags.None, EffectFlags.None))
                pixelShader = new PixelShader(device, bytecode);

            // create test vertex data, making sure to rewind the stream afterward
            /*var vertices = new DataStream(12 * 3, true, true);
            vertices.Write(new Vector3(0, .25f, .5f));
            vertices.Write(new Vector3(0, -.5f, .5f));
            vertices.Write(new Vector3(-.5f, -.5f, .5f));
            vertices.Position = 0;

            // create the vertex layout and buffer
            var elements = new[] { new InputElement("POSITION", 0, Format.R32G32B32_Float, 0) };
            var layout = new InputLayout(device, inputSignature, elements);
            var vertexBuffer = new Buffer(device, vertices, 12 * 3, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None,
                ResourceOptionFlags.None, 0);

            // configure the Input Assembler portion of the pipeline with the vertex data
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, 12, 0));

            // set the shaders
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);          

            // render
            Render(Color.Black);*/
        }

        public void SetMesh(Mesh mesh)
        {
            Mesh = mesh;

            DataStream stream = new DataStream(12 * mesh.Vertices.Length, true, true);
            for (int i = 0; i < mesh.Vertices.Length; i++)
            {
                stream.Write(new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z));
            }
            stream.Position = 0;
            
            // create the vertex layout and buffer
            var elements = new[] { new InputElement("POSITION", 0, Format.R32G32B32_Float, 0) };
            var layout = new InputLayout(device, inputSignature, elements);
            var vertexBuffer = new Buffer(device, stream, 12 * mesh.Vertices.Length, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None,
               ResourceOptionFlags.None, 0);

            // configure the Input Assembler portion of the pipeline with the vertex data
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, 12, 0));

            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);
        }

        public void Render(Color bgColor)
        {
            //camera.TakeALook();

            if (Mesh != null)
            {
                Control.Clear(bgColor);
                context.Draw(Mesh.Vertices.Length, 0);
                Control.Present();
            }
        }

        private void KeyDown(object sender, KeyEventArgs args)
        {
            switch (args.KeyCode)
            {
                case Keys.A:
                    break;
                case Keys.D:
                    break;
                default:
                    break;
            }
        }

        private void KeyUp(object sender, KeyEventArgs args)
        {
            switch (args.KeyCode)
            {
                case Keys.A:
                    //camera.CameraMove(camera.CamPosition + new Vector3(-.5f, 0, 0));
                    break;
                case Keys.D:
                    break;
                default:
                    break;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
        }
    }
}