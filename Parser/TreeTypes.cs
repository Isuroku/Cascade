using System;
using System.Collections.Generic;
using System.Text;

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

        public SPosition Position { get { return _pos; } }

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
    }

    public abstract class CBaseKey : CBaseElement
    {
        protected string _name = string.Empty;
        public virtual string Name { get { return _name; } }

        protected List<CBaseElement> _elements = new List<CBaseElement>();

        public int ElementCount { get { return _elements.Count; } }

        public CBaseElement this[int index]
        {
            get
            {
                return _elements[index];
            }
        }

        public IEnumerable<CBaseElement> GetElemets()
        {
            return _elements;
        }

        public CBaseKey(CBaseKey parent, SPosition pos): base(parent, pos)
        {
        }

        public CBaseKey(CBaseKey other): base(other)
        {
            _name = other._name;

            foreach(var el in other._elements)
            {
                var copy = el.GetCopy();
                copy.SetParent(this);
            }
        }

        public override string ToString()
        {
            if (_elements.Count == 0)
                return string.Format("{0} {1}", base.ToString(), Name);

            StringBuilder sb = new StringBuilder(": ");

            _elements.ForEach(e => sb.AppendFormat("{0} ", e.GetDebugText()));

            return string.Format("{0} {1}{2}", base.ToString(), Name, sb);
        }

        public override string GetDebugText()
        {
            return string.Format("{0} {1}", base.ToString(), Name);
        }

        public void SetName(string name) { _name = name; }

        public void AddChild(CBaseElement inElement)
        {
            _elements.Add(inElement);
        }

        public void RemoveChild(CBaseElement inElement)
        {
            _elements.Remove(inElement);
        }

        public void AddTokenTail(CTokenLine line, ITreeBuildSupport inLoger)
        {
            if(line.IsTailEmpty)
                return;

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

        public CBaseKey GetRoot()
        {
            if (_parent == null)
                return this;
            return _parent.GetRoot();
        }

        public CBaseKey FindKey(string[] path, int index = 0)
        {
            string need_name = path[index];

            CBaseElement element = _elements.Find(el =>
            {
                EElementType t = el.GetElementType();
                if (t == EElementType.ArrayKey || t == EElementType.Key)
                {
                    CBaseKey k = el as CBaseKey;
                    return string.Equals(k.Name, need_name, StringComparison.InvariantCultureIgnoreCase);
                }
                return false;
            });

            if (element == null)
                return null;

            var key = element as CBaseKey;
            if (index == path.Length - 1)
                return key;
            return key.FindKey(path, index + 1);
        }

        public void TakeAllElements(CBaseKey other_key, bool inClear)
        {
            List<CBaseElement> lst = new List<CBaseElement>(other_key._elements);

            if(inClear)
                _elements.Clear();

            for (int i = 0; i < lst.Count; ++i)
            {
                lst[i].SetParent(this);
            }
        }

        public bool IsKeyWithNamePresent(string inName)
        {
            for (int i = 0; i < _elements.Count; ++i)
            {
                CBaseElement el = _elements[i];
                if (el.GetElementType() == EElementType.ArrayKey ||
                    el.GetElementType() == EElementType.Key)
                {
                    CBaseKey key = el as CBaseKey;
                    if (string.Equals(key.Name, inName, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }
            return false;
        }
    }

    public class CKey: CBaseKey
    {
        public override EElementType GetElementType() { return EElementType.Key; }

        public CKey() : base(null, new SPosition())
        {
            _name = string.Empty;
        }

        public CKey(CBaseKey parent, CTokenLine line, ITreeBuildSupport inLoger) : base(parent, line.Head.Position)
        {
            _name = line.Head.Text;
            AddTokenTail(line, inLoger);
        }

        public CKey(CBaseKey parent, CToken head, ITreeBuildSupport inLoger) : base(parent, head.Position)
        {
            _name = head.Text;
        }

        public CKey(CBaseKey inTemplate) : base(inTemplate)
        {
        }

        public override CBaseElement GetCopy()
        {
            return new CKey(this);
        }

        internal void CheckOnOneArray(ITreeBuildSupport inLoger)
        {
            if(ElementCount == 1 && _elements[0].GetElementType() == EElementType.ArrayKey)
            {
                CArrayKey arr = _elements[0] as CArrayKey;

                if(!string.IsNullOrEmpty(arr.RealName))
                {
                    if (string.IsNullOrEmpty(_name))
                        _name = arr.RealName;
                    else
                    {
                        inLoger.LogError(EErrorCode.CantTransferName, this);
                    }
                }

                TakeAllElements(arr, true);
            }
        }
    }

    public class CArrayKey : CBaseKey
    {
        public override EElementType GetElementType() { return EElementType.ArrayKey; }

        int _index;
        public int Index { get { return _index; } }

        public string RealName { get { return _name; } }

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    return _index.ToString();
                return _name;
            }
        }
        
        public CArrayKey(CBaseKey parent, SPosition pos, int index) : base(parent, pos)
        {
            _index = index;
        }

        public CArrayKey(CArrayKey inTemplate) : base(inTemplate)
        {
            _index = inTemplate._index;
            _name = inTemplate._name;
        }

        public override CBaseElement GetCopy()
        {
            return new CArrayKey(this);
        }
    }

    public class CStringValue : CBaseElement
    {
        string _value;
        public override EElementType GetElementType() { return EElementType.String; }
        public CStringValue(CBaseKey parent, SPosition pos, string value) : base(parent, pos) { _value = value; }
        public CStringValue(CStringValue other) : base(other) { _value = other._value; }
        public override string ToString() { return string.Format("{0} {1}", base.ToString(), _value); }
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
        public override string GetDebugText() { return ToString(); }
        public override CBaseElement GetCopy() { return new CBoolValue(this); }
    }


}
