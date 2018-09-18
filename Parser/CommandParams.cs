using System;
using System.Collections.Generic;

namespace Parser
{
    public class CCommandParams
    {
        Dictionary<string, string> _dic = new Dictionary<string, string>();
        List<Tuple<string, string>> _list = new List<Tuple<string, string>>();

        internal void Add(string text1, string text2)
        {
            _dic.Add(text1, text2);
            _list.Add(new Tuple<string, string>(text1, text2));
        }

        public int Length { get { return _dic.Count; } }

        public string this[int index]
        {
            get
            {
                return _list[index].Item1;
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
    }
}
