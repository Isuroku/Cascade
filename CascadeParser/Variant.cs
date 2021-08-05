using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace CascadeParser
{
    public enum EValueType : byte
    {
        Undefined = 0,
        Bool,
        Float,
        Double,
        Byte,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        String,
        DateTime,
        TimeSpan,
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct Variant
    {
        [FieldOffset(0)]
        System.Object _objref;

        [FieldOffset(8)]
        ulong _ulong_value1;
        [FieldOffset(8)]
        double _double_value1;
        [FieldOffset(8)]
        float _float_value1;

        [FieldOffset(16)]
        byte _flags;

        const int TypeCodeBitMask = 0xf;

        public EValueType VariantType
        {
            get
            {
                byte t = (byte)(TypeCodeBitMask & _flags);
                return (EValueType)t;
            }
        }

        public static Variant Undefined = new Variant();

        public bool IsUndefined { get { return _flags == 0; } }

        public bool Equals(Variant other)
        {
            return
                _flags == other._flags
                && _ulong_value1 == other._ulong_value1
                && _objref == other._objref;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Variant && Equals((Variant)obj);
        }

        public static bool operator ==(Variant left, Variant right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Variant left, Variant right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int r = _objref == null ? 0 : _objref.GetHashCode();
                r = r * 397 + _ulong_value1.GetHashCode();
                r = r * 397 + _flags;
                return r;
            }
        }

        bool CheckType(EValueType inNeedType)
        {
            byte it = (byte)inNeedType;
            return (TypeCodeBitMask & _flags ^ it) == 0;
        }


        public Variant(byte value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _ulong_value1 = (ulong)value;

            _flags = (byte)EValueType.Byte;
        }

        public byte ToByte(byte inDefault = 0)
        {
            if (IsUndefined)
                return inDefault;

            if (CheckType(EValueType.String))
            {
                string str = (string)_objref;
                if (byte.TryParse(str, out byte res))
                    return res;
                return inDefault;
            }

            if (CheckType(EValueType.Float) || CheckType(EValueType.Double))
                return (byte)_double_value1;

            return (byte)_ulong_value1;
        }

        public Variant(short value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _ulong_value1 = (ulong)value;

            _flags = (byte)EValueType.Short;
        }

        public short ToShort(short inDefault = 0)
        {
            if (IsUndefined)
                return inDefault;

            if (CheckType(EValueType.String))
            {
                string str = (string)_objref;
                if (short.TryParse(str, out short res))
                    return res;
                return inDefault;
            }

            if (CheckType(EValueType.Float) || CheckType(EValueType.Double))
                return (short)_double_value1;

            return (short)_ulong_value1;
        }

        public Variant(ushort value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _ulong_value1 = (ulong)value;

            _flags = (byte)EValueType.UShort;
        }

        public ushort ToUShort(ushort inDefault = 0)
        {
            if (IsUndefined)
                return inDefault;

            if (CheckType(EValueType.String))
            {
                string str = (string)_objref;
                if (ushort.TryParse(str, out ushort res))
                    return res;
                return inDefault;
            }

            if (CheckType(EValueType.Float) || CheckType(EValueType.Double))
                return (ushort)_double_value1;

            return (ushort)_ulong_value1;
        }

        public Variant(int value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _ulong_value1 = (ulong)value;

            _flags = (byte)EValueType.Int;
        }

        public int ToInt(int inDefault = 0)
        {
            if (IsUndefined)
                return inDefault;

            if (CheckType(EValueType.String))
            {
                string str = (string)_objref;
                if (int.TryParse(str, out int res))
                    return res;
                return inDefault;
            }

            if (CheckType(EValueType.Float) || CheckType(EValueType.Double))
                return (int)_double_value1;

            return (int)_ulong_value1;
        }

        public Variant(uint value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _ulong_value1 = (ulong)value;

            _flags = (byte)EValueType.UInt;
        }

        public uint ToUInt(uint inDefault = 0)
        {
            if (IsUndefined)
                return inDefault;

            if (CheckType(EValueType.String))
            {
                string str = (string)_objref;
                if (uint.TryParse(str, out uint res))
                    return res;
                return inDefault;
            }

            if (CheckType(EValueType.Float) || CheckType(EValueType.Double))
                return (uint)_double_value1;

            return (uint)_ulong_value1;
        }

        public Variant(long value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _ulong_value1 = (ulong)value;

            _flags = (byte)EValueType.Long;
        }
        public long ToLong(long inDefault = 0)
        {
            if (IsUndefined)
                return inDefault;

            if (CheckType(EValueType.String))
            {
                string str = (string)_objref;
                if (long.TryParse(str, out long res))
                    return res;
                return inDefault;
            }

            if (CheckType(EValueType.Float) || CheckType(EValueType.Double))
                return (long)_double_value1;

            return (long)_ulong_value1;
        }

        public Variant(ulong value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _ulong_value1 = value;

            _flags = (byte)EValueType.UInt;
        }

        public ulong ToULong(ulong inDefault = 0)
        {
            if (IsUndefined)
                return inDefault;

            if (CheckType(EValueType.String))
            {
                string str = (string)_objref;
                if (ulong.TryParse(str, out ulong res))
                    return res;
                return inDefault;
            }

            if (CheckType(EValueType.Float) || CheckType(EValueType.Double))
                return (ulong)_double_value1;

            return (ulong)_ulong_value1;
        }

        public Variant(bool value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _ulong_value1 = value ? 1u : 0;

            _flags = (byte)EValueType.Bool;
        }

        public bool ToBool()
        {
            if (IsUndefined)
                return false;

            if (CheckType(EValueType.String))
            {
                string str = (string)_objref;
                if (bool.TryParse(str, out bool res))
                    return res;
                return false;
            }

            if (CheckType(EValueType.Float) || CheckType(EValueType.Double))
                return _double_value1 > 0;

            return _ulong_value1 > 0;
        }


        public Variant(float value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _float_value1 = value;
            _flags = (byte)EValueType.Float;
        }

        public float ToFloat(float inDefault = 0)
        {
            if (IsUndefined)
                return inDefault;

            if (CheckType(EValueType.String))
            {
                string str = (string)_objref;
                if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float res))
                    return res;
                return 0;
            }

            if (CheckType(EValueType.Float))
                return (float)_float_value1;

            if (CheckType(EValueType.Double))
                return (float)_double_value1;

            return (float)_ulong_value1;
        }

        public Variant(double value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _double_value1 = value;
            _flags = (byte)EValueType.Double;
        }

        public double ToDouble(double inDefault = 0)
        {
            if (IsUndefined)
                return inDefault;

            if (CheckType(EValueType.String))
            {
                string str = (string)_objref;
                if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double res))
                    return res;
                return 0;
            }

            if (CheckType(EValueType.Float))
                return (double)_float_value1;

            if (CheckType(EValueType.Double))
                return (double)_double_value1;

            return (double)_ulong_value1;
        }

        #region DateTime
        public Variant(DateTime value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _ulong_value1 = (ulong)value.ToBinary();

            _flags = (byte)EValueType.DateTime;
        }

        public DateTime ToDateTime(DateTime inDefault)
        {
            if (IsUndefined)
                return inDefault;

            if (CheckType(EValueType.String))
            {
                string str = (string)_objref;
                if (DateTime.TryParse(str, out DateTime res))
                    return res;
                return inDefault;
            }

            return DateTime.FromBinary((long)_ulong_value1);
        }
        #endregion DateTime

        #region TimeSpan
        public Variant(TimeSpan value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _ulong_value1 = (ulong)value.Ticks;

            _flags = (byte)EValueType.TimeSpan;
        }

        public TimeSpan ToTimeSpan(TimeSpan inDefault)
        {
            if (IsUndefined)
                return inDefault;

            if (CheckType(EValueType.String))
            {
                string str = (string)_objref;
                if (TimeSpan.TryParse(str, out TimeSpan res))
                    return res;
                return inDefault;
            }

            if (CheckType(EValueType.TimeSpan) || CheckType(EValueType.Int))
                return TimeSpan.FromTicks((long)_ulong_value1);

            return inDefault;
        }
        #endregion TimeSpan

        public Variant(string value)
        {
            _objref = null;
            _ulong_value1 = 0;
            _double_value1 = 0;
            _float_value1 = 0;

            _objref = value;

            _flags = (byte)EValueType.String;
        }

        public override string ToString()
        {
            if (IsUndefined)
                return string.Empty;

            if (CheckType(EValueType.String))
                return (string)_objref;

            if (CheckType(EValueType.Bool))
                return (_ulong_value1 > 0).ToString();

            if (CheckType(EValueType.Float))
                return _float_value1.ToString(CultureInfo.InvariantCulture);

            if (CheckType(EValueType.Double))
                return _double_value1.ToString(CultureInfo.InvariantCulture);

            if (CheckType(EValueType.Short) || CheckType(EValueType.Int) || CheckType(EValueType.Long))
                return ((long)_ulong_value1).ToString();

            if (CheckType(EValueType.DateTime))
                return ToDateTime(DateTime.UtcNow).ToString("o");

            if (CheckType(EValueType.TimeSpan))
                return ToTimeSpan(TimeSpan.Zero).ToString();

            return _ulong_value1.ToString();
        }

        public int GetMemorySize()
        {
            int res = 1; //byte for type
            switch (VariantType)
            {
                case EValueType.Bool: return res + sizeof(bool);
                case EValueType.Byte: return res + sizeof(byte);
                case EValueType.Short: return res + sizeof(short);
                case EValueType.UShort: return res + sizeof(ushort);
                case EValueType.Int: return res + sizeof(int);
                case EValueType.UInt: return res + sizeof(uint);
                case EValueType.Long: return res + sizeof(long);
                case EValueType.ULong: return res + sizeof(ulong);
                case EValueType.Float: return res + sizeof(float);
                case EValueType.Double: return res + sizeof(double);
                case EValueType.DateTime: return res + sizeof(long);
                case EValueType.TimeSpan: return res + sizeof(long);
                case EValueType.String: return res + BinarySerializeUtils.GetStringMemorySize((string)_objref);
            }

            throw new ArgumentException($"Unknow type of value: {VariantType}");
        }

        public int BinarySerialize(byte[] ioBuffer, int inOffset)
        {
            int offset = inOffset;

            offset = BinarySerializeUtils.Serialize((byte)VariantType, ioBuffer, offset);

            switch (VariantType)
            {
                case EValueType.Bool: 
                case EValueType.Byte: 
                    offset = BinarySerializeUtils.Serialize((byte)_ulong_value1, ioBuffer, offset); break;

                case EValueType.Short: 
                case EValueType.UShort: 
                    offset = BinarySerializeUtils.Serialize((ushort)_ulong_value1, ioBuffer, offset); break;

                case EValueType.Int: 
                case EValueType.UInt: 
                case EValueType.Float: 
                    offset = BinarySerializeUtils.Serialize((uint)_ulong_value1, ioBuffer, offset); break;

                case EValueType.Long: 
                case EValueType.ULong: 
                case EValueType.Double:
                case EValueType.DateTime:
                case EValueType.TimeSpan:
                    offset = BinarySerializeUtils.Serialize(_ulong_value1, ioBuffer, offset); break;

                case EValueType.String: 
                    offset = BinarySerializeUtils.Serialize(ToString(), ioBuffer, offset); break;

                default: throw new ArgumentException($"Unknow type of value: {VariantType}");
            }

            return offset;
        }

        public static int BinaryDeserialize(byte[] ioBuffer, int inOffset, out Variant outValue)
        {
            int offset = inOffset;

            offset = BinarySerializeUtils.Deserialize(ioBuffer, offset, out byte vt);
            EValueType valueType = (EValueType)vt;

            ulong ulong_value = 0;
            string str = null;
            switch (valueType)
            {
                case EValueType.Bool: 
                case EValueType.Byte: 
                    offset = BinarySerializeUtils.Deserialize(ioBuffer, offset, out byte byte_value); ulong_value = byte_value; break;

                case EValueType.Short: 
                case EValueType.UShort:
                    offset = BinarySerializeUtils.Deserialize(ioBuffer, offset, out ushort ushort_value); ulong_value = ushort_value; break;

                case EValueType.Int:
                case EValueType.UInt:
                case EValueType.Float:
                    offset = BinarySerializeUtils.Deserialize(ioBuffer, offset, out uint uint_value); ulong_value = uint_value; break;

                case EValueType.Long:
                case EValueType.ULong:
                case EValueType.Double:
                case EValueType.DateTime:
                case EValueType.TimeSpan:
                    offset = BinarySerializeUtils.Deserialize(ioBuffer, offset, out ulong_value); break;

                case EValueType.String: 
                    offset = BinarySerializeUtils.Deserialize(ioBuffer, offset, out str); break;

                default: 
                    throw new ArgumentException($"Unknow type of value: {valueType}");
            }

            outValue = new Variant();
            outValue._flags = (byte)valueType;
            outValue._ulong_value1 = ulong_value;
            outValue._objref = str;

            return offset;
        }
    }

    public static class VariantParser
    {
        static char[] trim_arr = new char[] { ' ', '\t' };
        public static Variant Parse(string inString)
        {
            string in_str = inString.Trim(trim_arr);
            if (string.Equals(in_str, "TRUE", StringComparison.InvariantCultureIgnoreCase))
                return new Variant(true);

            if (string.Equals(in_str, "FALSE", StringComparison.InvariantCultureIgnoreCase))
                return new Variant(false);

            bool only_digit = true;
            int point_count = 0;
            bool minus_was = false;
            for (int i = 0; i < in_str.Length && only_digit; ++i)
            {
                char c = in_str[i];

                bool point = c == '.';
                point_count += point ? 1 : 0;

                bool minus = c == '-' && i == 0;
                if (minus)
                    minus_was = true;

                only_digit = (minus || point || char.IsDigit(c)) && point_count < 2;
            }

            if (only_digit)
            {
                if (point_count > 0)
                {
                    //Float;
                    double val;
                    if (double.TryParse(in_str, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                    {
                        if (float.MinValue <= val && val <= float.MaxValue)
                            return new Variant((float)val);
                        return new Variant(val);
                    }
                }
                else if (in_str.Length <= 20)
                {
                    //long:  -9223372036854775808   to 9223372036854775807
                    //ulong: 0                      to 18446744073709551615
                    if (!minus_was && in_str.Length == 20)
                    {
                        //UInt;
                        ulong val;
                        if (ulong.TryParse(in_str, out val))
                            return new Variant(val);
                    }
                    else
                    {
                        //Int;
                        long val;
                        if (long.TryParse(in_str, out val))
                        {
                            if (byte.MinValue <= val && val <= byte.MaxValue)
                                return new Variant((byte)val);
                            else if (short.MinValue <= val && val <= short.MaxValue)
                                return new Variant((short)val);
                            else if (ushort.MinValue <= val && val <= ushort.MaxValue)
                                return new Variant((ushort)val);
                            else if (int.MinValue <= val && val <= int.MaxValue)
                                return new Variant((int)val);
                            else if (uint.MinValue <= val && val <= uint.MaxValue)
                                return new Variant((uint)val);
                            return new Variant(val);
                        }
                    }
                }
            }

            if (in_str.StartsWith("DT:", StringComparison.InvariantCulture))
            {
                string dt_str = in_str.Substring(3).Trim(trim_arr); ;
                if (DateTime.TryParse(dt_str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out DateTime dateTime))
                {
                    return new Variant(dateTime); // ??? тут есть сомнения, как правильно соскочить на UTC
                }
            }

            if (in_str.StartsWith("TS:", StringComparison.InvariantCulture))
            {
                string ts_str = in_str.Substring(3).Trim(trim_arr); ;
                if (TimeSpan.TryParse(ts_str, out TimeSpan timeSpan))
                {
                    return new Variant(timeSpan);
                }
            }

            return new Variant(in_str);
        }
    }
}