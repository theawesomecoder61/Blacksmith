using Blacksmith.Compressions;
using Blacksmith.Enums;
using Blacksmith.FileTypes;
using Blacksmith.Three;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static Blacksmith.Structs;

namespace Blacksmith.Games
{
    public class Steep
    {
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
                        if (identifierOffsets.Length < 2)
                            return false;

                        Console.WriteLine(identifierOffsets[0]);
                        Console.WriteLine(identifierOffsets[1]);
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
                                Zstd.Decompress(chunks[i].Data, indices[i].UncompressedSize);

                            // add decompressed data to combinedData
                            combinedStream.Write(decompressed, 0, decompressed.Length);
                        }
                    }
                }

                // write all decompressed data chunks (stored in combinedData) to a combined file
                Helpers.WriteToFile(writeToTemp ?
                    $"{Path.GetFileNameWithoutExtension(inputFileName)}.stp" :
                    outputFileName, combinedStream.ToArray(), writeToTemp);

                return true;
            }
        }

        public static byte[] ReadFile(byte[] rawData)
        {
            List<byte> combinedData = new List<byte>();
            using (Stream stream = new MemoryStream(rawData))
            {
                if (stream.Length == 0)
                    return null;

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    long[] identifierOffsets = Helpers.LocateRawDataIdentifier(reader);
                    if (identifierOffsets.Length < 2)
                        return null;

                    // skip to the "important" Raw Data Block
                    reader.BaseStream.Seek(identifierOffsets[1], SeekOrigin.Begin);

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
                            Zstd.Decompress(chunks[i].Data, indices[i].UncompressedSize);

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

        #region Textures
        public static void ExtractTextureMap(string fileName, EntryTreeNode node, Action<string> completionAction)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length == 0)
                    return;

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    DatafileHeader header = new DatafileHeader
                    {
                        ResourceIdentifier = reader.ReadUInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    header.FileName = reader.ReadChars(header.FileNameSize);
                    reader.BaseStream.Seek(9, SeekOrigin.Current); // fileID + 1 skipped bytes

                    if (header.ResourceIdentifier != (uint)ResourceIdentifier.TEXTURE_MAP)
                        Message.Fail("This is not proper Texture Map data.");

                    /*// mip 0
                    TextureMap mip0 = new TextureMap
                    {
                        Width = reader.ReadInt32(),
                        Height = reader.ReadInt32()
                    };
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    mip0.DXT = DXTExtensions.GetDXT(reader.ReadInt32());
                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    mip0.Mipmaps = reader.ReadInt32();

                    reader.BaseStream.Seek(39, SeekOrigin.Current); // go to next mip

                    // ignore mip 1
                    reader.BaseStream.Seek(28, SeekOrigin.Current);

                    // locate the two mips, if they exist
                    if (node.GetForge().FileEntries.Where(x => x.NameTable.Name.Contains(Path.GetFileNameWithoutExtension(fileName) + "_Mip")).Count() > 0)
                    {
                        Forge.FileEntry[] mipEntries = node.GetForge().FileEntries.Where(x => x.NameTable.Name == Path.GetFileNameWithoutExtension(fileName) + "_Mip0").ToArray();
                        if (mipEntries != null && mipEntries.Length > 0)
                        {
                            Forge.FileEntry mipEntry = mipEntries[0];

                            // extract, read, and create a DDS image with the first mip
                            byte[] rawTopMipData = node.GetForge().GetRawData(mipEntry);

                            // read
                            byte[] topMipData = ReadFile(rawTopMipData);
                            File.WriteAllBytes(Helpers.GetTempPath(mipEntry.NameTable.Name + "." + Helpers.GameToExtension(node.Game)), topMipData);

                            // extract
                            //ExtractTopMip(topMipData, mipEntry.NameTable.Name, mip0, completionAction);
                        }
                    }
                    else // mips do not exist. fear not! there is still image data found here. let us use that.
                    {
                        reader.BaseStream.Seek(12, SeekOrigin.Current);

                        /*TextureMap map = new TextureMap();
                        map.DataSize = reader.ReadInt32();

                        // test if this dataSize is too big
                        bool correctSize = true;
                        if (map.DataSize > reader.BaseStream.Length || map.DataSize < reader.BaseStream.Length - 300) // if the dataSize lies within a reasonable range
                        {
                            correctSize = false;
                        }

                        // test again, 14 bytes later
                        if (!correctSize)
                        {
                            reader.BaseStream.Seek(14, SeekOrigin.Current);
                            map.DataSize = reader.ReadInt32();
                        }

                        byte[] mipmapData = reader.ReadBytes(map.DataSize);

                        // write DDS file
                        TopMip mip = mip0;
                        Helpers.WriteTempDDS(name, mipmapData, mip.Width, mip.Height, mip.Mipmaps, mip.DXTType, () =>
                        {
                            Helpers.ConvertDDS($"{Helpers.GetTempPath(name)}.dds", fixNormals: name.Contains("NormalMap"), completionAction: (error) => {
                                if (error)
                                    completionAction("FAILED");
                                else
                                    completionAction($"{Helpers.GetTempPath(name)}.png");
                            });
                        });*
                    }*/

                    // TextureMap
                    reader.BaseStream.Seek(5, SeekOrigin.Current); // identifier + 1 skipped byte
                    TextureMap textureMap = new TextureMap
                    {
                        Width = reader.ReadInt32(),
                        Height = reader.ReadInt32()
                    };
                    reader.BaseStream.Seek(4, SeekOrigin.Current); // 4 skipped bytes
                    textureMap.DXT = (DXT)reader.ReadInt32();
                    reader.BaseStream.Seek(8, SeekOrigin.Current); // 8 skipped bytes
                    textureMap.Mipmaps = reader.ReadInt32();
                    reader.BaseStream.Seek(12, SeekOrigin.Current); // 3 skipped ints

                    reader.BaseStream.Seek(2, SeekOrigin.Current);

                    // CompiledTopMip
                    CompiledTopMip[] compiledTopMips = new CompiledTopMip[reader.ReadInt32()];
                    reader.BaseStream.Seek(1, SeekOrigin.Current);

                    // read CompiledTopMips
                    for (int i = 0; i < compiledTopMips.Length; i++)
                    {
                        compiledTopMips[i].FileID = reader.ReadInt64();
                        reader.BaseStream.Seek(1, SeekOrigin.Current); // 1 skipped byte
                    }

                    reader.BaseStream.Seek(8, SeekOrigin.Current);

                    // CompiledTextureMap
                    reader.BaseStream.Seek(12, SeekOrigin.Current); // identifier + two skipped ints
                    CompiledTextureMap compiledTextureMap = new CompiledTextureMap
                    {
                        Width = reader.ReadInt32(),
                        Height = reader.ReadInt32()
                    };
                    reader.BaseStream.Seek(4, SeekOrigin.Current); // 1 skipped int
                    compiledTextureMap.Mipmaps = reader.ReadInt32();
                    reader.BaseStream.Seek(4, SeekOrigin.Current); // 1 skipped int
                    compiledTextureMap.DXT = (DXT)reader.ReadInt32();
                    reader.BaseStream.Seek(16, SeekOrigin.Current); // 4 skipped ints

                    // locate the parent forge tree node
                    EntryTreeNode forgeNode = (EntryTreeNode)node.Parent.Parent;
                    if (forgeNode.Type != EntryTreeNodeType.FORGE)
                    {
                        Message.Fail("Failed to locate the forge node.");
                        return;
                    }

                    // check to see if there is a topmip
                    if (compiledTopMips.Select(x => x.FileID).Count() > 0 && compiledTopMips.Select(x => x.FileID).First() != 0)
                    {
                        // search for file IDs from the CompiledTextureMaps
                        if (forgeNode.Nodes.Cast<EntryTreeNode>().Where(x => x.FileID == compiledTopMips[0].FileID).Count() == 0)
                        {
                            if (Message.Show("Failed to locate the Mip0. Would you like to try extracting the TextureMap's internal image data?", "Failed", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                ExtractInternalTexture(reader, textureMap, name, completionAction);
                                return;
                            }
                            else
                                return;
                        }
                        EntryTreeNode mip0Node = forgeNode.Nodes.Cast<EntryTreeNode>().Where(x => x.FileID == compiledTopMips[0].FileID).First();

                        if (node.GetForge().FileEntries.Where(x => x.IndexTable.FileDataID == compiledTopMips[0].FileID).Count() == 0)
                        {
                            Message.Fail("Failed to locate the Mip0 in the forge.");
                            return;
                        }
                        Forge.FileEntry mip0Entry = node.GetForge().FileEntries.Where(x => x.IndexTable.FileDataID == compiledTopMips[0].FileID).First();

                        // read
                        byte[] rawMip0Data = node.GetForge().GetRawData(mip0Entry);
                        byte[] mip0Data = ReadFile(rawMip0Data);

                        // save for future use
                        File.WriteAllBytes(Helpers.GetTempPath($"{mip0Entry.NameTable.Name}.{Helpers.GameToExtension(node.Game)}"), mip0Data);

                        // extract
                        Odyssey.ExtractTopMip(mip0Data, mip0Entry.NameTable.Name, compiledTextureMap, completionAction);
                    }
                    else
                    {
                        // use the image data within this file
                        ExtractInternalTexture(reader, textureMap, name, completionAction);
                    }
                }
            }
        }

        public static void ExtractTextureMapFromFile(string fileName, Action<string> completionAction)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            Game game = Helpers.ExtensionToGame(Path.GetExtension(fileName).Substring(1));
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    DatafileHeader header = new DatafileHeader
                    {
                        ResourceIdentifier = reader.ReadUInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    header.FileName = reader.ReadChars(header.FileNameSize);
                    reader.BaseStream.Seek(9, SeekOrigin.Current); // fileID + 1 skipped bytes

                    if (header.ResourceIdentifier != (uint)ResourceIdentifier.TEXTURE_MAP)
                        Message.Fail("This is not proper Texture Map data.");

                    /*// ignore the 2 bytes, file ID, and resource type
                    reader.BaseStream.Seek(14, SeekOrigin.Current);

                    // mip 0
                    TextureMap mip0 = new TextureMap
                    {
                        Width = reader.ReadInt32(),
                        Height = reader.ReadInt32()
                    };
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    mip0.DXT = DXTExtensions.GetDXT(reader.ReadInt32());
                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    mip0.Mipmaps = reader.ReadInt32();

                    reader.BaseStream.Seek(39, SeekOrigin.Current); // go to next mip

                    // ignore mip 1
                    reader.BaseStream.Seek(28, SeekOrigin.Current);

                    Console.WriteLine("!!!!????????");
                    Console.WriteLine(reader.BaseStream.Position);
                    Console.WriteLine("!!!!????????");

                    // locate the two mips, if they exist
                    string origin = Path.GetDirectoryName(fileName);
                    string ext = "." + Helpers.GameToExtension(game);
                    string[] files = Directory.GetFiles(Path.GetDirectoryName(fileName));
                    files = files.Select(x => x = Path.GetFileNameWithoutExtension(x)).ToArray();

                    if (files.Where(x => x.Contains(name + "_Mip")).Count() > 0)
                    {
                        string[] mipEntries = files.Where(x => x.Contains(name + "_Mip0")).ToArray();
                        if (mipEntries != null && mipEntries.Length > 0)
                        {
                            string mipEntry = mipEntries[0] + ext;

                            // read
                            byte[] mipData = File.ReadAllBytes(Path.Combine(origin, mipEntry));

                            // test if the data has been decompressed
                            if (BitConverter.ToUInt32(mipData, 0) != (uint)ResourceIdentifier.MIPMAP)
                            {
                                mipData = ReadFile(mipData);
                            }
                            
                            // extract
                            //ExtractTopMip(mipData, Path.GetFileNameWithoutExtension(mipEntry), mip0, completionAction);
                        }
                    }
                    else // mips do not exist. fear not! there is still image data found here. let us use that.
                    {
                        reader.BaseStream.Seek(12, SeekOrigin.Current);

                        // skip all null bytes until the image data size
                        while (reader.PeekChar() == 0x0)
                        {
                            reader.BaseStream.Seek(1, SeekOrigin.Current);
                        }

#warning remove these ?????
                        Console.WriteLine("????????");
                        Console.WriteLine(reader.BaseStream.Position);

                        /*TextureMap map = new TextureMap
                        {
                            DataSize = reader.ReadInt32()
                        };
                        Console.WriteLine(map.DataSize);
                        Console.WriteLine("????????");

                        byte[] mipmapData = reader.ReadBytes(map.DataSize);

                        mipmapData.ToList().GetRange(0, 4).ForEach(x => Console.Write(x.ToString("X2") + " "));

                        // write DDS file
                        TopMip mip = mip0;
                        Helpers.WriteTempDDS(name, mipmapData, mip.Width, mip.Height, mip.Mipmaps, mip.DXTType, () =>
                        {
                            Helpers.ConvertDDS($"{Helpers.GetTempPath(name)}.dds", fixNormals: name.Contains("NormalMap"), completionAction: (error) => {
                                if (error)
                                    completionAction("FAILED");
                                else
                                    completionAction($"{Helpers.GetTempPath(name)}.png");
                            });
                        });*
                    }*/

                    // TextureMap
                    reader.BaseStream.Seek(5, SeekOrigin.Current); // identifier + 1 skipped byte
                    TextureMap textureMap = new TextureMap
                    {
                        Width = reader.ReadInt32(),
                        Height = reader.ReadInt32()
                    };
                    reader.BaseStream.Seek(4, SeekOrigin.Current); // 4 skipped bytes
                    textureMap.DXT = (DXT)reader.ReadInt32();
                    reader.BaseStream.Seek(8, SeekOrigin.Current); // 8 skipped bytes
                    textureMap.Mipmaps = reader.ReadInt32();
                    reader.BaseStream.Seek(12, SeekOrigin.Current); // 3 skipped ints

                    reader.BaseStream.Seek(2, SeekOrigin.Current);

                    // CompiledTopMip
                    CompiledTopMip[] compiledTopMips = new CompiledTopMip[reader.ReadInt32()];
                    reader.BaseStream.Seek(1, SeekOrigin.Current);

                    // read CompiledTopMips
                    for (int i = 0; i < compiledTopMips.Length; i++)
                    {
                        compiledTopMips[i].FileID = reader.ReadInt64();
                        reader.BaseStream.Seek(1, SeekOrigin.Current); // 1 skipped byte
                    }

                    reader.BaseStream.Seek(8, SeekOrigin.Current);

                    // CompiledTextureMap
                    reader.BaseStream.Seek(12, SeekOrigin.Current); // identifier + two skipped ints
                    CompiledTextureMap compiledTextureMap = new CompiledTextureMap
                    {
                        Width = reader.ReadInt32(),
                        Height = reader.ReadInt32()
                    };
                    reader.BaseStream.Seek(4, SeekOrigin.Current); // 1 skipped int
                    compiledTextureMap.Mipmaps = reader.ReadInt32();
                    reader.BaseStream.Seek(4, SeekOrigin.Current); // 1 skipped int
                    compiledTextureMap.DXT = (DXT)reader.ReadInt32();
                    reader.BaseStream.Seek(16, SeekOrigin.Current); // 4 skipped ints

                    // locate the two topmips, if they exist
                    string origin = Path.GetDirectoryName(fileName);
                    string ext = "." + Helpers.GameToExtension(game);
                    string[] files = Directory.GetFiles(Path.GetDirectoryName(fileName));
                    files = files.Select(x => x = Path.GetFileNameWithoutExtension(x)).ToArray();

                    // check to see if there is a topmip
                    if (compiledTopMips.Select(x => x.FileID).Count() > 0 && compiledTopMips.Select(x => x.FileID).First() != 0)
                    {
                        string[] mipEntries = files.Where(x => x.Contains(name + "_Mip0")).ToArray();
                        if (mipEntries != null && mipEntries.Length > 0)
                        {
                            string mip0Entry = mipEntries[0] + ext;

                            // read
                            byte[] mip0Data = File.ReadAllBytes(Path.Combine(origin, mip0Entry));

                            // test if the data has been decompressed
                            if (BitConverter.ToUInt32(mip0Data, 0) != (uint)ResourceIdentifier.MIPMAP)
                                mip0Data = ReadFile(mip0Data);

                            // extract
                            Odyssey.ExtractTopMip(mip0Data, Path.GetFileNameWithoutExtension(mip0Entry), compiledTextureMap, completionAction);
                        }
                    }
                    else
                    {
                        // use the image data within this file
                        ExtractInternalTexture(reader, textureMap, name, completionAction);
                    }
                }
            }
        }

        private static void ExtractInternalTexture(BinaryReader reader, TextureMap textureMap, string name, Action<string> completionAction)
        {
            int imageDataSize = reader.ReadInt32();
            byte[] imageData = reader.ReadBytes(imageDataSize);

            // write DDS file
            Helpers.WriteTempDDS(name, imageData, textureMap.Width, textureMap.Height, textureMap.Mipmaps, textureMap.DXT, () =>
            {
                Helpers.ConvertDDS($"{Helpers.GetTempPath(name)}.dds", fixNormals: name.Contains("NormalMap"), completionAction: (error) =>
                {
                    if (error)
                        completionAction("FAILED");
                    else
                        completionAction($"{Helpers.GetTempPath(name)}.png");
                });
            });
        }
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
                        ResourceIdentifier = reader.ReadUInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    header.FileName = reader.ReadChars(header.FileNameSize);

                    if (header.ResourceIdentifier != (uint)ResourceIdentifier.MESH)
                    {
                        Message.Fail("This is not proper Mesh data.");
                        return model;
                    }

                    // skip to the Mesh block, ignoring the Mesh block identifier
                    reader.BaseStream.Seek(10, SeekOrigin.Current);

                    try
                    {
                    }
                    catch (Exception e)
                    {
                        Message.Fail("Failed to read model. " + e.Message + e.StackTrace);
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
    }
}