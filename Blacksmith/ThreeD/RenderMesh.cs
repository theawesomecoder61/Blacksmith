using SlimDX;
using System.Collections.Generic;

namespace Blacksmith.ThreeD
{
    public class RenderMesh
    {
        public List<float[]> Vertices;

        public List<float[]> Normals;

        public List<float[]> Tangents;

        public List<float[]> Binormals;

        public List<float[]> TexCoords;

        public List<uint[]> Indices;

        public List<RenderMesh.RenderSection> SubSections;

        public Vector3 BoundingBox;

        public Vector3 Origin;

        public RenderMesh()
        {
        }

        public class RenderSection
        {
            public int Offset;

            public int Count;

            public MeshTexture Texture;

            public MeshTexture NormalTexture;

            public RenderSection()
            {
            }
        }
    }
}