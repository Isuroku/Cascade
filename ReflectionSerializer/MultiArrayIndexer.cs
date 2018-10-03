using System;
using System.Text;

namespace ReflectionSerializer
{
    public class CMultiArrayIndexer
    {
        int[] _lengthes;
        int[] _current;

        public int[] Current { get { return _current; } }
        public int[] Lengthes { get { return _lengthes; } }
        public int LineIndex { get; private set; }

        public CMultiArrayIndexer(Array array)
        {
            _current = new int[array.Rank];
            if(_current.Length > 0)
                _current[0] = -1;

            LineIndex = -1;

            _lengthes = new int[array.Rank];
            for (int i = 0; i < _lengthes.Length; ++i)
                _lengthes[i] = array.GetLength(i);
        }

        public bool MoveNext()
        {
            LineIndex++;

            if (_current[0] == -1)
            {
                _current[0] = 0;
                return true;
            }

            for(int i = _lengthes.Length - 1; i >= 0; --i)
            {
                int curr = _current[i];
                curr++;
                if (curr >= _lengthes[i])
                    _current[i] = 0;
                else
                {
                    _current[i] = curr;
                    return true;
                }
            }
            return false;
        }

        public void Reset()
        {
            LineIndex = -1;
            for (int i = 0; i < _current.Length; ++i)
            {
                if(i == 0)
                    _current[i] = -1;
                else
                    _current[i] = 0;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _current.Length; ++i)
            {
                if(i == 0)
                    sb.Append(_current[i]);
                else
                    sb.AppendFormat(" {0}", _current[i]);
            }

            sb.AppendFormat(" [{0}]", LineIndex);
            return sb.ToString();
        }
    }
}
