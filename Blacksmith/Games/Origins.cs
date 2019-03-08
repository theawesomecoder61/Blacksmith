using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Numerics;
using Blacksmith.Enums;
using Blacksmith.Three;

namespace Blacksmith.Games
{
    public class Origins
    {
        private static byte[] MESH_DATA = new byte[] {
            0x68, 0x95, 0x5D, 0x41
        };

        private static byte[] COMPILED_MESH_DATA = new byte[] {
            0x95, 0x15, 0x9E, 0xFC
        };

        #region Structs
        public struct DatafileHeader
        {
            public int ResourceType { get; internal set; }
            public int FileSize { get; internal set; }
            public int FileNameSize { get; internal set; }
            public char[] FileName { get; internal set; }
        }

        public struct MeshBlock
        {
            public int ModelType { get; internal set; }
            public int ACount { get; internal set; }
            public int BoneCount { get; internal set; }
            public Bone[] Bones { get; internal set; }
        }

        public struct CompiledMeshBlock
        {
            public int VertexTableSize { get; internal set; }
            public int Unknown1 { get; internal set; }
            public int Unknown2 { get; internal set; }
            public int MeshCount { get; internal set; }
            public int ShadowCount { get; internal set; }
        }

        public struct BuildTableEntry {
            public DatafileHeader Header { get; internal set; }
            public byte[] Data { get; internal set; }
        };
        #endregion

