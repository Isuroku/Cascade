using System;
using System.Reflection;
using CascadeParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReflectionSerializer;

namespace CascadeUnitTest
{
    [TestClass]
    public class UnitTest1: BaseUnitTest
    {
        public UnitTest1()
        {
        }

        [TestMethod]
        public void TestMethodBase()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestBase();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestBase>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodBaseDefault()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestBase();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestBase>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethod_DefaultValue_SimpleInherite_Convert_Ignore()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClass1();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClass1>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethod_DefaultValue_SimpleInherite_Convert_Ignore_Default()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClass1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClass1>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodCollectionsAtom()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClassCollectionsAtom();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassCollectionsAtom>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodCollectionsAtomDefault()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClassCollectionsAtom();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassCollectionsAtom>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodMAAtom1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClassMultiArrayAtom();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassMultiArrayAtom>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodMAAtom2()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClassMultiArrayAtom();
            v1.Init2();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassMultiArrayAtom>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodMAAtom3()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClassMultiArrayAtom();
            v1.Init3();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassMultiArrayAtom>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodCollectionsObject1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClassCollectionsObject();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassCollectionsObject>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodCollectionsObject2()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClassCollectionsObject();
            v1.Init2();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassCollectionsObject>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodMAObject1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClassMAObject();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassMAObject>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodMAObject2()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClassMAObject();
            v1.Init2();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassMAObject>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodPolimorf()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestPolimorf();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestPolimorf>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodNullable()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestNullable();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestNullable>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodNullableDefault()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestNullable();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestNullable>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        

        [TestMethod]
        public void TestMethodCharacterDescr()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = CCharacterDescr.CreateTestObject();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CCharacterDescr>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodConditionArrayTest()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CConditionArrayTest();
            v1.Init();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CConditionArrayTest>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodAllDefaultClass()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = CMoverDescr.CreateTestObject();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CMoverDescr>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodAllDefaultClassWithOwner()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CMoverDescrOwner();
            v1.Init();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CMoverDescrOwner>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodFromText1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            CSimpleClass[] v1 = new CSimpleClass[]
            {
                new CSimpleClass(),
                //new CSimpleClass()
            };

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CSimpleClass[]>(text, this);

            CheckInternalErrors();
            Assert.IsTrue(Utils.IsArrayEquals(v1, v2));
        }

    }
}
