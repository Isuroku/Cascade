using System.Runtime.InteropServices;
using System.Text;

namespace CascadeParser
{
    public static class BinarySerializeUtils
    {
        #region bool
        public static int Serialize(bool inValue, byte[] ioBuffer, int inOffset)
        {
            int offset = inOffset;
            ioBuffer[offset++] = inValue ? (byte)1 : (byte)0;
            return offset;
        }

        public static int Deserialize(byte[] ioBuffer, int inOffset, out bool outValue)
        {
            int offset = inOffset;
            outValue = ioBuffer[offset++] > 0;
            return offset;
        }
        #endregion bool

        #region byte
        public static int Serialize(byte inValue, byte[] ioBuffer, int inOffset)
        {
            int offset = inOffset;
            ioBuffer[offset++] = inValue;
            return offset;
        }

        public static int Deserialize(byte[] ioBuffer, int inOffset, out byte outValue)
        {
            int offset = inOffset;
            outValue = ioBuffer[offset++];
            return offset;
        }
        #endregion byte

        #region ushort
        public static int Serialize(ushort inValue, byte[] ioBuffer, int inOffset)
        {
            int offset = inOffset;
            ioBuffer[offset++] = (byte)(inValue >> 8);
            ioBuffer[offset++] = (byte)inValue;
            return offset;
        }

        public static int Deserialize(byte[] ioBuffer, int inOffset, out ushort outValue)
        {
            int offset = inOffset;
            ulong value = ioBuffer[offset++];
            value = value << 8;
            value += ioBuffer[offset++];

            outValue = (ushort)value;
            return offset;
        }
        #endregion ushort

        #region short
        public static int Serialize(short inValue, byte[] ioBuffer, int inOffset)
        {
            return Serialize((ushort)inValue, ioBuffer, inOffset);
        }

        public static int Deserialize(byte[] ioBuffer, int inOffset, out short outValue)
        {
            int offset = Deserialize(ioBuffer, inOffset, out ushort value);
            outValue = (short)value;
            return offset;
        }
        #endregion short

        #region uint
        public static int Serialize(uint inValue, byte[] ioBuffer, int inOffset)
        {
            int offset = inOffset;
            ioBuffer[offset++] = (byte)(inValue >> 24);
            ioBuffer[offset++] = (byte)(inValue >> 16);
            ioBuffer[offset++] = (byte)(inValue >> 8);
            ioBuffer[offset++] = (byte)inValue;
            return offset;
        }

        public static int Deserialize(byte[] ioBuffer, int inOffset, out uint outValue)
        {
            int offset = inOffset;
            ulong value = ioBuffer[offset++];
            value = value << 8;
            value += ioBuffer[offset++];
            value = value << 8;
            value += ioBuffer[offset++];
            value = value << 8;
            value += ioBuffer[offset++];

            outValue = (uint)value;
            return offset;
        }
        #endregion uint

        #region int
        public static int Serialize(int inValue, byte[] ioBuffer, int inOffset)
        {
            return Serialize((uint)inValue, ioBuffer, inOffset);
        }

        public static int Deserialize(byte[] ioBuffer, int inOffset, out int outValue)
        {
            int offset = Deserialize(ioBuffer, inOffset, out uint value);
            outValue = (int)value;
            return offset;
        }
        #endregion int

        #region ulong
        public static int Serialize(ulong inValue, byte[] ioBuffer, int inOffset)
        {
            int offset = inOffset;
            ioBuffer[offset++] = (byte)(inValue >> 56);
            ioBuffer[offset++] = (byte)(inValue >> 48);
            ioBuffer[offset++] = (byte)(inValue >> 40);
            ioBuffer[offset++] = (byte)(inValue >> 32);
            ioBuffer[offset++] = (byte)(inValue >> 24);
            ioBuffer[offset++] = (byte)(inValue >> 16);
            ioBuffer[offset++] = (byte)(inValue >> 8);
            ioBuffer[offset++] = (byte)inValue;
            return offset;
        }

        public static int Deserialize(byte[] ioBuffer, int inOffset, out ulong outValue)
        {
            int offset = inOffset;
            ulong value = ioBuffer[offset++];
            value = value << 8;
            value += ioBuffer[offset++];
            value = value << 8;
            value += ioBuffer[offset++];
            value = value << 8;
            value += ioBuffer[offset++];
            value = value << 8;
            value += ioBuffer[offset++];
            value = value << 8;
            value += ioBuffer[offset++];
            value = value << 8;
            value += ioBuffer[offset++];
            value = value << 8;
            value += ioBuffer[offset++];

            outValue = value;
            return offset;
        }
        #endregion ulong

        #region long
        public static int Serialize(long inValue, byte[] ioBuffer, int inOffset)
        {
            return Serialize((ulong)inValue, ioBuffer, inOffset);
        }

        public static int Deserialize(byte[] ioBuffer, int inOffset, out long outValue)
        {
            int offset = Deserialize(ioBuffer, inOffset, out ulong value);
            outValue = (long)value;
            return offset;
        }
        #endregion long

        #region float
        [StructLayout(LayoutKind.Explicit)]
        struct FloatIntUnion
        {
            [FieldOffset(0)] public float fvalue;
            [FieldOffset(0)] public int ivalue;

            public static FloatIntUnion Union = new FloatIntUnion();
        }

        public static int Serialize(float inValue, byte[] ioBuffer, int inOffset)
        {
            FloatIntUnion.Union.fvalue = inValue;
            return Serialize(FloatIntUnion.Union.ivalue, ioBuffer, inOffset);
        }

        public static int Deserialize(byte[] ioBuffer, int inOffset, out float outValue)
        {
            int offset = Deserialize(ioBuffer, inOffset, out int value);
            FloatIntUnion.Union.ivalue = value;
            outValue = FloatIntUnion.Union.fvalue;
            return offset;
        }
        #endregion float

        #region double
        [StructLayout(LayoutKind.Explicit)]
        struct DoubleLongUnion
        {
            [FieldOffset(0)] public double fvalue;
            [FieldOffset(0)] public long ivalue;

            public static DoubleLongUnion Union = new DoubleLongUnion();
        }

        public static int Serialize(double inValue, byte[] ioBuffer, int inOffset)
        {
            DoubleLongUnion.Union.fvalue = inValue;
            return Serialize(DoubleLongUnion.Union.ivalue, ioBuffer, inOffset);
        }

        public static int Deserialize(byte[] ioBuffer, int inOffset, out double outValue)
        {
            int offset = Deserialize(ioBuffer, inOffset, out long value);
            DoubleLongUnion.Union.ivalue = value;
            outValue = DoubleLongUnion.Union.fvalue;
            return offset;
        }
        #endregion double

        #region string
        public static int Serialize(string inStr, byte[] ioBuffer, int inOffset)
        {
            int offset = inOffset;

            offset = Serialize(inStr.Length, ioBuffer, offset);

            int bts = Encoding.UTF8.GetBytes(inStr, 0, inStr.Length, ioBuffer, offset);
            offset += bts;

            return offset;
        }

        public static int Deserialize(byte[] ioBuffer, int inOffset, out string outValue)
        {
            int offset = inOffset;

            offset = Deserialize(ioBuffer, offset, out int len);

            outValue = Encoding.UTF8.GetString(ioBuffer, offset, len);

            offset += len;

            return offset;
        }

        public static int GetStringMemorySize(string val)
        {
            //хранение длины строки
            int res = sizeof(int);
            //байт - длина самой строки
            res += Encoding.UTF8.GetByteCount(val);
            return res;
        }
        #endregion string
    }
}
