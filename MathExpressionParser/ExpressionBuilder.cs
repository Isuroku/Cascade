using System;
using System.Collections.Generic;

namespace MathExpressionParser
{
    struct SBuildElem
    {
        public CToken Token { get; private set; }
        public IMathFunc Op { get; private set; }

        public int StartLineIndex { get { return Token != null ? Token.StartLineIndex : Op.StartLineIndex; } }
        public int EndLineIndex { get { return Token != null ? Token.EndLineIndex : Op.EndLineIndex; } }

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

        public SBuildElem(IMathFunc op)
        {
            Token = null;
            Op = op;
        }
    }

    public static class CExpressionBuilder
    {
        public static CExpression Build(string inExpr, ILogger inLogger)
        {
            CToken[] tokens = CTokenFinder.GetTokens(inExpr);

            var lst = new List<SBuildElem>();

            for (int i = 0; i < tokens.Length; ++i)
                lst.Add(new SBuildElem(tokens[i]));

            KeyValuePair<bool, IMathFunc> res_op = CheckOneOrMany(0, lst.Count - 1, lst, inLogger);
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
                    left_arg = new CArg_Num(elem.Token.GetFloatValue(), elem.Token.StartLineIndex, elem.Token.EndLineIndex);
                else if (elem.Token.TokenType == ETokenType.Word)
                    left_arg = new CArg_Var(elem.Token.Text, elem.Token.StartLineIndex, elem.Token.EndLineIndex);
                else
                {
                    LogError(inLogger, EErrorCode.InvalidArgument, elem.Token);
                    return null;
                }
            }
            else if (elem.Op != null)
                left_arg = new CArg_BinOp(elem.Op);
            else
            {
                LogError(inLogger, EErrorCode.InvalidArgument, "Empty SBuildElem");
                return null;
            }
            return left_arg;
        }

        static KeyValuePair<bool, IMathFunc> CheckOneOrMany(int inStartPos, int inEndPos, List<SBuildElem> inList, ILogger inLogger)
        {
            if (inStartPos == inEndPos)
            {
                CArg arg = BuildElemToArg(inList[inStartPos], inLogger);
                return new KeyValuePair<bool, IMathFunc>(true, arg);
            }

            List<SBuildElem> lst_wo_bracers = ChangeBracers(inStartPos, inEndPos, inList, inLogger);
            if(lst_wo_bracers == null)
                return new KeyValuePair<bool, IMathFunc>(false, null);

            KeyValuePair<bool, IMathFunc>  res_op = BuildBinOp(lst_wo_bracers, inLogger);

            return res_op;
        }

        #region ChangeBracers
        static List<SBuildElem> ChangeBracers(int inStartPos, int inEndPos, List<SBuildElem> inList, ILogger inLogger)
        {
            List<SBuildElem> lst = new List<SBuildElem>();

            for (int i = inStartPos; i <= inEndPos;)
            {
                SBuildElem el = inList[i];
                if (el.Token != null && el.Token.TokenType == ETokenType.CloseBrace)
                {
                    LogError(inLogger, EErrorCode.InvalidCloseBracer, el.Token);
                    return null;
                }
                else if (el.Token != null && el.Token.TokenType == ETokenType.OpenBrace)
                {
                    KeyValuePair<bool, int> res_close_brace_pos = FindCloseBrace(i + 1, inList, inLogger);
                    if (!res_close_brace_pos.Key)
                        return null;

                    KeyValuePair<bool, IMathFunc> res_op;
                    if (i > inStartPos && inList[i - 1].Token != null && inList[i - 1].Token.TokenType == ETokenType.Word)
                    {//func
                        lst.RemoveAt(lst.Count - 1); //func name
                        res_op = BuildFunc(inList[i - 1].Token.Text, inList[i - 1].Token.StartLineIndex, i + 1, res_close_brace_pos.Value - 1, inList, inLogger);
                    }
                    else
                    {//simple ( )
                        res_op = CheckOneOrMany(i + 1, res_close_brace_pos.Value - 1, inList, inLogger);
                    }

                    if (!res_op.Key)
                        return null;

                    if (res_op.Value != null)
                        lst.Add(new SBuildElem(res_op.Value));

                    i = res_close_brace_pos.Value + 1;
                }
                else
                {
                    lst.Add(inList[i]);
                    i++;
                }
            }

            return lst;
        }

