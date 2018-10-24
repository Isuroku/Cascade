using ReflectionSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CascadeUnitTest
{
    [CascadeObject(MemberSerialization.Fields)]
    public class CEnumDictionaryTest
    {
        private EnumDictionary<ETestEnum> _params;
        public EnumDictionary<ETestEnum> Params
        {
            get
            {
                if (_params == null)
                    _params = new EnumDictionary<ETestEnum>();
                return _params;
            }
        }
    }

    [CascadeObject(MemberSerialization.OptIn)]
    public class EnumDictionary<T> : Dictionary<string, string> where T : struct, IConvertible
    {
        public bool ContainsParam(T inParam)
        {
            return ContainsKey(GetParamName(inParam));
        }

        [NonSerialized]
        private Dictionary<T, string> _name_cache;

        private string GetParamName(T inParam)
        {
            if (_name_cache == null)
                _name_cache = new Dictionary<T, string>();

            if (!_name_cache.ContainsKey(inParam))
                _name_cache.Add(inParam, Convert.ToString(inParam));

            return _name_cache[inParam];
        }

        [NonSerialized]
        private Dictionary<T, Int32> _int_cache;
        [NonSerialized]
        private Dictionary<T, string> _string_cache;
        [NonSerialized]
        private Dictionary<T, float> _float_cache;
        [NonSerialized]
        private Dictionary<T, bool> _bool_cache;

        //public IEnumerable<T> GetParams()
        //{
        //    List<T> lst = new List<T>();
        //    foreach (string p in Keys)
        //        lst.Add(EnumUtils.ToEnum<T>(p));
        //    return lst;
        //}

        //public string GetParam(T inParam, string inDefault, CLogContext inLogContext)
        //{
        //    if (_string_cache == null)
        //        _string_cache = new Dictionary<T, string>();
        //    if (_string_cache.ContainsKey(inParam))
        //        return _string_cache[inParam];
        //    string val = this.GetValue(GetParamName(inParam), inDefault, inLogContext);
        //    _string_cache.Add(inParam, val);
        //    return val;
        //}

        //public bool GetParam(T inParam, bool inDefault, CLogContext inLogContext)
        //{
        //    if (_bool_cache == null)
        //        _bool_cache = new Dictionary<T, bool>();
        //    if (_bool_cache.ContainsKey(inParam))
        //        return _bool_cache[inParam];
        //    bool val = this.GetValue(GetParamName(inParam), inDefault, inLogContext);
        //    _bool_cache.Add(inParam, val);
        //    return val;
        //}

        //public float GetParam(T inParam, float inDefault, CLogContext inLogContext)
        //{
        //    if (_float_cache == null)
        //        _float_cache = new Dictionary<T, float>();
        //    if (_float_cache.ContainsKey(inParam))
        //        return _float_cache[inParam];
        //    float val = this.GetValue(GetParamName(inParam), inDefault, inLogContext);
        //    _float_cache.Add(inParam, val);
        //    return val;
        //}

        //public int GetParam(T inParam, int inDefault, CLogContext inLogContext)
        //{
        //    if (_int_cache == null)
        //        _int_cache = new Dictionary<T, int>();
        //    if (_int_cache.ContainsKey(inParam))
        //        return _int_cache[inParam];
        //    int val = this.GetValue(GetParamName(inParam), inDefault, inLogContext);
        //    _int_cache.Add(inParam, val);
        //    return val;
        //}

        //public T2 GetParamEnum<T2>(T inParam, T2 inDefault, CLogContext inLogContext) where T2 : struct
        //{
        //    return this.GetValueEnum<T2>(GetParamName(inParam), inDefault, inLogContext);
        //}

        public override string ToString()
        {
            if (Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var kv in this)
                    sb.AppendFormat("{0}: {1}, ", kv.Key, kv.Value);
                return sb.ToString();
            }
            return "Undefined";
        }

        public bool Add<T2>(T inParamType, T2 inValue)
        {
            string key = Convert.ToString(inParamType);
            if (ContainsKey(key))
                return false;
            Add(key, Convert.ToString(inValue, Utils.GetCultureInfoFloatPoint()));
            return true;
        }

        public bool Add(T inParamType, float inValue)
        {
            string key = Convert.ToString(inParamType, Utils.GetCultureInfoFloatPoint());
            if (ContainsKey(key))
                return false;
            Add(key, inValue.ToString());

            if (_float_cache == null)
                _float_cache = new Dictionary<T, float>();
            _float_cache.Add(inParamType, inValue);

            return true;
        }

        public void SetParam(T inParam, float inValue)
        {
            string key = GetParamName(inParam);

            if (ContainsKey(key))
                this[key] = inValue.ToString(Utils.GetCultureInfoFloatPoint());
            else
                Add(key, inValue.ToString(Utils.GetCultureInfoFloatPoint()));
        }

        public bool Add(T inParamType, int inValue)
        {
            string key = Convert.ToString(inParamType);
            if (ContainsKey(key))
                return false;
            Add(key, inValue.ToString());

            if (_int_cache == null)
                _int_cache = new Dictionary<T, int>();
            _int_cache.Add(inParamType, inValue);

            return true;
        }

        public bool Add(T inParamType, bool inValue)
        {
            string key = Convert.ToString(inParamType);
            if (ContainsKey(key))
                return false;
            Add(key, inValue.ToString());

            if (_bool_cache == null)
                _bool_cache = new Dictionary<T, bool>();
            _bool_cache.Add(inParamType, inValue);

            return true;
        }
    }
}
