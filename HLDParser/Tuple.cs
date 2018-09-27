using System;

namespace CascadeParser
{
    public struct Tuple<T1, T2>
    {
        private readonly T1 _item1;
        private readonly T2 _item2;

        public Tuple(T1 item1, T2 item2) { _item1 = item1; _item2 = item2; }

        public T1 Item1 { get { return _item1; } }
        public T2 Item2 { get { return _item2; } }

        public override string ToString()
        {
            return string.Format("{0}:{1}", _item1, _item2);
        }
    }

    public struct Tuple<T1, T2, T3>
    {
        private readonly T1 _item1;
        private readonly T2 _item2;
        private readonly T3 _item3;

        public Tuple(T1 item1, T2 item2, T3 item3) { _item1 = item1; _item2 = item2; _item3 = item3; }

        public T1 Item1 { get { return _item1; } }
        public T2 Item2 { get { return _item2; } }
        public T3 Item3 { get { return _item3; } }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", _item1, _item2, _item3);
        }
    }

    public struct Tuple<T1, T2, T3, T4>
    {
        private readonly T1 _item1;
        private readonly T2 _item2;
        private readonly T3 _item3;
        private readonly T4 _item4;

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4) { _item1 = item1; _item2 = item2; _item3 = item3; _item4 = item4; }

        public T1 Item1 { get { return _item1; } }
        public T2 Item2 { get { return _item2; } }
        public T3 Item3 { get { return _item3; } }
        public T4 Item4 { get { return _item4; } }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}:{3}", _item1, _item2, _item3, _item4);
        }
    }

    [Serializable]
    public class COpenTuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public COpenTuple(T1 item1, T2 item2) { Item1 = item1; Item2 = item2; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Item1, Item2);
        }
    }

    [Serializable]
    public class COpenTuple<T1, T2, T3>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;

        public COpenTuple(T1 item1, T2 item2, T3 item3) { Item1 = item1; Item2 = item2; Item3 = item3; }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", Item1, Item2, Item3);
        }
    }


}
