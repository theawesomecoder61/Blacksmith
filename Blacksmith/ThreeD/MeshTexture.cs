using SlimDX.Direct3D10;

namespace Blacksmith.ThreeD
{
    public class MeshTexture
    {
        public byte[] Data;

        public int Width;

        public int Height;

        public string Format;

        public Texture2D Texture;

        public ShaderResourceView SRV;

        public MeshTexture()
        {
        }
    }
}