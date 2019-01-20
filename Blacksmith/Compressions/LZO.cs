using System.Runtime.InteropServices;

namespace Blacksmith.Compressions
{
    public static class LZO
    {
#if WIN32
        [DllImport("Binaries\\x86\\lzo.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern int lzo1c_1_compress(byte[] src, ushort src_len, byte[] dst, ref ushort dst_len, byte[] wrkmem);

        [DllImport("Binaries\\x86\\lzo.dll", CharSet = CharSet.None, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int lzo1c_decompress_safe(byte[] src, int src_len, byte[] dst, ref int dst_len, byte[] wrkmem);

        [DllImport("Binaries\\x86\\lzo.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern int lzo1x_1_compress(byte[] src, ushort src_len, byte[] dst, ref ushort dst_len, byte[] wrkmem);

        [DllImport("Binaries\\x86\\lzo.dll", CharSet = CharSet.None, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int lzo1x_decompress_safe(byte[] src, ushort src_len, byte[] dst, ref ushort dst_len, byte[] wrkmem);

        [DllImport("Binaries\\x86\\lzo.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern int lzo2a_1_compress(byte[] src, ushort src_len, byte[] dst, ref ushort dst_len, byte[] wrkmem);

        [DllImport("Binaries\\x86\\lzo.dll", CharSet = CharSet.None, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int lzo2a_decompress_safe(byte[] src, ushort src_len, byte[] dst, ref ushort dst_len, byte[] wrkmem);
#endif

        private static byte[] workMem = new byte[checked(65536)];

        public enum Algorithm
        {
            LZO1X = 0,
            LZO1X_ = 1, // 0 and 1 refer to the same algorithm
            LZO2A = 2,
            LZO1C = 5
        }

        public static byte[] Decompress(Algorithm algorithm, byte[] input, ushort decompressedSize)
        {
            byte[] output = new byte[decompressedSize];
            int outputSize = decompressedSize;
            /*if (algorithm < Algorithm.LZO2A) // LZO1X
                lzo1x_decompress_safe(input, (ushort)input.Length, output, ref outputSize, null);
            else if (algorithm == Algorithm.LZO2A) // LZO2A
                lzo2a_decompress_safe(input, (ushort)input.Length, output, ref outputSize, null);
            else // LZO1C
                lzo1c_decompress_safe(input, input.Length, output, ref outputSize, null);*/
            return output;
        }
    }
}