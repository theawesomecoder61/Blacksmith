using System.Collections.Generic;
using System.Linq;

// adapted from: https://github.com/Ploaj/SSBHLib/blob/fe395033f4/CrossMod/IO/IO_DAE.cs

namespace Blacksmith.Three
{
    public class DAE
    {
        public static void Export(string fileName, Model model, bool optimize, bool exportMaterials)
        {
            using (DAEWriter writer = new DAEWriter(fileName, optimize))
            {
                writer.WriteAsset();

                /*f (m.HasMaterials && ExportMaterials)
                {
                    List<string> TextureNames = new List<string>();
                    foreach (var mat in m.Materials)
                    {
                        if (mat.DiffuseTexture != null && !TextureNames.Contains(mat.DiffuseTexture.Name))
                            TextureNames.Add(mat.DiffuseTexture.Name);
                    }
                    writer.WriteLibraryImages(TextureNames.ToArray());

                    writer.StartMaterialSection();
                    foreach (var mat in m.Materials)
                    {
                        writer.WriteMaterial(mat.Name);
                    }
                    writer.EndMaterialSection();

                    writer.StartEffectSection();
                    foreach (var mat in m.Materials)
                    {
                        writer.WriteEffect(mat.Name, mat.DiffuseTexture == null ? "" : mat.DiffuseTexture.Name);
                    }
                    writer.EndEffectSection();
                }
                else
                {
                    writer.WriteLibraryImages();
                }

                /*if (m.HasSkeleton)
                {
                    foreach (var bone in m.Skeleton.Bones)
                    {
                        float[] Transform = new float[] { bone.Transform.M11, bone.Transform.M21, bone.Transform.M31, bone.Transform.M41,
                    bone.Transform.M12, bone.Transform.M22, bone.Transform.M32, bone.Transform.M42,
                    bone.Transform.M13, bone.Transform.M23, bone.Transform.M33, bone.Transform.M43,
                    bone.Transform.M14, bone.Transform.M24, bone.Transform.M34, bone.Transform.M44 };
                        float[] InvTransform = new float[] { bone.InvWorldTransform.M11, bone.InvWorldTransform.M21, bone.InvWorldTransform.M31, bone.InvWorldTransform.M41,
                    bone.InvWorldTransform.M12, bone.InvWorldTransform.M22, bone.InvWorldTransform.M32, bone.InvWorldTransform.M42,
                    bone.InvWorldTransform.M13, bone.InvWorldTransform.M23, bone.InvWorldTransform.M33, bone.InvWorldTransform.M43,
                    bone.InvWorldTransform.M14, bone.InvWorldTransform.M24, bone.InvWorldTransform.M34, bone.InvWorldTransform.M44 };
                        writer.AddJoint(bone.Name, bone.ParentID == -1 ? "" : m.Skeleton.Bones[bone.ParentID].Name, Transform, InvTransform);
                    }
                }*/

                writer.StartGeometrySection();
                foreach (Mesh mesh in model.Meshes)
                {
                    writer.StartGeometryMesh("Mesh " + mesh.ID);

                    /*if (mesh.MaterialIndex != -1)
                    {
                        writer.CurrentMaterial = m.Materials[mesh.MaterialIndex].Name;
                    }*/

                    // collect sources
                    List<float> Position = new List<float>();
                    List<float> Normal = new List<float>();
                    List<float> UV0 = new List<float>();
                    List<float> UV1 = new List<float>();
                    List<float> UV2 = new List<float>();
                    List<float> UV3 = new List<float>();
                    List<float> Color = new List<float>();
                    List<int[]> BoneIndices = new List<int[]>();
                    List<float[]> BoneWeights = new List<float[]>();

                    foreach (var vertex in mesh.Vertices)
                    {
                        Position.Add(vertex.Position.X);
                        Position.Add(vertex.Position.Y);
                        Position.Add(vertex.Position.Z);

                        Normal.Add(vertex.Normal.X);
                        Normal.Add(vertex.Normal.Y);
                        Normal.Add(vertex.Normal.Z);

                        UV0.Add(vertex.TextureCoordinate.X);
                        UV0.Add(vertex.TextureCoordinate.Y);
                        /*UV1.Add(vertex.UV1.X);
                        UV1.Add(vertex.UV1.Y);
                        UV2.Add(vertex.UV2.X);-
                        UV2.Add(vertex.UV2.Y);
                        UV3.Add(vertex.UV3.X);
                        UV3.Add(vertex.UV3.Y);*/

                        //Color.AddRange(new float[] { vertex.Color.X, vertex.Color.Y, vertex.Color.Z, vertex.Color.W });

                        List<int> bIndices = new List<int>();
                        List<float> bWeights = new List<float>();
                        if (vertex.BoneWeights.X > 0)
                        {
                            bIndices.Add((int)vertex.BoneIndices.X);
                            bWeights.Add(vertex.BoneWeights.X);
                        }
                        if (vertex.BoneWeights.Y > 0)
                        {
                            bIndices.Add((int)vertex.BoneIndices.Y);
                            bWeights.Add(vertex.BoneWeights.Y);
                        }
                        if (vertex.BoneWeights.Z > 0)
                        {
                            bIndices.Add((int)vertex.BoneIndices.Z);
                            bWeights.Add(vertex.BoneWeights.Z);
                        }
                        if (vertex.BoneWeights.W > 0)
                        {
                            bIndices.Add((int)vertex.BoneIndices.W);
                            bWeights.Add(vertex.BoneWeights.W);
                        }
                        BoneIndices.Add(bIndices.ToArray());
                        BoneWeights.Add(bWeights.ToArray());
                    }

                    // ?
                    /*uint[] faces = new uint[mesh.Faces.Count * 3];
                    for (int i = 0; i < mesh.Faces.Count; i++)
                    {
                        faces[i] = (uint)mesh.Faces[i].X;
                        faces[i + 1] = (uint)mesh.Faces[i].Y;
                        faces[i + 2] = (uint)mesh.Faces[i].Z;
                    }*/
                    // ?

                    string name = string.Format("mesh{0}", mesh.ID);

                    // write sources
                    if (mesh.Vertices.Count > 0)
                        writer.WriteGeometrySource(name, DAEWriter.VERTEX_SEMANTIC.POSITION, Position.ToArray(), mesh.Indices.Cast<uint>().ToArray());

#warning needs fixing
                    if (mesh.Normals.Count > 0)
                        writer.WriteGeometrySource(name, DAEWriter.VERTEX_SEMANTIC.NORMAL, Normal.ToArray(), new uint[0]);

                    /*if (mesh.HasColor)
                        writer.WriteGeometrySource(mesh.Name, DAEWriter.VERTEX_SEMANTIC.COLOR, Color.ToArray(), mesh.Indices.ToArray());*/

                    //if (mesh.HasUV0)
                        //writer.WriteGeometrySource(mesh.Name, DAEWriter.VERTEX_SEMANTIC.TEXCOORD, UV0.ToArray(), ii, 0); // ToDo: fix ii

                    /*if (mesh.HasUV1)
                        writer.WriteGeometrySource(mesh.Name, DAEWriter.VERTEX_SEMANTIC.TEXCOORD, UV1.ToArray(), mesh.Indices.ToArray(), 1);

                    if (mesh.HasUV2)
                        writer.WriteGeometrySource(mesh.Name, DAEWriter.VERTEX_SEMANTIC.TEXCOORD, UV2.ToArray(), mesh.Indices.ToArray(), 2);

                    if (mesh.HasUV3)
                        writer.WriteGeometrySource(mesh.Name, DAEWriter.VERTEX_SEMANTIC.TEXCOORD, UV3.ToArray(), mesh.Indices.ToArray(), 3);*/

                    if (mesh.Vertices[0].BoneWeights.Length > 0)
                        writer.AttachGeometryController(BoneIndices, BoneWeights);

                    writer.EndGeometryMesh();
                }
                writer.EndGeometrySection();
            }
            return;
        }
    }
}