using OpenTK;
using System.Collections.Generic;
using System.Linq;

namespace Blacksmith.Three
{
    public class Model
    {
        public List<Mesh> Meshes = new List<Mesh>();

        public Model(params Mesh[] meshes)
        {
            Meshes.AddRange(meshes);
        }

        public Vector3 GetCenter()
        {
            if (Meshes.Count <= 0)
                return Vector3.Zero;

            // finds the centers of each mesh and calculates the average
            List<Vector3> centers = new List<Vector3>();
            Meshes.ForEach(x => centers.Add(x.GetCenterOfAABB(x.CalculateAABB())));
            return new Vector3(centers.Select(x => x.X).Average(), centers.Select(x => x.Y).Average(), centers.Select(x => x.Z).Average());
        }

        public Mesh.Vertex[] GetVertices()
        {
            List<Mesh.Vertex> vertices = new List<Mesh.Vertex>();
            foreach (Mesh mesh in Meshes)
            {
                vertices.AddRange(mesh.Vertices);
            }
            return vertices.ToArray();
        }

        public static Model CreateFromMesh(Mesh mesh) => CreateFromMeshes(new List<Mesh>()
        {
            mesh
        });

        public static Model CreateFromMeshes(List<Mesh> meshes)
        {
            Model model = new Model();
            foreach (Mesh mesh in meshes)
            {
                model.Meshes.Add(mesh);
            }
            return model;
        }
    }
}