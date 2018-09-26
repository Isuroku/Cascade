using ReflectionSerializer;
using System;
using System.Collections.Generic;
using System.Text;

namespace HLDParser
{
    public class CKey : CBaseElement, IKey
    {
        public bool IsArray { get; private set; }

        protected string _name = string.Empty;

        public virtual string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(_name))
                    return _name;
                if (IsArray)
                    return GetIndex().ToString();
                return string.Empty;
            }
        }

        protected List<CBaseElement> _values = new List<CBaseElement>();
        protected List<CKey> _keys = new List<CKey>();

        public override bool IsKey() { return true; }

        public CKey GetKey(int index) { return _keys[index]; }
        public int KeyCount { get { return _keys.Count; } }

        public CBaseElement GetValue(int index) { return _values[index]; }
        public int ValuesCount { get { return _values.Count; } }

        public bool IsEmpty { get { return _keys.Count == 0 && _values.Count == 0; } }

        public override EElementType GetElementType() { return EElementType.Key; }

        public static CKey Create(CKey parent, SPosition pos)
        {
            return new CKey(parent, string.Empty, false, pos);
        }

        internal static CKey Create(CKey parent, CTokenLine line, ILogger inLoger)
        {
            return new CKey(parent, line, inLoger);
        }

        internal static CKey CreateCopy(CKey other)
        {
            return new CKey(other);
        }

        public static CKey CreateArrayKey(CKey parent)
        {
            return new CKey(parent, string.Empty, true, SPosition.zero);
        }

        public static CKey CreateArrayKey(CKey parent, SPosition pos)
        {
            return new CKey(parent, string.Empty, true, pos);
        }

        public static CKey CreateRoot()
        {
            return new CKey(null, string.Empty, false, SPosition.zero);
        }

        public static CKey CreateRoot(string inName)
        {
            return new CKey(null, inName, false, SPosition.zero);
        }

        public static CKey CreateChild(CKey parent, string inName)
        {
            return new CKey(parent, inName, false, SPosition.zero);
        }

        private CKey(CKey parent, string inName, bool inArray, SPosition pos) : base(parent, pos)
        {
            _name = inName;
            IsArray = inArray;
        }

        private CKey(CKey parent, CTokenLine line, ILogger inLoger) : base(parent, line.Head.Position)
        {
            _name = line.Head.Text;
            AddTokenTail(line, false, inLoger);
        }

        private CKey(CKey other) : base(other)
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

        public override CBaseElement GetCopy()
        {
            return CreateCopy(this);
        }

        public override string ToString()
        {
            return Name;
        }

        public void SetName(string name) { _name = name; }

        public void AddChild(CBaseElement inElement)
        {
            if (inElement.IsKey())
                _keys.Add(inElement as CKey);
            else
                _values.Add(inElement);
        }

        public void RemoveChild(CBaseElement inElement)
        {
            if (inElement.IsKey())
                _keys.Remove(inElement as CKey);
            else
                _values.Remove(inElement);
        }

        internal void AddTokenTail(CTokenLine line, bool inCommentForValue, ILogger inLoger)
        {
            if (line.Comments != null && !inCommentForValue)
                AddComments(line.Comments.Text);

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

                if (line.Comments != null && inCommentForValue && be != null)
                    be.AddComments(line.Comments.Text);

                if (be == null)
                    inLoger.LogError(EErrorCode.WrongTokenInTail, t);
            }
        }

        public int GetIndex()
        {
            for (int i = 0; i < _parent.KeyCount; i++)
            {
                if (_parent.GetKey(i) == this)
                    return i;
            }
            return -1;
        }

        #region IKey

        public IKey CreateChildKey(string name)
        {
            CKey child = CreateChild(this, name);
            return child;
        }

        public IKey CreateArrayKey()
        {
            CKey child = CreateArrayKey(this);
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

        internal void MergeKey(CKey inKey)
        {
            TakeAllValues(inKey, false);

            List<CKey> keys = new List<CKey>(inKey._keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var in_sub_key = keys[i];

                CKey child_key = FindChildKey(in_sub_key.Name);
                if (child_key != null)
                    child_key.MergeKey(in_sub_key);
                else
                    in_sub_key.SetParent(this);
            }
        }

        public CKey GetRoot()
        {
            if (_parent == null)
                return this;
            return _parent.GetRoot();
        }

        public CKey FindChildKey(string key_name)
        {
            for (int i = 0; i < _keys.Count; ++i)
            {
                CKey k = _keys[i];
                if (string.Equals(k.Name, key_name, StringComparison.InvariantCultureIgnoreCase))
                    return k;
            }
            return null;
        }

        public CKey FindKey(string[] path, int index = 0)
        {
            string need_name = path[index];

            CKey key = FindChildKey(need_name);
            if (key == null)
                return null;

            if (index == path.Length - 1)
                return key;
            return key.FindKey(path, index + 1);
        }

        void TakeAllValues(CKey other_key, bool inClear)
        {
            List<CBaseElement> lst = new List<CBaseElement>(other_key._values);

            if (inClear)
                _values.Clear();

            for (int i = 0; i < lst.Count; ++i)
                lst[i].SetParent(this);
        }

        void TakeAllKeys(CKey other_key, bool inClear)
        {
            List<CKey> lst = new List<CKey>(other_key._keys);

            if (inClear)
                _keys.Clear();

            for (int i = 0; i < lst.Count; ++i)
                lst[i].SetParent(this);
        }

        public void TakeAllElements(CKey other_key, bool inClear)
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

            if (!_keys[0].IsArray)
                return;

            CKey arr = _keys[0];

            if (!string.IsNullOrEmpty(arr._name) &&
                !string.IsNullOrEmpty(_name))
                return;

            if (!string.IsNullOrEmpty(arr._name))
                _name = arr._name;

            TakeAllElements(arr, false);
            arr.SetParent(null);
        }

        internal Tuple<CKey, int> FindLowerNearestKey(int inLineNumber)
        {
            int dist = Position.Line - inLineNumber;
            if (dist >= 0 && _parent != null)
                return new Tuple<CKey, int>(this, dist);

            CKey sub_key = null;
            int key_dist = int.MaxValue;
            for (int i = 0; i < _keys.Count; ++i)
            {
                var t = _keys[i].FindLowerNearestKey(inLineNumber);
                if (t.Item1 != null && t.Item2 < key_dist)
                {
                    sub_key = t.Item1;
                    key_dist = t.Item2;
                }
            }

            return new Tuple<CKey, int>(sub_key, key_dist);
        }

        public string SaveToString()
        {
            StringBuilder sb = new StringBuilder();
            SaveToString(sb, 0, 0);
            return sb.ToString();
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

        string GetStringCommentsForSave(bool with_values)
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(Comments))
                sb.AppendFormat("{0}; ", Comments);

            for (int i = 0; i < _values.Count; ++i)
                if (!string.IsNullOrEmpty(_values[i].Comments))
                    sb.AppendFormat("{0}; ", _values[i].Comments);

            return sb.ToString();
        }

        void AddStringValuesForSave(StringBuilder sb)
        {
            for (int i = 0; i < _values.Count; ++i)
            {
                sb.Append(_values[i].GetStringForSave());
                if (i < _values.Count - 1)
                    sb.Append(", ");
            }
        }

        bool IsValuesHasComments()
        {
            for (int i = 0; i < _values.Count; ++i)
            {
                if (!string.IsNullOrEmpty(_values[i].Comments))
                    return true;
            }
            return false;
        }

        protected void SaveToString(StringBuilder sb, int intent, int parent_index)
        {
            bool was_writing = false;
            int new_int = intent;
            if (!IsArray)
            {
                if (!string.IsNullOrEmpty(Comments))
                {
                    AppendIntent(sb, intent);
                    sb.AppendFormat("{0}{1}", CTokenFinder.Instance.GetTokenString(ETokenType.Comment), Comments);
                    sb.Append(Environment.NewLine);
                }

                bool vert_values_writing = _values.Count > 3 && _keys.Count == 0 || IsValuesHasComments();
                if (vert_values_writing)
                {
                    AppendIntent(sb, intent);
                    sb.AppendFormat("{0}: ", Name);

                    for (int i = 0; i < _values.Count; ++i)
                    {
                        sb.Append(Environment.NewLine);
                        AppendIntent(sb, intent + 1);

                        sb.Append(_values[i].GetStringForSave());

                        if (!string.IsNullOrEmpty(_values[i].Comments))
                            sb.AppendFormat(" {0}{1}", CTokenFinder.Instance.GetTokenString(ETokenType.Comment), _values[i].Comments);
                    }

                    was_writing = true;
                }
                else if (!string.IsNullOrEmpty(Name) || _values.Count > 0)
                {

                    AppendIntent(sb, intent);
                    sb.AppendFormat("{0}: ", Name);
                    AddStringValuesForSave(sb);
                    was_writing = true;
                    new_int = intent + 1;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Comments))
                {
                    AppendIntent(sb, intent);
                    sb.AppendFormat(" {0}{1}", CTokenFinder.Instance.GetTokenString(ETokenType.Comment), Comments);
                    was_writing = true;
                }

                if (!string.IsNullOrEmpty(_name))
                {
                    if (was_writing)
                        sb.Append(Environment.NewLine);
                    AppendIntent(sb, intent);
                    sb.AppendFormat("{0}{1} {2}", CTokenFinder.Instance.GetTokenString(CTokenFinder.COMMAND_PREFIX), ECommands.Name, _name);
                    was_writing = true;
                }

                if (_values.Count > 0)
                {
                    if (was_writing)
                        sb.Append(Environment.NewLine);

                    AppendIntent(sb, intent);
                    AddStringValuesForSave(sb);

                    was_writing = true;
                }
            }

            if (was_writing)
                sb.Append(Environment.NewLine);

            for (int i = 0; i < _keys.Count; ++i)
            {
                if (_keys[i].IsArray && i > 0 && _keys[i].KeyCount > 0)
                {
                    AppendIntent(sb, new_int);
                    string rd_str = CTokenFinder.Instance.GetTokenString(ETokenType.RecordDivider);
                    sb.Append(rd_str);
                    sb.Append(Environment.NewLine);
                }
                _keys[i].SaveToString(sb, new_int, i);
            }


        }
    }
}