using CascadeParser;
using CascadeSerializer;
using System;
using System.Collections.Generic;

namespace CascadeUnitTest
{
    public struct NamedId
    {
        public uint id { get; private set; }
        public string name { get; private set; }

        private NamedId(uint inId, string inNames)
        {
            id = inId;
            name = inNames;
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]", name, id);
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
            return Equals((NamedId)obj);
        }

        public bool Equals(NamedId other)
        {
            return id == other.id;
        }

        public static bool operator ==(NamedId left, NamedId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NamedId left, NamedId right)
        {
            return !left.Equals(right);
        }

        public void DeserializationFromCscd(IKey key, ILogPrinter inLogger)
        {
            if (key.GetValuesCount() == 0)
            {
                inLogger.LogError(string.Format("NamedId.CscdConverter: Key {0} hasnt value!", key.GetPath()));
                return;
            }
            string n = key.GetValue(0).ToString();
            NamedId nid = GetNamedId(n);
            id = nid.id;
            name = nid.name;
        }

        public void SerializationToCscd(IKey key, ILogPrinter inLogger)
        {
            key.AddValue(name);
        }

        static Dictionary<string, NamedId> _named_ids = new Dictionary<string, NamedId>();

        static uint _id_counter;

        public static NamedId GetNamedId(string inName)
        {
            string uname = inName.ToUpper();

            NamedId id;

            if (_named_ids.TryGetValue(uname, out id))
                return id;

            _id_counter++;
            id = new NamedId(_id_counter, inName);
            _named_ids.Add(uname, id);

            return id;
        }

        public static NamedId empty = new NamedId(0, string.Empty);
        public static NamedId Name = GetNamedId("Name");
        public static NamedId Undefined = GetNamedId("Undefined");

        public class CNamedIdComparer : IEqualityComparer<NamedId>
        {
            public bool Equals(NamedId x, NamedId y) { return x.id == y.id; }
            public int GetHashCode(NamedId obj) { return obj.id.GetHashCode(); }
        }

        public static CNamedIdComparer NamedIdComparer = new CNamedIdComparer();

        public static NamedId CreateNamedId(uint inId, string inNames)
        {
            return new NamedId(inId, inNames);
        }
    }

    public class CNamedIdArray
    {
        [CascadeConverter(typeof(NamedIdArrayCscdConverter))]
        public NamedId[] Names;

        public void Init()
        {
            Names = new NamedId[]
            {
                NamedId.GetNamedId("Test1"),
                NamedId.GetNamedId("Test2"),
            };
        }

        public bool Equals(CNamedIdArray other)
        {
            return Utils.IsArrayEquals(Names, other.Names);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CNamedIdArray)obj);
        }

        public override int GetHashCode()
        {
            return 1635486599 + EqualityComparer<NamedId[]>.Default.GetHashCode(Names);
        }
    }

    public class NamedIdArrayCscdConverter : CascadeConverter
    {
        public bool CanConvert(Type objectType)
        {
            return objectType == typeof(NamedId[]);
        }

        public object ReadKey(IKey key, ILogPrinter inLogger)
        {
            int count = key.GetValuesCount();
            var arr = new NamedId[count];
            for(int i = 0; i < count; i++)
                arr[i] = NamedId.GetNamedId(key.GetValueAsString(i));
            return arr;
        }

        public void WriteKey(IKey key, object instance, ILogPrinter inLogger)
        {
            NamedId[] arr = (NamedId[])instance;
            for (int i = 0; i < arr.Length; i++)
                key.AddValue(arr[i].name);
        }
    }
}
