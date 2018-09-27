using System.Globalization;
using System.Threading;

namespace CascadeParser
{
    public enum EElementType
    {
        Key,
        String,
        Float,
        Int,
        //ArrayKey,
        Bool
    }

    public abstract class CBaseElement
    {
        protected CKey _parent;
        protected SPosition _pos;

        private string _comments;
        public string Comments { get { return _comments; } }

        static CultureInfo _custom_culture;
        protected static CultureInfo GetCultureInfo()
        {
            if (_custom_culture == null)
            {
                _custom_culture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
                _custom_culture.NumberFormat.NumberDecimalSeparator = ".";
            }
            return _custom_culture;
        }

        public abstract EElementType GetElementType();

        public virtual bool IsKey() { return false; }

        public SPosition Position { get { return _pos; } }
        public CKey Parent { get { return _parent; } }

        public CBaseElement() { }
        public CBaseElement(CKey parent, SPosition pos)
        {
            _parent = parent;
            if(_parent != null)
                _parent.AddChild(this);
            _pos = pos;
        }

        public CBaseElement(CBaseElement other)
        {
            _pos = other._pos;
        }

        public abstract CBaseElement GetCopy();

        public void SetParent(CKey parent)
        {
            if (_parent != null)
                _parent.RemoveChild(this);

            _parent = parent;
            if (_parent != null)
                _parent.AddChild(this);
        }

        public override string ToString()
        {
            return string.Format("{0} [pos {1}]", GetElementType(), _pos);
        }

        public abstract string GetStringForSave();

        public void AddComments(string text)
        {
            if (string.IsNullOrEmpty(_comments))
                _comments = text;
            else
                _comments += string.Format(" {0}", text);
        }

        public abstract float GetValueAsFloat();
        public abstract decimal GetValueAsDecimal();
        public abstract int GetValueAsInt();
        public abstract long GetValueAsLong();
        public abstract uint GetValueAsUInt();
        public abstract ulong GetValueAsULong();
        public abstract bool GetValueAsBool();
    }

    public class CStringValue : CBaseElement
    {
        string _value;
        public override EElementType GetElementType() { return EElementType.String; }
        public CStringValue(CKey parent, SPosition pos, string value) : base(parent, pos) { _value = value; }
        public CStringValue(CKey parent, string value) : base(parent, SPosition.zero) { _value = value; }
        public CStringValue(CStringValue other) : base(other) { _value = other._value; }
        public override string ToString() { return _value; }
        public override CBaseElement GetCopy() { return new CStringValue(this); }

        public override string GetStringForSave()
        {
            bool only_alfa_digit = true;
            for (int i = 0; i < _value.Length && only_alfa_digit; ++i)
                only_alfa_digit = char.IsLetterOrDigit(_value[i]);

            if (only_alfa_digit)
                return _value;

            return string.Format("{0}{1}{2}", "\"", _value, "\"");
        }

        public override float GetValueAsFloat()
        {
            float v;
            if (!float.TryParse(_value, out v))
                return 0;
            return v;
        }

        public override decimal GetValueAsDecimal()
        {
            decimal v;
            if (!decimal.TryParse(_value, out v))
                return 0;
            return v;
        }

        public override int GetValueAsInt()
        {
            int v;
            if (!int.TryParse(_value, out v))
                return 0;
            return v;
        }

        public override long GetValueAsLong()
        {
            long v;
            if (!long.TryParse(_value, out v))
                return 0;
            return v;
        }

        public override uint GetValueAsUInt()
        {
            uint v;
            if (!uint.TryParse(_value, out v))
                return 0;
            return v;
        }

        public override ulong GetValueAsULong()
        {
            ulong v;
            if (!ulong.TryParse(_value, out v))
                return 0;
            return v;
        }

        public override bool GetValueAsBool()
        {
            bool v;
            if (!bool.TryParse(_value, out v))
                return false;
            return v;
        }
    }

