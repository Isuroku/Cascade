﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace CascadeUnitTest
{
    [TestClass]
    public class UnitTestCollectionsInharitance : BaseUnitTest
    {
        [TestMethod]
        public void TestMethodDicInheriteWithoutFieldOneRec()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CAIActionDescrs();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CAIActionDescrs>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodDicInheriteWithoutFieldTwoRec()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CAIActionDescrs();
            v1.Init2();

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CAIActionDescrs>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodDicInheriteWithFieldOneRec()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CDicInheriteWithField();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CDicInheriteWithField>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodDicInheriteWithFieldTwoRec()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CDicInheriteWithField();
            v1.Init2();

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CDicInheriteWithField>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodDicInheriteKeyObjOneRec()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CDicInherite();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CDicInherite>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodDicInheriteKeyObjTwoRec()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CDicInherite();
            v1.Init2();

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
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

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CListInheriteTestObj>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestMethodArrayOfDicInheritance()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            var v1 = new CArrayOfDicInheritance();
            v1.Init1();

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CArrayOfDicInheritance>(text, this);

            CheckInternalErrors();
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestEnumDictionary()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            CEnumDictionaryTest v1 = new CEnumDictionaryTest();
            v1.Params.Add(ETestEnum.TestEnumValue1, 0.1);
            v1.Params.Add(ETestEnum.TestEnumValue2, 0.2);
            v1.Params.Add(ETestEnum.TestEnumValue3, 0.3);

            string text = _serializer.SerializeToCascade(v1, this);
            Console.WriteLine(text);
            var v2 = _serializer.Deserialize<CEnumDictionaryTest>(text, this);
        }
    }
}