        #region Models
        public static List<Mesh> ExtractModel(string fileName, Action<string> completionAction)
        {
            List<Mesh> meshes = new List<Mesh>();
            StringBuilder str = new StringBuilder(); // used to print information in the Text Viewer
            byte[] allData = File.ReadAllBytes(fileName);

            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length == 0)
                    return null;

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    // header
                    DatafileHeader header = new DatafileHeader
                    {
                        ResourceType = reader.ReadInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    header.FileName = reader.ReadChars(header.FileNameSize);

                    reader.BaseStream.Seek(29, SeekOrigin.Current);

                    // Bones chunk
                    /*uint boneCount = reader.ReadUInt32();
                    str.AppendLine($"Bone Count: {boneCount}");

                    // check if EOS
                    if (Helpers.CheckIfEOS(stream))
                    {
                        MessageBox.Show("Failed to read the model.", "Failure");
                        return new Tuple<List<Mesh>, string>(meshes, "");
                    }

                    // Bones chunk continued
                    Bone[] bones = new Bone[boneCount];
                    if (boneCount > 0)
                    {
                        for (int i = 0; i < boneCount; i++)
                        {
                            bones[i] = new Bone
                            {
                                ID = reader.ReadInt64(),
                                Type = reader.ReadInt32(),
                                Name = reader.ReadInt32(),
                                TransformMatrix = new Matrix4x4
                                {
                                    M11 = reader.ReadSingle(),
                                    M12 = reader.ReadSingle(),
                                    M13 = reader.ReadSingle(),
                                    M14 = reader.ReadSingle(),
                                    M21 = reader.ReadSingle(),
                                    M22 = reader.ReadSingle(),
                                    M23 = reader.ReadSingle(),
                                    M24 = reader.ReadSingle(),
                                    M31 = reader.ReadSingle(),
                                    M32 = reader.ReadSingle(),
                                    M33 = reader.ReadSingle(),
                                    M34 = reader.ReadSingle(),
                                    M41 = reader.ReadSingle(),
                                    M42 = reader.ReadSingle(),
                                    M43 = reader.ReadSingle(),
                                    M44 = reader.ReadSingle()
                                }
                            };

                            // invert matrix
                            Matrix4x4 matrix = Matrix4x4.Identity;
                            if (Matrix4x4.Invert(bones[i].TransformMatrix, out matrix))
                            {
                                bones[i].TransformMatrix = matrix;
                            }

                            reader.BaseStream.Seek(1, SeekOrigin.Current);
                        }
                    }*/

                    try
                    {
                        // locate the Compiled Mesh
                        Tuple<int[], long> cmOffset = Helpers.LocateBytes(reader, BitConverter.GetBytes((uint)ResourceType.COMPILED_MESH));
                        reader.BaseStream.Position = cmOffset.Item2;
                        reader.BaseStream.Seek(cmOffset.Item1[0], SeekOrigin.Begin);
                        Console.WriteLine("Compiled Mesh OFFSET: " + cmOffset.Item1[0]);

                        // Compiled Mesh
                        CompiledMeshBlock compiledMesh = new CompiledMeshBlock();
                        if (reader.ReadUInt32() != (uint)ResourceType.COMPILED_MESH)
                        {
                            MessageBox.Show("Failed to read model.", "Failure");
                            return meshes;
                        }
                        reader.BaseStream.Seek(22, SeekOrigin.Current);
                        compiledMesh.VertexTableSize = reader.ReadInt32();
                        str.AppendLine($"Vertex table size: {compiledMesh.VertexTableSize}");
                        compiledMesh.Unknown1 = reader.ReadInt32();
                        str.AppendLine($"Alternate vertex table size: {compiledMesh.Unknown1}");
                        compiledMesh.Unknown2 = reader.ReadInt32();
                        compiledMesh.MeshCount = reader.ReadInt32();
                        reader.BaseStream.Seek(32, SeekOrigin.Current);
                        compiledMesh.ShadowCount = reader.ReadInt32();
                        reader.BaseStream.Seek(1, SeekOrigin.Current);

                        // Clueless blocks
                        /* I have no clue what this does. All I can say is the int at index 4 in the last int[]
                         * gives the dataSize of the Unknown1 block. The identifier for each Clueless block is
                         * 0x07b19a87. Most likely this Clueless block will be removed in Version 1.5.
                         */
                        Console.WriteLine("Cluesless OFFSET: " + reader.BaseStream.Position);
                        Tuple<int[], long> cluelessIdentifier = Helpers.LocateBytes(reader, BitConverter.GetBytes(129079943));
                        int occurencesOfCluelessIdentifier = cluelessIdentifier.Item1.Length;
                        reader.BaseStream.Position = cluelessIdentifier.Item2;
                        int[][] cluelessData = new int[occurencesOfCluelessIdentifier][];
                        for (int i = 0; i < occurencesOfCluelessIdentifier; i++)
                        {
                            cluelessData[i] = Helpers.ReadInt32s(reader, 6);
                        }

                        // Unknown1 block - does not exist in every model
                        Console.WriteLine("Unknown1 OFFSET: " + reader.BaseStream.Position);
                        if (compiledMesh.Unknown2 != 0)
                        {
                            int unknown1DataSize = cluelessData[cluelessData.Length - 1][4];
                            reader.BaseStream.Seek(unknown1DataSize - 4, SeekOrigin.Current); // subtract 4 because the iHaveNoClue cuts into the Unknown1 data
                        }
                        else
                        {
                            reader.BaseStream.Seek(-8, SeekOrigin.Current);
                        }

                        // Vertex block
                        Console.WriteLine("Vertex OFFSET: " + reader.BaseStream.Position);
                        int vertexDataSize = reader.ReadInt32();
                        int actualVertexTableSize = compiledMesh.VertexTableSize != 8 && compiledMesh.VertexTableSize != 16 ? compiledMesh.VertexTableSize : compiledMesh.Unknown1;
                        long vertexOffset = reader.BaseStream.Position;
                        reader.BaseStream.Seek(vertexDataSize, SeekOrigin.Current);

                        // Unknown2 block - does not exist in every model
                        Console.WriteLine("Unknown2 OFFSET: " + reader.BaseStream.Position);
                        if (compiledMesh.Unknown2 != 0)
                        {
                            int unknown2DataSize = reader.ReadInt32();
                            reader.BaseStream.Seek(unknown2DataSize, SeekOrigin.Current);
                        }
                        else
                        {
                            reader.BaseStream.Seek(8, SeekOrigin.Current);
                        }

                        // Face block
                        Console.WriteLine("Face OFFSET: " + reader.BaseStream.Position);
                        int faceDataSize = reader.ReadInt32();
                        long faceOffset = reader.BaseStream.Position;
                        reader.BaseStream.Seek(faceDataSize, SeekOrigin.Current);

                        // locate the Mesh Data
                        Tuple<int[], long> mdOffset = Helpers.LocateBytes(reader, BitConverter.GetBytes((uint)ResourceType.MESH_DATA));
                        reader.BaseStream.Position = mdOffset.Item2;
                        reader.BaseStream.Seek(mdOffset.Item1[0] + 10, SeekOrigin.Begin); // skip the identifier and 6 bytes
                        Console.WriteLine("Mesh Data OFFSET: " + (mdOffset.Item1[0] + 10));

                        // Mesh Data block
                        uint meshCount = reader.ReadUInt32();
                        str.AppendLine($"Meshes: {meshCount}");
                        for (int i = 0; i < meshCount; i++)
                        {
                            Mesh mesh = new Mesh();

                            mesh.ID = reader.ReadInt16();
                            reader.BaseStream.Seek(18, SeekOrigin.Current);
                            mesh.Vertices = new Vertex[reader.ReadInt32()];
                            reader.BaseStream.Seek(4, SeekOrigin.Current);
                            mesh.Faces = new Face[reader.ReadInt32()];
                            mesh.TextureIndex = reader.ReadInt32();
                            mesh.OBJData = "";
                            meshes.Add(mesh);

                            str.AppendLine($"\tMesh {i}: {meshes[i].Faces.Length} faces\t{meshes[i].Vertices.Length} vertices");
                        }

                        // populate the meshes with vertices
                        reader.BaseStream.Seek(vertexOffset, SeekOrigin.Begin);
                        for (int i = 0; i < meshes.Count; i++)
                        {
                            Mesh mesh = meshes[i];
                            for (int j = 0; j < meshes[i].Vertices.Length; j++)
                            {
                                Vertex v = new Vertex
                                {
                                    Position = new Vector3
                                    {
                                        X = reader.ReadInt16(),
                                        Z = reader.ReadInt16() * -1,
                                        Y = reader.ReadInt16()
                                    }
                                };

                                int scaleFactor;
                                switch (actualVertexTableSize)
                                {
                                    case 16:
                                        scaleFactor = reader.ReadInt16();
                                        v.Position /= scaleFactor;
                                        reader.BaseStream.Seek(4, SeekOrigin.Current);
                                        v.TextureCoordinate = new Vector2(reader.ReadInt16(), reader.ReadInt16());
                                        break;
                                    case 20:
                                        scaleFactor = reader.ReadInt16();
                                        v.Position /= scaleFactor;
                                        v.Normal = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                                        reader.BaseStream.Seek(2, SeekOrigin.Current);
                                        v.TextureCoordinate = new Vector2(reader.ReadInt16(), reader.ReadInt16());
                                        break;
                                    case 24:
                                        reader.BaseStream.Seek(2, SeekOrigin.Current);
                                        v.Normal = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                                        reader.BaseStream.Seek(6, SeekOrigin.Current);
                                        v.TextureCoordinate = new Vector2(reader.ReadInt16(), reader.ReadInt16());
                                        break;
                                    case 28:
                                        scaleFactor = reader.ReadInt16();
                                        v.Position /= scaleFactor;
                                        v.Normal = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                                        reader.BaseStream.Seek(6, SeekOrigin.Current);
                                        v.TextureCoordinate = new Vector2(reader.ReadInt16(), reader.ReadInt16());
                                        reader.BaseStream.Seek(4, SeekOrigin.Current);
                                        break;
                                    case 32:
                                        reader.BaseStream.Seek(2, SeekOrigin.Current);
                                        v.Normal = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                                        reader.BaseStream.Seek(6, SeekOrigin.Current);
                                        v.TextureCoordinate = new Vector2(reader.ReadInt16(), reader.ReadInt16());
                                        v.BoneIndices = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v.BoneWeights = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        break;
                                    case 36:
                                        reader.BaseStream.Seek(2, SeekOrigin.Current);
                                        v.Normal = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                                        reader.BaseStream.Seek(6, SeekOrigin.Current);
                                        v.TextureCoordinate = new Vector2(reader.ReadInt16(), reader.ReadInt16());
                                        reader.BaseStream.Seek(4, SeekOrigin.Current);
                                        v.BoneIndices = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v.BoneWeights = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        break;
                                    case 40:
                                        reader.BaseStream.Seek(2, SeekOrigin.Current);
                                        v.Normal = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                                        reader.BaseStream.Seek(6, SeekOrigin.Current);
                                        v.TextureCoordinate = new Vector2(reader.ReadInt16(), reader.ReadInt16());
                                        v.BoneIndices = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v.BoneIndices2 = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v.BoneWeights = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v.BoneWeights2 = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        break;
                                    case 44:
                                        reader.BaseStream.Seek(14, SeekOrigin.Current);
                                        v.TextureCoordinate = new Vector2(reader.ReadInt16(), reader.ReadInt16());
                                        v.BoneIndices = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v.BoneIndices2 = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v.BoneWeights = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v.BoneWeights2 = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        reader.BaseStream.Seek(4, SeekOrigin.Current);
                                        break;
                                    case 48:
                                        reader.BaseStream.Seek(2, SeekOrigin.Current);
                                        v.Normal = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                                        reader.BaseStream.Seek(6, SeekOrigin.Current);
                                        v.TextureCoordinate = new Vector2(reader.ReadInt16(), reader.ReadInt16());
                                        v.BoneIndices = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v.BoneIndices2 = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v.BoneWeights = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v.BoneWeights2 = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        reader.BaseStream.Seek(4, SeekOrigin.Current);
                                        break;
                                    default:
                                        reader.BaseStream.Seek(actualVertexTableSize - 6, SeekOrigin.Current);
                                        break;
                                }

                                mesh.Vertices[j] = v;
                                mesh.OBJData += $"v {v.Position.X} {v.Position.Y} {v.Position.Z}\n";
                                mesh.OBJData += $"vn {v.Normal.X} {v.Normal.Y} {v.Normal.X}\n";
                                mesh.OBJData += $"vt {v.TextureCoordinate.X} {v.TextureCoordinate.Y}\n";
                            }
                            meshes[i] = mesh;
                        }

                        // populate the meshes with faces
                        reader.BaseStream.Seek(faceOffset, SeekOrigin.Begin);
                        for (int i = 0; i < meshes.Count; i++)
                        {
                            Mesh mesh = meshes[i];
                            for (int j = 0; j < meshes[i].Faces.Length; j++)
                            {
                                Face f = new Face
                                {
                                    X = reader.ReadUInt16() + 1,
                                    Y = reader.ReadUInt16() + 1,
                                    Z = reader.ReadUInt16() + 1
                                };

                                mesh.Faces[j] = f;
                                mesh.OBJData += $"f {f}\n";
                            }

                            str.AppendLine(meshes[i].OBJData);
                            str.AppendLine();

                            meshes[i] = mesh;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Failed to read model. " + e.Message, "Failure");
                    }
                    finally
                    {
                        completionAction(str.ToString());
                    }

                    #region Partially working code
                    /* BONES

                    // skip two Unknown chunks
                    reader.BaseStream.Seek(2 * 0x10, SeekOrigin.Current);
                    reader.BaseStream.Seek(1 + (2 * 0x11), SeekOrigin.Current);

                    // vertex table size
                    int vertexTableSize = reader.ReadInt32();
                    if (vertexTableSize == 8) // read again if the result is 8
                    {
                        vertexTableSize = reader.ReadInt32();
                        reader.BaseStream.Seek(33, SeekOrigin.Current);
                    }
                    else
                    {
                        reader.BaseStream.Seek(37, SeekOrigin.Current);
                    }

                    // Unknown 1 chunk
                    uint unknown1DataCount = reader.ReadUInt32();
                    for (int i = 0; i < unknown1DataCount; i++)
                    {
                        reader.BaseStream.Seek(0x18, SeekOrigin.Current); // skip it for now
                    }

                    // Internal UV chunk
                    uint uvDataSize = reader.ReadUInt32();
                    long uvOffset = reader.BaseStream.Position; // we revisit this later
                    reader.BaseStream.Seek(uvDataSize, SeekOrigin.Current);

                    // Vertices chunk
                    uint verticesDataSize = reader.ReadUInt32();
                    long verticesOffset = reader.BaseStream.Position; // we revisit this later
                    reader.BaseStream.Seek(verticesDataSize, SeekOrigin.Current);

                    // Unknown 2 chunk
                    uint unknown2DataCount = reader.ReadUInt32();
                    reader.BaseStream.Seek(unknown2DataCount, SeekOrigin.Current); // skip it for now

                    // Faces chunk
                    uint facesDataSize = reader.ReadUInt32();
                    long facesOffset = reader.BaseStream.Position; // we revisit this later
                    reader.BaseStream.Seek(facesDataSize, SeekOrigin.Current);

                    // Unknown 3 chunk
                    uint unknown3DataSize = reader.ReadUInt32();
                    reader.BaseStream.Seek(unknown3DataSize, SeekOrigin.Current); // skip it for now

                    // Unknown 4 chunk
                    uint unknown4DataSize = reader.ReadUInt32();
                    reader.BaseStream.Seek(unknown4DataSize, SeekOrigin.Current); // skip it for now

                    // Unknown 5 chunk
                    uint unknown5DataSize = reader.ReadUInt32();
                    reader.BaseStream.Seek(unknown5DataSize, SeekOrigin.Current); // skip it for now

                    reader.BaseStream.Seek(1 * 0x12, SeekOrigin.Current);

                    // Mesh Data chunk
                    uint meshCount = reader.ReadUInt32();
                    sb.AppendLine($"Meshes: {meshCount}");
                    meshes = new Mesh[meshCount];
                    for (int i = 0; i < meshCount; i++)
                    {
                        meshes[i].ID = reader.ReadInt16();
                        reader.BaseStream.Seek(18, SeekOrigin.Current);
                        meshes[i] = new Mesh();
                        meshes[i].Vertices = new Vertex[reader.ReadInt32()];
                        reader.BaseStream.Seek(4, SeekOrigin.Current);
                        meshes[i].Faces = new Face[reader.ReadInt32()];
                        meshes[i].TextureIndex = reader.ReadInt32();
                        meshes[i].OBJData = "";

                        sb.AppendLine($"\tMesh {i}: {meshes[i].Faces.Length} faces\t{meshes[i].Vertices.Length} vertices");
                    }

                    // revisit the Vertices
                    reader.BaseStream.Seek(verticesOffset, SeekOrigin.Begin);
                    for (int i = 0; i < meshes.Length; i++)
                    {
                        for (int j = 0; j < meshes[i].Vertices.Length; j++)
                        {
                            Vertex v = new Vertex
                            {
                                Position = new Vector3
                                {
                                    X = reader.ReadInt16(),
                                    Y = reader.ReadInt16(),
                                    Z = reader.ReadInt16()
                                }
                            };

                            meshes[i].Vertices[j] = v;
                            meshes[i].OBJData += $"v {v}\n";
                            obj.AppendLine($"v {v}");

                            reader.BaseStream.Seek(vertexTableSize - 6, SeekOrigin.Current); // skip the other data for now
                        }
                    }

                    // revisit the Faces
                    reader.BaseStream.Seek(facesOffset, SeekOrigin.Begin);
                    for (int i = 0; i < meshes.Length; i++)
                    {
                        for (int j = 0; j < meshes[i].Faces.Length; j++)
                        {
                            Face f = new Face
                            {
                                X = reader.ReadUInt16() + 1,
                                Y = reader.ReadUInt16() + 1,
                                Z = reader.ReadUInt16() + 1
                            };

                            meshes[i].Faces[j] = f;
                            meshes[i].OBJData += $"f {f}\n";
                            obj.AppendLine($"f {f}");
                        }

                        sb.AppendLine(meshes[i].OBJData);
                        sb.AppendLine();
                    }
                    */
                    #endregion

                    #region Not working code
                    /*// locate the Mesh block
                        reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        int meshBlockLoc = Helpers.RecurringIndexes(allData, MESH_DATA, 4).LastOrDefault().Item1;
                        reader.BaseStream.Seek(meshBlockLoc + 7, SeekOrigin.Begin); // skip the identifier + 3 bytes

                        // Mesh block
                        MeshBlock meshBlock = new MeshBlock
                        {
                            ModelType = reader.ReadInt32(),
                            ACount = reader.ReadInt32(),
                            BoneCount = reader.ReadInt32()
                        };

                        // locate the Compiled Mesh block
                        reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        IList<Tuple<int, int>> tt = Helpers.RecurringIndexes(allData, COMPILED_MESH_DATA, 4);
                        int compiledMeshBlockLoc = tt.LastOrDefault().Item1;
                        reader.BaseStream.Seek(compiledMeshBlockLoc + 26, SeekOrigin.Begin); // identifier + 22 bytes

                        // Compiled Mesh block
                        CompiledMeshBlock compiledMeshBlock = new CompiledMeshBlock
                        {
                            VertexTableSize = reader.ReadInt32()
                        };
                        reader.BaseStream.Seek(37, SeekOrigin.Current);
                        compiledMeshBlock.MeshCount = reader.ReadInt32();
                        compiledMeshBlock.ShadowCount = reader.ReadInt32();
                        reader.BaseStream.Seek(compiledMeshBlock.MeshCount * 16, SeekOrigin.Current);
                        reader.BaseStream.Seek(4, SeekOrigin.Current); // unknown1

                        int sizeOfVertexData = reader.ReadInt32();

                        // show error
                        if (compiledMeshBlock.MeshCount > 1)
                        {
                            MessageBox.Show("Currently, Blacksmith cannot handle models with more than 1 submesh.", "Failure");
                            return null;
                        }

                        // populate data in the Submeshes
                        submeshes = new Submesh[compiledMeshBlock.MeshCount];
                        for (int i = 0; i < submeshes.Length; i++)
                        {
                            // Vertices
                            List<Vertex> vertices = new List<Vertex>();
                            for (int j = 0; j < sizeOfVertexData / compiledMeshBlock.VertexTableSize; j++)
                            {
                                Vertex v = new Vertex();
                                v.Position = new Vector3
                                {
                                    X = reader.ReadInt16(),
                                    Y = reader.ReadInt16(),
                                    Z = reader.ReadInt16()
                                };

                                //short scaleFactor = reader.ReadInt16();
                                //v.Position /= scaleFactor;

                                /*if (compiledMeshBlock.VertexTableSize == 16)
                                {
                                    v.Normal = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                                }*

                                vertices.Add(v);
                                sb.AppendLine("v " + v);

                                reader.BaseStream.Seek(compiledMeshBlock.VertexTableSize - 6, SeekOrigin.Current);
                            }

                            // onto the Faces
                            reader.BaseStream.Seek(8, SeekOrigin.Current);

                            // Faces
                            List<Face> faces = new List<Face>();
                            int sizeOfFaceData = reader.ReadInt32();
                            for (int j = 0; j < sizeOfFaceData / 6; j++) // a Face contains 6 bytes
                            {
                                Face f = new Face
                                {
                                    Y = reader.ReadUInt16() + 1,
                                    X = reader.ReadUInt16() + 1,
                                    Z = reader.ReadUInt16() + 1
                                };

                                faces.Add(f);
                                sb.AppendLine("f " + f);
                            }

                            // set properties of this Submesh
                            submeshes[i].Vertices = vertices.ToArray();
                            submeshes[i].Faces = faces.ToArray();
                            submeshes[i].OBJData = sb.ToString();
                        }*/
                    #endregion

                    #region Poop
                    /*// block A - 1
                    uint aCount = reader.ReadUInt32();
                    Block blockA = new Block();
                    blockA.Bytes = new byte[aCount][];
                    for (int i = 0; i < aCount; i++)
                    {
                        blockA.Bytes[i] = reader.ReadBytes(0x51);
                    }

                    // block B - 1-0
                    Block blockB = new Block();
                    blockB.Bytes = new byte[2][];
                    for (int i = 0; i < 2; i++)
                    {
                        blockB.Bytes[i] = reader.ReadBytes(0x10);
                    }

                    // block C - 1-1
                    reader.BaseStream.Seek(1, SeekOrigin.Current);
                    Block blockC = new Block();
                    blockC.Bytes = new byte[2][];
                    for (int i = 0; i < 2; i++)
                    {
                        blockC.Bytes[i] = reader.ReadBytes(0x11);
                    }

                    // block D - 1-2
                    reader.BaseStream.Seek(0x29, SeekOrigin.Current);
                    uint dCount = reader.ReadUInt32();
                    Block blockD = new Block();
                    blockD.Bytes = new byte[dCount][];
                    for (int i = 0; i < dCount; i++)
                    {
                        blockD.Bytes[i] = reader.ReadBytes(0x18);
                    }

                    // internal/external UVs (block E) - 20
                    uint eCount = reader.ReadUInt32();
                    reader.BaseStream.Seek(eCount, SeekOrigin.Current);

                    // vertices (block F) - 30
                    uint fCount = reader.ReadUInt32() / 12;
                    long verticesOffset = reader.BaseStream.Position;
                    Block blockF = new Block();
                    blockF.Bytes = new byte[fCount][];
                    for (int i = 0; i < fCount; i++)
                    {
                        blockF.Bytes[i] = reader.ReadBytes(0x0c);
                    }

                    // block G - 40
                    uint gSize = reader.ReadUInt32();
                    Block blockG = new Block();
                    blockG.Bytes = new byte[gSize][];
                    reader.BaseStream.Seek(gSize, SeekOrigin.Current);

                    // faces (block) H - 50
                    uint hCount = reader.ReadUInt32() / 6;
                    long facesOffset = reader.BaseStream.Position;
                    Block blockH = new Block();
                    blockH.Bytes = new byte[hCount][];
                    for (int i = 0; i < hCount; i++)
                    {
                        blockH.Bytes[i] = reader.ReadBytes(0x06);
                    }

                    // block I - 60
                    uint iSize = reader.ReadUInt32();
                    Block blockI = new Block();
                    blockI.Bytes = new byte[iSize][];
                    reader.BaseStream.Seek(iSize, SeekOrigin.Current);

                    // block J - 70
                    uint jSize = reader.ReadUInt32();
                    Block blockJ = new Block();
                    blockJ.Bytes = new byte[jSize][];
                    reader.BaseStream.Seek(jSize, SeekOrigin.Current);

                    // block K - 80
                    uint kSize = reader.ReadUInt32();
                    Block blockK = new Block();
                    blockK.Bytes = new byte[kSize][];
                    reader.BaseStream.Seek(kSize, SeekOrigin.Current);

                    // block L
                    Block blockL = new Block();
                    blockL.Bytes = new byte[1][];
                    for (int i = 0; i < 1; i++)
                    {
                        blockL.Bytes[i] = reader.ReadBytes(0x12);
                    }

                    // block M - 90
                    uint mSize = reader.ReadUInt32(); // number of submeshes

                    // create submeshes
                    submeshes = new Submesh[mSize];
                    for (int i = 0; i < mSize; i++)
                    {
                        submeshes[i] = new Submesh();

                        reader.BaseStream.Seek(0x14, SeekOrigin.Current);
                        submeshes[i].VertexCount = reader.ReadUInt32();
                        reader.BaseStream.Seek(0x04, SeekOrigin.Current);
                        submeshes[i].FaceCount = reader.ReadUInt32();
                        reader.BaseStream.Seek(0x04, SeekOrigin.Current);
                    }

                    /*reader.BaseStream.Seek(0x10, SeekOrigin.Current);

                    // block N - 100
                    uint nSize = reader.ReadUInt32();
                    Block blockN = new Block();
                    blockN.Bytes = new byte[nSize][];
                    for (int i = 0; i < nSize; i++)
                    {
                        blockN.Bytes[i] = reader.ReadBytes(0x1e);
                    }

                    // block O - 110
                    reader.BaseStream.Seek(0x16, SeekOrigin.Current);
                    uint oSize = reader.ReadUInt32();
                    Block blockO = new Block();
                    blockO.Bytes = new byte[nSize][];
                    for (int i = 0; i < oSize; i++)
                    {
                        blockO.Bytes[i] = reader.ReadBytes(0x0a);
                    }*

                    // get faces
                    reader.BaseStream.Seek(facesOffset, SeekOrigin.Begin);
                    for (int i = 0; i < submeshes.Length; i++)
                    {
                        submeshes[i].Faces = new Face[submeshes[i].FaceCount];
                        for (int j = 0; j < submeshes[i].FaceCount; j++)
                        {
                            submeshes[i].Faces[j] = new Face
                            {
                                X = reader.ReadUInt16(),
                                Y = reader.ReadUInt16(),
                                Z = reader.ReadUInt16()
                            };

                            /*ObjParser.Types.Face f = new ObjParser.Types.Face
                            {
                                X = submeshes[i].Vertices[j].X,
                                Y = submeshes[i].Vertices[j].Y,
                                Z = submeshes[i].Vertices[j].Z
                            };
                            faces.Add(f);*
                        }
                    }

                    // get vertices
                    reader.BaseStream.Seek(verticesOffset, SeekOrigin.Begin);
                    List<ObjParser.Types.Vertex> verts = new List<ObjParser.Types.Vertex>();
                    for (int i = 0; i < submeshes.Length; i++)
                    {
                        submeshes[i].Vertices = new Vertex[submeshes[i].VertexCount];
                        for (int j = 0; j < submeshes[i].VertexCount; j++)
                        {
                            submeshes[i].Vertices[j] = new Vertex
                            {
                                X = reader.ReadInt16(),
                                Y = reader.ReadInt16(),
                                Z = reader.ReadInt16(),
                                U = reader.ReadInt16(),
                                V = reader.ReadInt16(),
                                Dummy = reader.ReadInt16()
                            };

                            ObjParser.Types.Vertex v = new ObjParser.Types.Vertex
                            {
                                X = submeshes[i].Vertices[j].X,
                                Y = submeshes[i].Vertices[j].Y,
                                Z = submeshes[i].Vertices[j].Z
                            };
                            verts.Add(v);
                        }
                    }

                    Obj obj = new Obj();
                    obj.VertexList = verts;
                    obj.WriteObjFile(@"C:\Users\pinea\bag-new.obj", new string[] { "" });

                    completionAction();*/
                    #endregion
                }
            }

            return meshes;
        }
        #endregion

        #region Other
        public BuildTableEntry[] ReadBuildTable(string fileName, Action<string> completionAction)
        {
            List<BuildTableEntry> entries = new List<BuildTableEntry>();
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length == 0)
                    return null;

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    // header
                    DatafileHeader header = new DatafileHeader
                    {
                        ResourceType = reader.ReadInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    header.FileName = reader.ReadChars(header.FileNameSize);

                    // go until we reach the end of the file
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                        entries.Add(new BuildTableEntry());
                }
            }
            return entries.ToArray();
        }
        #endregion
    }
}