        private static KeyValuePair<bool, int> FindCloseBrace(int inStartPos, List<SBuildElem> inList, ILogger inLogger)
        {
            int bdeep = 0;
            for (int i = inStartPos; i < inList.Count; i++)
            {
                SBuildElem el = inList[i];
                if (el.Token != null && el.Token.TokenType == ETokenType.OpenBrace)
                    bdeep++;

                if (el.Token != null && el.Token.TokenType == ETokenType.CloseBrace)
                {
                    if (bdeep == 0)
                        return new KeyValuePair<bool, int>(true, i);
                    else
                        bdeep--;
                }
            }

            SBuildElem fel = inList[inStartPos];
            inLogger.LogError(string.Format("Can't find close bracer from {0} position!", fel.StartLineIndex));
            return new KeyValuePair<bool, int>(false, -1);
        }

        private static int FindComma(int inStartPos, int inEndPos, List<SBuildElem> inList, ILogger inLogger)
        {
            for (int i = inStartPos; i <= inEndPos; i++)
            {
                SBuildElem el = inList[i];
                if (el.Token != null && el.Token.TokenType == ETokenType.Comma)
                    return i;
            }

            return -1;
        }

        static KeyValuePair<bool, IMathFunc> BuildFunc(string inFuncName, int inStartLineIndex, int inStartPos, int inEndPos, List<SBuildElem> inList, ILogger inLogger)
        {
            int start = inStartPos;
            var args = new List<IMathFunc>();

            while (start <= inEndPos)
            {
                int cp = FindComma(start, inEndPos, inList, inLogger);
                int last = cp == -1 ? inEndPos : cp - 1;
                KeyValuePair<bool, IMathFunc> res_op = CheckOneOrMany(start, last, inList, inLogger);
                if (!res_op.Key)
                    return new KeyValuePair<bool, IMathFunc>(false, null);

                args.Add(res_op.Value);
                start = cp == -1 ? inEndPos + 1 : cp + 1;
            }

            CInternalFunc func = CInternalFunc.Create(inFuncName, args, inStartLineIndex, inList[inEndPos].EndLineIndex);
            return new KeyValuePair<bool, IMathFunc>(true, func);
        }
        #endregion ChangeBracers

        #region BuildBinOp
        static KeyValuePair<bool, IMathFunc> BuildBinOp(List<SBuildElem> inList, ILogger inLogger)
        {
            return BuildBinOp(0, inList.Count - 1, inList, inLogger);
        }

        static KeyValuePair<bool, IMathFunc> BuildBinOp(int inStartPos, int inEndPos, List<SBuildElem> inList, ILogger inLogger)
        {
            bool WasError;
            //binary operations            
            List<SBuildElem> list = inList;
            for (int pr_i = 0; pr_i < _ops_by_priority.Length; ++pr_i)
            {
                ETokenType[] ops = _ops_by_priority[pr_i];

                int op_index = FindFirstOp(list, ops, inLogger, out WasError);
                if (WasError)
                    return new KeyValuePair<bool, IMathFunc>(false, null);

                while (op_index != -1)
                {
                    list = ChangeBinOps(op_index, list, inLogger);
                    if (list == null)
                        return new KeyValuePair<bool, IMathFunc>(false, null);
                    op_index = FindFirstOp(list, ops, inLogger, out WasError);
                    if (WasError)
                        return new KeyValuePair<bool, IMathFunc>(false, null);
                }
            }

            if(list.Count == 1 && list[0].Op != null)
                return new KeyValuePair<bool, IMathFunc>(true, list[0].Op);

            LogError(inLogger, EErrorCode.CantParse, $"{list[0].StartLineIndex} - {list[list.Count - 1].EndLineIndex}");
            return new KeyValuePair<bool, IMathFunc>(false, null);
        }

