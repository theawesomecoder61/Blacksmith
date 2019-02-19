using ObjParser;
using System;
using System.Collections.Generic;
using System.IO;

namespace Blacksmith.Games
{
    public class Origins
    {
        public struct DatafileHeader
        {
            public int ResourceType { get; internal set; }
            public int FileSize { get; internal set; }
            public int FileNameSize { get; internal set; }
            public char[] FileName { get; internal set; }
        }

        public struct Block
        {
            public byte[][] Bytes { get; internal set; }
        }

        public struct Submesh
        {
            public uint ID { get; internal set; }
            public uint VertexCount { get; internal set; }
            public uint FaceCount { get; internal set; }
            public Vertex[] Vertices { get; internal set; }
            public Face[] Faces { get; internal set; }
        }

        public struct Vertex
        {
            public short X { get; internal set; }
            public short Y { get; internal set; }
            public short Z { get; internal set; }
            public short U { get; internal set; }
            public short V { get; internal set; }
            public short Dummy { get; internal set; }

            public override string ToString()
            {
                return $"{X},{Y},{Z}";
            }
        }

        public struct Face
        {
            public ushort X { get; internal set; }
            public ushort Y { get; internal set; }
            public ushort Z { get; internal set; }

            public override string ToString()
            {
                return $"{X},{Y},{Z}";
            }
        }

        public static Submesh[] ExtractModel(string fileName, EntryTreeNode node, Action completionAction)
        {
            Submesh[] submeshes;

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

                    // ignore the 2 bytes, file ID, and resource type
                    reader.BaseStream.Seek(0x1d, SeekOrigin.Current);

                    // block A - 1
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
                    }*/

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
                            faces.Add(f);*/
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

                    completionAction();
                }
            }

            return submeshes;
        }
    }
}