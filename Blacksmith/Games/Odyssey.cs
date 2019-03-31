using Blacksmith.Compressions;
using Blacksmith.Enums;
using Blacksmith.FileTypes;
using Blacksmith.Three;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static Blacksmith.Games.Origins;

// not Super Mario Odyssey!

namespace Blacksmith.Games
{
    public class Odyssey
    {
        #region Structs
        public struct HeaderBlock
        {
            public long Identifier { get; internal set; }
            public short Version { get; internal set; }
            public Compression Compression { get; internal set; }
            // skip 4 bytes
            public int BlockCount { get; internal set; }
        }

        public struct BlockIndex
        {
            public int UncompressedSize { get; internal set; } // both are uint, but who cares?
            public int CompressedSize { get; internal set; }
        }

        public struct DataChunk
        {
            public int Checksum { get; internal set; }
            public byte[] Data { get; internal set; }
        }

        public struct DatafileHeader
        {
            public int ResourceType { get; internal set; }
            public int FileSize { get; internal set; }
            public int FileNameSize { get; internal set; }
            public char[] FileName { get; internal set; }
        }

        public struct TopMip
        {
            public int Width { get; internal set; }
            public int Height { get; internal set; }
            // skip 8 bytes
            public DXT DXTType { get; internal set; }
            // skip 4 bytes
            public int Mipmaps { get; internal set; }
        }

        public struct TextureMap
        {
            public int Width { get; internal set; }
            public int Height { get; internal set; }
            // skip 8 bytes
            public DXT DXTType { get; internal set; }
            // skip 8 bytes
            public int MipmapCount { get; internal set; }
            // skip 130 bytes
            public int DataSize { get; internal set; }
            public byte[] Data { get; internal set; }
        }
        #endregion

        #region Raw Files
        /// <summary>
        /// Reads a file extracted from the forge file and writes all its decompressed data chunks to a file
        /// </summary>
        /// <param name="fileName"></param>
        public static bool ReadFile(string fileName)
        {
            return ReadFile(fileName, true);
        }

        public static bool ReadFile(string inputFileName, bool writeToTemp, string outputFileName = null)
        {
            string name = Path.GetFileNameWithoutExtension(inputFileName);
            using (MemoryStream combinedStream = new MemoryStream())
            {
                using (Stream stream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (stream.Length == 0)
                        return false;

                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        long[] identifierOffsets = Helpers.LocateRawDataIdentifier(reader);
                        long x = identifierOffsets[1];

                        // skip to this Raw Data Block's offset
                        reader.BaseStream.Seek(x, SeekOrigin.Begin);

                        // Header Block
                        HeaderBlock header = new HeaderBlock
                        {
                            Identifier = reader.ReadInt64(),
                            Version = reader.ReadInt16(),
                            Compression = (Compression)Enum.ToObject(typeof(Compression), reader.ReadByte())
                        };

                        // skip 4 bytes
                        reader.BaseStream.Seek(4, SeekOrigin.Current);

                        // finish reading the Header Block
                        header.BlockCount = reader.ReadInt32();

                        // Block Indices
                        BlockIndex[] indices = new BlockIndex[header.BlockCount];
                        for (int i = 0; i < header.BlockCount; i++)
                        {
                            indices[i] = new BlockIndex
                            {
                                UncompressedSize = reader.ReadInt32(),
                                CompressedSize = reader.ReadInt32()
                            };
                        }

                        // Data Chunks
                        DataChunk[] chunks = new DataChunk[header.BlockCount];
                        for (int i = 0; i < header.BlockCount; i++)
                        {
                            chunks[i] = new DataChunk
                            {
                                Checksum = reader.ReadInt32(),
                                Data = reader.ReadBytes(indices[i].CompressedSize)
                            };

                            // if the compressedSize and uncompressedSize do not match, decompress data
                            // otherwise, the data was not ever compressed
                            byte[] decompressed = indices[i].CompressedSize == indices[i].UncompressedSize ?
                                chunks[i].Data :
                                Oodle.Decompress(chunks[i].Data, indices[i].UncompressedSize);

                            // add decompressed data to combinedData
                            combinedStream.Write(decompressed, 0, decompressed.Length);
                        }
                    }
                }

                // write all decompressed data chunks (stored in combinedData) to a combined file
                Helpers.WriteToFile(writeToTemp ?
                    $"{Path.GetFileNameWithoutExtension(inputFileName)}.acod" :
                    outputFileName, combinedStream.ToArray(), writeToTemp);

                return true;
            }
        }

