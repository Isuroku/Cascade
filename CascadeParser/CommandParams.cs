using System.Collections.Generic;

namespace CascadeParser
{
    internal class CCommandParams
    {
        Dictionary<string, string> _dic = new Dictionary<string, string>();
        List<KeyValuePair<string, string>> _list = new List<KeyValuePair<string, string>>();

        internal void Add(string text1, string text2)
        {
            _dic.Add(text1, text2);
            _list.Add(new KeyValuePair<string, string>(text1, text2));
        }

        public int Length { get { return _list.Count; } }

        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= _list.Count)
                    return string.Empty;

                return _list[index].Key;
            }
        }

        public string this[string key]
        {
            get
            {
                string res;
                if (_dic.TryGetValue(key, out res))
                    return res;
                return string.Empty;
            }
        }

        internal bool ContainsKey(string v)
        {
            return _dic.ContainsKey(v);
        }

        public KeyValuePair<string, string> GetPair(int index)
        {
            if (index < 0 || index >= _list.Count)
                return new KeyValuePair<string, string>(string.Empty, string.Empty);
            return _list[index];
        }

        public IDictionary<string, string> GetDictionary()
        {
            return _dic;
        }
    }
}
