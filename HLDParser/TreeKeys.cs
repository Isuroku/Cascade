using ReflectionSerializer;
using System;
using System.Collections.Generic;
using System.Text;

namespace HLDParser
{
    public abstract class CBaseKey : CBaseElement, IKey
    {
        protected string _name = string.Empty;
        public virtual string Name { get { return _name; } }

        protected List<CBaseElement> _values = new List<CBaseElement>();
        protected List<CBaseKey> _keys = new List<CBaseKey>();

        public override bool IsKey() { return true; }

        public CBaseKey GetKey(int index) { return _keys[index]; }
        public int KeyCount { get { return _keys.Count; } }

        public CBaseElement GetValue(int index) { return _values[index]; }
        public int ValuesCount { get { return _values.Count; } }

        public bool IsEmpty { get { return _keys.Count == 0 && _values.Count == 0; } }

        public CBaseKey(CBaseKey parent, SPosition pos) : base(parent, pos)
        {
        }

        public CBaseKey(CBaseKey other) : base(other)
        {
            _name = other._name;

            foreach (var el in other._values)
            {
                var copy = el.GetCopy();
                copy.SetParent(this);
            }

            foreach (var el in other._keys)
            {
                var copy = el.GetCopy();
                copy.SetParent(this);
            }
        }

        public override string ToString()
        {
            return Name;
            //if (_elements.Count == 0)
            //    return Name;//string.Format("{0} {1}", base.ToString(), Name);

            //if (_elements.Count == 1)
            //    return string.Format("{0}: {1}", Name, _elements[0].ToStringShort());

            //return string.Format("{0}: elcount {1}", Name, _elements.Count);

            //StringBuilder sb = new StringBuilder(": ");

            //_elements.ForEach(e => sb.AppendFormat("{0} ", e.GetDebugText()));

            //return string.Format("{0} {1}{2}", base.ToString(), Name, sb);
        }

        //public override string GetDebugText()
        //{
        //    return string.Format("{0} {1}", base.ToString(), Name);
        //}

        public void SetName(string name) { _name = name; }

        public void AddChild(CBaseElement inElement)
        {
            if (inElement.IsKey())
                _keys.Add(inElement as CBaseKey);
            else
                _values.Add(inElement);
        }

        public void RemoveChild(CBaseElement inElement)
        {
            if (inElement.IsKey())
                _keys.Remove(inElement as CBaseKey);
            else
                _values.Remove(inElement);
        }

        internal void AddTokenTail(CTokenLine line, ILogger inLoger)
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
                    case ETokenType.UInt: be = new CUIntValue(this, t.Position, t.GetUIntValue()); break;
                    case ETokenType.Float: be = new CFloatValue(this, t.Position, t.GetFloatValue()); break;
                    case ETokenType.True: be = new CBoolValue(this, t.Position, true); break;
                    case ETokenType.False: be = new CBoolValue(this, t.Position, false); break;
                }

