using Blacksmith.Compressions;
using Blacksmith.Enums;
using Blacksmith.FileTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Blacksmith.Games
{
    public class Steep
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

        public struct Mip // Steep does not have TopMips, rather Mips
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
                        ResourceType = reader.ReadInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    header.FileName = reader.ReadChars(header.FileNameSize);

                    // ignore the 1 byte, file ID, resource type, and 1 extra byte
                    reader.BaseStream.Seek(14, SeekOrigin.Current);

                    // mip 0
                    Mip mip0 = new Mip
                    {
                        Width = reader.ReadInt32(),
                        Height = reader.ReadInt32()
                    };
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    mip0.DXTType = DXTExtensions.GetDXT(reader.ReadInt32());
                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    mip0.Mipmaps = reader.ReadInt32();

                    reader.BaseStream.Seek(39, SeekOrigin.Current); // go to next mip

                    // mip 1
                    Mip mip1 = new Mip
                    {
                        Width = reader.ReadInt32(),
                        Height = reader.ReadInt32()
                    };
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    mip1.DXTType = DXTExtensions.GetDXT(reader.ReadInt32());
                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    mip1.Mipmaps = reader.ReadInt32();

                    // locate the two mips, if they exist
                    if (node.GetForge().FileEntries.Where(x => x.NameTable.Name.Contains(Path.GetFileNameWithoutExtension(fileName) + "_Mip")).Count() > 0)
                    {
                        Forge.FileEntry[] mipEntries = node.GetForge().FileEntries.Where(x => x.NameTable.Name == Path.GetFileNameWithoutExtension(fileName) + "_Mip0").ToArray();
                        if (mipEntries.Length > 0)
                        {
                            Forge.FileEntry mipEntry = mipEntries[0];

                            // extract, read, and create a DDS image with the first mip
                            byte[] rawTopMipData = node.GetForge().GetRawData(mipEntry);
                            //Helpers.WriteToFile(mipEntry.NameTable.Name, rawData, true);

                            // read
                            //ReadFile(Helpers.GetTempPath(mipEntry.NameTable.Name));
                            byte[] topMipData = ReadFile(rawTopMipData);

                            // extract
                            //ExtractTopMip(Helpers.GetTempPath(mipEntry.NameTable.Name), mip0, completionAction);
                            Mip mip = MipToUse(mipEntry.NameTable.Name) == 0 ? mip0 : mip1;
                            ExtractTopMip(topMipData, mipEntry.NameTable.Name, mip, completionAction);
                        }
                    }
                    else // mips do not exist. fear not! there is still image data found here. let us use that.
                    {
                        reader.BaseStream.Seek(12, SeekOrigin.Current);

                        TextureMap map = new TextureMap();
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
                        Mip mip = mip0;
                        Helpers.WriteTempDDS(name, mipmapData, mip.Width, mip.Height, mip.Mipmaps, mip.DXTType, () =>
                        {
                            Helpers.ConvertDDS($"{Helpers.GetTempPath(name)}.dds", (bool error) => {
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

        private static void ExtractTopMip(string fileName, Mip mip, Action<string> completionAction)
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
                        ResourceType = reader.ReadInt32(),
                        FileSize = reader.ReadInt32(),
                        FileNameSize = reader.ReadInt32()
                    };
                    header.FileName = reader.ReadChars(header.FileNameSize);

                    reader.BaseStream.Seek(18, SeekOrigin.Current);
                    byte[] data = reader.ReadBytes(header.FileSize - 18);

                    // write DDS file
                    Helpers.WriteTempDDS(name, data, mip.Width, mip.Height, mip.Mipmaps, mip.DXTType, () =>
                    {
                        Helpers.ConvertDDS($"{Helpers.GetTempPath(name)}.dds", (bool error) =>
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

        private static void ExtractTopMip(byte[] topMipData, string name, Mip mip, Action<string> completionAction)
        {
            using (Stream stream = new MemoryStream(topMipData))
            {
                if (stream.Length == 0)
                    return;

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
                    Helpers.WriteTempDDS(name, data, mip.Width, mip.Height, mip.Mipmaps, mip.DXTType, () =>
                    {
                        Helpers.ConvertDDS($"{Helpers.GetTempPath(name)}.dds", (bool error) =>
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

        private static int MipToUse(string fileName)
        {
            int mip = 0;
            /*if (fileName.Contains("NormalMap"))
                mip = 0;*/
            return mip;
        }
    }
}