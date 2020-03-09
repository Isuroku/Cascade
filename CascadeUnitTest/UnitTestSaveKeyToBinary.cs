using CascadeParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CascadeUnitTest
{
    [TestClass]
    public class UnitTestSaveKeyToBinary : BaseUnitTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            CTestBase v1 = new CTestBase();
            v1.Init1();

            Console.WriteLine("Key1:");
            IKey key = IKeyFactory.CreateKey("RootKey");
            _serializer.Serialize(v1, key, this);
            string text = key.SaveToString();

            //BaseInt: 7
            //BaseNullable: 9
            //BaseFloat: 101.909
            //BaseString: str2
            //BaseEnum: TestEnumValue3

            Console.WriteLine(text);

            //string input = "sun";
            //byte[] array = Encoding.UTF8.GetBytes(input);
            int mem_sz = key.GetMemorySize();

            byte[] array = new byte[mem_sz];

            key.BinarySerialize(array, 0);
            Console.WriteLine($"Array Size: {array.Length} bytes");

            Console.WriteLine("Key2:");
            IKey key2 = IKeyFactory.CreateKey(array, 0);

            text = key2.SaveToString();
            Console.WriteLine(text);

            var v2 = _serializer.Deserialize<CTestBase>(key2, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethod2()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            var v1 = new CTestClass1();
            v1.Init1();

            Console.WriteLine("Key1:");
            IKey key = IKeyFactory.CreateKey("RootKey");
            _serializer.Serialize(v1, key, this);
            string text = key.SaveToString();
            Console.WriteLine(text);

            int mem_sz = key.GetMemorySize();
            byte[] array = new byte[mem_sz];

            key.BinarySerialize(array, 0);
            Console.WriteLine($"Array Size: {array.Length} bytes");

            Console.WriteLine("Key2:");
            IKey key2 = IKeyFactory.CreateKey(array, 0);

            text = key2.SaveToString();
            Console.WriteLine(text);

            var v2 = _serializer.Deserialize<CTestClass1>(key2, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethod3()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            var v1 = new CTestClassCollectionsAtom();
            v1.Init1();

            Console.WriteLine("Key1:");
            IKey key = IKeyFactory.CreateKey("RootKey");
            _serializer.Serialize(v1, key, this);
            string text = key.SaveToString();
            Console.WriteLine(text);

            int mem_sz = key.GetMemorySize();
            byte[] array = new byte[mem_sz];

            key.BinarySerialize(array, 0);
            Console.WriteLine($"Array Size: {array.Length} bytes");

            Console.WriteLine("Key2:");
            IKey key2 = IKeyFactory.CreateKey(array, 0);

            text = key2.SaveToString();
            Console.WriteLine(text);

            var v2 = _serializer.Deserialize<CTestClassCollectionsAtom>(key2, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethod41()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            var v1 = new CDicInheriteWithField();
            v1.Init1();

            Console.WriteLine("Key1:");
            IKey key = IKeyFactory.CreateKey("RootKey");
            _serializer.Serialize(v1, key, this);
            string text = key.SaveToString();
            Console.WriteLine(text);

            int mem_sz = key.GetMemorySize();
            byte[] array = new byte[mem_sz];

            key.BinarySerialize(array, 0);
            Console.WriteLine($"Array Size: {array.Length} bytes");

            Console.WriteLine("Key2:");
            IKey key2 = IKeyFactory.CreateKey(array, 0);

            text = key2.SaveToString();
            Console.WriteLine(text);

            var v2 = _serializer.Deserialize<CDicInheriteWithField>(key2, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethod42()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            var v1 = new CDicInheriteWithField();
            v1.Init2();

            Console.WriteLine("Key1:");
            IKey key = IKeyFactory.CreateKey("RootKey");
            _serializer.Serialize(v1, key, this);
            string text = key.SaveToString();
            Console.WriteLine(text);

            int mem_sz = key.GetMemorySize();
            byte[] array = new byte[mem_sz];

            key.BinarySerialize(array, 0);
            Console.WriteLine($"Array Size: {array.Length} bytes");

            Console.WriteLine("Key2:");
            IKey key2 = IKeyFactory.CreateKey(array, 0);

            text = key2.SaveToString();
            Console.WriteLine(text);

            var v2 = _serializer.Deserialize<CDicInheriteWithField>(key2, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }
    }
}
