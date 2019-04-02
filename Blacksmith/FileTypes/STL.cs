using Blacksmith.Three;
using OpenTK;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Blacksmith.FileTypes
{
    public class STL
    {
        public static void ExportBinary(string fileName, Model model, bool exportingIndividually = false)
        {
            try
            {
                using (Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(new char[80]);
                        writer.Write(model.Meshes.Select(x => x.FaceCount).Sum());

                        for (int i = 0; i < model.Meshes.Count; i++)
                        {
                            for (int j = 0; j < model.Meshes[i].FaceCount; j++)
                            {
                                // normal of vertex 1
                                Vector3 n = Vector3.Zero;
                                if (model.Meshes[i].Faces[j].X < model.Meshes[i].VertexCount)
                                    n = model.Meshes[i].Vertices[model.Meshes[i].Faces[j].X].Normal;
                                writer.Write(n.X);
                                writer.Write(n.Y);
                                writer.Write(n.Z);

                                // vertex 1
                                if (model.Meshes[i].Faces[j].X < model.Meshes[i].VertexCount)
                                {
                                    Vector3 v1 = model.Meshes[i].Vertices[model.Meshes[i].Faces[j].X].Position;
                                    writer.Write(v1.X);
                                    writer.Write(v1.Y);
                                    writer.Write(v1.Z);
                                }

                                // vertex 2
                                if (model.Meshes[i].Faces[j].Y < model.Meshes[i].VertexCount)
                                {
                                    Vector3 v2 = model.Meshes[i].Vertices[model.Meshes[i].Faces[j].Y].Position;
                                    writer.Write(v2.X);
                                    writer.Write(v2.Y);
                                    writer.Write(v2.Z);
                                }

                                // vertex 3
                                if (model.Meshes[i].Faces[j].Z < model.Meshes[i].VertexCount)
                                {
                                    Vector3 v3 = model.Meshes[i].Vertices[model.Meshes[i].Faces[j].Z].Position;
                                    writer.Write(v3.X);
                                    writer.Write(v3.Y);
                                    writer.Write(v3.Z);
                                }

                                writer.Write((short)0);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Message.Fail(e.Message + e.StackTrace);
            }
        }
    }
}