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

            CExpression exp = CExpressionBuilder.Build("1 / 2 + var1 * var2 - 5.5 + 2^2", this);

            double res = exp.GetValue(dic.TryGetValue, this);

            CheckInternalErrors();
            Assert.AreEqual(res, 11);
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
    }
}
