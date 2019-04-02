using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D10;
using SlimDX.Direct3D10_1;
using SlimDX.DirectInput;
using SlimDX.DXGI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Buffer = SlimDX.Direct3D10.Buffer;
using Device1 = SlimDX.Direct3D10_1.Device1;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Effect = SlimDX.Direct3D10.Effect;
using System.Windows.Forms;

namespace Blacksmith.Three
{
    public class D3DScene
    {
        /*private Device1 D3DDevice;

        private DataStream SampleStream;

        private InputLayout SampleLayout;

        private Buffer SampleVertices;

        private Buffer SampleIndices;

        private RenderTargetView SampleRenderView;

        private DepthStencilView SampleDepthView;

        private Effect SampleEffect;

        private Texture2D DepthTexture;

        private int WindowWidth;

        private int WindowHeight;

        private Texture2D PreviewTexture;

        private ShaderResourceView PreviewSRV;

        private uint ColorMask;

        private bool bRenderStaticMesh;

        private int NumVertices;

        private int NumIndices;

        private Camera TheCamera;

        private double AngleX;

        private double AngleY;

        private double TransX;

        private double TransY;

        private double Zoom;

        private Point PrevPoint;

        private RenderMesh StaticMesh;

        private RasterizerState DefaultState;

        private RasterizerState WireframeState;

        private bool bRenderNormals;

        private bool bShowNormalTexture;

        private bool bShowNormals;

        private Effect LineEffect;

        private Buffer LineBuffer;

        private InputLayout LineLayout;

        public Texture2D SharedTexture
        {
            get;
            set;
        }

        public D3DScene(int InWidth, int InHeight)
        {
            this.WindowWidth = InWidth;
            this.WindowHeight = InHeight;
            this.ColorMask = 0;
            this.bRenderStaticMesh = false;
            this.bRenderNormals = true;
            this.bShowNormalTexture = false;
            this.InitD3D();
        }

        private void CalculateNormals(RenderMesh Mesh)
        {
            Mesh.Normals = new List<float[]>();
            for (int i = 0; i < Mesh.Vertices.Count; i++)
            {
                Mesh.Normals.Add(new float[3]);
            }
            for (int j = 0; j < Mesh.Indices.Count; j++)
            {
                int item = (int)Mesh.Indices[j][0];
                int x = (int)Mesh.Indices[j][1];
                int y = (int)Mesh.Indices[j][2];
                Vector3 vector3 = new Vector3(Mesh.Vertices[item][0], Mesh.Vertices[item][1], Mesh.Vertices[item][2]);
                Vector3 vector31 = new Vector3(Mesh.Vertices[x][0], Mesh.Vertices[x][1], Mesh.Vertices[x][2]);
                Vector3 vector32 = new Vector3(Mesh.Vertices[y][0], Mesh.Vertices[y][1], Mesh.Vertices[y][2]);
                Vector3 vector33 = new Vector3()
                {
                    X = vector31.X - vector3.X,
                    Y = vector31.Y - vector3.Y,
                    Z = vector31.Z - vector3.Z
                };
                Vector3 vector34 = new Vector3()
                {
                    X = vector3.X - vector32.X,
                    Y = vector3.Y - vector32.Y,
                    Z = vector3.Z - vector32.Z
                };
                Vector3 vector35 = Vector3.Cross(vector33, vector34);
                vector35.Normalize();
                Mesh.Normals[item][0] += vector35.X;
                Mesh.Normals[item][1] += vector35.Y;
                Mesh.Normals[item][2] += vector35.Z;
                Mesh.Normals[x][0] += vector35.X;
                Mesh.Normals[x][1] += vector35.Y;
                Mesh.Normals[x][2] += vector35.Z;
                Mesh.Normals[y][0] += vector35.X;
                Mesh.Normals[y][1] += vector35.Y;
                Mesh.Normals[y][2] += vector35.Z;
            }
            for (int k = 0; k < Mesh.Vertices.Count; k++)
            {
                Vector3 vector36 = new Vector3(Mesh.Normals[k][0], Mesh.Normals[k][1], Mesh.Normals[k][2]);
                vector36.Normalize();
                Mesh.Normals[k][0] = vector36.X;
                Mesh.Normals[k][1] = vector36.Y;
                Mesh.Normals[k][2] = vector36.Z;
            }
        }

        private void CalculateTangentBinormal(RenderMesh Mesh)
        {
            Vector3 vector3;
            Mesh.Tangents = new List<float[]>();
            Mesh.Binormals = new List<float[]>();
            for (int i = 0; i < Mesh.Vertices.Count; i++)
            {
                Mesh.Tangents.Add(new float[4]);
                Mesh.Binormals.Add(new float[3]);
            }
            for (int j = 0; j < Mesh.Indices.Count; j++)
            {
                int item = (int)Mesh.Indices[j][0];
                int x = (int)Mesh.Indices[j][1];
                int y = (int)Mesh.Indices[j][2];
                Vector3 vector31 = this.ToVector3(Mesh.Vertices[item]);
                Vector3 vector32 = this.ToVector3(Mesh.Vertices[x]);
                Vector3 vector33 = this.ToVector3(Mesh.Vertices[y]);
                Vector2 vector2 = this.ToVector2(Mesh.TexCoords[item]);
                Vector2 vector21 = this.ToVector2(Mesh.TexCoords[x]);
                Vector2 vector22 = this.ToVector2(Mesh.TexCoords[y]);
                float single = vector21.X - vector2.X;
                float x1 = vector22.X - vector2.X;
                float y1 = vector21.Y - vector2.Y;
                float single1 = vector22.Y - vector2.Y;
                float single2 = x1 * y1 - single * x1;
                Vector3 vector34 = vector32 - vector31;
                Vector3 vector35 = vector33 - vector31;
                if (single2 != 0f)
                {
                    vector3 = Vector3.Normalize(((y1 * vector35) - (single1 * vector34)) * (1f / single2));
                }
                else if (single == 0f)
                {
                    vector3 = (x1 == 0f ? new Vector3(0f, 0f, 0f) : Vector3.Normalize(vector35 * (1f / x1)));
                }
                else
                {
                    vector3 = Vector3.Normalize(vector34 * (1f / single));
                }
                Vector3 vector36 = Vector3.Normalize(Vector3.Cross(vector32 - vector31, vector33 - vector31));
                Vector3 vector37 = Vector3.Normalize(Vector3.Cross(vector36, vector3));
                Mesh.Tangents[item][0] += vector3.X;
                Mesh.Tangents[item][1] += vector3.Y;
                Mesh.Tangents[item][2] += vector3.Z;
                Mesh.Tangents[x][0] += vector3.X;
                Mesh.Tangents[x][1] += vector3.Y;
                Mesh.Tangents[x][2] += vector3.Z;
                Mesh.Tangents[y][0] += vector3.X;
                Mesh.Tangents[y][1] += vector3.Y;
                Mesh.Tangents[y][2] += vector3.Z;
                Mesh.Binormals[item][0] += vector37.X;
                Mesh.Binormals[item][1] += vector37.Y;
                Mesh.Binormals[item][2] += vector37.Z;
                Mesh.Binormals[x][0] += vector37.X;
                Mesh.Binormals[x][1] += vector37.Y;
                Mesh.Binormals[x][2] += vector37.Z;
                Mesh.Binormals[y][0] += vector37.X;
                Mesh.Binormals[y][1] += vector37.Y;
                Mesh.Binormals[y][2] += vector37.Z;
            }
            for (int k = 0; k < Mesh.Vertices.Count; k++)
            {
                Vector3 vector38 = this.ToVector3(Mesh.Tangents[k]);
                Vector3 vector39 = this.ToVector3(Mesh.Binormals[k]);
                vector38.Normalize();
                vector39.Normalize();
                Mesh.Tangents[k][0] = vector38.X;
                Mesh.Tangents[k][1] = vector38.Y;
                Mesh.Tangents[k][2] = vector38.Z;
                Mesh.Binormals[k][0] = vector39.X;
                Mesh.Binormals[k][1] = vector39.Y;
                Mesh.Binormals[k][2] = vector39.Z;
            }
        }

        public void CreateTexture(ref MeshTexture Texture)
        {
            Format format = Format.Unknown;
            int num = 0;
            if (Texture.Format == "PF_DXT1")
            {
                format = Format.BC1_UNorm;
                num = 2;
            }
            else if (Texture.Format == "PF_DXT5")
            {
                format = Format.BC3_UNorm;
                num = 4;
            }
            else if (Texture.Format == "PF_BC5")
            {
                format = Format.BC5_UNorm;
                num = 4;
            }
            else if (Texture.Format == "PF_V8U8")
            {
                format = Format.R8G8_UNorm;
                num = 2;
            }
            else if (Texture.Format == "PF_A8R8G8B8")
            {
                format = Format.B8G8R8A8_UNorm;
                num = 4;
            }
            if (format == Format.Unknown)
            {
                return;
            }
            Texture2DDescription texture2DDescription = new Texture2DDescription()
            {
                BindFlags = BindFlags.ShaderResource,
                Format = format,
                Width = Texture.Width,
                Height = Texture.Height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Dynamic,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.Write,
                ArraySize = 1
            };
            DataStream dataStream = new DataStream((long)((int)Texture.Data.Length), true, true);
            dataStream.WriteRange<byte>(Texture.Data);
            dataStream.Position = (long)0;
            ShaderResourceViewDescription shaderResourceViewDescription = new ShaderResourceViewDescription()
            {
                Format = format,
                MipLevels = 1,
                MostDetailedMip = 0,
                Dimension = ShaderResourceViewDimension.Texture2D
            };
            Texture.Texture = new Texture2D(this.D3DDevice, texture2DDescription, new DataRectangle(num * Texture.Width, dataStream));
            Texture.SRV = new ShaderResourceView(this.D3DDevice, Texture.Texture, shaderResourceViewDescription);
        }

        private void DestroyD3D()
        {
            this.DisposeStaticMesh();
            if (this.DefaultState != null)
            {
                this.DefaultState.Dispose();
                this.DefaultState = null;
            }
            if (this.WireframeState != null)
            {
                this.WireframeState.Dispose();
                this.WireframeState = null;
            }
            if (this.SampleVertices != null)
            {
                this.SampleVertices.Dispose();
                this.SampleVertices = null;
            }
            if (this.SampleLayout != null)
            {
                this.SampleLayout.Dispose();
                this.SampleLayout = null;
            }
            if (this.SampleEffect != null)
            {
                this.SampleEffect.Dispose();
                this.SampleEffect = null;
            }
            if (this.SampleRenderView != null)
            {
                this.SampleRenderView.Dispose();
                this.SampleRenderView = null;
            }
            if (this.SampleDepthView != null)
            {
                this.SampleDepthView.Dispose();
                this.SampleDepthView = null;
            }
            if (this.SampleStream != null)
            {
                this.SampleStream.Dispose();
                this.SampleStream = null;
            }
            if (this.SampleLayout != null)
            {
                this.SampleLayout.Dispose();
                this.SampleLayout = null;
            }
            if (this.SharedTexture != null)
            {
                this.SharedTexture.Dispose();
                this.SharedTexture = null;
            }
            if (this.DepthTexture != null)
            {
                this.DepthTexture.Dispose();
                this.DepthTexture = null;
            }
            if (this.D3DDevice != null)
            {
                this.D3DDevice.Dispose();
                this.D3DDevice = null;
            }
        }

        public void Dispose()
        {
            this.DestroyD3D();
        }

        public void DisposeStaticMesh()
        {
            if (this.StaticMesh != null)
            {
                for (int i = 0; i < this.StaticMesh.SubSections.Count; i++)
                {
                    if (this.StaticMesh.SubSections[i].Texture != null)
                    {
                        this.StaticMesh.SubSections[i].Texture.Texture.Dispose();
                        this.StaticMesh.SubSections[i].Texture.SRV.Dispose();
                    }
                    if (this.StaticMesh.SubSections[i].NormalTexture != null)
                    {
                        this.StaticMesh.SubSections[i].NormalTexture.Texture.Dispose();
                        this.StaticMesh.SubSections[i].NormalTexture.SRV.Dispose();
                    }
                }
            }
            if (this.SampleLayout != null)
            {
                this.SampleLayout.Dispose();
                this.SampleLayout = null;
            }
            if (this.SampleVertices != null)
            {
                this.SampleVertices.Dispose();
                this.SampleVertices = null;
            }
            if (this.SampleIndices != null)
            {
                this.SampleIndices.Dispose();
                this.SampleIndices = null;
            }
            if (this.SampleEffect != null)
            {
                this.SampleEffect.Dispose();
                this.SampleEffect = null;
            }
        }

        public void GenerateLines(RenderMesh Mesh)
        {
            this.LineEffect = Effect.FromString(this.D3DDevice, "\r\n\r\n            Matrix World;\r\n            Matrix View;\r\n            Matrix Projection;\r\n            float3 Origin;\r\n\r\n            struct VS_IN\r\n            {\r\n                float4 pos : POSITION;\r\n                float4 col : COLOR0;\r\n            };\r\n\r\n            struct PS_IN\r\n            {\r\n                float4 pos : SV_POSITION;\r\n                float4 col : TEXCOORD0;\r\n            };\r\n\r\n            PS_IN VS( VS_IN input )\r\n            {\r\n                PS_IN output = (PS_IN)0;\r\n\r\n                float4 Pos = input.pos - float4( Origin, 1.0f );\r\n                Pos.w = 1.0f;\r\n\r\n                float4 WorldPos = mul( Pos, World );\r\n                output.pos = mul( WorldPos, View );\r\n                output.pos = mul( output.pos, Projection );\r\n                output.col = input.col;\r\n\r\n                return output;\r\n            }\r\n\r\n            float4 PS( PS_IN input ) : SV_Target\r\n            {\r\n                return input.col;\r\n            }\r\n\r\n            technique10 Render\r\n            {\r\n                pass P0\r\n                {\r\n                    SetVertexShader( CompileShader( vs_4_0, VS() ) );\r\n                    SetGeometryShader( 0 );\r\n                    SetPixelShader( CompileShader( ps_4_0, PS() ) );\r\n                }\r\n            }\r\n            ", "fx_4_0");
            EffectPass passByIndex = this.LineEffect.GetTechniqueByIndex(0).GetPassByIndex(0);
            Device1 d3DDevice = this.D3DDevice;
            ShaderSignature signature = passByIndex.Description.Signature;
            InputElement[] inputElement = new InputElement[] { new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0), new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0) };
            this.LineLayout = new InputLayout(d3DDevice, signature, inputElement);
            this.SampleStream = new DataStream((long)(Mesh.Vertices.Count * 3 * 2 * 32), true, true);
            for (int i = 0; i < Mesh.Vertices.Count; i++)
            {
                Vector3 vector3 = new Vector3(Mesh.Vertices[i][0], Mesh.Vertices[i][1], Mesh.Vertices[i][2]);
                Vector3 vector31 = new Vector3(Mesh.Normals[i][4], Mesh.Normals[i][5], Mesh.Normals[i][6]);
                Vector3 vector32 = new Vector3(Mesh.Tangents[i][0], Mesh.Tangents[i][1], Mesh.Tangents[i][2]);
                Vector3 vector33 = new Vector3(Mesh.Binormals[i][0], Mesh.Binormals[i][1], Mesh.Binormals[i][2]);
                Vector3 vector34 = vector3 + (2f * vector31);
                Vector3 vector35 = vector3 + (2f * vector32);
                Vector3 vector36 = vector3 + (2f * vector33);
                this.SampleStream.Write<float>(vector3.X);
                this.SampleStream.Write<float>(vector3.Y * -1f);
                this.SampleStream.Write<float>(vector3.Z);
                this.SampleStream.Write<float>(1f);
                this.SampleStream.Write<Vector4>(new Vector4(0f, 1f, 0f, 1f));
                this.SampleStream.Write<float>(vector34.X);
                this.SampleStream.Write<float>(vector34.Y * -1f);
                this.SampleStream.Write<float>(vector34.Z);
                this.SampleStream.Write<float>(1f);
                this.SampleStream.Write<Vector4>(new Vector4(0f, 1f, 0f, 1f));
                this.SampleStream.Write<float>(vector3.X);
                this.SampleStream.Write<float>(vector3.Y * -1f);
                this.SampleStream.Write<float>(vector3.Z);
                this.SampleStream.Write<float>(1f);
                this.SampleStream.Write<Vector4>(new Vector4(0f, 0f, 1f, 1f));
                this.SampleStream.Write<float>(vector35.X);
                this.SampleStream.Write<float>(vector35.Y * -1f);
                this.SampleStream.Write<float>(vector35.Z);
                this.SampleStream.Write<float>(1f);
                this.SampleStream.Write<Vector4>(new Vector4(0f, 0f, 1f, 1f));
                this.SampleStream.Write<float>(vector3.X);
                this.SampleStream.Write<float>(vector3.Y * -1f);
                this.SampleStream.Write<float>(vector3.Z);
                this.SampleStream.Write<float>(1f);
                this.SampleStream.Write<Vector4>(new Vector4(1f, 0f, 0f, 1f));
                this.SampleStream.Write<float>(vector36.X);
                this.SampleStream.Write<float>(vector36.Y * -1f);
                this.SampleStream.Write<float>(vector36.Z);
                this.SampleStream.Write<float>(1f);
                this.SampleStream.Write<Vector4>(new Vector4(1f, 0f, 0f, 1f));
            }
            this.SampleStream.Position = (long)0;
            Device1 device1 = this.D3DDevice;
            DataStream sampleStream = this.SampleStream;
            BufferDescription bufferDescription = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Mesh.Vertices.Count * 3 * 2 * 32,
                Usage = ResourceUsage.Default
            };
            this.LineBuffer = new Buffer(device1, sampleStream, bufferDescription);
        }

        private void InitD3D()
        {
            try
            {
                this.D3DDevice = new Device1(DriverType.Hardware, DeviceCreationFlags.BgraSupport, FeatureLevel.Level_10_0);
                Texture2DDescription texture2DDescription = new Texture2DDescription()
                {
                    BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = this.WindowWidth,
                    Height = this.WindowHeight,
                    MipLevels = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    OptionFlags = ResourceOptionFlags.Shared,
                    CpuAccessFlags = CpuAccessFlags.None,
                    ArraySize = 1
                };
                Texture2DDescription texture2DDescription1 = new Texture2DDescription()
                {
                    BindFlags = BindFlags.DepthStencil,
                    Format = Format.D32_Float_S8X24_UInt,
                    Width = this.WindowWidth,
                    Height = this.WindowHeight,
                    MipLevels = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.None,
                    ArraySize = 1
                };
                this.SharedTexture = new Texture2D(this.D3DDevice, texture2DDescription);
                this.DepthTexture = new Texture2D(this.D3DDevice, texture2DDescription1);
                this.SampleRenderView = new RenderTargetView(this.D3DDevice, this.SharedTexture);
                this.SampleDepthView = new DepthStencilView(this.D3DDevice, this.DepthTexture);
                RasterizerStateDescription rasterizerStateDescription = new RasterizerStateDescription()
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid
                };
                RasterizerStateDescription rasterizerStateDescription1 = new RasterizerStateDescription()
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Wireframe
                };
                this.DefaultState = RasterizerState.FromDescription(this.D3DDevice, rasterizerStateDescription);
                this.WireframeState = RasterizerState.FromDescription(this.D3DDevice, rasterizerStateDescription1);
                this.D3DDevice.Flush();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        public void KeyUp(System.Windows.Input.Key KeyPressed)
        {
            if (KeyPressed == System.Windows.Input.Key.L)
            {
                this.bRenderNormals = !this.bRenderNormals;
                return;
            }
            if (KeyPressed == System.Windows.Input.Key.N)
            {
                this.bShowNormals = !this.bShowNormals;
            }
        }

        public void MouseMove(object sender, MouseEventArgs e, object PreviewGrid)
        {
            if (this.bRenderStaticMesh)
            {
                /*Point position = e.GetPosition(PreviewGrid);
                double x = position.X - this.PrevPoint.X;
                double y = position.Y - this.PrevPoint.Y;
                if (e.LeftButton == MouseButtonState.Pressed && this.PrevPoint.X != 0 && this.PrevPoint.Y != 0)
                {
                    D3DScene angleY = this;
                    angleY.AngleY = angleY.AngleY + 1 * y;
                    if (this.AngleY > 360)
                    {
                        this.AngleY = 0;
                    }
                    if (this.AngleY < 0)
                    {
                        this.AngleY = 360;
                    }
                    D3DScene angleX = this;
                    angleX.AngleX = angleX.AngleX + 1 * x;
                    if (this.AngleX > 360)
                    {
                        this.AngleX = 0;
                    }
                    if (this.AngleX < 0)
                    {
                        this.AngleX = 360;
                    }
                    RotateTransform3D rotateTransform3D = new RotateTransform3D();
                    RotateTransform3D axisAngleRotation3D = new RotateTransform3D();
                    rotateTransform3D.Rotation = new AxisAngleRotation3D(new Vector3D(1, 0, 0), this.AngleY);
                    axisAngleRotation3D.Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), this.AngleX);
                    Matrix3D value = rotateTransform3D.Value;
                    value.Append(axisAngleRotation3D.Value);
                    this.TheCamera.ModelRotate.M11 = (float)value.M11;
                    this.TheCamera.ModelRotate.M12 = (float)value.M12;
                    this.TheCamera.ModelRotate.M13 = (float)value.M13;
                    this.TheCamera.ModelRotate.M14 = (float)value.M14;
                    this.TheCamera.ModelRotate.M21 = (float)value.M21;
                    this.TheCamera.ModelRotate.M22 = (float)value.M22;
                    this.TheCamera.ModelRotate.M23 = (float)value.M23;
                    this.TheCamera.ModelRotate.M24 = (float)value.M24;
                    this.TheCamera.ModelRotate.M31 = (float)value.M31;
                    this.TheCamera.ModelRotate.M32 = (float)value.M32;
                    this.TheCamera.ModelRotate.M33 = (float)value.M33;
                    this.TheCamera.ModelRotate.M34 = (float)value.M34;
                    this.TheCamera.ModelRotate.M41 = (float)value.OffsetX;
                    this.TheCamera.ModelRotate.M42 = (float)value.OffsetY;
                    this.TheCamera.ModelRotate.M43 = (float)value.OffsetZ;
                    this.TheCamera.ModelRotate.M44 = (float)value.M44;
                }
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    D3DScene transY = this;
                    transY.TransY = transY.TransY - 1 * y;
                    D3DScene transX = this;
                    transX.TransX = transX.TransX + 1 * x;
                    this.TheCamera.ModelTranslate = Matrix.Translation((float)this.TransX, (float)this.TransY, 0f);
                }
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    D3DScene zoom = this;
                    zoom.Zoom = zoom.Zoom + 1 * y;
                    if (this.Zoom < 0)
                    {
                        this.Zoom = 0;
                    }
                }
                if (this.TheCamera != null)
                {
                    this.TheCamera.View = Matrix.LookAtRH(new Vector3(0f, 0f, (float)this.Zoom), Vector3.Zero, new Vector3(0f, 1f, 0f));
                }
                this.PrevPoint = position;*
            }
        }

        public void Render(int arg)
        {
            if (this.D3DDevice == null)
            {
                return;
            }
            this.D3DDevice.OutputMerger.SetTargets(this.SampleDepthView, this.SampleRenderView);
            this.D3DDevice.Rasterizer.SetViewports(new Viewport(0, 0, this.WindowWidth, this.WindowHeight, 0f, 1f));
            this.D3DDevice.ClearDepthStencilView(this.SampleDepthView, DepthStencilClearFlags.Depth, 1f, 0);
            this.D3DDevice.ClearRenderTargetView(this.SampleRenderView, new Color4(1f, 0f, 0f, 0f));
            if (this.SampleEffect != null)
            {
                if (this.bRenderStaticMesh)
                {
                    this.D3DDevice.InputAssembler.SetInputLayout(this.SampleLayout);
                    this.D3DDevice.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
                    this.D3DDevice.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.SampleVertices, 60, 0));
                    this.D3DDevice.InputAssembler.SetIndexBuffer(this.SampleIndices, Format.R16_UInt, 0);
                    EffectTechnique techniqueByIndex = this.SampleEffect.GetTechniqueByIndex(0);
                    EffectPass passByIndex = techniqueByIndex.GetPassByIndex(0);
                    this.SampleEffect.GetVariableByName("World").AsMatrix().SetMatrix(this.TheCamera.ModelRotate * this.TheCamera.ModelTranslate);
                    this.SampleEffect.GetVariableByName("View").AsMatrix().SetMatrix(this.TheCamera.View);
                    this.SampleEffect.GetVariableByName("Projection").AsMatrix().SetMatrix(this.TheCamera.Projection);
                    for (int i = 0; i < this.StaticMesh.SubSections.Count; i++)
                    {
                        ShaderResourceView sRV = null;
                        if (this.bShowNormalTexture && this.StaticMesh.SubSections[i].NormalTexture != null)
                        {
                            sRV = this.StaticMesh.SubSections[i].NormalTexture.SRV;
                        }
                        if ((!this.bShowNormalTexture || sRV == null) && this.StaticMesh.SubSections[i].Texture != null)
                        {
                            sRV = this.StaticMesh.SubSections[i].Texture.SRV;
                        }
                        this.D3DDevice.Rasterizer.State = (sRV == null ? this.WireframeState : this.DefaultState);
                        this.SampleEffect.GetVariableByName("Origin").AsVector().Set(this.StaticMesh.Origin);
                        this.SampleEffect.GetVariableByName("PreviewTexture").AsResource().SetResource(sRV);
                        this.SampleEffect.GetVariableByName("Color").AsScalar().Set((sRV == null ? 1f : 0f));
                        this.SampleEffect.GetVariableByName("bRenderNormal").AsScalar().Set((this.StaticMesh.SubSections[i].NormalTexture == null || !this.bRenderNormals ? false : true));
                        sRV = null;
                        if (this.StaticMesh.SubSections[i].NormalTexture != null && this.bRenderNormals && !this.bShowNormalTexture)
                        {
                            Matrix matrix = Matrix.Invert(this.TheCamera.View);
                            sRV = this.StaticMesh.SubSections[i].NormalTexture.SRV;
                            this.SampleEffect.GetVariableByName("LightPosition").AsVector().Set(new Vector4(matrix.M41, matrix.M42, matrix.M43, 1f));
                            this.SampleEffect.GetVariableByName("InvWorld").AsMatrix().SetMatrix(Matrix.Invert(this.TheCamera.ModelRotate * this.TheCamera.ModelTranslate));
                        }
                        this.SampleEffect.GetVariableByName("NormalTexture").AsResource().SetResource(sRV);
                        for (int j = 0; j < techniqueByIndex.Description.PassCount; j++)
                        {
                            passByIndex.Apply();
                            this.D3DDevice.DrawIndexed(this.StaticMesh.SubSections[i].Count * 3, this.StaticMesh.SubSections[i].Offset * 3, 0);
                        }
                    }
                    if (this.bShowNormals)
                    {
                        this.LineEffect.GetVariableByName("Origin").AsVector().Set(this.StaticMesh.Origin);
                        this.LineEffect.GetVariableByName("World").AsMatrix().SetMatrix(this.TheCamera.ModelRotate * this.TheCamera.ModelTranslate);
                        this.LineEffect.GetVariableByName("View").AsMatrix().SetMatrix(this.TheCamera.View);
                        this.LineEffect.GetVariableByName("Projection").AsMatrix().SetMatrix(this.TheCamera.Projection);
                        this.D3DDevice.InputAssembler.SetInputLayout(this.LineLayout);
                        this.D3DDevice.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.LineList);
                        this.D3DDevice.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.LineBuffer, 32, 0));
                        techniqueByIndex = this.LineEffect.GetTechniqueByIndex(0);
                        passByIndex = techniqueByIndex.GetPassByIndex(0);
                        for (int k = 0; k < techniqueByIndex.Description.PassCount; k++)
                        {
                            passByIndex.Apply();
                            this.D3DDevice.Draw(this.StaticMesh.Vertices.Count * 2 * 3, 0);
                        }
                    }
                }
                else
                {
                    this.D3DDevice.InputAssembler.SetInputLayout(this.SampleLayout);
                    this.D3DDevice.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
                    this.D3DDevice.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.SampleVertices, 32, 0));
                    this.D3DDevice.Rasterizer.State = this.DefaultState;
                    EffectTechnique effectTechnique = this.SampleEffect.GetTechniqueByIndex(0);
                    EffectPass effectPass = effectTechnique.GetPassByIndex(0);
                    this.SampleEffect.GetVariableByName("PreviewTexture").AsResource().SetResource(this.StaticMesh.SubSections[0].Texture.SRV);
                    this.SampleEffect.GetVariableByName("MaskR").AsScalar().Set(((this.ColorMask & -16777216) == -16777216 ? 1f : 0f));
                    this.SampleEffect.GetVariableByName("MaskG").AsScalar().Set(((this.ColorMask & 16711680) == 16711680 ? 1f : 0f));
                    this.SampleEffect.GetVariableByName("MaskB").AsScalar().Set(((this.ColorMask & 65280) == 65280 ? 1f : 0f));
                    this.SampleEffect.GetVariableByName("MaskA").AsScalar().Set(((this.ColorMask & 255) == 255 ? 1f : 0f));
                    for (int l = 0; l < effectTechnique.Description.PassCount; l++)
                    {
                        effectPass.Apply();
                        this.D3DDevice.Draw(6, 0);
                    }
                }
            }
            this.D3DDevice.Flush();
        }

        public void ResetCamera()
        {
            this.TheCamera = new Camera()
            {
                View = Matrix.LookAtRH(new Vector3(0f, 0f, 90f), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f)),
                Projection = Matrix.PerspectiveFovRH(1f, (float)this.WindowWidth / (float)this.WindowHeight, 1f, 100000f),
                ModelTranslate = Matrix.Identity,
                ModelRotate = Matrix.Identity
            };
            this.TransX = 0;
            this.TransY = 0;
            this.AngleX = 0;
            this.AngleY = 0;
            this.Zoom = 0;
        }

        public void ResizeD3D(int NewWidth, int NewHeight)
        {
            if (NewWidth == this.WindowWidth && NewHeight == this.WindowHeight)
            {
                return;
            }
            this.WindowWidth = NewWidth;
            this.WindowHeight = NewHeight;
            this.SharedTexture.Dispose();
            this.DepthTexture.Dispose();
            this.SampleRenderView.Dispose();
            this.SampleDepthView.Dispose();
            Texture2DDescription texture2DDescription = new Texture2DDescription()
            {
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                Format = Format.B8G8R8A8_UNorm,
                Width = this.WindowWidth,
                Height = this.WindowHeight,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.Shared,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };
            Texture2DDescription texture2DDescription1 = new Texture2DDescription()
            {
                BindFlags = BindFlags.DepthStencil,
                Format = Format.D32_Float,
                Width = this.WindowWidth,
                Height = this.WindowHeight,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };
            this.SharedTexture = new Texture2D(this.D3DDevice, texture2DDescription);
            this.DepthTexture = new Texture2D(this.D3DDevice, texture2DDescription1);
            this.SampleRenderView = new RenderTargetView(this.D3DDevice, this.SharedTexture);
            this.SampleDepthView = new DepthStencilView(this.D3DDevice, this.DepthTexture);
            this.TheCamera.Projection = Matrix.PerspectiveFovRH(1f, (float)this.WindowWidth / (float)this.WindowHeight, 1f, 100000f);
            this.D3DDevice.Flush();
        }

        public void SetColorMask(uint InColorMask)
        {
            this.ColorMask ^= InColorMask;
        }

        public void SetStaticMeshData(RenderMesh Mesh)
        {
            this.DisposeStaticMesh();
            this.SampleEffect = Effect.FromString(this.D3DDevice, "\r\n            \r\n            float Color;\r\n            float3 Origin;\r\n            float4 LightPosition;\r\n\r\n            Matrix World;\r\n            Matrix InvWorld;\r\n            Matrix View;\r\n            Matrix Projection;\r\n\r\n            Texture2D PreviewTexture;\r\n            Texture2D NormalTexture;\r\n\r\n            bool bRenderNormal;\r\n\r\n            SamplerState Sampler\r\n            {\r\n                Texture = PreviewTexture;\r\n                Filter = MIN_MAG_MIP_LINEAR;\r\n                AddressU = Wrap;\r\n                AddressV = Wrap;\r\n            };\r\n\r\n            BlendState SrcAlphaBlending\r\n            {\r\n                AlphaToCoverageEnable = FALSE;\r\n                BlendEnable[0] = TRUE;\r\n                SrcBlend = One;\r\n                DestBlend = Inv_Src_Alpha;\r\n                BlendOp = Add;\r\n                SrcBlendAlpha = One;\r\n                DestBlendAlpha = Inv_Src_Alpha;\r\n                BlendOpAlpha = Add;\r\n                RenderTargetWriteMask[0] = 0x0F;\r\n            };\r\n\r\n            struct VS_IN\r\n            {\r\n                float4 pos      : POSITION;\r\n                float3 Normal   : NORMAL;\r\n                float3 Tangent  : TANGENT;\r\n                float3 Binormal : BINORMAL;\r\n                float2 tex      : TEXCOORD0;\r\n            };\r\n\r\n            struct PS_IN\r\n            {\r\n                float4 pos      : SV_POSITION;\r\n                float2 tex      : TEXCOORD0;\r\n                float3 LightVec : TEXCOORD1;\r\n                float3 Normal   : TEXCOORD2;\r\n                float3 EyeVec   : TEXCOORD3;\r\n            };\r\n\r\n            float3x3 cotangent_frame( float3 N, float3 p, float2 UV )\r\n            {\r\n                float3 dp1 = ddx( p );\r\n                float3 dp2 = ddy( p );\r\n                float2 duv1 = ddx( UV );\r\n                float2 duv2 = ddy( UV );\r\n\r\n                float3 dp2perp = cross( dp2, N );\r\n                float3 dp1perp = cross( N, dp1 );\r\n                float3 T = dp2perp * duv1.x + dp1perp * duv2.x;\r\n                float3 B = dp2perp * duv1.y + dp1perp * duv2.y;\r\n   \r\n                float invmax = sqrt( max( dot( T, T ), dot( B, B ) ) );\r\n                return float3x3( T * invmax, B * invmax, N );\r\n            }\r\n\r\n            PS_IN VS( VS_IN input )\r\n            {\r\n                PS_IN output = (PS_IN)0;\r\n\r\n                float4 Pos = input.pos - float4( Origin, 1.0f );\r\n                Pos.w = 1.0f;\r\n\r\n                float4 WorldPos = mul( Pos, World );\r\n                output.pos = mul( WorldPos, View );\r\n                output.pos = mul( output.pos, Projection );\r\n                output.tex = input.tex;\r\n\r\n                //input.Binormal = float3( 0.0f, -1.0f, 0.0f );\r\n                //output.TBN = float3x3( input.Tangent, input.Binormal, input.Normal );\r\n\r\n                float3 LightToPoint = float3( 1000.0f, 1000.0f, 1000.0f ) - WorldPos;\r\n                //float3 LightToPoint = LightPosition - WorldPos;\r\n                float3 LightWorld = mul( LightToPoint, (float3x3) InvWorld ).xyz;\r\n\r\n                output.LightVec = LightToPoint;\r\n                output.Normal = mul( input.Normal, (float3x3) World );\r\n                output.EyeVec = -WorldPos;\r\n\r\n                return output;\r\n            }\r\n\r\n            float3 perturb_normal( float3 N, float3 V, float2 texcoord )\r\n            {\r\n                float3 map = NormalTexture.Sample( Sampler, texcoord );\r\n                map.z = sqrt( 1.0f - ( map.x * map.x ) - ( map.y * map.y ) );\r\n                map = 2.0f * map - 1.0f;\r\n\r\n                float3x3 TBN = cotangent_frame( N, -V, texcoord );\r\n                return mul( map, TBN );\r\n            }\r\n\r\n            float4 PS( PS_IN input ) : SV_Target\r\n            {\r\n\t            float4 Final = PreviewTexture.Sample( Sampler, input.tex );            \r\n\t\t        float Opacity = 1.0f;//GetOpacity( input.tex );\r\n                \r\n                if( bRenderNormal )\r\n\t            {\r\n                    float4 Diffuse = Final;\r\n\r\n                    float3 N = normalize( input.Normal );\r\n                    float3 L = normalize( input.LightVec );\r\n                    float3 V = normalize( input.EyeVec );\r\n                    float3 PN = perturb_normal( N, V, input.tex );\r\n\r\n                    float4 AmbientColor = float4( 0.0f, 0.0f, 0.0f, 1.0f );\r\n                    Final = AmbientColor * Diffuse;\r\n\r\n                    float NdotL = max( dot( PN, L ), 0.2f );\r\n                    if( NdotL > 0.0f )\r\n                    {\r\n                    float4 DiffuseColor = float4( 1.0f, 1.0f, 1.0f, 1.0f );\r\n                    float4 SpecularColor = float4( 1.0f, 1.0f, 1.0f, 1.0f );\r\n\r\n                    float3 E = normalize( input.EyeVec );\r\n                    float3 R = reflect( -L, PN );\r\n                    float specular = pow( max( dot( R, E ), 0.0f ), 1.0f );\r\n\r\n                    Final += (Diffuse * DiffuseColor) * 2;\r\n                    Final *= NdotL;\r\n                    Final = saturate( Final );\r\n                    }\r\n\t            }\r\n\t\r\n\t            Final += float4( Color, Color, Color, Color );\r\n\t            return float4( Final.xyz, Opacity );\r\n            }\r\n\r\n            technique10 Render\r\n            {\r\n                pass P0\r\n                {\r\n                    SetBlendState( SrcAlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );\r\n                    SetVertexShader( CompileShader( vs_4_0, VS() ) );\r\n                    SetGeometryShader( 0 );\r\n                    SetPixelShader( CompileShader( ps_4_0, PS() ) );\r\n                }\r\n            }\r\n            ", "fx_4_0");
            EffectPass passByIndex = this.SampleEffect.GetTechniqueByIndex(0).GetPassByIndex(0);
            Device1 d3DDevice = this.D3DDevice;
            ShaderSignature signature = passByIndex.Description.Signature;
            InputElement[] inputElement = new InputElement[] { new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0), new InputElement("NORMAL", 0, Format.R32G32B32_Float, 16, 0), new InputElement("TANGENT", 0, Format.R32G32B32_Float, 28, 0), new InputElement("BINORMAL", 0, Format.R32G32B32_Float, 40, 0), new InputElement("TEXCOORD", 0, Format.R32G32_Float, 52, 0) };
            this.SampleLayout = new InputLayout(d3DDevice, signature, inputElement);
            this.CalculateTangentBinormal(Mesh);
            Vector3 item = new Vector3();
            Vector3 vector3 = new Vector3();
            this.SampleStream = new DataStream((long)(Mesh.Vertices.Count * 60), true, true);
            for (int i = 0; i < Mesh.Vertices.Count; i++)
            {
                this.SampleStream.Write<float>(Mesh.Vertices[i][0]);
                this.SampleStream.Write<float>(Mesh.Vertices[i][1] * -1f);
                this.SampleStream.Write<float>(Mesh.Vertices[i][2]);
                this.SampleStream.Write<float>(1f);
                this.SampleStream.Write<float>(Mesh.Normals[i][4]);
                this.SampleStream.Write<float>(Mesh.Normals[i][5] * -1f);
                this.SampleStream.Write<float>(Mesh.Normals[i][6]);
                this.SampleStream.Write<float>(Mesh.Normals[i][0]);
                this.SampleStream.Write<float>(Mesh.Normals[i][1] * -1f);
                this.SampleStream.Write<float>(Mesh.Normals[i][2]);
                this.SampleStream.Write<float>(Mesh.Binormals[i][0]);
                this.SampleStream.Write<float>(Mesh.Binormals[i][1] * -1f);
                this.SampleStream.Write<float>(Mesh.Binormals[i][2]);
                this.SampleStream.Write<float>(Mesh.TexCoords[i][0]);
                this.SampleStream.Write<float>(Mesh.TexCoords[i][1]);
                if (Mesh.Vertices[i][0] < item.X)
                {
                    item.X = Mesh.Vertices[i][0];
                }
                if (Mesh.Vertices[i][1] * -1f < item.Y)
                {
                    item.Y = Mesh.Vertices[i][1] * -1f;
                }
                if (Mesh.Vertices[i][2] < item.Z)
                {
                    item.Z = Mesh.Vertices[i][2];
                }
                if (Mesh.Vertices[i][0] > vector3.X)
                {
                    vector3.X = Mesh.Vertices[i][0];
                }
                if (Mesh.Vertices[i][1] * -1f > vector3.Y)
                {
                    vector3.Y = Mesh.Vertices[i][1] * -1f;
                }
                if (Mesh.Vertices[i][2] > vector3.Z)
                {
                    vector3.Z = Mesh.Vertices[i][2];
                }
            }
            this.SampleStream.Position = (long)0;
            Mesh.Origin.X = (item.X + vector3.X) / 2f;
            Mesh.Origin.Y = (item.Y + vector3.Y) / 2f;
            Mesh.Origin.Z = (item.Z + vector3.Z) / 2f;
            Device1 device1 = this.D3DDevice;
            DataStream sampleStream = this.SampleStream;
            BufferDescription bufferDescription = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Mesh.Vertices.Count * 60,
                Usage = ResourceUsage.Default
            };
            this.SampleVertices = new Buffer(device1, sampleStream, bufferDescription);
            DataStream dataStream = new DataStream((long)(Mesh.Indices.Count * 2 * 3), true, true);
            for (int j = 0; j < Mesh.Indices.Count; j++)
            {
                dataStream.Write<short>((short)Mesh.Indices[j][0]);
                dataStream.Write<short>((short)Mesh.Indices[j][2]);
                dataStream.Write<short>((short)Mesh.Indices[j][1]);
            }
            dataStream.Position = (long)0;
            Device1 d3DDevice1 = this.D3DDevice;
            BufferDescription bufferDescription1 = new BufferDescription()
            {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Mesh.Indices.Count * 2 * 3,
                Usage = ResourceUsage.Default
            };
            this.SampleIndices = new Buffer(d3DDevice1, dataStream, bufferDescription1);
            this.ResetCamera();
            this.Zoom = (double)(Mesh.BoundingBox.Z * 2f + 100f);
            this.StaticMesh = Mesh;
            for (int k = 0; k < this.StaticMesh.SubSections.Count; k++)
            {
                if (this.StaticMesh.SubSections[k].Texture != null)
                {
                    this.CreateTexture(ref this.StaticMesh.SubSections[k].Texture);
                }
                if (this.StaticMesh.SubSections[k].NormalTexture != null)
                {
                    this.CreateTexture(ref this.StaticMesh.SubSections[k].NormalTexture);
                }
            }
            this.bRenderStaticMesh = true;
            this.GenerateLines(Mesh);
        }

        public void SetTextureData(MeshTexture Texture)
        {
            this.DisposeStaticMesh();
            RenderMesh renderMesh = new RenderMesh()
            {
                SubSections = new List<RenderMesh.RenderSection>()
            };
            RenderMesh.RenderSection renderSection = new RenderMesh.RenderSection()
            {
                Count = 6,
                Texture = Texture
            };
            renderMesh.SubSections.Add(renderSection);
            this.CreateTexture(ref renderSection.Texture);
            this.SampleEffect = Effect.FromString(this.D3DDevice, "\r\n            \r\n            float MaskR = 1.0f;\r\n            float MaskG = 1.0f;\r\n            float MaskB = 1.0f;\r\n            float MaskA = 1.0f;\r\n\r\n            Texture2D PreviewTexture;\r\n\r\n            SamplerState Sampler\r\n            {\r\n                Texture = PreviewTexture;\r\n                Filter = MIN_MAG_MIP_LINEAR;\r\n                AddressU = Wrap;\r\n                AddressV = Wrap;\r\n            };\r\n\r\n            BlendState SrcAlphaBlending\r\n            {\r\n                AlphaToCoverageEnable = FALSE;\r\n                BlendEnable[0] = TRUE;\r\n                SrcBlend = Src_Alpha;\r\n                DestBlend = Inv_Src_Alpha;\r\n                BlendOp = Add;\r\n                SrcBlendAlpha = One;\r\n                DestBlendAlpha = Inv_Src_Alpha;\r\n                BlendOpAlpha = Add;\r\n                RenderTargetWriteMask[0] = 0x0F;\r\n            };\r\n\r\n            struct VS_IN\r\n            {\r\n                float4 pos : POSITION;\r\n                float4 col : TEXCOORD0;\r\n            };\r\n\r\n            struct PS_IN\r\n            {\r\n                float4 pos : SV_POSITION;\r\n                float2 col : TEXCOORD0;\r\n            };\r\n\r\n            PS_IN VS( VS_IN input )\r\n            {\r\n                PS_IN output = (PS_IN)0;\r\n\r\n                output.pos = input.pos;\r\n                output.col = float2( input.col.x, input.col.y );\r\n\r\n                return output;\r\n            }\r\n\r\n            float4 PS( PS_IN input ) : SV_Target\r\n            {\r\n                float4 Color = PreviewTexture.Sample( Sampler, input.col.xy );\r\n                float4 Mask = float4( MaskR, MaskG, MaskB, MaskA );\r\n                float4 Final = Color * Mask;\r\n\r\n                return Final;\r\n            }\r\n\r\n            technique10 Render\r\n            {\r\n                pass P0\r\n                {\r\n                    SetBlendState( SrcAlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );\r\n                    SetVertexShader( CompileShader( vs_4_0, VS() ) );\r\n                    SetGeometryShader( 0 );\r\n                    SetPixelShader( CompileShader( ps_4_0, PS() ) );\r\n                }\r\n            }\r\n            ", "fx_4_0");
            EffectPass passByIndex = this.SampleEffect.GetTechniqueByIndex(0).GetPassByIndex(0);
            Device1 d3DDevice = this.D3DDevice;
            ShaderSignature signature = passByIndex.Description.Signature;
            InputElement[] inputElement = new InputElement[] { new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0), new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, 16, 0) };
            this.SampleLayout = new InputLayout(d3DDevice, signature, inputElement);
            float single = (renderSection.Texture.Width > renderSection.Texture.Height ? 1f : (float)renderSection.Texture.Width / (float)renderSection.Texture.Height);
            float single1 = (renderSection.Texture.Height > renderSection.Texture.Width ? 1f : (float)renderSection.Texture.Height / (float)renderSection.Texture.Width);
            this.SampleStream = new DataStream((long)192, true, true);
            DataStream sampleStream = this.SampleStream;
            Vector4[] vector4 = new Vector4[] { new Vector4(single, single1, 0.5f, 1f), new Vector4(1f, 0f, 0f, 0f), new Vector4(single, -single1, 0.5f, 1f), new Vector4(1f, 1f, 0f, 0f), new Vector4(-single, -single1, 0.5f, 1f), new Vector4(0f, 1f, 0f, 0f), new Vector4(-single, -single1, 0.5f, 1f), new Vector4(0f, 1f, 0f, 0f), new Vector4(-single, single1, 0.5f, 1f), new Vector4(0f, 0f, 0f, 0f), new Vector4(single, single1, 0.5f, 1f), new Vector4(1f, 0f, 0f, 0f) };
            sampleStream.WriteRange<Vector4>(vector4);
            this.SampleStream.Position = (long)0;
            Device1 device1 = this.D3DDevice;
            DataStream dataStream = this.SampleStream;
            BufferDescription bufferDescription = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = 192,
                Usage = ResourceUsage.Default
            };
            this.SampleVertices = new Buffer(device1, dataStream, bufferDescription);
            this.bRenderStaticMesh = false;
            this.StaticMesh = renderMesh;
        }

        public Vector2 ToVector2(float[] v)
        {
            return new Vector2(v[0], v[1]);
        }

        private Vector3 ToVector3(float[] v)
        {
            return new Vector3(v[0], v[1], v[2]);
        }*/
    }
}