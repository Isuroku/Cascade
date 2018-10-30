using CascadeParser;
using ReflectionSerializer;
using System;
using System.Collections.Generic;

#pragma warning disable 0649, 0659

namespace CascadeUnitTest
{
    public enum ETestEnum { TestEnumValue1, TestEnumValue2, TestEnumValue3 }

    [CascadeObject(MemberSerialization.All)]
    public class CTestBase
    {
        private float _base_float;
        private string _base_string;
        private ETestEnum _base_enum;

        public float BaseInt { get; set; }
        public double? BaseNullable { get; set; }

        public virtual void Init1()
        {
            _base_float = 101.909f;
            _base_string = "str2";
            _base_enum = ETestEnum.TestEnumValue3;
            BaseInt = 7;
            BaseNullable = 9;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CTestBase;
            return _base_enum == v._base_enum &&
                _base_float == v._base_float &&
                _base_string == v._base_string &&
                BaseInt == v.BaseInt &&
                BaseNullable == v.BaseNullable;
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

        public object ReadKey(IKey key, ILogPrinter inLogger)
        {
            Vector3 v = new Vector3();

            v.x = key.GetValue(0).GetValueAsFloat();
            v.y = key.GetValue(1).GetValueAsFloat();
            v.z = key.GetValue(2).GetValueAsFloat();

            return v;
        }

        public void WriteKey(IKey key, object instance, ILogPrinter inLogger)
        {
            Vector3 v = (Vector3)instance;
            key.AddValue(v.x);
            key.AddValue(v.y);
            key.AddValue(v.z);
        }
    }

    [CascadeObject(MemberSerialization.All)]
    public class CTestClass1 : CTestBase
    {
        [CascadeProperty(Default = 10)]
        private int _int;

        [CascadeIgnore]
        public int PubProp { get; set; }

        [CascadeProperty("Position")]
        [CascadeConverter(typeof(VectorConverter))]
        Vector3 _pos;

        public CTestClass1()
        {
            _int = 10;
        }

        public override void Init1()
        {
            base.Init1();
            _int = 8;
            _pos = new Vector3(2, 1, 1);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CTestClass1;

            if (!base.Equals(v))
                return false;

            return _int == v._int &&
                _pos.Equals(v._pos) &&
                PubProp == v.PubProp;
        }
    }

    public class CTestClassCollectionsAtom
    {
        public int[] _array;
        public Dictionary<string, int> _dict;
        public List<int> _list;
        public HashSet<int> _hash;

        public CTestClassCollectionsAtom()
        {
            _array = new int[] { };
            _dict = new Dictionary<string, int>();
            _list = new List<int>();
            _hash = new HashSet<int>();
        }

        public void Init1()
        {

            _array = new int[] { 1, 2, 3, 4, 5, 6 };
            _dict.Add("dickey1", 1);
            _dict.Add("dickey2", 2);
            _list.Add(1);
            _list.Add(2);
            _hash.Add(1);
            _hash.Add(2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CTestClassCollectionsAtom;

            return Utils.IsCollectionEquals(_array, v._array) &&
                Utils.IsCollectionEquals(_dict, v._dict) &&
                Utils.IsHashSetEquals(_hash, v._hash) &&
                Utils.IsCollectionEquals(_list, v._list);
        }
    }

    public class CTestClassMultiArrayAtom
    {
        public int[,,] _multi_array_r3;
        public int[,] _multi_array;

        public void Init1()
        {
            _multi_array_r3 = new int[3, 2, 2] 
            { 
                { { 1, 11 }, { 2, 22 } }, 
                { { 3, 33 }, { 4, 44 } }, 
                { { 5, 55 }, { 6, 66 } }
            };

            _multi_array = new int[3, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 } };
        }

        public void Init2()
        {
            //_multi_array = new int[,] { { } };
            //_array = new int[] { };
            _multi_array_r3 = new int[1, 2, 1] { { { 1 }, { 2 } } };
            _multi_array = new int[2, 1] { { 1 }, { 2 } };
        }

        public void Init3()
        {
            //_multi_array = new int[,] { { } };
            //_array = new int[] { };
            _multi_array_r3 = new int[1, 1, 1] { { { 1 } } };
            _multi_array = new int[1, 1] { { 1 } };
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CTestClassMultiArrayAtom;

            return Utils.IsArrayEquals(_multi_array, v._multi_array) &&
                Utils.IsArrayEquals(_multi_array_r3, v._multi_array_r3);
        }
    }

    public class CSimpleClass
    {
        public int _int_data = 9;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CSimpleClass;

            return _int_data == v._int_data;
        }
    }

    public class CTestClassCollectionsObject
    {
        public CSimpleClass[] _array;
        public Dictionary<string, CSimpleClass> _dict;
        public List<CSimpleClass> _list;

        public void Init1()
        {
            var to = new CSimpleClass();
            to._int_data = 10;
            _array = new CSimpleClass[] { to, to };
            _dict = new Dictionary<string, CSimpleClass>();
            _dict.Add("dickey1", to);
            _dict.Add("dickey2", to);
            _list = new List<CSimpleClass>();
            _list.Add(to);
            _list.Add(to);
        }