    public class CIntValue : CBaseElement
    {
        long _value;
        public override EElementType GetElementType() { return EElementType.Int; }
        public CIntValue(CKey parent, SPosition pos, long value) : base(parent, pos) { _value = value; }
        public CIntValue(CKey parent, long value) : base(parent, SPosition.zero) { _value = value; }
        public CIntValue(CIntValue other) : base(other) { _value = other._value; }
        public override string ToString() { return _value.ToString(); }
        public override CBaseElement GetCopy() { return new CIntValue(this); }

        public override string GetStringForSave()
        {
            return _value.ToString();
        }

        public override float GetValueAsFloat() { return _value; }
        public override decimal GetValueAsDecimal() { return _value; }
        public override int GetValueAsInt() { return (int)_value; }
        public override long GetValueAsLong() { return (long)_value; }
        public override uint GetValueAsUInt() { return (uint)_value; }
        public override ulong GetValueAsULong() { return (ulong)_value; }
        public override bool GetValueAsBool() { return _value > 0; }
    }

    public class CUIntValue : CBaseElement
    {
        ulong _value;
        public override EElementType GetElementType() { return EElementType.Int; }
        public CUIntValue(CKey parent, SPosition pos, ulong value) : base(parent, pos) { _value = value; }
        public CUIntValue(CKey parent, ulong value) : base(parent, SPosition.zero) { _value = value; }
        public CUIntValue(CUIntValue other) : base(other) { _value = other._value; }
        public override string ToString() { return _value.ToString(); }
        public override CBaseElement GetCopy() { return new CUIntValue(this); }

        public override string GetStringForSave()
        {
            return _value.ToString();
        }

        public override float GetValueAsFloat() { return _value; }
        public override decimal GetValueAsDecimal() { return _value; }
        public override int GetValueAsInt() { return (int)_value; }
        public override long GetValueAsLong() { return (long)_value; }
        public override uint GetValueAsUInt() { return (uint)_value; }
        public override ulong GetValueAsULong() { return (ulong)_value; }
        public override bool GetValueAsBool() { return _value > 0; }
    }

    public class CFloatValue : CBaseElement
    {
        decimal _value;
        public override EElementType GetElementType() { return EElementType.Float; }
        public CFloatValue(CKey parent, SPosition pos, decimal value) : base(parent, pos) { _value = value; }
        public CFloatValue(CKey parent, decimal value) : base(parent, SPosition.zero) { _value = value; }
        public CFloatValue(CFloatValue other) : base(other) { _value = other._value; }
        public override string ToString() { return _value.ToString(); }
        public override CBaseElement GetCopy() { return new CFloatValue(this); }

        public override string GetStringForSave()
        {
            return _value.ToString(GetCultureInfo());
        }

        public override float GetValueAsFloat() { return (float)_value; }
        public override decimal GetValueAsDecimal() { return _value; }
        public override int GetValueAsInt() { return (int)_value; }
        public override long GetValueAsLong() { return (long)_value; }
        public override uint GetValueAsUInt() { return (uint)_value; }
        public override ulong GetValueAsULong() { return (ulong)_value; }
        public override bool GetValueAsBool() { return _value > 0; }
    }

    public class CBoolValue : CBaseElement
    {
        bool _value;
        public override EElementType GetElementType() { return EElementType.Bool; }
        public CBoolValue(CKey parent, SPosition pos, bool value) : base(parent, pos) { _value = value; }
        public CBoolValue(CKey parent, bool value) : base(parent, SPosition.zero) { _value = value; }
        public CBoolValue(CBoolValue other) : base(other) { _value = other._value; }
        public override string ToString() { return _value.ToString(); }
        public override CBaseElement GetCopy() { return new CBoolValue(this); }

        public override string GetStringForSave()
        {
            return _value.ToString();
        }

        public override float GetValueAsFloat() { return _value ? 1 : 0; }
        public override decimal GetValueAsDecimal() { return _value ? 1 : 0; }
        public override int GetValueAsInt() { return _value ? 1 : 0; }
        public override long GetValueAsLong() { return _value ? 1 : 0; }
        public override uint GetValueAsUInt() { return _value ? 1u : 0; }
        public override ulong GetValueAsULong() { return _value ? 1u : 0; }
        public override bool GetValueAsBool() { return _value; }
    }


}