        private static int FindFirstOp(List<SBuildElem> inList, ETokenType[] ops, ILogger inLogger, out bool outWasError)
        {
            outWasError = false;

            int op_index = inList.FindIndex(el => el.Token != null && Array.Exists(ops, tt => tt == el.Token.TokenType));
            if (op_index == -1)
                return -1;

            if (op_index <= 0 || op_index >= inList.Count - 1)
            {
                outWasError = true;
                LogError(inLogger, EErrorCode.InvalidOperationPosition, $"{inList[0].StartLineIndex} - {inList[inList.Count - 1].EndLineIndex}; op_index {op_index}");
                return -1;
            }

            return op_index;
        }

        static List<SBuildElem> ChangeBinOps(int inOpIndex, List<SBuildElem> inList, ILogger inLogger)
        {
            List<SBuildElem> new_list = new List<SBuildElem>();
            for (int i = 0; i < inList.Count;)
            {
                if (i == inOpIndex - 1)
                {
                    KeyValuePair<bool, IMathFunc> res_op = BuildTriplet(inOpIndex, inList, inLogger);
                    if (!res_op.Key)
                        return null;

                    if (res_op.Value != null)
                        new_list.Add(new SBuildElem(res_op.Value));

                    i = inOpIndex + 2;
                }
                else
                {
                    new_list.Add(inList[i]);
                    i++;
                }
            }

            return new_list;
        }

        static KeyValuePair<bool, IMathFunc> BuildTriplet(int inPos, List<SBuildElem> inList, ILogger inLogger)
        {
            if(inPos <= 0 || inPos >= inList.Count - 1)
            {
                LogError(inLogger, EErrorCode.InvalidTripletPosition, $"{inList[inPos].StartLineIndex}");
                return new KeyValuePair<bool, IMathFunc>(false, null);
            }

            EBinOp op_type = EBinOp.Undefined;
            SBuildElem el = inList[inPos];
            if (el.Token != null)
                op_type = el.Token.GetBinOp();

            if (op_type == EBinOp.Undefined)
            {
                if (el.Token != null)
                    LogError(inLogger, EErrorCode.BinOperationUndefined, el.Token);
                else
                    LogError(inLogger, EErrorCode.BinOperationUndefined, string.Format("el.Op {0}", el.Op));
                return new KeyValuePair<bool, IMathFunc>(false, null);
            }

            //left
            el = inList[inPos - 1];
            CArg left_arg = BuildElemToArg(el, inLogger);
            //right
            el = inList[inPos + 1];
            CArg right_arg = BuildElemToArg(el, inLogger);

            if (left_arg == null || right_arg == null)
                return new KeyValuePair<bool, IMathFunc>(false, null);

            return new KeyValuePair<bool, IMathFunc>(true, new CBinOp(op_type, left_arg, right_arg));
        }
        #endregion BuildBinOp

        

        static void LogWarning(ILogger inLogger, EErrorCode inErrorCode, CToken inToken)
        {
            string s = string.Format("{0}: {1} [{2}-{3}]", inErrorCode, inToken, inToken.StartLineIndex, inToken.EndLineIndex);
            inLogger.LogWarning(s);
        }

        static void LogError(ILogger inLogger, EErrorCode inErrorCode, CToken inToken)
        {
            string s = string.Format("{0}: {1} [{2}-{3}]", inErrorCode, inToken, inToken.StartLineIndex, inToken.EndLineIndex);
            inLogger.LogError(s);
        }

        static void LogError(ILogger inLogger, EErrorCode inErrorCode, string inText)
        {
            string s = string.Format("{0}: {1}", inErrorCode, inText);
            inLogger.LogError(s);
        }
    }
}
