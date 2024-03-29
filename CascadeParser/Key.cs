﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CascadeParser
{
    internal class CKey : CBaseElement, IKey
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

        IKey IKey.Parent { get { return base.Parent; } }

        protected List<CValue> _values = new List<CValue>();
        protected List<CKey> _keys = new List<CKey>();

        public override bool IsKey() { return true; }

        public CKey GetKey(int index) { return _keys[index]; }
        public int KeyCount { get { return _keys.Count; } }

        public int ValuesCount { get { return _values.Count; } }

        public bool IsEmpty { get { return _keys.Count == 0 && _values.Count == 0; } }

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
            if (!parent.CheckNameForChild(inName))
                return null;
            return new CKey(parent, inName, false, SPosition.zero);
        }

        private CKey(CKey parent, string inName, bool inArray, SPosition pos) : base(parent, pos)
        {
            SetName(inName);
            IsArray = inArray;
        }

        private CKey(CKey parent, CTokenLine line, ILogger inLoger) : base(parent, line.Head.Position)
        {
            _name = line.Head.Text;
            AddTokenTail(line, inLoger);
        }

        private CKey(CKey other) : base(other)
        {
            _name = other._name;

            IsArray = other.IsArray;

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

        public bool SetName(string name)
        {
            var nname = Utils.GetStringForSave(name);
            if (!CheckNameByParent(nname))
                return false;

            _name = nname;
            return true;
        }

        public void AddChild(CBaseElement inElement)
        {
            if (inElement.IsKey())
                _keys.Add(inElement as CKey);
            else
                _values.Add(inElement as CValue);
        }

        public void RemoveChild(CBaseElement inElement)
        {
            if (inElement.IsKey())
                _keys.Remove(inElement as CKey);
            else
                _values.Remove(inElement as CValue);
        }

        internal void AddTokenTail(CTokenLine line, ILogger inLoger)
        {
            if (line.Comments != null)
                AddComments(line.Comments.Text);

            if (line.IsTailEmpty)
                return;

            for (int i = 0; i < line.TailLength; i++)
            {
                CToken t = line.Tail[i];

                CBaseElement be = CValue.CreateBaseValue(this, t);

                if (be == null)
                    inLoger.LogError(EErrorCode.WrongTokenInTail, t);
            }
        }

        public void ClearAllArrayKeys()
        {
            for (int i = _keys.Count - 1; i >= 0; i--)
                if (_keys[i].IsArray)
                    _keys.RemoveAt(i);
        }

        public int GetIndex()
        {
            if (_parent == null)
                return 0;
            for (int i = 0; i < _parent.KeyCount; i++)
            {
                if (_parent.GetKey(i) == this)
                    return i;
            }
            return -1;
        }

        #region IKey

        public IKey CreateCopy()
        {
            return CreateCopy(this);
        }

        bool CheckNameByParent(string inName)
        {
            if (Parent == null)
                return true;

            for (int i = 0; i < Parent._keys.Count; i++)
            {
                IKey k = Parent._keys[i];
                if (string.Equals(k.GetName(), inName, StringComparison.InvariantCulture))
                    return false;
            }
            return true;
        }

        bool CheckNameForChild(string inName)
        {
            for (int i = 0; i < _keys.Count; i++)
            {
                IKey k = _keys[i];
                if (string.Equals(k.GetName(), inName, StringComparison.InvariantCulture))
                    return false;
            }
            return true;
        }

        public IKey CreateChildKey(string inName)
        {
            string name = Utils.GetStringForSave(inName);
            if (!CheckNameForChild(name))
                return null;

            CKey child = CreateChild(this, name);
            return child;
        }

        public IKey CreateArrayKey()
        {
            CKey child = CreateArrayKey(this);
            return child;
        }

        EKeyOpResult IsKeyAddable(IKey inNewChild)
        {
            if (!(inNewChild is CKey))
                return EKeyOpResult.UnnativeKey;

            for (int i = 0; i < _keys.Count; i++)
            {
                IKey k = _keys[i];
                if (k == inNewChild)
                    return EKeyOpResult.AlreadyPresent;

                if (string.Equals(k.GetName(), inNewChild.GetName(), StringComparison.InvariantCulture))
                    return EKeyOpResult.DublicateName;
            }

            return EKeyOpResult.OK;
        }

        public EKeyOpResult AddChild(IKey inNewChild)
        {
            EKeyOpResult res = IsKeyAddable(inNewChild);
            if (res != EKeyOpResult.OK)
                return res;

            CKey k = inNewChild as CKey;
            k.SetParent(this);
            return res;
        }

        public EKeyOpResult RemoveChild(IKey inChild)
        {
            CKey k = inChild as CKey;
            if (k == null)
                return EKeyOpResult.UnnativeKey;

            if (!_keys.Contains(k))
                return EKeyOpResult.NotFound;

            k.SetParent(null);
            return EKeyOpResult.OK;
        }

        public EKeyOpResult InsertChild(int inIndexPos, IKey inChild)
        {
            EKeyOpResult res = IsKeyAddable(inChild);
            if (res != EKeyOpResult.OK)
                return res;

            CKey k = inChild as CKey;
            k.SetParent(this);

            _keys.RemoveAt(_keys.Count - 1);
            _keys.Insert(inIndexPos, k);

            return EKeyOpResult.OK;
        }

        #region quicksort
        static void Swap(List<CKey> keys, int left, int right)
        {
            if (left != right)
            {
                CKey temp = keys[left];
                keys[left] = keys[right];
                keys[right] = temp;
            }
        }

        static private int partition(List<CKey> keys, int left, int right, int pivotIndex, Comparison<IKey> comparison)
        {
            CKey pivotValue = keys[pivotIndex];

            Swap(keys, pivotIndex, right);

            int storeIndex = left;

            for (int i = left; i < right; i++)
            {
                if (comparison(keys[i], pivotValue) < 0)
                {
                    Swap(keys, i, storeIndex);
                    storeIndex += 1;
                }
            }

            Swap(keys, storeIndex, right);
            return storeIndex;
        }

        static Random _pivotRng = new Random();
        static private void quicksort(List<CKey> keys, int left, int right, Comparison<IKey> comparison)
        {
            if (left < right)
            {
                int pivotIndex = _pivotRng.Next(left, right);

                int newPivot = partition(keys, left, right, pivotIndex, comparison);
                quicksort(keys, left, newPivot - 1, comparison);
                quicksort(keys, newPivot + 1, right, comparison);
            }
        }
        #endregion quicksort

        public void SortKeys(Comparison<IKey> comparison)
        {
            quicksort(_keys, 0, _keys.Count - 1, comparison);
        }

        public bool UpInParent()
        {
            if (Parent == null)
                return false;

            int ind = Parent._keys.IndexOf(this);
            if (ind == -1)
                return false;

            if (ind == 0)
                return false;

            Swap(Parent._keys, ind - 1, ind);
            return true;
        }

        public bool DownInParent()
        {
            if (Parent == null)
                return false;

            int ind = Parent._keys.IndexOf(this);
            if (ind == -1)
                return false;

            if (ind == Parent._keys.Count - 1)
                return false;

            Swap(Parent._keys, ind, ind + 1);
            return true;
        }

        public EKeyOpResult SwapChild(IKey inChild1, IKey inChild2)
        {
            int ind1 = _keys.IndexOf(inChild1 as CKey);
            if (ind1 == -1)
                return EKeyOpResult.NotFound;

            int ind2 = _keys.IndexOf(inChild2 as CKey);
            if (ind2 == -1)
                return EKeyOpResult.NotFound;

            Swap(_keys, ind1, ind2);

            return EKeyOpResult.OK;
        }

        public int GetIndexInParent()
        {
            if (Parent == null)
                return -1;

            int ind = Parent._keys.IndexOf(this);
            return ind;
        }

        public IKeyValue AddValue(bool v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(byte v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(short v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(ushort v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(int v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(uint v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(long v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(ulong v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(float v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(double v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(DateTime v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(TimeSpan v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(string v) { return new CValue(this, new Variant(v)); }
        public IKeyValue AddValue(Variant v) { return new CValue(this, v); }

        public void RemoveValueAt(int index)
        {
            _values.RemoveAt(index);
        }

        public void ClearValues()
        {
            _values.Clear();
        }

        public string GetName() { return Name; }
        public bool IsArrayKey() { return IsArray; }

        public int GetChildCount() { return _keys.Count; }
        public IKey GetChild(int index) { return _keys[index]; }
        public IKey GetChild(string name) { return FindChildKey(name); }

        public int GetValuesCount() { return _values.Count; }

        public IKeyValue GetValue(int index) { return _values[index]; }

        public string GetValueAsString(int index) { return _values[index].ToString(); }

        public int GetMemorySize()
        {
            int res = BinarySerializeUtils.GetStringMemorySize(Name);

            res += 1; //IsArray

            //2 байта - количество значений
            res += 2;
            //значения: 
            for (int i = 0; i < _values.Count; i++)
            {
                IKeyValue val = _values[i];
                res += val.GetMemorySize();
            }

            //2 байта - количество подключей
            res += 2;
            //значения: 
            for (int i = 0; i < _keys.Count; i++)
            {
                res += _keys[i].GetMemorySize();
            }

            return res;
        }

        public int AddValue(byte[] ioBuffer, int inOffset) 
        { 
            new CValue(this, ioBuffer, ref inOffset);
            return inOffset;
        }

        public int BinaryDeserialize(byte[] ioBuffer, int inOffset)
        {
            int offset = inOffset;

            offset = BinarySerializeUtils.Deserialize(ioBuffer, offset, out _name);

            offset = BinarySerializeUtils.Deserialize(ioBuffer, offset, out bool is_array);
            IsArray = is_array;

            offset = BinarySerializeUtils.Deserialize(ioBuffer, offset, out short size);

            _values.Clear();
            for (int i = 0; i < size; i++)
                offset = AddValue(ioBuffer, offset);

            offset = BinarySerializeUtils.Deserialize(ioBuffer, offset, out size);
            _keys.Clear();

            for (int i = 0; i < size; i++)
            {
                CKey child = CreateChild(this, string.Empty);
                offset = child.BinaryDeserialize(ioBuffer, offset);
            }

            return offset;
        }

        public int BinarySerialize(byte[] ioBuffer, int inOffset)
        {
            int offset = inOffset;

            offset = BinarySerializeUtils.Serialize(_name, ioBuffer, offset);

            offset = BinarySerializeUtils.Serialize(IsArray, ioBuffer, offset);

            if (_values.Count > ushort.MaxValue)
                throw new ArgumentException($"Key {GetPath()} has too many values (>{ushort.MaxValue})!");

            offset = BinarySerializeUtils.Serialize((ushort)_values.Count, ioBuffer, offset);

            for (int i = 0; i < _values.Count; i++)
            {
                IKeyValue val = _values[i];
                offset = val.BinarySerialize(ioBuffer, offset);
            }

            if (_keys.Count > ushort.MaxValue)
                throw new ArgumentException($"Key {GetPath()} has too many child keys (>{ushort.MaxValue})!");

            offset = BinarySerializeUtils.Serialize((ushort)_keys.Count, ioBuffer, offset);

            //значения: 
            for (int i = 0; i < _keys.Count; i++)
            {
                IKey key = _keys[i];
                offset = key.BinarySerialize(ioBuffer, offset);
            }

            return offset;
        }

        #endregion IKey

        internal void MergeKey(CKey inKey)
        {
            AddComments(inKey.Comments);

            TakeAllValues(inKey, false);

            List<CKey> keys = new List<CKey>(inKey._keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var in_sub_key = keys[i];

                CKey child_key = null;

                if(!in_sub_key.IsArray)
                    child_key = FindChildKey(in_sub_key.Name);

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

        public string GetPath()
        {
            if (_parent == null)
                return Name;
            return string.Format("{0}\\{1}", _parent.GetPath(), Name);
        }

        public IKey FindKey(string key_path)
        {
            if (string.IsNullOrEmpty(key_path))
                return null;

            string[] path = key_path.Split(new char[] { '\\', '/' });
            return FindKey(path);
        }

        public CKey FindKey(IList<string> path, int index = 0)
        {
            string need_name = path[index];

            CKey key = FindChildKey(need_name);
            if (key == null)
                return null;

            if (index == path.Count - 1)
                return key;
            return key.FindKey(path, index + 1);
        }

        void TakeAllValues(CKey other_key, bool inClear)
        {
            List<CValue> lst = new List<CValue>(other_key._values);

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

            if (_keys[0].ValuesCount > 0)
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

            int intent = 0;
            if (_parent == null && string.IsNullOrEmpty(_name) && IsAllSubKeysArrays())
                intent = -1;

            SaveToString(sb, intent);
            return sb.ToString();
        }

        public string SaveChildsToString()
        {
            StringBuilder sb = new StringBuilder();
            SaveChildToString(sb, 0);
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

        int GetCommonLengthStringsValuesForSave()
        {
            int len = 0;
            for (int i = 0; i < _values.Count; ++i)
                len += _values[i].GetStringForSave().Length;
            return len;
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

        public bool IsValuesPresentRecursive()
        {
            if (_values.Count > 0)
                return true;

            for (int i = 0; i < _keys.Count; ++i)
            {
                if (_keys[i].IsValuesPresentRecursive())
                    return true;
            }
            return false;
        }

        public bool IsAllSubKeysArrays()
        {
            for (int i = 0; i < _keys.Count; ++i)
            {
                if (!_keys[i].IsArray)
                    return false;
            }
            return true;
        }

        protected void SaveChildToString(StringBuilder sb, int intent)
        {
            for (int i = 0; i < _keys.Count; ++i)
            {
                if (_keys[i].IsArray &&
                    (i > 0 || _keys.Count == 1) &&
                    _keys[i].KeyCount > 0)
                {
                    AppendIntent(sb, intent);
                    string rd_str = CTokenFinder.Instance.GetTokenString(ETokenType.RecordDivider);
                    sb.Append(rd_str);
                    sb.Append(Environment.NewLine);
                }
                _keys[i].SaveToString(sb, intent);
            }
        }

        protected void SaveToString(StringBuilder sb, int intent)
        {
            if (!IsValuesPresentRecursive())
                return;

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

                if (!string.IsNullOrEmpty(Name) || _values.Count > 0)
                {
                    AppendIntent(sb, intent);
                    sb.AppendFormat("{0}: ", Name);
                    AddStringValuesForSave(sb);
                    was_writing = true;
                }

                new_int = intent + 1;
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
                    //IsNewArrayLine
                    //if(_values.Count == 1)
                    //    sb.AppendFormat("{0} ", CTokenFinder.Instance.GetTokenString(ETokenType.Colon));
                    AddStringValuesForSave(sb);

                    was_writing = true;
                }
                else if(IsAllSubKeysArrays())
                    new_int = intent + 1;
            }

            if (was_writing)
                sb.Append(Environment.NewLine);

            SaveChildToString(sb, new_int);
        }

        internal void GetTerminalPathes(List<List<string>> outPathes, List<string> ioPath)
        {
            ioPath.Add(Name);

            if (_keys.Count == 0)
                outPathes.Add(ioPath);
            else
            {
                List<string>[] pathes = new List<string>[_keys.Count];
                for (int i = 0; i < _keys.Count; i++)
                {
                    List<string> path = ioPath;
                    if (i != 0)
                        path = new List<string>(ioPath);
                    pathes[i] = path;
                }
                for (int i = 0; i < _keys.Count; i++)
                {
                    List<string> path = pathes[i];
                    _keys[i].GetTerminalPathes(outPathes, path);
                }
            }
        }

        internal void ChangeValues(IDictionary<string, string> dictionary)
        {
            for (int i = 0; i < _values.Count; ++i)
            {
                CValue old_val = _values[i];
                string val = old_val.ToString();
                string new_val;
                if (dictionary.TryGetValue(val, out new_val))
                {
                    ETokenType tt = Utils.GetTokenType(new_val);
                    var t = new CToken(tt, new_val, old_val.Position);

                    CValue be = CValue.CreateBaseValue(this, t); //_values + 1 to end
                    _values[i] = _values[_values.Count - 1]; //insert to need pos

                    old_val.SetParent(null); //_values count same
                    _values.RemoveAt(_values.Count - 1); //_values - 1
                }
            }

            for (int i = 0; i < _keys.Count; ++i)
                _keys[i].ChangeValues(dictionary);
        }

        
    }
}