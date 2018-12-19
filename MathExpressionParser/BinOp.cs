using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpressionParser
{
    public enum EBinOp
    {
        Undefined,
        Sum, //+
        Diff, //-
        Mult, // *
        Div, // /
        Power, //^
    }

    public class CBinOp
    {
        EBinOp _op;
        CArg _arg1;
        CArg _arg2;

        public int StartPos { get { return _arg1.StartPos; } }
        public int EndPos { get { return _arg2.EndPos; } }

        public CBinOp(EBinOp op, CArg arg1, CArg arg2)
        {
            _op = op;
            _arg1 = arg1;
            _arg2 = arg2;

            _arg1.SetOwner(this);
            _arg2.SetOwner(this);
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}:{2}) [{3}-{4}]", _op, _arg1, _arg2, StartPos, EndPos);
        }

        public double GetValue(CExpression.DTryGetValue inGetValue, ILogger inLogger)
        {
            double v1 = _arg1.GetValue(inGetValue, inLogger);
            double v2 = _arg2.GetValue(inGetValue, inLogger);

            switch(_op)
            {
                case EBinOp.Power: return Math.Pow(v1, v2);
                case EBinOp.Mult: return v1 * v2;
                case EBinOp.Div: return v1 / v2;
                case EBinOp.Sum: return v1 + v2;
                case EBinOp.Diff: return v1 - v2;
            }

            inLogger.LogError(string.Format("Invalid operation: {0}", _op));
            return 0;
        }
    }

    public abstract class CArg
    {
        CBinOp _owner;

        public int StartPos { get; private set; }
        public int EndPos { get; private set; }

        public CArg(int start_pos, int end_pos)
        {
            StartPos = start_pos;
            EndPos = end_pos;
        }

        public void SetOwner(CBinOp owner)
        {
            _owner = owner;
        }

        public abstract double GetValue(CExpression.DTryGetValue inGetValue, ILogger inLogger);
    }

    public class CArg_Num: CArg
    {
        double _value;

        public CArg_Num(double v, int start_pos, int end_pos) :
            base(start_pos, end_pos)
        {
            _value = v;
        }

        public override string ToString()
        {
            return string.Format("{0}", _value);
        }

        public override double GetValue(CExpression.DTryGetValue inGetValue, ILogger inLogger)
        {
            return _value;
        }
    }

    public class CArg_Var : CArg
    {
        string _name;

        public CArg_Var(string text, int start_pos, int end_pos):
            base(start_pos, end_pos)
        {
            _name = text;
        }

        public override string ToString()
        {
            return _name;
        }

        public override double GetValue(CExpression.DTryGetValue inGetValue, ILogger inLogger)
        {
            double val;
            if(inGetValue(_name, out val))
                return val;
            return 0;
        }
    }

    public class CArg_BinOp : CArg
    {
        CBinOp _op;

        public CArg_BinOp(CBinOp op, int start_pos, int end_pos):
            base(start_pos, end_pos)
        {
            _op = op;
        }

        public override string ToString()
        {
            return $"{_op}";
        }

        public override double GetValue(CExpression.DTryGetValue inGetValue, ILogger inLogger)
        {
            return _op.GetValue(inGetValue, inLogger);
        }
    }

    public class CExpression
    {
        CBinOp _op;

        public CExpression(CBinOp op)
        {
            _op = op;
        }

        public delegate bool DTryGetValue(string key, out double value);

        public double GetValue(DTryGetValue inGetValue, ILogger inLogger)
        {
            return _op.GetValue(inGetValue, inLogger);
        }
    }
}
