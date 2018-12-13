using System;
using System.Collections.Generic;
using System.Reflection;
using CascadeParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CascadeSerializer;

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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassCollectionsAtom>(text, this);

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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassCollectionsObject>(text, this);

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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CMoverDescrOwner>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodSerArrayObjects()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            CSimpleClass[] v1 = new CSimpleClass[]
            {
                new CSimpleClass(),
                //new CSimpleClass()
            };

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CSimpleClass[]>(text, this);

            CheckInternalErrors();
            Assert.IsTrue(Utils.IsArrayEquals(v1, v2));
        }

        [TestMethod]
        public void TestMethodWithouDefCtor()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            CClassWithoutDefCtor v1 = new CClassWithoutDefCtor(3);

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CClassWithoutDefCtor>(text, this);
        }

        [TestMethod]
        public void TestMethodShipUpgradeDescr2()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            CShipUpgradeDescr2 v1 = new CShipUpgradeDescr2("one", "two");

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CShipUpgradeDescr2>(text, this);
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodEnumFlag()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            CShipUpgradeDescr2 v1 = new CShipUpgradeDescr2("one", "two");

            EEntityPlaces pl = EEntityPlaces.Front | EEntityPlaces.Left;

            string text = pl.ToString();
            Console.WriteLine(text);

            EEntityPlaces p2 = (EEntityPlaces)Enum.Parse(typeof(EEntityPlaces), text);
            Console.WriteLine(p2);

            Dictionary<EEntityPlaces, int> dic = new Dictionary<EEntityPlaces, int>();
            dic.Add(pl, 9);

            text = _serializer.SerializeToCascade(dic, this);
            Console.WriteLine(text);
            var dic2 = _serializer.Deserialize<Dictionary<EEntityPlaces, int>>(text, this);
            Utils.IsCollectionEquals(dic, dic2);
        }

        [TestMethod]
        public void TestMethod_SerializationInt()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            int v1 = 9;
            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<int>(text, this);
            Assert.AreEqual(v1, v2);

            CheckInternalErrors();
        }

        [TestMethod]
        public void TestMethod_SerializationArrayInt()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            int[] v1 = new int[] { 9, 7 };
            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<int[]>(text, this);
            Utils.IsCollectionEquals(v1, v2);

            CheckInternalErrors();
        }

        [TestMethod]
        public void TestMethod_SerializationDictionary()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            Dictionary<int, int> v1 = new Dictionary<int, int>();
            v1.Add(1, 11);
            v1.Add(2, 22);
            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<Dictionary<int, int>>(text, this);
            Utils.IsCollectionEquals(v1, v2);

            CheckInternalErrors();
        }

        [TestMethod]
        public void TestMethod_SerializationList()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            List<int> v1 = new List<int> { 9, 7 };
            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<List<int>>(text, this);
            Utils.IsCollectionEquals(v1, v2);

            CheckInternalErrors();
        }

        [TestMethod]
        public void TestMethod_SerializationMethods()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            var v1 = new Vector3(1, 2, 3);
            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<Vector3>(text, this);
            Assert.AreEqual(v1, v2);

            CheckInternalErrors();
        }
    }
}
