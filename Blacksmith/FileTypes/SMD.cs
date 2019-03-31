using Blacksmith.Three;
using OpenTK;
using System.Text;

namespace Blacksmith.FileTypes
{
    public class SMD
    {
        public static string Export(Model model, bool exportingIndividually = false)
        {
            StringBuilder smd = new StringBuilder();
            smd.AppendLine("version 1");

            // create a dummy node
            smd.AppendLine("nodes");
            smd.AppendLine("0 \"root\" -1");
            smd.AppendLine("end");

            // create a dummy skeleton
            smd.AppendLine("skeleton");
            smd.AppendLine("time 0");
            smd.AppendLine("0    0 0 0   0 0 0");
            smd.AppendLine("end");

            // write vertices
            smd.AppendLine("triangles");
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                for (int j = 0; j < model.Meshes[i].FaceCount; j++)
                {
                    int i1 = model.Meshes[i].Faces[j].X;
                    int i2 = model.Meshes[i].Faces[j].Y;
                    int i3 = model.Meshes[i].Faces[j].Z;

                    Vector3 v1 = model.GetVertices()[i1].Position;
                    Vector3 n1 = model.GetVertices()[i1].Normal;
                    Vector2 t1 = model.GetVertices()[i1].TextureCoordinate;
                    smd.AppendFormat("{0}\t{1} {2} {3}\t{4} {5} {6}\t{7} {8}\n", 0, v1.X, v1.Y, v1.Z, 0, 0, 0, t1.X, t1.Y);

                    Vector3 v2 = model.GetVertices()[i2].Position;
                    Vector3 n2 = model.GetVertices()[i2].Normal;
                    Vector2 t2 = model.GetVertices()[i2].TextureCoordinate;
                    smd.AppendFormat("{0}\t{1} {2} {3}\t{4} {5} {6}\t{7} {8}\n", 0, v2.X, v2.Y, v2.Z, n2.X, n2.Y, n2.Z, t2.X, t2.Y);

                    Vector3 v3 = model.GetVertices()[i3].Position;
                    Vector3 n3 = model.GetVertices()[i3].Normal;
                    Vector2 t3 = model.GetVertices()[i3].TextureCoordinate;
                    smd.AppendFormat("{0}\t{1} {2} {3}\t{4} {5} {6}\t{7} {8}\n", 0, v3.X, v3.Y, v3.Z, n3.X, n3.Y, n3.Z, t3.X, t3.Y);
                }
            }
            smd.AppendLine("end");
            return smd.ToString();
        }
    }
}