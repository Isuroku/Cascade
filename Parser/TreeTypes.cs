using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
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

        public CBaseElement() { }
        public CBaseElement(CBaseKey parent, SPosition pos)
        {
            _parent = parent;
            if(_parent != null)
                _parent.AddChild(this);
            _pos = pos;
        }

        public override string ToString()
        {
            return string.Format("{0}", GetElementType());
        }
    }

    public abstract class CBaseKey : CBaseElement
    {
        List<CBaseElement> _elements = new List<CBaseElement>();

        public int ElementCount { get { return _elements.Count; } }

        public CBaseElement this[int index]
        {
            get
            {
                return _elements[index];
            }
        }

        public abstract string GetName();

        public CBaseKey(CBaseKey parent, SPosition pos): base(parent, pos) { }

        public override string ToString()
        {
            if (_elements.Count == 0)
                return string.Format("{0} {1}", base.ToString(), GetName());

            StringBuilder sb = new StringBuilder(": ");

            _elements.ForEach(e => sb.AppendFormat("{0} ", e.GetDebugText()));

            return string.Format("{0} {1}{2}", base.ToString(), GetName(), sb);
        }

        public override string GetDebugText()
        {
            return string.Format("{0} {1}", base.ToString(), GetName());
        }

        public void AddChild(CBaseElement inElement)
        {
            _elements.Add(inElement);
        }

        public void AddTokenTail(CTokenLine line, CLoger inLoger)
        {
            if(line.IsTailEmpty)
            {
                inLoger.LogInternalError(EInternalErrorCode.EmptyTailAdded, line.ToString());
                return;
            }

            for (int i = 0; i < line.TailLength; i++)
            {
                CToken t = line.Tail[i];

                CBaseElement be = null;

                switch (t.TokenType)
                {
                    case ETokenType.Word: be = new CStringValue(this, t.Position, t.Text); break;
                    case ETokenType.Int: be = new CIntValue(this, t.Position, t.GetIntValue()); break;
                    case ETokenType.Float: be = new CFloatValue(this, t.Position, t.GetFloatValue()); break;
                    case ETokenType.True: be = new CBoolValue(this, t.Position, true); break;
                    case ETokenType.False: be = new CBoolValue(this, t.Position, false); break;
                }

                if (be == null)
                    inLoger.LogError(EErrorCode.WrongTokenInTail, t);
            }
        }
    }

    public class CKey: CBaseKey
    {
        public override EElementType GetElementType() { return EElementType.Key; }

        string _name;

        public override string GetName() { return _name; }

        public CKey() : base(null, new SPosition())
        {
            _name = "Root";
        }

        public CKey(CBaseKey parent, CTokenLine line, CLoger inLoger) : base(parent, line.Head.Position)
        {
            _name = line.Head.Text;
            AddTokenTail(line, inLoger);
        }

        public CKey(CBaseKey parent, CToken head, CLoger inLoger) : base(parent, head.Position)
        {
            _name = head.Text;
        }
    }

    public class CArrayKey : CBaseKey
    {
        public override EElementType GetElementType() { return EElementType.ArrayKey; }

        int _index;
        public override string GetName() { return _index.ToString(); }

        public CArrayKey(CBaseKey parent, SPosition pos, int index) : base(parent, pos)
        {
            _index = index;
        }
    }

    public class CStringValue : CBaseElement
    {
        string _value;
        public override EElementType GetElementType() { return EElementType.String; }
        public CStringValue(CBaseKey parent, SPosition pos, string value) : base(parent, pos) { _value = value; }
        public override string ToString() { return string.Format("{0} {1}", base.ToString(), _value); }
        public override string GetDebugText() { return ToString(); }
    }

    public class CIntValue : CBaseElement
    {
        int _value;
        public override EElementType GetElementType() { return EElementType.Int; }
        public CIntValue(CBaseKey parent, SPosition pos, int value) : base(parent, pos) { _value = value; }
        public override string ToString() { return string.Format("{0} {1}", base.ToString(), _value); }
        public override string GetDebugText() { return ToString(); }
    }

    public class CFloatValue : CBaseElement
    {
        float _value;
        public override EElementType GetElementType() { return EElementType.Float; }
        public CFloatValue(CBaseKey parent, SPosition pos, float value) : base(parent, pos) { _value = value; }
        public override string ToString() { return string.Format("{0} {1}", base.ToString(), _value); }
        public override string GetDebugText() { return ToString(); }
    }

    public class CBoolValue : CBaseElement
    {
        bool _value;
        public override EElementType GetElementType() { return EElementType.Bool; }
        public CBoolValue(CBaseKey parent, SPosition pos, bool value) : base(parent, pos) { _value = value; }
        public override string ToString() { return string.Format("{0} {1}", base.ToString(), _value); }
        public override string GetDebugText() { return ToString(); }
    }


}
