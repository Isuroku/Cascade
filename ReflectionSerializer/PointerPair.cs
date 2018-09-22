using System;

namespace ReflectionSerializer
{
    public struct PointerPair : IEquatable<PointerPair>
    {
        readonly IntPtr first;
        readonly IntPtr second;
        readonly int hash;

        public PointerPair(IntPtr first, IntPtr second)
        {
            this.first = first;
            this.second = second;
            // More sophisticated hashing algorithms didn't yield significantly better performance
            hash = first.GetHashCode() + second.GetHashCode();
        }

        public override int GetHashCode()
        {
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is PointerPair))
                return false;
            return Equals((PointerPair)obj);
        }
        public bool Equals(PointerPair other)
        {
            return (other.first == first && other.second == second);
        }

        public IntPtr First
        {
            get { return first; }
        }
        public IntPtr Second
        {
            get { return second; }
        }
    }

}
