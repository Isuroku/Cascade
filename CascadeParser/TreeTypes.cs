﻿using System.Globalization;
using System.Threading;

namespace CascadeParser
{
    public enum EElementType
    {
        Key,
        String,
        Float,
        Int,
        UInt,
        //ArrayKey,
        Bool
    }

    internal abstract class CBaseElement
    {
        protected CKey _parent;
        protected SPosition _pos;

        private string _comments;
        public string Comments { get { return _comments; } }

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
            _comments = other._comments;
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

        public void ClearComments()
        {
            _comments = string.Empty;
        }
    }

    internal abstract class CBaseValue : CBaseElement, IKeyValue
    {
        IKey IKeyValue.Parent { get { return Parent; } }

        public EElementType ElementType { get { return GetElementType(); } }

        public CBaseValue() { }
        public CBaseValue(CKey parent, SPosition pos): base(parent, pos) { }
        public CBaseValue(CBaseElement other): base(other) { }

        public abstract float GetValueAsFloat();
        public abstract double GetValueAsDouble();
        public abstract decimal GetValueAsDecimal();
        public abstract int GetValueAsInt();
        public abstract long GetValueAsLong();
        public abstract uint GetValueAsUInt();
        public abstract ulong GetValueAsULong();
        public abstract bool GetValueAsBool();

        public string GetValueAsString() { return ToString(); }

        public static CBaseValue CreateBaseValue(CKey parent, CToken token)
        {
            CBaseValue be = null;

            switch (token.TokenType)
            {
                case ETokenType.Word: be = new CStringValue(parent, token.Position, token.Text); break;
                case ETokenType.Int: be = new CIntValue(parent, token.Position, token.GetIntValue()); break;
                case ETokenType.UInt: be = new CUIntValue(parent, token.Position, token.GetUIntValue()); break;
                case ETokenType.Float: be = new CFloatValue(parent, token.Position, token.GetFloatValue()); break;
                case ETokenType.True: be = new CBoolValue(parent, token.Position, true); break;
                case ETokenType.False: be = new CBoolValue(parent, token.Position, false); break;
            }
            return be;
        }
    }

    internal class CStringValue : CBaseValue
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
            return Utils.GetStringForSave(_value);
        }

        public override float GetValueAsFloat()
        {
            float v;
            if (!float.TryParse(_value, out v))
                return 0;
            return v;
        }

        public override double GetValueAsDouble()
        {
            double v;
            if (!double.TryParse(_value, out v))
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

    internal class CIntValue : CBaseValue
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
        public override double GetValueAsDouble() { return _value; }
        public override decimal GetValueAsDecimal() { return _value; }
        public override int GetValueAsInt() { return (int)_value; }
        public override long GetValueAsLong() { return (long)_value; }
        public override uint GetValueAsUInt() { return (uint)_value; }
        public override ulong GetValueAsULong() { return (ulong)_value; }
        public override bool GetValueAsBool() { return _value > 0; }
    }

    internal class CUIntValue : CBaseValue
    {
        ulong _value;
        public override EElementType GetElementType() { return EElementType.UInt; }
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
        public override double GetValueAsDouble() { return _value; }
        public override decimal GetValueAsDecimal() { return _value; }
        public override int GetValueAsInt() { return (int)_value; }
        public override long GetValueAsLong() { return (long)_value; }
        public override uint GetValueAsUInt() { return (uint)_value; }
        public override ulong GetValueAsULong() { return (ulong)_value; }
        public override bool GetValueAsBool() { return _value > 0; }
    }

    internal class CFloatValue : CBaseValue
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
            return _value.ToString(Utils.GetCultureInfoFloatPoint());
        }

        public override float GetValueAsFloat() { return (float)_value; }
        public override double GetValueAsDouble() { return (double)_value; }
        public override decimal GetValueAsDecimal() { return _value; }
        public override int GetValueAsInt() { return (int)_value; }
        public override long GetValueAsLong() { return (long)_value; }
        public override uint GetValueAsUInt() { return (uint)_value; }
        public override ulong GetValueAsULong() { return (ulong)_value; }
        public override bool GetValueAsBool() { return _value > 0; }
    }

    internal class CBoolValue : CBaseValue
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
        public override double GetValueAsDouble() { return _value ? 1 : 0; }
        public override decimal GetValueAsDecimal() { return _value ? 1 : 0; }
        public override int GetValueAsInt() { return _value ? 1 : 0; }
        public override long GetValueAsLong() { return _value ? 1 : 0; }
        public override uint GetValueAsUInt() { return _value ? 1u : 0; }
        public override ulong GetValueAsULong() { return _value ? 1u : 0; }
        public override bool GetValueAsBool() { return _value; }
    }


}
