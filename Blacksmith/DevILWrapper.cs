using System.Runtime.InteropServices;
using System.Drawing;

namespace Blacksmith
{
    /*public class DevILWrapper
    {
#if WIN32
        [DllImport("Binaries\\x86\\.dll", EntryPoint = "_OodleLZ_Compress@40")]
        private static extern int OodleLZ_Compress(int algo, byte[] inBuf, int inSize, byte[] outBuf, int max, int a, int b, int c, int d, int e);

        [DllImport("Binaries\\x86\\.dll", EntryPoint = "_OodleLZ_Decompress@56")]
        private static extern int OodleLZ_Decompress(byte[] inBuf, int inSize, byte[] outBuf, int outSize, int a, int b, int c, int d, int e, int f, int g, int h, int i, int j);
#else
        [DllImport("Binaries\\x64\\DevIL.dll")]
        private static extern int ilInit();

        [DllImport("Binaries\\x64\\DevIL.dll")]
        private static extern bool ilLoadImage(string fileName);

        [DllImport("Binaries\\x64\\DevIL.dll")]
        private static extern bool ilSaveImage(string fileName);

        [DllImport("Binaries\\x64\\DevIL.dll")]
        private static extern bool ilGenImages(int a, int b);

        [DllImport("Binaries\\x64\\DevIL.dll")]
        private static extern int ilGetInteger(DevILValue value);
#endif

        public struct ImageProperties
        {
            public int Width;
            public int Height;
            public int Depth;
            public int BitsPerPixel;
        }

        public enum DevILValue
        {
            IL_VERSION_NUM  = 0x0DE2,
            IL_IMAGE_WIDTH  = 0x0DE4,
            IL_IMAGE_HEIGHT = 0x0DE5,
            IL_IMAGE_DEPTH  = 0x0DE6,
            IL_IMAGE_SIZE_OF_DATA   = 0x0DE7,
            IL_IMAGE_BPP    = 0x0DE8,
            IL_IMAGE_BYTES_PER_PIXEL    = 0x0DE8,
            IL_IMAGE_BITS_PER_PIXEL = 0x0DE9,
            IL_IMAGE_FORMAT = 0x0DEA,
            IL_IMAGE_TYPE   = 0x0DEB,
            IL_PALETTE_TYPE         = 0x0DEC,
            IL_PALETTE_SIZE         = 0x0DED,
            IL_PALETTE_BPP          = 0x0DEE,
            IL_PALETTE_NUM_COLS     = 0x0DEF,
            IL_PALETTE_BASE_TYPE    = 0x0DF0,
            IL_NUM_FACES            = 0x0DE1,
            IL_NUM_IMAGES           = 0x0DF1,
            IL_NUM_MIPMAPS          = 0x0DF2,
            IL_NUM_LAYERS           = 0x0DF3,
            IL_ACTIVE_IMAGE         = 0x0DF4,
            IL_ACTIVE_MIPMAP        = 0x0DF5,
            IL_ACTIVE_LAYER         = 0x0DF6,
            IL_ACTIVE_FACE          = 0x0E00,
            IL_CUR_IMAGE            = 0x0DF7,
            IL_IMAGE_DURATION       = 0x0DF8,
            IL_IMAGE_PLANESIZE      = 0x0DF9,
            IL_IMAGE_BPC            = 0x0DFA,
            IL_IMAGE_OFFX           = 0x0DFB,
            IL_IMAGE_OFFY           = 0x0DFC,
            IL_IMAGE_CUBEFLAGS      = 0x0DFD,
            IL_IMAGE_ORIGIN = 0x0DFE,
            IL_IMAGE_CHANNELS = 0x0DFF
        }

        private int ImgId;

        public DevILWrapper()
        {
            ilInit();
            /*unsafe
            {
                fixed(1) {
                    ilGenImages(1, &ImgId);
                }

                // Bind this image name.
                ilBindImage(ImgId);
            }*
        }

        public Bitmap LoadImage(string fileName)
        {
            Bitmap bmp = null;
            if (ilLoadImage(fileName))
            {

            }
            return bmp;
        }

        public void SaveImage(string newFileName)
        {
            ilSaveImage(newFileName);
        }

        public void Delete()
        {

        }

        public ImageProperties GetProperties()
        {
            return new ImageProperties
            {
                Width = ilGetInteger(DevILValue.IL_IMAGE_WIDTH),
                Height = ilGetInteger(DevILValue.IL_IMAGE_HEIGHT),
                Depth = ilGetInteger(DevILValue.IL_IMAGE_DEPTH),
                BitsPerPixel = ilGetInteger(DevILValue.IL_IMAGE_BITS_PER_PIXEL)
            };
        }
    }*/
}