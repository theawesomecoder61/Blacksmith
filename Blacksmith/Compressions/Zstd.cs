using System.IO;
using System.IO.Compression;
using Zstandard.Net;

namespace Blacksmith.Compressions
{
    public class Zstd
    {
        public static byte[] Decompress(byte[] data, int decompressedSize)
        {
            using (var memoryStream = new MemoryStream(data))
            using (var compressionStream = new ZstandardStream(memoryStream, CompressionMode.Decompress))
            using (var temp = new MemoryStream())
            {
                compressionStream.CopyTo(temp);
                return temp.ToArray();
            }
        }
    }
}