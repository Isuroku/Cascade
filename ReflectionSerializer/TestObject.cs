
using System.Collections.Generic;

namespace ReflectionSerializer
{
    public enum EntityKind { None, Fooish, Barish }
    public struct SomeStruct { public int A, B; }

    [JsonObject(MemberSerialization.All)]
    public class TestObject
    {
        #region Atoms

        public string AtomicField;
        public float AtomicProperty { get; set; }

        public double? Nullable { get; set; }
        public EntityKind Enumeration { get; set; }

        #endregion

        #region Aggregates

        public TestObject AggregateClass { get; set; }
        public SomeStruct AggregateStruct { get; set; }
        public object Polymorphic { get; set; }

        //[Serialization(Required = true)]
        //public TestObject Required { get; set; }

        //[DataMember(Name = "AnotherName")]
        [JsonProperty("AnotherName")]
        public TestObject Renamed { get; set; }

        #endregion

        #region Collections

        public List<string> AtomicList { get; set; }
        public List<TestObject> AggregateList { get; set; }
        public string[] AtomicArray { get; set; }
        public TestObject[] AggregateArray { get; set; }
        public HashSet<string> AtomicSet { get; set; }
        public HashSet<TestObject> AggregateSet { get; set; }
        public Dictionary<string, int> AtomicDictionary { get; set; }
        public Dictionary<string, TestObject> HybridDictionary { get; set; }

        [JsonIgnore]
        public Dictionary<TestObject, TestObject> AggregateDictionary { get; set; }

        #endregion

        #region Non-Serializable Members

        int PrivateField;
        protected int ProtectedField;
        internal int InternalField;

        [DataMember(Ignore = true)]
        public int Ignored { get; set; }

        public int NonWritableAuto { get; private set; }
        public int NonReadableAuto { private get; set; }

        int nonReadable;
        public int NonReadable
        {
            set { nonReadable = value; }
        }

        int nonWritable;
        public int NonWritable
        {
            get { return nonReadable; }
        }

        readonly Dictionary<int, string> indexer = new Dictionary<int, string>();
        public string this[int index]
        {
            get { return indexer[index]; }
            set { indexer[index] = value; }
        }

        #endregion

        public static TestObject CreateTestObject()
        {
            var entity = new TestObject
            {
                AggregateList = new List<TestObject> { new TestObject { AtomicField = "1" }, new TestObject { AtomicField = "2" } },
                AggregateClass = new TestObject(),
                AggregateStruct = new SomeStruct { A = 1, B = 2 },
                AggregateArray = new[] { new TestObject { AtomicField = "1" }, new TestObject { AtomicField = "2" } },
                AggregateDictionary = new Dictionary<TestObject, TestObject>
                {
                    { new TestObject { AtomicField = "Key1" }, new TestObject { AtomicField = "Value1" } },
                    { new TestObject { AtomicField = "Key2" }, new TestObject { AtomicField = "Value2" } }
                },
                AggregateSet = new HashSet<TestObject> { new TestObject { AtomicField = "1" }, new TestObject { AtomicField = "2" } },
                AtomicArray = new[] { "1", "2" },
                AtomicDictionary = new Dictionary<string, int> { { "Key1", 1 }, { "Key2", 2 } },
                AtomicField = "Bar",
                AtomicList = new List<string> { "1", "2" },
                AtomicProperty = 13.37f,
                AtomicSet = new HashSet<string> { "1", "2" },
                Enumeration = EntityKind.Fooish,
                HybridDictionary = new Dictionary<string, TestObject>
                {
                    { "Key1", new TestObject { AtomicField = "Value1" } },
                    { "Key2", new TestObject { AtomicField = "Value2" } }
                },
                Ignored = 1,
                NonReadableAuto = 2,
                NonReadable = 4,
                Nullable = 6,
                InternalField = 8,
                //Required = new TestObject(),
                Renamed = new TestObject(),
                Polymorphic = new TestObject()
            };

            return entity;
        }
    }
}