                if (be == null)
                    inLoger.LogError(EErrorCode.WrongTokenInTail, t);
            }
        }

        #region IKey

        public IKey CreateChildKey(string name)
        {
            CKey child = new CKey(this, name);
            return child;
        }

        public IKey CreateArrayKey(int index)
        {
            CArrayKey child = new CArrayKey(this, index);
            return child;
        }

        public void AddValue(long v) { new CIntValue(this, SPosition.zero, v); }
        public void AddValue(ulong v) { new CUIntValue(this, SPosition.zero, v); }
        public void AddValue(decimal v) { new CFloatValue(this, SPosition.zero, v); }
        public void AddValue(bool v) { new CBoolValue(this, SPosition.zero, v); }
        public void AddValue(string v) { new CStringValue(this, SPosition.zero, v); }

        public string GetName() { return Name; }

        public int GetChildCount() { return _keys.Count; }
        public IKey GetChild(int index) { return _keys[index]; }
        public IKey GetChild(string name) { return FindChildKey(name); }

        public int GetValuesCount() { return _values.Count; }
        public string GetValueAsString(int index) { return _values[index].ToString(); }
        #endregion IKey

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

        //internal void OverrideKey(CBaseKey key)
        //{

        //}

        internal void MergeKey(CBaseKey inKey)
        {
            TakeAllValues(inKey, false);

            List<CBaseKey> keys = new List<CBaseKey>(inKey._keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var in_sub_key = keys[i];

                CBaseKey child_key = FindChildKey(in_sub_key.Name);
                if (child_key != null)
                    child_key.MergeKey(in_sub_key);
                else
                    in_sub_key.SetParent(this);
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
            for(int i = 0; i < _keys.Count; ++i)
            {
                CBaseKey k = _keys[i];
                if (string.Equals(k.Name, key_name, StringComparison.InvariantCultureIgnoreCase))
                    return k;
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

        void TakeAllValues(CBaseKey other_key, bool inClear)
        {
            List<CBaseElement> lst = new List<CBaseElement>(other_key._values);

            if (inClear)
                _values.Clear();

            for (int i = 0; i < lst.Count; ++i)
                lst[i].SetParent(this);
        }

        void TakeAllKeys(CBaseKey other_key, bool inClear)
        {
            List<CBaseKey> lst = new List<CBaseKey>(other_key._keys);

            if (inClear)
                _keys.Clear();

            for (int i = 0; i < lst.Count; ++i)
                lst[i].SetParent(this);
        }

        public void TakeAllElements(CBaseKey other_key, bool inClear)
        {
            TakeAllValues(other_key, inClear);
            TakeAllKeys(other_key, inClear);
        }

        public bool IsKeyWithNamePresent(string inName)
        {
            return FindChildKey(inName) != null;
        }

        internal void CheckOnOneArray()
        {
            if (_keys.Count != 1)
                return;

            if (_keys[0].GetElementType() != EElementType.ArrayKey)
                return;

            CArrayKey arr = _keys[0] as CArrayKey;

            if (!string.IsNullOrEmpty(arr.RealName) &&
                !string.IsNullOrEmpty(_name))
                return;

            if (!string.IsNullOrEmpty(arr.RealName))
                _name = arr.RealName;

            TakeAllElements(arr, false);
            arr.SetParent(null);
        }

        public override string GetStringForSave()
        {
            return Name;
        }

        void AppendIntent(StringBuilder sb, int intent)
        {
            for (int i = 0; i < intent; ++i)
                sb.Append('\t');
        }

        bool IsKeyWithNamePresent()
        {
            for (int i = 0; i < _keys.Count; ++i)
                if (!string.IsNullOrEmpty(_keys[i]._name))
                    return true;
            return false;
        }

        protected void SaveToString(StringBuilder sb, int intent, int parent_index)
        {
            if (sb.Length > 0)
                sb.Append(Environment.NewLine);

            if (parent_index > 0 && GetElementType() == EElementType.ArrayKey && KeyCount > 0)
            {
                AppendIntent(sb, intent);
                string rd_str = CTokenFinder.Instance.GetTokenString(ETokenType.RecordDivider);
                sb.Append(rd_str);
                sb.Append(Environment.NewLine);
            }

            bool write_head = false;
            if (!string.IsNullOrEmpty(_name))
                write_head = true;
            else if(_parent != null && _parent.IsKeyWithNamePresent())
                write_head = true;

            int new_int = intent;
            if (write_head)
            {
                AppendIntent(sb, intent);

                if (GetElementType() == EElementType.ArrayKey && _parent != null && _parent.KeyCount > 1)
                {
                    if(!string.IsNullOrEmpty(_name))
                        sb.AppendFormat("{0}{1} {2}", CTokenFinder.Instance.GetTokenString(CTokenFinder.COMMAND_PREFIX), ECommands.Name, _name);
                }
                else
                {
                    sb.AppendFormat("{0}: ", Name);
                    new_int = intent + 1;
                }
            }
            //else if (GetElementType() == EElementType.ArrayKey && !string.IsNullOrEmpty(_name))
            //{
            //    if (sb.Length > 0)
            //        sb.Append(Environment.NewLine);

            //    AppendIntent(sb, intent);

            //    sb.AppendFormat("{0}{1} {2}", CTokenFinder.Instance.GetTokenString(CTokenFinder.COMMAND_PREFIX), ECommands.Name, _name);
            //}

            string values = string.Empty;
            if(_values.Count > 0)
            {
                bool vert = _values.Count > 3 && _keys.Count == 0;

                if (!vert && !write_head)
                    AppendIntent(sb, intent);

                for (int i = 0; i < _values.Count; ++i)
                {
                    if (vert)
                    {
                        sb.Append(Environment.NewLine);
                        AppendIntent(sb, new_int);
                    }
                    
                    sb.Append(_values[i].GetStringForSave());

                    if (!vert && i < _values.Count - 1)
                        sb.Append(", ");
                }
            }

            for (int i = 0; i < _keys.Count; ++i)
                _keys[i].SaveToString(sb, new_int, i);
        }
    }

    public class CKey : CBaseKey
    {
        public override EElementType GetElementType() { return EElementType.Key; }

        public CKey() : base(null, new SPosition())
        {
            _name = string.Empty;
        }

        internal CKey(CBaseKey parent, CTokenLine line, ILogger inLoger) : base(parent, line.Head.Position)
        {
            _name = line.Head.Text;
            AddTokenTail(line, inLoger);
        }

        internal CKey(CBaseKey parent, CToken head, ILogger inLoger) : base(parent, head.Position)
        {
            _name = head.Text;
        }

        public CKey(CBaseKey parent, string inName) : base(parent, SPosition.zero)
        {
            _name = inName;
        }

        public CKey(CBaseKey inTemplate) : base(inTemplate)
        {
        }

        public override CBaseElement GetCopy()
        {
            return new CKey(this);
        }

        public string SaveToString()
        {
            StringBuilder sb = new StringBuilder();
            SaveToString(sb, 0, 0);
            return sb.ToString();
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

        public CArrayKey(CBaseKey parent, int index) : base(parent, SPosition.zero)
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