        public void Init2()
        {
            //_array = new int[] { };
            var to = new CSimpleClass();
            to._int_data = 10;
            _array = new CSimpleClass[] { to };
            _dict = new Dictionary<string, CSimpleClass>();
            _dict.Add("dickey1", to);
            _list = new List<CSimpleClass>();
            _list.Add(to);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CTestClassCollectionsObject;

            return Utils.IsArrayEquals(_array, v._array) &&
                Utils.IsCollectionEquals(_dict, v._dict) &&
                Utils.IsCollectionEquals(_list, v._list);
        }
    }

    public class CTestClassMAObject
    {
        public CSimpleClass[,,] _multi_array_r3;
        public CSimpleClass[,] _multi_array;

        public void Init1()
        {
            var to = new CSimpleClass();
            to._int_data = 10;
            Console.WriteLine("3, 2, 2");
            _multi_array_r3 = new CSimpleClass[3, 2, 2] { { { to, to }, { to, to } }, { { to, to }, { to, to } }, { { to, to }, { to, to } } };
            Console.WriteLine("3, 2");
            _multi_array = new CSimpleClass[3, 2] { { to, to }, { to, to }, { to, to } };
        }

        public void Init2()
        {
            //_multi_array = new int[,] { { } };
            //_array = new int[] { };
            var to = new CSimpleClass();
            to._int_data = 10;
            Console.WriteLine("1, 1, 1");
            _multi_array_r3 = new CSimpleClass[1, 1, 1] { { { to } } };
            Console.WriteLine("1, 1");
            _multi_array = new CSimpleClass[1, 1] { { to } };
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CTestClassMAObject;

            return Utils.IsArrayEquals(_multi_array_r3, v._multi_array_r3) &&
                Utils.IsArrayEquals(_multi_array, v._multi_array);
        }
    }

    public class CTestPolimorf
    {
        public object _polimorf;

        public void Init1()
        {
            _polimorf = new CTestBase();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CTestPolimorf;

            return _polimorf.Equals(v._polimorf);
        }
    }

    public class CTestNullable
    {
        public double? Nullable { get; set; }

        public void Init1()
        {
            Nullable = 9;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CTestNullable;

            return Nullable.Equals(v.Nullable);
        }
    }

    public class CAIActionDescrs : Dictionary<string, string>
    {
        public void Init1()
        {
            Add("aikey", "aivalue");
        }

        public void Init2()
        {
            Add("aikey1", "aivalue1");
            Add("aikey2", "aivalue1");
        }

        public static CAIActionDescrs CreateTestObject()
        {
            CAIActionDescrs obj = new CAIActionDescrs();
            obj.Add("aikey", "aivalue");
            return obj;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CAIActionDescrs;

            return Utils.IsCollectionEquals(this, v);
        }
    }

    public class CDicInheriteWithField : Dictionary<string, string>
    {
        public int _some_int;

        public void Init1()
        {
            Add("aikey", "aivalue");
            _some_int = 99;
        }

        public void Init2()
        {
            Add("aikey1", "aivalue1");
            Add("aikey2", "aivalue1");
            _some_int = 99;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CDicInheriteWithField;

            return _some_int.Equals(v._some_int) &&
                Utils.IsCollectionEquals(this, v);
        }
    }

    public class CDicInherite : Dictionary<int, CSimpleClass>
    {
        public int _some_int;

        public void Init1()
        {
            Add(1, new CSimpleClass());
            _some_int = 99;
        }

        public void Init2()
        {
            Add(1, new CSimpleClass());
            Add(2, new CSimpleClass());
            _some_int = 99;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CDicInherite;

            return _some_int.Equals(v._some_int) &&
                Utils.IsCollectionEquals(this, v);
        }
    }

    public class CListInheriteTest : List<int>
    {
        public int _some_int;

        public void Init1()
        {
            Add(222);
            _some_int = 99;
        }

        public void Init2()
        {
            Add(222);
            Add(333);
            _some_int = 99;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CListInheriteTest;

            return _some_int.Equals(v._some_int) &&
                Utils.IsCollectionEquals(this, v);
        }
    }

    public class CListInheriteTestObj : List<CSimpleClass>
    {
        public int _some_int;

        public void Init1()
        {
            Add(new CSimpleClass());
            _some_int = 99;
        }

        public void Init2()
        {
            Add(new CSimpleClass());
            Add(new CSimpleClass());
            _some_int = 99;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CListInheriteTestObj;

            return _some_int.Equals(v._some_int) &&
                Utils.IsCollectionEquals(this, v);
        }
    }

    public class CClassWithoutDefCtor
    {
        public int _int_data = 9;

        public CClassWithoutDefCtor(int v)
        {
            _int_data = v;
        }
    }

    public class CShipUpgradeDescr2 : CCommonDescr<string>
    {
        public static string GetDataPath() { return "World/Ships/Upgrades/"; }
        public static string GetFilePatternCSCD() { return "*.cscd"; }

        public string RequireSlot { get; private set; }
        public string[] AddSlots { get; private set; }

        public CShipUpgradeDescr2()
        {
            AddSlots = new string[0];
        }

        public CShipUpgradeDescr2(params string[] inp): base(inp[0])
        {
            RequireSlot = inp[0];
            AddSlots = inp;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CShipUpgradeDescr2;

            return Equals(RequireSlot, v.RequireSlot) &&
                Equals(Name, v.Name) &&
                Utils.IsArrayEquals(AddSlots, v.AddSlots);
        }
    }
}
