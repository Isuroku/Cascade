using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MathExpressionParser
{
    public interface IMathFunc
    {
        int StartLineIndex { get; }
        int EndLineIndex { get; }

        double GetValue(CExpression.DTryGetValue inGetValue, ILogger inLogger);
    }

    public enum EBinOp
    {
        Undefined,
        Sum, //+
        Diff, //-
        Mult, // *
        Div, // /
        Power, //^
    }

    public sealed class CBinOp: IMathFunc
    {
        EBinOp _op;
        CArg _arg1;
        CArg _arg2;

        public int StartLineIndex { get { return _arg1.StartLineIndex; } }
        public int EndLineIndex { get { return _arg2.EndLineIndex; } }

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
            return string.Format("{0} ({1}:{2}) [{3}-{4}]", _op, _arg1, _arg2, StartLineIndex, EndLineIndex);
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

    public abstract class CArg: IMathFunc
    {
        CBinOp _owner;

        public int StartLineIndex { get; private set; }
        public int EndLineIndex { get; private set; }

        public CArg(int start_pos, int end_pos)
        {
            StartLineIndex = start_pos;
            EndLineIndex = end_pos;
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

        public CArg_Num(double v, int start_li, int end_li) :
            base(start_li, end_li)
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

        public CArg_Var(string text, int start_li, int end_li) :
            base(start_li, end_li)
        {
            _name = text;
        }

        public override string ToString()
        {
            return _name;
        }

        public override double GetValue(CExpression.DTryGetValue inGetValue, ILogger inLogger)
        {
            if (inGetValue == null)
            {
                inLogger.LogError(string.Format("Arg {0} needs TryGetValue!", _name));
                return 0;
            }

            double val;
            if(inGetValue(_name, out val))
                return val;
            return 0;
        }
    }

    public class CArg_BinOp : CArg
    {
        IMathFunc _op;

        public CArg_BinOp(IMathFunc op):
            base(op.StartLineIndex, op.EndLineIndex)
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

    class CArgArray
    {
        IMathFunc[] _args;
        double[] _values;

        public int Length { get { return _args.Length; } }

        public CArgArray(IMathFunc[] args)
        {
            _args = args;
            _values = new double[_args.Length];
        }

        public double this[int index] { get { return _values[index]; } }

        public void FillValue(CExpression.DTryGetValue inGetValue, ILogger inLogger)
        {
            for(int i = 0; i < _args.Length; ++i)
                _values[i] = _args[i].GetValue(inGetValue, inLogger);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _args.Length; ++i)
            {
                if (sb.Length > 0)
                    sb.AppendFormat(", {0}", _args[i]);
                else
                    sb.Append(_args[i]);
            }
            return sb.ToString();
        }
    }

    public enum EInternalFunc
    {
        Undefined,
        Neg, //*-1
        Abs,
        Sign,
        Sqrt,
        Round,
        Min,
        Max,
        Sin,
        Cos,
        Tan,
        Asin,
        Acos,
        Atan,
        Atan2
    }

    public sealed class CInternalFunc : IMathFunc
    {
        EInternalFunc _type;
        CArgArray _args;

        public int StartLineIndex { get; private set; }
        public int EndLineIndex { get; private set; }

        public static CInternalFunc Create(string inFuncType, List<IMathFunc> args, int inStartPos, int inEndPos)
        {
            EInternalFunc t = Utils.ToEnum(inFuncType, EInternalFunc.Undefined);
            if (t == EInternalFunc.Undefined)
                return null;

            if(args == null || args.Count == 0 || args.Count > 2)
                return null;

            if(args.Count == 2 && 
                t != EInternalFunc.Max 
                && t != EInternalFunc.Min
                && t != EInternalFunc.Atan2)
                return null;

            return new CInternalFunc(t, args.ToArray(), inStartPos, inEndPos);
        }

        CInternalFunc(EInternalFunc inFuncType, IMathFunc[] args, int inStartPos, int inEndPos)
        {
            _type = inFuncType;
            _args = new CArgArray(args);
            StartLineIndex = inStartPos;
            EndLineIndex = inEndPos;
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", _type, _args);
        }

        public double GetValue(CExpression.DTryGetValue inGetValue, ILogger inLogger)
        {
            _args.FillValue(inGetValue, inLogger);

            switch (_type)
            {
                case EInternalFunc.Neg: return -1 * _args[0];
                case EInternalFunc.Abs: return Math.Abs(_args[0]);
                case EInternalFunc.Sign: return Math.Sign(_args[0]);
                case EInternalFunc.Sqrt: return Math.Sqrt(_args[0]);
                case EInternalFunc.Round: return Math.Round(_args[0]);
                case EInternalFunc.Min: return Math.Min(_args[0], _args[1]);
                case EInternalFunc.Max: return Math.Max(_args[0], _args[1]);
                case EInternalFunc.Sin: return Math.Sin(_args[0]);
                case EInternalFunc.Cos: return Math.Cos(_args[0]);
                case EInternalFunc.Tan: return Math.Tan(_args[0]);
                case EInternalFunc.Asin: return Math.Asin(_args[0]);
                case EInternalFunc.Acos: return Math.Acos(_args[0]);
                case EInternalFunc.Atan: return Math.Atan(_args[0]);
                case EInternalFunc.Atan2: return Math.Atan2(_args[0], _args[1]);
            }

            inLogger.LogError(string.Format("Invalid func: {0}", _type));
            return 0;
        }
    }

    public class CExpression
    {
        IMathFunc _op;

        public CExpression(IMathFunc op)
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
