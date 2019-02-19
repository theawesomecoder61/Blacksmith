using Blacksmith.Games;
using OpenTK;
using System;
using System.Collections.Generic;

namespace Blacksmith.Three
{
    public class DynamicMesh : Mesh
    {
        private List<Vector3> vertices;
        private List<Tuple<int, int, int>> faces;

        public DynamicMesh(Origins.Submesh submesh)
        {
            vertices = new List<Vector3>();
            faces = new List<Tuple<int, int, int>>();

            foreach (Origins.Vertex v in submesh.Vertices)
            {
                vertices.Add(new Vector3(v.X, v.Y, v.Z));
            }

            foreach (Origins.Face f in submesh.Faces)
            {
                faces.Add(new Tuple<int, int, int>(f.X, f.Y, f.Z));
            }
        }

        public override void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
        }

        public override Vector3[] GetColorData() => new Vector3[] { Vector3.Zero }; // dummy

        public override int[] GetIndices(int offset = 0)
        {
            List<int> temp = new List<int>();
            foreach (var face in faces)
            {
                temp.Add(face.Item1 + offset);
                temp.Add(face.Item2 + offset);
                temp.Add(face.Item3 + offset);
            }
            return temp.ToArray();
        }

        public override Vector3[] GetNormals() => new Vector3[] { Vector3.Zero }; // dummy

        public override Vector2[] GetTextureCoords() => new Vector2[] { Vector2.Zero }; // dymmy

        public override Vector3[] GetVertices() => vertices.ToArray();
    }
}