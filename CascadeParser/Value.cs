
using System;

namespace CascadeParser
{
    internal class CValue : CBaseElement, IKeyValue
    {
        IKey IKeyValue.Parent { get { return Parent; } }

        Variant _value;

        public EValueType ValueType { get { return _value.VariantType; } }

        public CValue() { }
        public CValue(CKey parent, CToken token) : base(parent, token.Position) 
        {
            _value = VariantParser.Parse(token.Text);
        }

        public CValue(CKey parent, Variant inValue) : base(parent, SPosition.zero)
        {
            _value = inValue;
        }

        public CValue(CKey parent, byte[] ioBuffer, ref int ioOffset) : base(parent, SPosition.zero)
        {
            ioOffset = Variant.BinaryDeserialize(ioBuffer, ioOffset, out _value);
        }

        public static CValue CreateBaseValue(CKey parent, CToken token)
        {
            return new CValue(parent, token);
        }

        public CValue(CValue other) : base(other)
        {
            _value = other._value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public bool ToBool() { return _value.ToBool(); }
        public byte ToByte() { return _value.ToByte(); }
        public short ToShort() { return _value.ToShort(); }
        public ushort ToUShort() { return _value.ToUShort(); }
        public int ToInt() { return _value.ToInt(); }
        public uint ToUInt() { return _value.ToUInt(); }
        public long ToLong() { return _value.ToLong(); }
        public ulong ToULong() { return _value.ToULong(); }
        public float ToFloat() { return _value.ToFloat(); }
        public double ToDouble() { return _value.ToDouble(); }
        public DateTime ToDateTime() { return _value.ToDateTime(DateTime.UtcNow); }
        public TimeSpan ToTimeSpan() { return _value.ToTimeSpan(TimeSpan.Zero); }

        public override CBaseElement GetCopy()
        {
            return new CValue(this);
        }

        public override string GetStringForSave()
        {
            string s = _value.ToString();
            if(_value.VariantType == EValueType.String)
                s = Utils.GetStringForSave(s);
            return s;
        }

        public int GetMemorySize()
        {
            return _value.GetMemorySize();
        }

        public int BinarySerialize(byte[] ioBuffer, int inOffset)
        {
            return _value.BinarySerialize(ioBuffer, inOffset);
        }
    }

}
