using System;

namespace Blacksmith.Enums
{
    public enum DXT
    {
        DXT0 = 0,
        DXT1 = 1,
        DXT1_ = 2,
        DXT1__ = 3,
        DXT3 = 4,
        DXT5 = 5,
        DXT5_ = 6,
        DXT0_ = 7,
        DX10 = 8,
        DX10_ = 9,
        DXT5__ = 12,
        DX10__ = 16
    }

    public static class DXTExtensions
    {
        /// <summary>
        /// Returns an DXT enum for an int
        /// </summary>
        /// <param name="dxt"></param>
        /// <returns></returns>
        public static DXT GetDXT(int dxt) => (DXT)Enum.ToObject(typeof(DXT), dxt);

        /// <summary>
        /// Returns an DXT enum as a char array for an int
        /// </summary>
        /// <param name="dxtType"></param>
        /// <returns></returns>
        public static char[] GetDXTAsChars(int dxt) => Enum.ToObject(typeof(DXT), dxt).ToString().Replace("_", "").ToCharArray();
    }
}