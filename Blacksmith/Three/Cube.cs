using Blacksmith.Three;
using OpenTK;
using System.Collections.Generic;

// adapted from: https://github.com/neokabuto/OpenTKTutorialContent/blob/master/OpenTKTutorial8-1/OpenTKTutorial8/Cube.cs

namespace Blacksmith
{
    public class Cube : Mesh
    {
        public Cube()
        {
            Vector3[] v = new Vector3[]
            {
                new Vector3(-1, -1, -1),
                new Vector3(1, -1, -1),
                new Vector3(1, 1, -1),
                new Vector3(-1, 1, -1),
                new Vector3(-1, -1, 1),
                new Vector3(1, -1, 1),
                new Vector3(1, 1, 1),
                new Vector3(-1, 1, 1),
            };
            for (int i = 0; i < v.Length; i++)
            {
                Vertices.Add(new Vertex(v[i]));
            }
            VertexCount = Vertices.Count;

            int[] ind = new int[]
            {
                0, 2, 1, // left
                0, 3, 2,
                1, 2, 6, // back
                6, 5, 1,
                4, 5, 6, // right
                6, 7, 4,
                2, 3, 6, // top
                6, 3, 7,
                0, 7, 3, // front
                0, 4, 7,
                0, 1, 5, // bottom
                0, 5, 4
            };
            for (int i = 0; i < ind.Length; i++)
            {
                Indices.Add(ind[i]);
            }
            IndexCount = Indices.Count;
        }
    }
}