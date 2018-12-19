using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace MathExpressionParser
{
    struct SBuildElem
    {
        public CToken Token { get; private set; }
        public CBinOp Op { get; private set; }

        public int StartPos { get { return Token != null ? Token.StartPos : Op.StartPos; } }
        public int EndPos { get { return Token != null ? Token.EndPos : Op.EndPos; } }

        public override string ToString()
        {
            if(Token != null)
                return string.Format("{0}", Token);
            if (Op != null)
                return string.Format("{0}", Op);
            return "Undefined!";
        }

        public SBuildElem(CToken token)
        {
            Token = token;
            Op = null;
        }

        public SBuildElem(CBinOp op)
        {
            Token = null;
            Op = op;
        }
    }

    public static class CExpressionBuilder
    {
        public static CExpression Build(CToken[] inTokens, ILogger inLogger)
        {
            var lst = new List<SBuildElem>();

            for (int i = 0; i < inTokens.Length; ++i)
                lst.Add(new SBuildElem(inTokens[i]));

            KeyValuePair<bool, CBinOp> res_op = BuildBinOp(lst, inLogger);
            if (res_op.Value == null)
                return null;

            return new CExpression(res_op.Value);
        }

        static ETokenType[][] _ops_by_priority = new ETokenType[][]
        {
            new ETokenType[] { ETokenType.Power },
            new ETokenType[] { ETokenType.Mult, ETokenType.Div },
            new ETokenType[] { ETokenType.Sum, ETokenType.Diff },
        };

        static CArg BuildElemToArg(SBuildElem elem, ILogger inLogger)
        {
            CArg left_arg;
            if (elem.Token != null)
            {
                if (elem.Token.TokenType == ETokenType.Float ||
                    elem.Token.TokenType == ETokenType.Int ||
                    elem.Token.TokenType == ETokenType.UInt)
                    left_arg = new CArg_Num(elem.Token.GetFloatValue(), elem.Token.StartPos, elem.Token.EndPos);
                else if (elem.Token.TokenType == ETokenType.Word)
                    left_arg = new CArg_Var(elem.Token.Text, elem.Token.StartPos, elem.Token.EndPos);
                else
                {
                    LogError(inLogger, EErrorCode.InvalidArgument, elem.Token);
                    return null;
                }
            }
            else if (elem.Op != null)
                left_arg = new CArg_BinOp(elem.Op, elem.Op.StartPos, elem.Op.EndPos);
            else
            {
                LogError(inLogger, EErrorCode.InvalidArgument, "Empty SBuildElem");
                return null;
            }
            return left_arg;
        }

        static KeyValuePair<bool, CBinOp> BuildBinOp(List<SBuildElem> inList, ILogger inLogger)
        {
            return BuildBinOp(0, inList.Count - 1, inList, inLogger);
        }

        static KeyValuePair<bool, CBinOp> BuildBinOp(int inStartPos, int inEndPos, List<SBuildElem> inList, ILogger inLogger)
        {
            if (inEndPos - inStartPos == 0)
            {
                return new KeyValuePair<bool, CBinOp>(true, null);
            }

            if (inEndPos - inStartPos == 1)
            {
                LogError(inLogger, EErrorCode.TooLowOperands, $"{inList[inStartPos].StartPos} - {inList[inEndPos].EndPos}");
                return new KeyValuePair<bool, CBinOp>(false, null);
            }

            if (inEndPos - inStartPos == 2)
            {
                EBinOp op_type = EBinOp.Undefined;
                SBuildElem el = inList[inStartPos + 1];
                if (el.Token != null)
                    op_type = el.Token.GetBinOp();

                if(op_type == EBinOp.Undefined)
                {
                    if(el.Token != null)
                        LogError(inLogger, EErrorCode.BinOperationUndefined, el.Token);
                    else
                        LogError(inLogger, EErrorCode.BinOperationUndefined, string.Format("el.Op {0}", el.Op));
                    return new KeyValuePair<bool, CBinOp>(false, null);
                }

                //left
                el = inList[inStartPos];
                CArg left_arg = BuildElemToArg(el, inLogger);
                //right
                el = inList[inStartPos + 2];
                CArg right_arg = BuildElemToArg(el, inLogger);

                if (left_arg == null || right_arg == null)
                    return new KeyValuePair<bool, CBinOp>(false, null);

                return new KeyValuePair<bool, CBinOp>(true, new CBinOp(op_type, left_arg, right_arg));
            }

            List<SBuildElem> lst = inList;
            bool WasError;
            KeyValuePair<int, int>? bracers = FindFirstBracers(inStartPos, inEndPos, lst, inLogger, out WasError);
            if (WasError)
                return new KeyValuePair<bool, CBinOp>(false, null);

            while (bracers.HasValue)
            {
                lst = ChangeOps(bracers.Value, lst, inLogger);
                if(lst == null)
                    return new KeyValuePair<bool, CBinOp>(false, null);
                bracers = FindFirstBracers(lst, inLogger, out WasError);
                if (WasError)
                    return new KeyValuePair<bool, CBinOp>(false, null);
            }
                        
            for(int pr_i = 0; pr_i < _ops_by_priority.Length; ++pr_i)
            {
                ETokenType[] ops = _ops_by_priority[pr_i];

                KeyValuePair<int, int>? op = FindFirstOp(lst, ops, inLogger, out WasError);
                if (WasError)
                    return new KeyValuePair<bool, CBinOp>(false, null);

                while (op.HasValue)
                {
                    lst = ChangeOps(op.Value, lst, inLogger);
                    if (lst == null)
                        return new KeyValuePair<bool, CBinOp>(false, null);
                    op = FindFirstOp(lst, ops, inLogger, out WasError);
                    if (WasError)
                        return new KeyValuePair<bool, CBinOp>(false, null);
                }
            }

            if(lst.Count == 1 && lst[0].Op != null)
                return new KeyValuePair<bool, CBinOp>(true, lst[0].Op);

            LogError(inLogger, EErrorCode.CantParse, $"{lst[0].StartPos} - {lst[lst.Count - 1].EndPos}");
            return new KeyValuePair<bool, CBinOp>(false, null);
        }

        private static KeyValuePair<int, int>? FindFirstOp(List<SBuildElem> lst, ETokenType[] ops, ILogger inLogger, out bool outWasError)
        {
            return FindFirstOp(0, lst.Count - 1, lst, ops, inLogger, out outWasError);
        }

        private static KeyValuePair<int, int>? FindFirstOp(int inStartPos, int inEndPos, List<SBuildElem> lst, ETokenType[] ops, ILogger inLogger, out bool outWasError)
        {
            outWasError = false;

            int op_index = lst.FindIndex(el => el.Token != null && Array.Exists(ops, tt => tt == el.Token.TokenType));
            if (op_index == -1)
                return null;

            if (op_index <= inStartPos || op_index >= inEndPos)
            {
                outWasError = true;
                LogError(inLogger, EErrorCode.InvalidOperationPosition, $"{lst[inStartPos].StartPos} - {lst[inEndPos].EndPos}; op_index {op_index}");
                return null;
            }

            return new KeyValuePair<int, int>(op_index - 1, op_index + 1);
        }

        private static KeyValuePair<int, int>? FindFirstBracers(List<SBuildElem> lst, ILogger inLogger, out bool outWasError)
        {
            return FindFirstBracers(0, lst.Count - 1, lst, inLogger, out outWasError);
        }

        private static KeyValuePair<int, int>? FindFirstBracers(int inStartPos, int inEndPos, List<SBuildElem> lst, ILogger inLogger, out bool outWasError)
        {
            outWasError = false;
            int bp = -1;
            int bdeep = 0;
            for (int i = inStartPos; i <= inEndPos; i++)
            {
                SBuildElem el = lst[i];
                if (el.Token != null && el.Token.TokenType == ETokenType.OpenBrace)
                {
                    if (bp == -1)
                        bp = i;
                    else
                        bdeep++;
                }

                if (el.Token != null && el.Token.TokenType == ETokenType.CloseBrace)
                {
                    if (bp == -1)
                    {
                        outWasError = true;
                        LogError(inLogger, EErrorCode.InvalidCloseBracer, el.Token);
                    }
                    else if (bdeep == 0)
                        return new KeyValuePair<int, int>(bp, i - 1);
                    else
                        bdeep--;
                }
            }

            if (bp == -1)
                return null;

            return new KeyValuePair<int, int>(bp, -1);
        }

        static List<SBuildElem> ChangeOps(KeyValuePair<int, int> inSubExpr, List<SBuildElem> inList, ILogger inLogger)
        {
            return ChangeOps(0, inList.Count - 1, inSubExpr, inList, inLogger);
        }

        static List<SBuildElem> ChangeOps(int StartPos, int inEndPos, KeyValuePair<int, int> inSubExpr, List<SBuildElem> inList, ILogger inLogger)
        {
            List<SBuildElem> new_list = new List<SBuildElem>();
            for (int i = StartPos; i <= inEndPos;)
            {
                if (i == inSubExpr.Key)
                {
                    KeyValuePair<bool, CBinOp> res_op = BuildBinOp(inSubExpr.Key, inSubExpr.Value, inList, inLogger);
                    if (!res_op.Key)
                        return null;

                    if (res_op.Value != null)
                        new_list.Add(new SBuildElem(res_op.Value));

                    i = inSubExpr.Value + 1;
                }
                else
                {
                    new_list.Add(inList[i]);
                    i++;
                }
            }

            return new_list;
        }

        static void LogWarning(ILogger inLogger, EErrorCode inErrorCode, CToken inToken)
        {
            string s = string.Format("{0}: {1} [{2}-{3}]", inErrorCode, inToken, inToken.StartPos, inToken.EndPos);
            inLogger.LogWarning(s);
        }

        static void LogError(ILogger inLogger, EErrorCode inErrorCode, CToken inToken)
        {
            string s = string.Format("{0}: {1} [{2}-{3}]", inErrorCode, inToken, inToken.StartPos, inToken.EndPos);
            inLogger.LogError(s);
        }

        static void LogError(ILogger inLogger, EErrorCode inErrorCode, string inText)
        {
            string s = string.Format("{0}: {1}", inErrorCode, inText);
            inLogger.LogError(s);
        }
    }
}
