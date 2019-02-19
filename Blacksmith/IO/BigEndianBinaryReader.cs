using System;
using System.IO;

namespace Blacksmith
{
    public class BigEndianBinaryReader : BinaryReader
    {
        public BigEndianBinaryReader(Stream input) : base(input) { }

        public override short ReadInt16()
        {
            byte[] byteBuffer = base.ReadBytes(2);
            return (short)((byteBuffer[0] << 8) | byteBuffer[1]);
        }

        public override int ReadInt32()
        {
            byte[] byteBuffer = base.ReadBytes(4);
            return (byteBuffer[0] << 24) | (byteBuffer[1] << 16) | (byteBuffer[2] << 8) | byteBuffer[3];
        }

        public override long ReadInt64()
        {
            byte[] byteBuffer = base.ReadBytes(8);
            return (byteBuffer[0] << 56) | (byteBuffer[1] << 48) | (byteBuffer[2] << 40) | (byteBuffer[3] << 32) | (byteBuffer[4] << 24) | (byteBuffer[5] << 16) | (byteBuffer[6] << 8) | byteBuffer[7];
        }

        public override ushort ReadUInt16()
        {
            byte[] byteBuffer = base.ReadBytes(2);
            return (ushort)((byteBuffer[0] << 8) | byteBuffer[1]);
        }

        public override uint ReadUInt32()
        {
            byte[] byteBuffer = base.ReadBytes(4);
            return (uint)((byteBuffer[0] << 24) | (byteBuffer[1] << 16) | (byteBuffer[2] << 8) | byteBuffer[3]);
        }

        public override ulong ReadUInt64()
        {
            byte[] byteBuffer = base.ReadBytes(8);
            return (ulong)((byteBuffer[0] << 56) | (byteBuffer[1] << 48) | (byteBuffer[2] << 40) | (byteBuffer[3] << 32) | (byteBuffer[4] << 24) | (byteBuffer[5] << 16) | (byteBuffer[6] << 8) | byteBuffer[7]);
        }

        public override float ReadSingle()
        {
            byte[] byteBuffer = BitConverter.GetBytes(ReadUInt32());
            return BitConverter.ToSingle(byteBuffer, 0);
        }

        public override double ReadDouble()
        {
            byte[] byteBuffer = BitConverter.GetBytes(ReadUInt64());
            return BitConverter.ToDouble(byteBuffer, 0);
        }

        public override decimal ReadDecimal()
        {
            int flags = ReadInt32();
            int hi = ReadInt32();
            int mid = ReadInt32();
            int lo = ReadInt32();
            return new decimal(new int[] { lo, mid, hi, flags });
        }

        public override char ReadChar()
        {
            char c = base.ReadChar();
            byte[] byteBuffer = BitConverter.GetBytes(c);
            Array.Reverse(byteBuffer);
            return BitConverter.ToChar(byteBuffer, 0);
        }
    }
}