using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    public abstract class CBaseKey : CBaseElement
    {
        protected string _name = string.Empty;
        public virtual string Name { get { return _name; } }

        protected List<CBaseElement> _elements = new List<CBaseElement>();

        public int ElementCount { get { return _elements.Count; } }

        public override bool IsKey() { return true; }

        public CBaseElement this[int index]
        {
            get
            {
                return _elements[index];
            }
        }

        public IEnumerable<CBaseElement> GetElements()
        {
            return _elements;
        }

        public CBaseKey(CBaseKey parent, SPosition pos) : base(parent, pos)
        {
        }

        public CBaseKey(CBaseKey other) : base(other)
        {
            _name = other._name;

            foreach (var el in other._elements)
            {
                var copy = el.GetCopy();
                copy.SetParent(this);
            }
        }

        public override string ToString()
        {
            if (_elements.Count == 0)
                return Name;//string.Format("{0} {1}", base.ToString(), Name);

            if (_elements.Count == 1)
                return string.Format("{0}: {1}", Name, _elements[0].ToStringShort());

            return string.Format("{0}: elcount {1}", Name, _elements.Count);

            //StringBuilder sb = new StringBuilder(": ");

            //_elements.ForEach(e => sb.AppendFormat("{0} ", e.GetDebugText()));

            //return string.Format("{0} {1}{2}", base.ToString(), Name, sb);
        }

        public override string ToStringShort() { return Name; }

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
            if (line.IsTailEmpty)
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

        //internal bool DeleteKey(CBaseKey inKey, ITreeBuildSupport inLoger)
        //{
        //    CBaseKey child_key = FindChildKey(inKey.Name);

        //    if(child_key == null)
        //    {
        //        inLoger.LogError(EErrorCode.CantFindKey, inKey.Name, Position.Line);
        //        return false;
        //    }

        //    List<CBaseKey> in_sub_keys = new List<CBaseKey>();
        //    for(int i = 0; i < inKey.ElementCount; i++)
        //    {
        //        CBaseElement el = inKey[i];
        //        if (el.IsKey())
        //            in_sub_keys.Add(el as CBaseKey);
        //    }

        //    if(in_sub_keys.Count > 0)
        //    {
        //        for (int i = 0; i < in_sub_keys.Count; i++)
        //        {
        //            CBaseKey in_sub_key = in_sub_keys[i];
        //            bool res = child_key.DeleteKey(in_sub_key, inLoger);
        //        }

        //        if(child_key.ElementCount == 0)
        //            child_key.SetParent(null);
        //        return true;
        //    }
        //    else
        //    {
        //        child_key.SetParent(null);
        //        return true;
        //    }
        //}

        internal void OverrideKey(CBaseKey key)
        {
            
        }

        internal void MergeKey(CBaseKey inKey)
        {
            List<CBaseElement> in_elements = new List<CBaseElement>(inKey.GetElements());
            for (int i = 0; i < in_elements.Count; i++)
            {
                CBaseElement el = in_elements[i];
                if (el.IsKey())
                {
                    var in_sub_key = el as CBaseKey;

                    CBaseKey child_key = FindChildKey(in_sub_key.Name);
                    if (child_key != null)
                        child_key.MergeKey(in_sub_key);
                    else
                        in_sub_key.SetParent(this);
                }
                else
                    el.SetParent(this);
            }
        }

        public CBaseKey GetRoot()
        {
            if (_parent == null)
                return this;
            return _parent.GetRoot();
        }

        public CBaseKey FindChildKey(string key_name)
        {
            for(int i = 0; i < _elements.Count; ++i)
            {
                CBaseElement el = _elements[i];
                if (el.IsKey())
                {
                    CBaseKey k = el as CBaseKey;
                    if (string.Equals(k.Name, key_name, StringComparison.InvariantCultureIgnoreCase))
                        return k;
                }
            }
            return null;
        }

        public CBaseKey FindKey(string[] path, int index = 0)
        {
            string need_name = path[index];

            CBaseKey key = FindChildKey(need_name);
            if (key == null)
                return null;

            if (index == path.Length - 1)
                return key;
            return key.FindKey(path, index + 1);
        }

        public void TakeAllElements(CBaseKey other_key, bool inClear)
        {
            List<CBaseElement> lst = new List<CBaseElement>(other_key._elements);

            if (inClear)
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

    public class CKey : CBaseKey
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
            int arr_key_count = 0;
            CArrayKey arr = null;
            for (int i = 0; i < _elements.Count; i++)
                if (_elements[i].GetElementType() == EElementType.ArrayKey)
                {
                    arr_key_count++;
                    arr = _elements[i] as CArrayKey;
                }

            if (arr_key_count == 1)
            {
                if (!string.IsNullOrEmpty(arr.RealName))
                {
                    if (string.IsNullOrEmpty(_name))
                        _name = arr.RealName;
                    else
                    {
                        inLoger.LogError(EErrorCode.CantTransferName, this);
                    }
                }

                TakeAllElements(arr, false);
                arr.SetParent(null);
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
}
