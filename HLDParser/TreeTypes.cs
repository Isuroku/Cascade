using System;
using System.Collections.Generic;
using System.Text;

namespace HLDParser
{
    public enum EElementType
    {
        Key,
        String,
        Float,
        Int,
        ArrayKey,
        Bool
    }

    public abstract class CBaseElement
    {
        protected CBaseKey _parent;
        protected SPosition _pos;

        public abstract EElementType GetElementType();
        public abstract string GetDebugText();

        public virtual bool IsKey() { return false; }

        public SPosition Position { get { return _pos; } }
        public CBaseKey Parent { get { return _parent; } }

        public CBaseElement() { }
        public CBaseElement(CBaseKey parent, SPosition pos)
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

        public void SetParent(CBaseKey parent)
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

        public abstract string ToStringShort();
    }

    public class CStringValue : CBaseElement
    {
        string _value;
        public override EElementType GetElementType() { return EElementType.String; }
        public CStringValue(CBaseKey parent, SPosition pos, string value) : base(parent, pos) { _value = value; }
        public CStringValue(CStringValue other) : base(other) { _value = other._value; }
        public override string ToString() { return string.Format("{0} {1}", base.ToString(), _value); }
        public override string ToStringShort() { return _value; }
        public override string GetDebugText() { return ToString(); }
        public override CBaseElement GetCopy() { return new CStringValue(this); }
    }

    public class CIntValue : CBaseElement
    {
        int _value;
        public override EElementType GetElementType() { return EElementType.Int; }
        public CIntValue(CBaseKey parent, SPosition pos, int value) : base(parent, pos) { _value = value; }
        public CIntValue(CIntValue other) : base(other) { _value = other._value; }
        public override string ToString() { return string.Format("{0} {1}", base.ToString(), _value); }
        public override string ToStringShort() { return _value.ToString(); }
        public override string GetDebugText() { return ToString(); }
        public override CBaseElement GetCopy() { return new CIntValue(this); }
    }

    public class CFloatValue : CBaseElement
    {
        float _value;
        public override EElementType GetElementType() { return EElementType.Float; }
        public CFloatValue(CBaseKey parent, SPosition pos, float value) : base(parent, pos) { _value = value; }
        public CFloatValue(CFloatValue other) : base(other) { _value = other._value; }
        public override string ToString() { return string.Format("{0} {1}", base.ToString(), _value); }
        public override string ToStringShort() { return _value.ToString(); }
        public override string GetDebugText() { return ToString(); }
        public override CBaseElement GetCopy() { return new CFloatValue(this); }
    }

    public class CBoolValue : CBaseElement
    {
        bool _value;
        public override EElementType GetElementType() { return EElementType.Bool; }
        public CBoolValue(CBaseKey parent, SPosition pos, bool value) : base(parent, pos) { _value = value; }
        public CBoolValue(CBoolValue other) : base(other) { _value = other._value; }
        public override string ToString() { return string.Format("{0} {1}", base.ToString(), _value); }
        public override string ToStringShort() { return _value.ToString(); }
        public override string GetDebugText() { return ToString(); }
        public override CBaseElement GetCopy() { return new CBoolValue(this); }
    }


}
