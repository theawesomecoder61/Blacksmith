using System.Numerics;

// Blacksmith.Three.Model = to be used with GLViewer
// Blacksmith.Three.Mesh = to be used with the Blacksmith.Game classes for holding mesh properties

namespace Blacksmith.Three
{
    public struct Mesh
    {
        public int ID { get; internal set; }
        public string Name { get; internal set; }
        public Vertex[] Vertices { get; internal set; }
        public int TextureIndex { get; internal set; }
        public Face[] Faces { get; internal set; }
        public string OBJData { get; internal set; }
    }

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

        public override string ToString()
        {
            return $"{Position.X} {Position.Y} {Position.Z}";
        }
    }

    public struct Face
    {
        public int X { get; internal set; }
        public int Y { get; internal set; }
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
        public Matrix4x4 TransformMatrix { get; internal set; }
    }
}