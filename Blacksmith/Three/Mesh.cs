using OpenTK;
using System.Collections.Generic;
using System.Linq;

namespace Blacksmith.Three
{
    public class Mesh
    {
        public int ID;

        public List<Vertex> Vertices = new List<Vertex>();
        public int VertexCount;
        public int NumOfVerticesBeforeMe;

        public List<Face> Faces = new List<Face>();
        public int FaceCount;

        public List<int> Indices = new List<int>();
        public int IndexCount;

        public List<Vector3> Normals = new List<Vector3>();
        public List<Vector3> CalculatedNormals = new List<Vector3>();

        public bool IsVisible = true;

        public int MinFaceIndex;
        public int MaxFaceIndex;

        public bool IsTextured = false;
        public int TextureIndex;

        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public Matrix4 ModelMatrix = Matrix4.Identity;
        public Matrix4 ViewProjectionMatrix = Matrix4.Identity;
        public Matrix4 ModelViewProjectionMatrix = Matrix4.Identity;

        public struct Vertex
        {
            // the numbers at the end indicate which vertexTableSize in which that property is utilized
            public Vector3 Position { get; internal set; } // all
            public Vector3 Normal { get; internal set; } // 20, 24, 28, 32, 36, 40, 48
            public Vector2 TextureCoordinate { get; internal set; } // 16, 20, 24, 28, 32, 36, 40, 48
            public Vector4 BoneIndices { get; internal set; } // 32, 36, 40, 48
            public Vector4 BoneIndices2 { get; internal set; } // 40, 48
            public Vector4 BoneWeights { get; internal set; } // 32, 36, 40, 48
            public Vector4 BoneWeights2 { get; internal set; } // 40, 48

            public Vertex(Vector3 pos)
            {
                Position = pos;
                Normal = Vector3.Zero;
                TextureCoordinate = Vector2.Zero;
                BoneIndices = BoneIndices2 = BoneWeights = BoneWeights2 = Vector4.Zero;
            }

            public override string ToString()
            {
                return $"{Position.X} {Position.Y} {Position.Z}";
            }
        }

        public struct Face
        {
            public int Y { get; internal set; }
            public int X { get; internal set; }
            public int Z { get; internal set; }

            public override string ToString()
            {
                return $"{Y} {X} {Z}";
            }
        }

        public struct Bone
        {
            public long ID { get; internal set; }
            public int Type { get; internal set; }
            public int Name { get; internal set; }
            public System.Numerics.Matrix4x4 TransformMatrix { get; internal set; }
        }

        public Vector3[] CalculateAABB()
        {
            float minX = Vertices.Select(x => x.Position.X).Min() * Scale.X;
            float maxX = Vertices.Select(x => x.Position.X).Max() * Scale.X;
            float minY = Vertices.Select(x => x.Position.Y).Min() * Scale.X;
            float maxY = Vertices.Select(x => x.Position.Y).Max() * Scale.X;
            float minZ = Vertices.Select(x => x.Position.Z).Min() * Scale.X;
            float maxZ = Vertices.Select(x => x.Position.Z).Max() * Scale.X;
            /* Illustration of order of Vector3s
             *   E __________________ F
             *    /|               /|
             *   / |              / |
             * A ----------------- B|
             *   | |             |  |
             *   | |G____________|__| H         ^
             *   | /             | /           / Z+
             * C |/______________|/ D   X- <--/
             */
            return new Vector3[]
            {
                new Vector3(minX, maxY, minZ), // A
                new Vector3(maxX, maxY, minZ), // B
                new Vector3(minX, minY, minZ), // C
                new Vector3(minX, minY, minZ), // D
                new Vector3(minX, maxY, maxZ), // E
                new Vector3(maxX, maxY, maxZ), // F
                new Vector3(minX, minY, maxZ), // G
                new Vector3(maxX, minY, maxZ)  // H
            };
        }

        public void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
        }

        public void CalculateNormals()
        {
            Vector3[] normals = new Vector3[Vertices.Count];
            Vector3[] verts = Vertices.Select(x => x.Position).ToArray();
            int[] inds = Indices.ToArray();
            
            for (int i = 0; i < Indices.Count; i += 3)
            {
                if (inds[i] < verts.Length && inds[i + 1] < verts.Length && inds[i + 2] < verts.Length)
                {
                    Vector3 v1 = verts[inds[i]];
                    Vector3 v2 = verts[inds[i + 1]];
                    Vector3 v3 = verts[inds[i + 2]];

                    // The normal is the cross-product of two sides of the triangle
                    normals[inds[i]] += Vector3.Cross(v2 - v1, v3 - v1);
                    normals[inds[i + 1]] += Vector3.Cross(v2 - v1, v3 - v1);
                    normals[inds[i + 2]] += Vector3.Cross(v2 - v1, v3 - v1);
                }
            }

            for (int i = 0; i < Vertices.Count; i++)
            {
                normals[i] = normals[i].Normalized();
            }

            CalculatedNormals = normals.ToList();
        }

        public Vector3 GetCenterOfAABB(Vector3[] aabb)
        {
            float[] x = new float[] { aabb[1].X, aabb[0].X };
            float[] y = new float[] { aabb[0].Y, aabb[2].Y };
            float[] z = new float[] { aabb[4].Z, aabb[0].Z };
            return new Vector3(x.Average(), y.Average(), z.Average());
        }

        public Vector3[] GetVertices() => Vertices.Select(x => x.Position).ToArray();
    }
}