        public static byte[] ReadFile(byte[] rawData)
        {
            List<byte> combinedData = new List<byte>();
            using (Stream stream = new MemoryStream(rawData))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    long[] identifierOffsets = Helpers.LocateRawDataIdentifier(reader);
                    if (identifierOffsets.Length < 2)
                        return null;

                    long x = identifierOffsets[1];

                    // skip to this Raw Data Block's offset
                    reader.BaseStream.Seek(x, SeekOrigin.Begin);

                    // Header Block
                    HeaderBlock header = new HeaderBlock
                    {
                        Identifier = reader.ReadInt64(),
                        Version = reader.ReadInt16(),
                        Compression = (Compression)Enum.ToObject(typeof(Compression), reader.ReadByte())
                    };

                    // skip 4 bytes
                    reader.BaseStream.Seek(4, SeekOrigin.Current);

                    // finish reading the Header Block
                    header.BlockCount = reader.ReadInt32();

                    // Block Indices
                    BlockIndex[] indices = new BlockIndex[header.BlockCount];
                    for (int i = 0; i < header.BlockCount; i++)
                    {
                        indices[i] = new BlockIndex
                        {
                            UncompressedSize = reader.ReadInt32(),
                            CompressedSize = reader.ReadInt32()
                        };
                    }

                    // Data Chunks
                    DataChunk[] chunks = new DataChunk[header.BlockCount];
                    for (int i = 0; i < header.BlockCount; i++)
                    {
                        chunks[i] = new DataChunk
                        {
                            Checksum = reader.ReadInt32(),
                            Data = reader.ReadBytes(indices[i].CompressedSize)
                        };

                        // if the compressedSize and uncompressedSize do not match, decompress data
                        // otherwise, the data was not ever compressed
                        byte[] decompressed = indices[i].CompressedSize == indices[i].UncompressedSize ?
                            chunks[i].Data :
                            Oodle.Decompress(chunks[i].Data, indices[i].UncompressedSize);

                        // add decompressed data to combinedData
                        combinedData.AddRange(decompressed);
                    }
                }
            }

            using (Stream stream = new MemoryStream(combinedData.ToArray()))
            {
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                return data;
            }
        }
        #endregion

        #region Datafile
        public static void ReadDatafile(string fileName)
        {
            using (Stream stream = new FileStream($"{fileName}.dec", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    DatafileHeader header = new DatafileHeader
                    {
                        ResourceType = reader.ReadInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    header.FileName = reader.ReadChars(header.FileNameSize);
                }
            }
        }
        #endregion

        #region Textures
        public static void ExtractTextureMap(string fileName, EntryTreeNode node, Action<string> completionAction)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    DatafileHeader header = new DatafileHeader
                    {
                        ResourceType = reader.ReadInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    header.FileName = reader.ReadChars(header.FileNameSize);

                    // ignore the 2 bytes, file ID, and resource type
                    reader.BaseStream.Seek(14, SeekOrigin.Current);

                    // toppmip 0
                    TopMip mip0 = new TopMip
                    {
                        Height = reader.ReadInt32(),
                        Width = reader.ReadInt32()
                    };
                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    mip0.DXTType = DXTExtensions.GetDXT(reader.ReadInt32());
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    mip0.Mipmaps = reader.ReadInt32();

                    reader.BaseStream.Seek(81, SeekOrigin.Current);

                    // topmip 1, ignored
                    TopMip mip1 = new TopMip
                    {
                        Height = reader.ReadInt32(),
                        Width = reader.ReadInt32()
                    };
                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    mip1.DXTType = DXTExtensions.GetDXT(reader.ReadInt32());
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    mip1.Mipmaps = reader.ReadInt32();

                    // locate the two topmips, if they exist
                    if (node.GetForge().FileEntries.Where(x => x.NameTable.Name.Contains(name + "_TopMip")).Count() > 0)
                    {
                        Forge.FileEntry topMipEntry = node.GetForge().FileEntries.Where(x => x.NameTable.Name == name + "_TopMip_0").First();

                        // extract, read, and create a DDS image with the first topmip
                        byte[] rawTopMipData = node.GetForge().GetRawData(topMipEntry);
                        //Helpers.WriteToFile(topMipEntry.NameTable.Name, rawTopMipData, true);

                        // read
                        //ReadFile(Helpers.GetTempPath(topMipEntry.NameTable.Name));
                        byte[] topMipData = ReadFile(rawTopMipData);

                        // extract
                        //ExtractTopMip(Helpers.GetTempPath(topMipEntry.NameTable.Name), mip0, completionAction);
                        ExtractTopMip(topMipData, topMipEntry.NameTable.Name, mip0, completionAction);
                    }
                    else // topmips do not exist. fear not! there is still image data found here. let us use that.
                    {
                        reader.BaseStream.Seek(25, SeekOrigin.Current);
                        TextureMap map = new TextureMap
                        {
                            DataSize = reader.ReadInt32()
                        };
                        byte[] mipmapData = reader.ReadBytes(map.DataSize);

                        // write DDS file
                        Helpers.WriteTempDDS(name, mipmapData, mip0.Width, mip0.Height, mip0.Mipmaps, mip0.DXTType, () =>
                        {
                            Helpers.ConvertDDS($"{Helpers.GetTempPath(name)}.dds", "png", (error) =>
                            {
                                if (error)
                                    completionAction("FAILED");
                                else
                                    completionAction($"{Helpers.GetTempPath(name)}.png");
                            });
                        });
                    }
                }
            }
        }

        public static void ExtractTextureMapFromFile(string fileName, Action<string> completionAction)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    DatafileHeader header = new DatafileHeader
                    {
                        ResourceType = reader.ReadInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    header.FileName = reader.ReadChars(header.FileNameSize);

                    // ignore the 2 bytes, file ID, and resource type
                    reader.BaseStream.Seek(14, SeekOrigin.Current);

                    // toppmip 0
                    TopMip mip0 = new TopMip
                    {
                        Height = reader.ReadInt32(),
                        Width = reader.ReadInt32()
                    };
                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    mip0.DXTType = DXTExtensions.GetDXT(reader.ReadInt32());
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    mip0.Mipmaps = reader.ReadInt32();

                    reader.BaseStream.Seek(81, SeekOrigin.Current);

                    // topmip 1, ignored
                    TopMip mip1 = new TopMip
                    {
                        Height = reader.ReadInt32(),
                        Width = reader.ReadInt32()
                    };
                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    mip1.DXTType = DXTExtensions.GetDXT(reader.ReadInt32());
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    mip1.Mipmaps = reader.ReadInt32();

                    // locate the two topmips, if they exist
                    string origin = Path.GetDirectoryName(fileName);
                    string ext = Path.GetExtension(fileName);
                    string[] files = Directory.GetFiles(Path.GetDirectoryName(fileName));
                    files = files.Select(x => x = Path.GetFileNameWithoutExtension(x)).ToArray();

                    if (files.Where(x => x.Contains(name + "_TopMip")).Count() > 0)
                    {
                        string topMipEntry = files.Where(x => x == name + "_TopMip_0").First();
                        if (topMipEntry != null)
                        {
                            // extract, read, and create a DDS image with the first topmip
                            byte[] rawTopMipData = File.ReadAllBytes(Path.Combine(origin, topMipEntry + ext));
                            //Helpers.WriteToFile(topMipEntry.NameTable.Name, rawTopMipData, true);

                            // read
                            //ReadFile(Helpers.GetTempPath(topMipEntry.NameTable.Name));
                            byte[] topMipData = ReadFile(rawTopMipData);

                            // extract
                            //ExtractTopMip(Helpers.GetTempPath(topMipEntry.NameTable.Name), mip0, completionAction);
                            ExtractTopMip(topMipData, topMipEntry, mip0, completionAction);
                        }
                    }
                    else // topmips do not exist. fear not! there is still image data found here. let us use that.
                    {
                        reader.BaseStream.Seek(25, SeekOrigin.Current);
                        TextureMap map = new TextureMap
                        {
                            DataSize = reader.ReadInt32()
                        };
                        byte[] mipmapData = reader.ReadBytes(map.DataSize);

                        // write DDS file
                        Helpers.WriteTempDDS(name, mipmapData, mip0.Width, mip0.Height, mip0.Mipmaps, mip0.DXTType, () =>
                        {
                            Helpers.ConvertDDS($"{Helpers.GetTempPath(name)}.dds", "png", (error) =>
                            {
                                if (error)
                                    completionAction("FAILED");
                                else
                                    completionAction($"{Helpers.GetTempPath(name)}.png");
                            });
                        });
                    }
                }
            }
        }

        private static void ExtractTopMip(string fileName, TopMip topMip, Action<string> completionAction)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    DatafileHeader header = new DatafileHeader
                    {
                        ResourceType = reader.ReadInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    header.FileName = reader.ReadChars(header.FileNameSize);

                    reader.BaseStream.Seek(18, SeekOrigin.Current);
                    byte[] data = reader.ReadBytes(header.FileSize - 18);

                    // write DDS file
                    Helpers.WriteTempDDS(name, data, topMip.Width, topMip.Height, topMip.Mipmaps, topMip.DXTType, () =>
                    {
                        Helpers.ConvertDDS($"{Helpers.GetTempPath(name)}.dds", "png", (error) =>
                        {
                            if (!error)
                                completionAction("FAILED");
                            else
                                completionAction($"{Helpers.GetTempPath(name)}.png");
                        });
                    });
                }
            }
        }

        private static void ExtractTopMip(byte[] topMipData, string name, TopMip topMip, Action<string> completionAction)
        {
            using (Stream stream = new MemoryStream(topMipData))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    DatafileHeader header = new DatafileHeader
                    {
                        ResourceType = reader.ReadInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    header.FileName = reader.ReadChars(header.FileNameSize);

                    reader.BaseStream.Seek(18, SeekOrigin.Current);
                    byte[] data = reader.ReadBytes(header.FileSize - 18);

                    // write DDS file
                    Helpers.WriteTempDDS(name, data, topMip.Width, topMip.Height, topMip.Mipmaps, topMip.DXTType, () =>
                    {
                        Helpers.ConvertDDS($"{Helpers.GetTempPath(name)}.dds", "png", (error) =>
                        {
                            if (error)
                                completionAction("FAILED");
                            else
                                completionAction($"{Helpers.GetTempPath(name)}.png");
                        });
                    });
                }
            }
        }
        #endregion

        #region Materials
        #endregion

        #region Models
        public static Model ExtractModel(string fileName, Action<string> completionAction)
        {
            Model model = new Model();
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

                    // skip to the Mesh block, ignoring the Mesh block identifier
                    reader.BaseStream.Seek(10, SeekOrigin.Current);

                    try
                    {
                        // Mesh block
                        MeshBlock meshBlock = new MeshBlock();
                        reader.BaseStream.Seek(3, SeekOrigin.Current);
                        meshBlock.ModelType = reader.ReadInt32();
                        meshBlock.ACount = reader.ReadInt32();
                        if (meshBlock.ACount > 0)
                        {
                            reader.BaseStream.Seek(4, SeekOrigin.Current);
                            meshBlock.BoneCount = reader.ReadInt32();
                        }

                        // Bones block
                        if (meshBlock.BoneCount > 0)
                        {
                            str.AppendLine($"Bone Count: {meshBlock.BoneCount}");

                            // Bones block continued
                            Mesh.Bone[] bones = new Mesh.Bone[meshBlock.BoneCount];
                            for (int i = 0; i < meshBlock.BoneCount; i++)
                            {
                                bones[i] = new Mesh.Bone
                                {
                                    ID = reader.ReadInt64(),
                                    Type = reader.ReadInt32(),
                                    Name = reader.ReadInt32(),
                                    TransformMatrix = new System.Numerics.Matrix4x4
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
                                System.Numerics.Matrix4x4 matrix = System.Numerics.Matrix4x4.Identity;
                                if (System.Numerics.Matrix4x4.Invert(bones[i].TransformMatrix, out matrix))
                                {
                                    bones[i].TransformMatrix = matrix;
                                }

                                reader.BaseStream.Seek(1, SeekOrigin.Current);
                            }
                        }

                        // locate the Compiled Mesh
                        Tuple<int[], long> cmOffset = Helpers.LocateBytes(reader, BitConverter.GetBytes((uint)ResourceType.COMPILED_MESH));
                        reader.BaseStream.Seek(cmOffset.Item1[0], SeekOrigin.Begin);
                        //Console.WriteLine("Compiled Mesh OFFSET: " + cmOffset.Item1[0]);

                        // Compiled Mesh block
                        if (reader.ReadUInt32() != (uint)ResourceType.COMPILED_MESH)
                        {
                            MessageBox.Show("Failed to read model.", "Failure");
                            return new Model();
                        }
                        reader.BaseStream.Seek(22, SeekOrigin.Current);
                        CompiledMeshBlock compiledMesh = new CompiledMeshBlock
                        {
                            VertexTableSize = reader.ReadInt32(),
                            Unknown1 = reader.ReadInt32(),
                            Unknown2 = reader.ReadInt32()
                        };
                        reader.BaseStream.Seek(33, SeekOrigin.Current);

                        // Submesh block
                        //Console.WriteLine("Submesh OFFSET: " + reader.BaseStream.Position);
                        SubmeshBlock submeshBlock = new SubmeshBlock();
                        submeshBlock.MeshCount = reader.ReadInt32();
                        submeshBlock.Entries = new SubmeshEntry[submeshBlock.MeshCount];
                        for (int i = 0; i < submeshBlock.MeshCount; i++)
                        {
                            reader.BaseStream.Seek(16, SeekOrigin.Current);
                            submeshBlock.Entries[i] = new SubmeshEntry()
                            {
                                FaceOffset = reader.ReadInt32(),
                                VertexOffset = reader.ReadInt32()
                            };
                            Console.WriteLine("Faces offset: {0} | Vertex offset: {1}", submeshBlock.Entries[i].FaceOffset, submeshBlock.Entries[i].VertexOffset);
                        }

                        // Unknown0 block - does not exist in every model
                        //Console.WriteLine("Unknown0 OFFSET: " + reader.BaseStream.Position);
                        if (compiledMesh.Unknown2 != 0)
                        {
                            int unknown0DataSize = reader.ReadInt32();
                            //reader.BaseStream.Seek(unknown0DataSize, SeekOrigin.Current);
                        }
                        else
                        {
                            //reader.BaseStream.Seek(-8, SeekOrigin.Current);
                        }

                        // Vertex block
                        //Console.WriteLine("Vertex OFFSET: " + reader.BaseStream.Position);
                        int vertexDataSize = reader.ReadInt32();
                        int actualVertexTableSize = compiledMesh.VertexTableSize != 8 && compiledMesh.VertexTableSize != 16 ? compiledMesh.VertexTableSize : compiledMesh.Unknown1; // this variable now holds the true vertex table size
                        long vertexOffset = reader.BaseStream.Position;
                        reader.BaseStream.Seek(vertexDataSize, SeekOrigin.Current);

                        // Vertex Weight block - does not exist in every model
                        //Console.WriteLine("Vertex Weight OFFSET: " + reader.BaseStream.Position);
                        if (compiledMesh.Unknown2 == 0)
                        {
                            reader.BaseStream.Seek(12, SeekOrigin.Current);
                        }
                        else
                        {
                            int vertexWeightDataSize = reader.ReadInt32();
                            reader.BaseStream.Seek(vertexWeightDataSize, SeekOrigin.Current);
                        }

                        // Face block
                        //Console.WriteLine("Face OFFSET: " + reader.BaseStream.Position);
                        int faceDataSize = reader.ReadInt32();
                        long faceOffset = reader.BaseStream.Position;
                        reader.BaseStream.Seek(faceDataSize, SeekOrigin.Current);

                        // Unknown1 block
                        int unknown1DataSize = reader.ReadInt32();
                        reader.BaseStream.Seek(unknown1DataSize, SeekOrigin.Current);

                        // Unknown2 block
                        int unknown2DataSize = reader.ReadInt32();
                        reader.BaseStream.Seek(unknown2DataSize, SeekOrigin.Current);

                        // Unknown3 block
                        int unknown3DataSize = reader.ReadInt32();
                        reader.BaseStream.Seek(unknown3DataSize, SeekOrigin.Current);

                        // go to the Mesh Data
                        reader.BaseStream.Seek(23, SeekOrigin.Current);
                        //Tuple<int[], long> mdOffset = Helpers.LocateBytes(reader, BitConverter.GetBytes((uint)ResourceType.MESH_DATA));
                        //reader.BaseStream.Position = mdOffset.Item2;
                        //reader.BaseStream.Seek(mdOffset.Item1[0] + 10, SeekOrigin.Begin); // skip the identifier and 6 bytes
                        //Console.WriteLine("Mesh Data OFFSET: " + (reader.BaseStream.Position));

                        // Mesh Data block
                        uint meshCount = reader.ReadUInt32();
                        Console.WriteLine(reader.BaseStream.Position);

                        int totalVertices = 0;
                        for (int i = 0; i < meshCount; i++)
                        {
                            Mesh mesh = new Mesh();
                            mesh.ID = reader.ReadInt16();
                            reader.BaseStream.Seek(18, SeekOrigin.Current);

                            mesh.VertexCount = reader.ReadInt32();
                            reader.BaseStream.Seek(4, SeekOrigin.Current);

                            mesh.FaceCount = reader.ReadInt32();
                            mesh.IndexCount = mesh.FaceCount * 3;
                            mesh.MinFaceIndex = short.MaxValue; // an unlikely value, this will be set later

                            mesh.TextureIndex = reader.ReadInt32();

                            // store the position for later use
                            long pos = reader.BaseStream.Position;

                            // populate the meshes with vertices
                            reader.BaseStream.Seek(vertexOffset + (submeshBlock.Entries[i].VertexOffset * actualVertexTableSize), SeekOrigin.Begin);
                            for (int j = 0; j < mesh.VertexCount; j++)
                            {
                                short x = reader.ReadInt16();
                                short y = reader.ReadInt16();
                                short z = reader.ReadInt16();

                                Mesh.Vertex v = new Mesh.Vertex
                                {
                                    Position = new Vector3 // the coordinates are read like this, so that the model stands upright
                                    {
                                        Z = x,
                                        X = y,
                                        Y = z
                                    }
                                };

                                int scaleFactor;
                                switch (actualVertexTableSize)
                                {
                                    case 12:
                                        reader.BaseStream.Seek(2, SeekOrigin.Current);
                                        v.TextureCoordinate = new Vector2(reader.ReadInt16(), reader.ReadInt16());
                                        break;
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
                                
                                v.TextureCoordinate = new Vector2(v.TextureCoordinate.X / 65536 * 32, 1 - (v.TextureCoordinate.Y / 65536 * 32));
                                mesh.Vertices.Add(v);
                                //mesh.Normals.Add(v.Normal);
                            }

                            // add to the total vertices
                            if (i > 0)
                            {
                                totalVertices += model.Meshes[i - 1].VertexCount;
                                mesh.NumOfVerticesBeforeMe = totalVertices;
                            }

                            // populate the meshes with faces
                            reader.BaseStream.Seek(faceOffset + (submeshBlock.Entries[i].FaceOffset * 2), SeekOrigin.Begin);
                            for (int j = 0; j < mesh.FaceCount; j++)
                            {
                                int x = reader.ReadUInt16() + mesh.NumOfVerticesBeforeMe;
                                int y = reader.ReadUInt16() + mesh.NumOfVerticesBeforeMe;
                                int z = reader.ReadUInt16() + mesh.NumOfVerticesBeforeMe;

                                if (x != y && y != z && x != z)
                                {
                                    if (x < mesh.MinFaceIndex)
                                        mesh.MinFaceIndex = x;
                                    if (y < mesh.MinFaceIndex)
                                        mesh.MinFaceIndex = y;
                                    if (z < mesh.MinFaceIndex)
                                        mesh.MinFaceIndex = z;
                                    if (x > mesh.MaxFaceIndex)
                                        mesh.MaxFaceIndex = x;
                                    if (y > mesh.MaxFaceIndex)
                                        mesh.MaxFaceIndex = y;
                                    if (z > mesh.MaxFaceIndex)
                                        mesh.MaxFaceIndex = z;

                                    Mesh.Face f = new Mesh.Face
                                    {
                                        X = x,
                                        Y = y,
                                        Z = z
                                    };
                                    mesh.Faces.Add(f);

                                    mesh.Indices.Add(x);
                                    mesh.Indices.Add(y);
                                    mesh.Indices.Add(z);
                                }
                            }
                            model.Meshes.Add(mesh);

                            // go back to the Mesh Data
                            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                        }

                        // print information
                        str.AppendLine($"Vertex table size: {actualVertexTableSize}");
                        str.AppendLine($"Meshes: {meshCount}");
                        for (int i = 0; i < model.Meshes.Count; i++)
                        {
                            model.Meshes[i].CalculateNormals();
                            str.AppendLine($"\tMesh {i}:");
                            str.AppendLine($"\t\tVertices: {model.Meshes[i].Vertices.Count}");
                            str.AppendLine($"\t\tFaces: {model.Meshes[i].FaceCount}");
                            str.AppendLine($"\t\tIndices: {model.Meshes[i].IndexCount}");
                            str.AppendLine($"\t\tMin Face Index: {model.Meshes[i].MinFaceIndex}");
                            str.AppendLine($"\t\tMax Face Index: {model.Meshes[i].MaxFaceIndex}");
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
                }
            }

            return model;
        }
        #endregion

        #region Localization
        public static void ExtractLocalizationPackage(string fileName)
        {
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    DatafileHeader file = new DatafileHeader
                    {
                        ResourceType = reader.ReadInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    file.FileName = reader.ReadChars(file.FileNameSize);

                    // ToDo: implement the localization stuff
                }
            }
        }
        #endregion
    }
}