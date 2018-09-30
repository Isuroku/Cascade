
using System;
using System.Collections.Generic;
using CascadeParser;
using ReflectionSerializer;

namespace Parser
{
    [CascadeObject(MemberSerialization.Fields)]
    public class CTestBase
    {
        private float _base_float;

        public CTestBase()
        {
            _base_float = 3.1415f;
        }

        public virtual void Change()
        {
            _base_float = 101.909f;
        }
    }

    struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float in_x, float in_y, float in_z) { x = in_x; y = in_y; z = in_z; }
    }

    public class VectorConverter : CascadeConverter
    {
        public bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector3);
        }

        public object ReadKey(IKey key, ILogger inLogger)
        {
            Vector3 v = new Vector3();

            v.x = key.GetValue(0).GetValueAsFloat();
            v.y = key.GetValue(1).GetValueAsFloat();
            v.z = key.GetValue(2).GetValueAsFloat();

            return v;
        }

        public void WriteKey(IKey key, object instance, ILogger inLogger)
        {
            Vector3 v = (Vector3)instance;
            key.AddValue(v.x);
            key.AddValue(v.y);
            key.AddValue(v.z);
        }
    }

    [CascadeObject(MemberSerialization.All)]
    class CTestClass : CTestBase
    {
        [CascadeProperty(Default = 10)]
        private int _int;

        [CascadeIgnore]
        public int PubProp { get; set; }

        [CascadeProperty("Position")]
        [CascadeConverter(typeof(VectorConverter))]
        Vector3 _pos;

        public CTestClass()
        {
            _int = 9;
            PubProp = 11;
        }

        public override void Change()
        {
            base.Change();
            _int = 10;
            PubProp = 888;
            _pos = new Vector3(1, 1, 1);
        }

        public static CTestClass CreateTestObject()
        {
            CTestClass obj = new CTestClass();
            obj.Change();
            return obj;
        }
    }

    public class CTestClassMA : CTestBase
    {
        public int[,] _multi_array;
        public int[] _array;
        public Dictionary<string, int> _dict;

        public static CTestClassMA CreateTestObject()
        {
            CTestClassMA obj = new CTestClassMA();
            obj._multi_array = new int[3, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 } };
            obj._array = new int[] { 1, 2, 3, 4, 5, 6 };
            obj._dict = new Dictionary<string, int>();
            obj._dict.Add("dickey1", 1);
            obj._dict.Add("dickey2", 2);
            return obj;
        }
    }

    public class CAIActionDescrs : Dictionary<string, string>
    {
        public int _some_int;

        public static CAIActionDescrs CreateTestObject()
        {
            CAIActionDescrs obj = new CAIActionDescrs();
            obj.Add("aikey", "aivalue");
            obj._some_int = 99;
            return obj;
        }
    }

    public class CListInheriteTest: List<int>
    {
        public int _some_int;

        public static CListInheriteTest CreateTestObject()
        {
            CListInheriteTest obj = new CListInheriteTest();
            obj.Add(101);
            obj._some_int = 99;
            return obj;
        }
    }

    public class CTestEnum
    {
        public enum ETestEnum { TestEnum1, TestEnum2 }
        public ETestEnum _enum_field;
        public object _polimorf;

        public static CTestEnum CreateTestObject()
        {
            CTestEnum obj = new CTestEnum();
            obj._enum_field = ETestEnum.TestEnum2;
            obj._polimorf = new CTestBase();
            return obj;
        }
    }

    public class CTestNullable
    {
        public double? Nullable { get; set; }

        public static CTestNullable CreateTestObject()
        {
            CTestNullable obj = new CTestNullable();
            obj.Nullable = 9;
            return obj;
        }
    }
}
