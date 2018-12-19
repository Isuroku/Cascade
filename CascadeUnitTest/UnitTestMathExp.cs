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

            CToken[] tks = CTokenFinder.GetTokens("1.1 + 2 + var1");

            Dictionary<string, double> dic = new Dictionary<string, double>();
            dic.Add("var1", 3);

            CExpression exp = CExpressionBuilder.Build(tks, this);

            double res = exp.GetValue(dic.TryGetValue, this);

            CheckInternalErrors();
            Assert.AreEqual(res, 6.1);
        }

        [TestMethod]
        public void TestMethodSimpleAllOp()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            CToken[] tks = CTokenFinder.GetTokens("1 / 2 + var1 * var2 - 5.5 + 2^2");

            Dictionary<string, double> dic = new Dictionary<string, double>();
            dic.Add("var1", 3);
            dic.Add("var2", 4);

            CExpression exp = CExpressionBuilder.Build(tks, this);

            double res = exp.GetValue(dic.TryGetValue, this);

            CheckInternalErrors();
            Assert.AreEqual(res, 11);
        }
    }
}
