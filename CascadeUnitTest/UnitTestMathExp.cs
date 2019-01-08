using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using MathExpressionParser;
using System.Collections.Generic;

namespace CascadeUnitTest
{
    [TestClass]
    public class UnitTestMathExp : BaseUnitTest
    {
        [TestMethod]
        public void TestMethodSimpleSum()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            Dictionary<string, double> dic = new Dictionary<string, double>();
            dic.Add("var1", 3);

            CExpression exp = CExpressionBuilder.Build("1.1 + 2 + var1", this);

            double res = exp.GetValue(dic.TryGetValue, this);

            CheckInternalErrors();
            Assert.AreEqual(res, 6.1);
        }

        [TestMethod]
        public void TestMethodSimpleAllOp()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            Dictionary<string, double> dic = new Dictionary<string, double>();
            dic.Add("var1", 3);
            dic.Add("var2", 4);

            CExpression exp = CExpressionBuilder.Build("1 / 2 + var1 * var2 - 5.5 + 4^(1/2)", this);

            double res = exp.GetValue(dic.TryGetValue, this);

            CheckInternalErrors();
            Assert.AreEqual(res, 9);
        }

        [TestMethod]
        public void TestMethodBracers()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            CExpression exp = CExpressionBuilder.Build("(1 + 2) * ((3 + 4) - (5 + 1))", this);

            double res = exp.GetValue(null, this);

            CheckInternalErrors();
            Assert.AreEqual(res, 3);
        }

        [TestMethod]
        public void TestMethodOneValue1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            CExpression exp = CExpressionBuilder.Build("99", this);

            double res = exp.GetValue(null, this);

            CheckInternalErrors();
            Assert.AreEqual(res, 99);
        }

        [TestMethod]
        public void TestMethodOneValue2()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            CExpression exp = CExpressionBuilder.Build("var1", this);

            Dictionary<string, double> dic = new Dictionary<string, double>();
            dic.Add("var1", 3);

            double res = exp.GetValue(dic.TryGetValue, this);

            CheckInternalErrors();
            Assert.AreEqual(res, 3);
        }

        [TestMethod]
        public void TestMethodOneInternalFunc1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            CExpression exp = CExpressionBuilder.Build("Round(3.1)", this);

            double res = exp.GetValue(null, this);

            CheckInternalErrors();
            Assert.AreEqual(res, 3);
        }

        [TestMethod]
        public void TestMethodOneInternalFunc2()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            CExpression exp = CExpressionBuilder.Build("Min(3, 2) * (Neg(var1) + 4)", this);

            Dictionary<string, double> dic = new Dictionary<string, double>();
            dic.Add("var1", 3);

            double res = exp.GetValue(dic.TryGetValue, this);

            CheckInternalErrors();
            Assert.AreEqual(res, 2);

            dic["var1"] = -3;
            res = exp.GetValue(dic.TryGetValue, this);

            CheckInternalErrors();
            Assert.AreEqual(res, 14);
        }
    }
}
