using Blacksmith.Enums;
using Blacksmith.Three;

namespace Blacksmith
{
    public class Structs
    {
        #region from Odyssey
        // Forge
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

        // TextureMap
        public struct TextureMap
        {
            public int Width { get; internal set; }
            public int Height { get; internal set; }
            public DXT DXT { get; internal set; }
            public int Mipmaps { get; internal set; }
        }

        public struct CompiledTopMip
        {
            public long FileID { get; internal set; }
        }

        public struct CompiledTextureMap
        {
            public int Width { get; internal set; }
            public int Height { get; internal set; }
            public int Mipmaps { get; internal set; }
            public DXT DXT { get; internal set; }
        }
        #endregion

        #region from Origins
        public struct DatafileHeader
        {
            public uint ResourceIdentifier { get; internal set; }
            public int FileSize { get; internal set; }
            public int FileNameSize { get; internal set; }
            public char[] FileName { get; internal set; }
        }

        public struct MeshBlock
        {
            public int ModelType { get; internal set; }
            public int ACount { get; internal set; }
            public int BoneCount { get; internal set; }
            public Mesh.Bone[] Bones { get; internal set; }
        }

        public struct CompiledMeshBlock
        {
            public int VertexTableSize { get; internal set; }
            public byte TypeSwitch { get; internal set; }
            public int Unknown1 { get; internal set; }
            public int Unknown2 { get; internal set; }
        }

        public struct SubmeshBlock
        {
            public int MeshCount { get; internal set; }
            public SubmeshEntry[] Entries { get; internal set; }
        }

        public struct SubmeshEntry
        {
            public int FaceOffset { get; internal set; }
            public int VertexOffset { get; internal set; }
        }

        public struct MultifileEntry
        {
            public DatafileHeader Header { get; internal set; }
            public byte[] AllData { get; internal set; }
        }
        #endregion
    }
}