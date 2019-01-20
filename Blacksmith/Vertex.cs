using OpenTK;
using OpenTK.Graphics;

namespace Blacksmith
{
    public class Vertex
    {
        public Vector4 Pos;
        public Color4 Color;

        public Vertex(Vector4 pos, Color4 color)
        {
            Pos = pos;
            Color = color;
        }
    }
}