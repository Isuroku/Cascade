using System;
using System.Globalization;
using System.Runtime.InteropServices;

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
    //DateTime,
    //TimeSpan,
}

[StructLayout(LayoutKind.Explicit, Pack = 4)]
public struct Variant
{
    [FieldOffset(0)]
    System.Object _objref;

    [FieldOffset(8)]
    ulong _long_value1;
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
            && _long_value1 == other._long_value1
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
            r = r * 397 + _long_value1.GetHashCode();
            r = r * 397 + _flags;
            return r;
        }
    }

    bool CheckType(EValueType inNeedType)
    {
        int it = (int)inNeedType;
        return (TypeCodeBitMask & _flags ^ it) == 0;
    }


    public Variant(byte value)
    {
        _objref = null;
        _long_value1 = 0;
        _double_value1 = 0;
        _float_value1 = 0;

        _long_value1 = (ulong)value;

        _flags = (int)EValueType.Byte;
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

        return (byte)_long_value1;
    }

    public Variant(short value)
    {
        _objref = null;
        _long_value1 = 0;
        _double_value1 = 0;
        _float_value1 = 0;

        _long_value1 = (ulong)value;

        _flags = (int)EValueType.Short;
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

        return (short)_long_value1;
    }

    public Variant(ushort value)
    {
        _objref = null;
        _long_value1 = 0;
        _double_value1 = 0;
        _float_value1 = 0;

        _long_value1 = (ulong)value;

        _flags = (int)EValueType.UShort;
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

        return (ushort)_long_value1;
    }

    public Variant(int value)
    {
        _objref = null;
        _long_value1 = 0;
        _double_value1 = 0;
        _float_value1 = 0;

        _long_value1 = (ulong)value;

        _flags = (int)EValueType.Int;
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

        return (int)_long_value1;
    }

    public Variant(uint value)
    {
        _objref = null;
        _long_value1 = 0;
        _double_value1 = 0;
        _float_value1 = 0;

        _long_value1 = (ulong)value;

        _flags = (int)EValueType.UInt;
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

        return (uint)_long_value1;
    }

    public Variant(long value)
    {
        _objref = null;
        _long_value1 = 0;
        _double_value1 = 0;
        _float_value1 = 0;

        _long_value1 = (ulong)value;

        _flags = (int)EValueType.Long;
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

        return (long)_long_value1;
    }

    public Variant(ulong value)
    {
        _objref = null;
        _long_value1 = 0;
        _double_value1 = 0;
        _float_value1 = 0;

        _long_value1 = value;

        _flags = (int)EValueType.UInt;
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

        return (ulong)_long_value1;
    }

    public Variant(bool value)
    {
        _objref = null;
        _long_value1 = 0;
        _double_value1 = 0;
        _float_value1 = 0;

        _long_value1 = value ? 1u : 0;

        _flags = (int)EValueType.Bool;
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

        return _long_value1 > 0;
    }


    public Variant(float value)
    {
        _objref = null;
        _long_value1 = 0;
        _double_value1 = 0;
        _float_value1 = 0;

        _float_value1 = value;
        _flags = (int)EValueType.Float;
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

        return (float)_long_value1;
    }

    public Variant(double value)
    {
        _objref = null;
        _long_value1 = 0;
        _double_value1 = 0;
        _float_value1 = 0;

        _double_value1 = value;
        _flags = (int)EValueType.Double;
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

        return (double)_long_value1;
    }

    public Variant(string value)
    {
        _objref = null;
        _long_value1 = 0;
        _double_value1 = 0;
        _float_value1 = 0;

        _objref = value;

        _flags = (int)EValueType.String;
    }

    public override string ToString()
    {
        if (IsUndefined)
            return string.Empty;

        if (CheckType(EValueType.String))
            return (string)_objref;

        if (CheckType(EValueType.Bool))
            return (_long_value1 > 0).ToString();

        if (CheckType(EValueType.Float))
            return _float_value1.ToString(CultureInfo.InvariantCulture);

        if (CheckType(EValueType.Double))
            return _double_value1.ToString(CultureInfo.InvariantCulture);

        return _long_value1.ToString();
    }
}

public static class VariantParser
{
    public static Variant Parse(string inString)
    {
        string in_str = inString.Trim(new char[] { ' ', '\t' });
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
                if(double.TryParse(in_str, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                {
                    if(float.MinValue <= val && val <= float.MaxValue)
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

        return new Variant(in_str);
    }
}
