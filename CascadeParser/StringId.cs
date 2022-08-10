using System;
using System.Collections.Generic;

namespace CascadeParser
{
    [Serializable]
    public readonly struct StringId
    {
        private readonly uint _id;
        public uint id { get { return _id; } }

        private readonly string _name;
        public string name { get { return _name; } }

        private StringId(uint inId, string inNames)
        {
            _id = inId;
            _name = inNames;
        }

        public StringId(string inName)
        {
            this = StringId.GetNamedId(inName);
        }

        public override string ToString()
        {
            return name ?? string.Empty;
            //return string.Format("{0} [{1}]", name, id);
        }

        public bool IsEmpty { get { return id == 0; } }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StringId)obj);
        }

        public bool Equals(StringId other)
        {
            return id == other.id;
        }

        public static bool operator ==(StringId left, StringId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StringId left, StringId right)
        {
            return !left.Equals(right);
        }

        private static Dictionary<string, StringId> _named_ids = new Dictionary<string, StringId>();
        private static Dictionary<uint, StringId> _id_ids = new Dictionary<uint, StringId>();
        private static uint _id_counter = uint.MinValue;

        public static StringId GetNamedId(string inName)
        {
            if (string.IsNullOrEmpty(inName))
                return Empty;

            string uname = inName.ToUpper();

            StringId id;

            if (_named_ids.TryGetValue(uname, out id))
                return id;

            id = new StringId(++_id_counter, inName);
            _named_ids.Add(uname, id);
            _id_ids.Add(id.id, id);

            return id;
        }

        public static StringId GetNamedId(uint inNameId)
        {
            StringId id;
            if (_id_ids.TryGetValue(inNameId, out id))
                return id;
            return Empty;
        }

        public static StringId Empty = new StringId(0, string.Empty);
        public static StringId Name = GetNamedId("Name");
        public static StringId Undefined = GetNamedId("Undefined");

        public class CNamedIdComparer : IEqualityComparer<StringId>
        {
            public bool Equals(StringId x, StringId y) { return x.id == y.id; }
            public int GetHashCode(StringId obj) { return obj.id.GetHashCode(); }
        }

        public static CNamedIdComparer NamedIdComparer = new CNamedIdComparer();

        public static StringId CreateNamedId(uint inId, string inNames)
        {
            return new StringId(inId, inNames);
        }

        public StringId(IKey key, ILogPrinter inLogger)
        {
            _id = 0;
            _name = string.Empty;

            if (key.GetValuesCount() == 0)
            {
                inLogger.LogError(string.Format("NamedId.CscdConverter: Key {0} hasnt value!", key.GetPath()));
                return;
            }
            string n = key.GetValue(0).ToString();
            StringId nid = GetNamedId(n);
            _id = nid.id;
            _name = nid.name;
        }

        public void SerializationToCscd(IKey key, ILogPrinter inLogger)
        {
            key.AddValue(name);
        }
    }
}
