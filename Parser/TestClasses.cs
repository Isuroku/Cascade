
using System;
using CascadeParser;
using ReflectionSerializer;

namespace Parser
{
    class CTestBase
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

    class CTestClassMA
    {
        public int[,] _muti_array;
        public int[] _array;

        public static CTestClassMA CreateTestObject()
        {
            CTestClassMA obj = new CTestClassMA();
            obj._muti_array = new int[3, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 } };
            obj._array = new int[] { 1, 2, 3, 4, 5, 6 };
            return obj;
        }
    }
}
