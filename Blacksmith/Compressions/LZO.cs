using System.Runtime.InteropServices;

namespace Blacksmith.Compressions
{
    public static class LZO
    {
#if WIN32
        [DllImport(LzoDll32Bit, EntryPoint = "__lzo_init_v2", CallingConvention = CallingConvention.Cdecl)]
        private static extern int __lzo_init_v2_32(uint v, int s1, int s2, int s3, int s4, int s5, int s6, int s7, int s8, int s9);

        [DllImport("Binaries\\x86\\lzo.dll", CharSet = CharSet.None, CallingConvention = CallingConvention.Cdecl)]
        private static extern int lzo1c_decompress_safe(byte[] src, int src_len, byte[] dst, ref int dst_len, byte[] wrkmem);
#else
        [DllImport("Binaries\\x64\\lzo2_64.dll")]
        private static extern int __lzo_init_v2(uint v, int s1, int s2, int s3, int s4, int s5, int s6, int s7, int s8, int s9);

        [DllImport("Binaries\\x64\\lzo2_64.dll")]
        private static extern int lzo1c_decompress(byte[] src, int src_len, byte[] dst, ref int dst_len, byte[] wrkmem);
#endif

        private static byte[] workMem = new byte[16384L * 4];

        public enum Algorithm
        {
            LZO1X = 0,
            LZO1X_ = 1, // 0 and 1 refer to the same algorithm
            LZO2A = 2,
            LZO1C = 5
        }

        public static byte[] Decompress(byte[] input, ushort decompressedSize)
        {
#if WIN32
            if (__lzo_init_v2_32(1, -1, -1, -1, -1, -1, -1, -1, -1, -1) != 0)
#else
            if (__lzo_init_v2(1, -1, -1, -1, -1, -1, -1, -1, -1, -1) != 0)
#endif
            return new byte[]{};

            byte[] output = new byte[decompressedSize];
            int outputSize = decompressedSize;
            lzo1c_decompress(input, input.Length, output, ref outputSize, workMem);
            return output;
        }
    }
}