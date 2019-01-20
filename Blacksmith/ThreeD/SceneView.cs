using System;
using System.Drawing;
using System.Windows.Forms;
using SlimDX.Direct3D10;
using SlimDX.DXGI;

namespace Blacksmith.ThreeD
{
    public partial class SceneView : UserControl
    {
        private SlimDX.Direct3D10.Device _d3dDevice;
        private SwapChain _swapChain;

        private Texture2D _depthStencilBuffer;
        private RenderTargetView _renderTargetView;
        private DepthStencilView _depthStencilView;

        public SceneView()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
                InitDirect3D();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_d3dDevice != null && (Width > 0) && (Height > 0))
            {
                ResizeBackBuffer();
                //SetProjection();

                // The control is not repainted when we size down, but we need it to
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Don't use Direct3D in design mode
            if (DesignMode || _d3dDevice == null)
            {
                e.Graphics.Clear(Color.White);
            }
            else
            {
                if (_d3dDevice != null)
                {
                    _d3dDevice.ClearRenderTargetView(_renderTargetView, Color.LightGray);
                    _d3dDevice.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);

                    _swapChain.Present(0, PresentFlags.None);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        private void InitDirect3D()
        {
            SwapChainDescription sd = new SwapChainDescription
            {
                ModeDescription = new ModeDescription(Width, Height, new SlimDX.Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = Handle,
                IsWindowed = true,
                SwapEffect = SwapEffect.Discard,
                Flags = SwapChainFlags.None
            };

            // Create the device
            DeviceCreationFlags createDeviceFlags = DeviceCreationFlags.None;
#if DEBUG
            createDeviceFlags |= DeviceCreationFlags.Debug;
#endif

            SlimDX.Direct3D10.Device.CreateWithSwapChain(null, DriverType.Hardware, createDeviceFlags, sd, out _d3dDevice, out _swapChain);

            ResizeBackBuffer();
        }

        private void ResizeBackBuffer()
        {
            // Release the old views, as they hold references to the buffers we
            // will be destroying.  Also release the old depth/stencil buffer

            if (_renderTargetView != null)
                _renderTargetView.Dispose();

            if (_depthStencilView != null)
                _depthStencilView.Dispose();

            if (_depthStencilBuffer != null)
                _depthStencilBuffer.Dispose();

            _swapChain.ResizeBuffers(1, ClientSize.Width, ClientSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

            Texture2D backBuffer = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0);
            _renderTargetView = new RenderTargetView(_d3dDevice, backBuffer);
            backBuffer.Dispose();

            // Create the depth/stencil buffer and view
            Texture2DDescription depthStencilDesc = new Texture2DDescription
            {
                Width = ClientSize.Width,
                Height = ClientSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = 0,
                OptionFlags = ResourceOptionFlags.None
            };

            _depthStencilBuffer = new Texture2D(_d3dDevice, depthStencilDesc);
            _depthStencilView = new DepthStencilView(_d3dDevice, _depthStencilBuffer);

            // Bind the render target view and depth/stencil view to the pipeline
            _d3dDevice.OutputMerger.SetTargets(_depthStencilView, _renderTargetView);

            // Set the viewport transform
            Viewport vp = new Viewport(0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f);
            _d3dDevice.Rasterizer.SetViewports(vp);
        }

        /// <summary>
        /// DOES NOT WORK
        /// </summary>
        /// <param name="fileName"></param>
        public void DisplayTexture(string fileName)
        {
            ImageLoadInformation loadInfo = new ImageLoadInformation
            {
                OptionFlags = ResourceOptionFlags.TextureCube
            };

            Texture2D texture = Texture2D.FromFile(_d3dDevice, fileName);
            ShaderResourceView textureResourceView = new ShaderResourceView(_d3dDevice, texture);
            _d3dDevice.PixelShader.SetShaderResource(textureResourceView, 0);
        }
    }
}