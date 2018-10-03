using System;
using System.Reflection;
using CascadeParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReflectionSerializer;

namespace CascadeUnitTest
{
    [TestClass]
    public class UnitTest1: IParserOwner, ILogPrinter
    {
        CParserManager _parser;
        CCascadeSerializer _serializer;

        int _error_count;

        public UnitTest1()
        {
            _parser = new CParserManager(this, this);
            _serializer = new CCascadeSerializer(_parser);
        }

        public string GetTextFromFile(string inFileName)
        {
            throw new NotImplementedException();
        }

        public void LogError(string inText)
        {
            _error_count++;
            Console.WriteLine("Error: {0}", inText);
        }

        public void LogWarning(string inText)
        {
            _error_count++;
            Console.WriteLine("Warning: {0}", inText);
        }

        public void ResetTestState()
        {
            _error_count = 0;
        }

        public void CheckInternalErrors()
        {
            Assert.IsTrue(_error_count == 0, "Internal errors were detected!");
        }

        public void Trace(string inText)
        {
            Console.WriteLine(inText);
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
        public void TestMethodDicInherite1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CAIActionDescrs();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CAIActionDescrs>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodDicInherite2()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CAIActionDescrs();
            v1.Init2();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CAIActionDescrs>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodDicInherite3()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CDicInherite();
            v1.Init2();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CDicInherite>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodListInherite1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CListInheriteTest();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CListInheriteTest>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodListInherite2()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CListInheriteTest();
            v1.Init2();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CListInheriteTest>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodListInherite3()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CListInheriteTestObj();
            v1.Init2();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CListInheriteTestObj>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

    }
}
