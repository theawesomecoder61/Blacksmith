using OpenTK;
using System;

// adapted from: https://github.com/neokabuto/OpenTKTutorialContent/blob/master/OpenTKTutorial9-3/OpenTKTutorial9-3/Volume.cs

namespace Blacksmith.Three
{
    public abstract class Model
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public virtual int VertexCount { get; set; }
        public virtual int FaceCount { get; set; }
        public virtual int IndexCount { get; set; }
        public virtual int ColorDataCount { get; set; }
        public virtual int NormalCount { get { return Normals.Length; } }
        public virtual int TextureCoordsCount { get; set; }

        public Matrix4 ModelMatrix = Matrix4.Identity;
        public Matrix4 ViewProjectionMatrix = Matrix4.Identity;
        public Matrix4 ModelViewProjectionMatrix = Matrix4.Identity;

        Vector3[] Normals = new Vector3[0];

        public abstract Vector3[] GetVertices();
        public abstract int[] GetIndices(int offset = 0);
        public abstract Vector3[] GetColorData();
        public abstract void CalculateModelMatrix();

        public virtual Vector3[] GetNormals()
        {
            return Normals;
        }

        public void CalculateNormals()
        {
            Vector3[] normals = new Vector3[VertexCount];
            Vector3[] verts = GetVertices();
            int[] inds = GetIndices();

            // Compute normals for each face
            for (int i = 0; i < IndexCount; i += 3)
            {
                Vector3 v1 = verts[inds[i]];
                Vector3 v2 = verts[inds[i + 1]];
                Vector3 v3 = verts[inds[i + 2]];

                // The normal is the cross-product of two sides of the triangle
                normals[inds[i]] += Vector3.Cross(v2 - v1, v3 - v1);
                normals[inds[i + 1]] += Vector3.Cross(v2 - v1, v3 - v1);
                normals[inds[i + 2]] += Vector3.Cross(v2 - v1, v3 - v1);
            }

            for (int i = 0; i < NormalCount; i++)
            {
                normals[i] = normals[i].Normalized();
            }

            Normals = normals;
        }

        public bool IsTextured = false;
        public int TextureID;
        public abstract Vector2[] GetTextureCoords();

        /// <summary>
        /// Calculates the AABB (bounding box) of the model.
        /// </summary>
        /// <returns></returns>
        public abstract Vector3[] CalculateAABB();

        /// <summary>
        /// Returns the center of the AABB (bounding box).
        /// </summary>
        /// <param name="aabb"></param>
        /// <returns></returns>
        public abstract Vector3 GetCenterOfAABB(Vector3[] aabb);
    }
}