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
		public void TestMethodSimpleSumI()
		{
			ResetTestState();
			Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			CExpression exp = CExpressionBuilder.Build("1.1 + 2 + var1", this);

			exp.SetArgIds(name => 0);

			double res = exp.GetValue(
				(int id, out double value) => 
				{ 
					value = 3; 
					return true; 
				}, 
			this);

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
		public void TestMethodSimpleAllOp1()
		{
			ResetTestState();
			Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			List<Tuple<string, double>> lst = new List<Tuple<string, double>>();
			lst.Add(new Tuple<string, double>("var1", 3));
			lst.Add(new Tuple<string, double>("var2", 4));

			CExpression exp = CExpressionBuilder.Build("1 / 2 + var1 * var2 - 5.5 + 4^(1/2)", this);

			exp.SetArgIds(name =>
			{
				return lst.FindIndex(t => t.Item1 == name);
			});

			double res = exp.GetValue(
				(int id, out double value) =>
				{
					value = 0;
					if (id < 0)
						return false;

					value = lst[id].Item2;
					return true;
				},
			this);

			CheckInternalErrors();
			Assert.AreEqual(res, 9);
		}

		[TestMethod]
        public void TestMethodBracers()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            CExpression exp = CExpressionBuilder.Build("(1 + 2) * ((3 + 4) - (5 + 1))", this);

            double res = exp.GetValue(this);

            CheckInternalErrors();
            Assert.AreEqual(res, 3);
        }

        [TestMethod]
        public void TestMethodOneValue1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            CExpression exp = CExpressionBuilder.Build("99", this);

            double res = exp.GetValue(this);

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
		public void TestMethodOneValue2I()
		{
			ResetTestState();
			Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			CExpression exp = CExpressionBuilder.Build("var1", this);

			double res = exp.GetValue(
				(int id, out double value) =>
				{
					value = 3;
					return true;
				},
			this);

			CheckInternalErrors();
			Assert.AreEqual(res, 3);
		}

		[TestMethod]
        public void TestMethodOneInternalFunc1()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            CExpression exp = CExpressionBuilder.Build("Round(3.1)", this);

            double res = exp.GetValue(this);

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

		[TestMethod]
		public void TestMethodOneInternalFunc2I()
		{
			ResetTestState();
			Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			CExpression exp = CExpressionBuilder.Build("Min(3, 2) * (Neg(var1) + 4)", this);

			double res = exp.GetValue(
				(int id, out double value) =>
				{
					value = 3;
					return true;
				},
			this);

			CheckInternalErrors();
			Assert.AreEqual(res, 2);

			res = exp.GetValue(
				(int id, out double value) =>
				{
					value = -3;
					return true;
				},
			this);

			CheckInternalErrors();
			Assert.AreEqual(res, 14);
		}

		[TestMethod]
        public void TestMethodOneInternalFunc3()
        {
            ResetTestState();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);

            CExpression exp = CExpressionBuilder.Build("Rand(1,100)", this);

            double res = exp.GetValue(this);

            CheckInternalErrors();

            Assert.IsTrue(res>= 1 && res <= 100);
        }

		[TestMethod]
		public void TestMethodLogic1()
		{
			ResetTestState();
			Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			CExpression exp = CExpressionBuilder.Build("1 & 1", this);
			double res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 1);

			exp = CExpressionBuilder.Build("1 & 0", this);
			res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 0);

			exp = CExpressionBuilder.Build("0 & 0", this);
			res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 0);
		}

		[TestMethod]
		public void TestMethodLogic2()
		{
			ResetTestState();
			Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			CExpression exp = CExpressionBuilder.Build("1 | 1", this);
			double res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 1);

			exp = CExpressionBuilder.Build("1 | 0", this);
			res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 1);

			exp = CExpressionBuilder.Build("0 | 0", this);
			res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 0);
		}

		[TestMethod]
		public void TestMethodLogic3()
		{
			ResetTestState();
			Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			CExpression exp = CExpressionBuilder.Build("1 | 1 & 0", this);
			double res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 1);

			exp = CExpressionBuilder.Build("(1 | 1) & 0", this);
			res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 0);

			exp = CExpressionBuilder.Build("(1 | 1) & (0 | 1)", this);
			res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 1);
		}

		[TestMethod]
		public void TestMethodLogic4()
		{
			ResetTestState();
			Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			CExpression exp = CExpressionBuilder.Build("Not(1 | 1 & 0)", this);
			double res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 0);

			exp = CExpressionBuilder.Build("Not((1 | 1) & 0)", this);
			res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 1);

			exp = CExpressionBuilder.Build("Not((1 | 1) & (0 | 1))", this);
			res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 0);
		}

		[TestMethod]
		public void TestMethodFactorial()
		{
			ResetTestState();
			Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			CExpression exp = CExpressionBuilder.Build("Fact(1)", this);
			double res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 1);

			exp = CExpressionBuilder.Build("Fact(4)", this);
			res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 24);

			exp = CExpressionBuilder.Build("Fact(0)", this);
			res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 1);

			exp = CExpressionBuilder.Build("Fact(Neg(5))", this);
			res = exp.GetValue(this);
			CheckInternalErrors();
			Assert.AreEqual(res, 1);
		}
	}
}
