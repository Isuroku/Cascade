using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace CascadeUnitTest
{
    [TestClass]
    public class UnitTestArrays : BaseUnitTest
    {
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
        public void TestMethodAAAtom1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClassArrayArraysAtom();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassArrayArraysAtom>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodAAObject1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CTestClassAAObject();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, string.Empty, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CTestClassAAObject>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }
    }
}
