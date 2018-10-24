using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace CascadeUnitTest
{
    [TestClass]
    public class UnitTestSaveString : BaseUnitTest
    {
        [TestMethod]
        public void TestMethodSaveInt()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            string str = CascadeParser.Utils.GetStringForSave(10.ToString());

            Console.WriteLine(str);

            Assert.AreEqual(str, "10");
        }

        [TestMethod]
        public void TestMethodSaveNegInt()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            string str = CascadeParser.Utils.GetStringForSave((-10).ToString());

            Console.WriteLine(str);

            Assert.AreEqual(str, "-10");
        }

        [TestMethod]
        public void TestMethodSaveFloat()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            string str = CascadeParser.Utils.GetStringForSave((-10.54).ToString(Utils.GetCultureInfoFloatPoint()));

            Console.WriteLine(str);

            Assert.AreEqual(str, "-10.54");
        }

        [TestMethod]
        public void TestMethodSaveLightString()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            string str = CascadeParser.Utils.GetStringForSave("_test_ ");

            Console.WriteLine(str);

            Assert.AreEqual(str, "_test_");
        }

        [TestMethod]
        public void TestMethodSaveString()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            string str = CascadeParser.Utils.GetStringForSave(" _te.st_");

            Console.WriteLine(str);

            Assert.AreEqual(str, "\"_te.st_\"");
        }
    }
}
