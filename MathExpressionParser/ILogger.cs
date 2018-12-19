using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpressionParser
{
    public enum EErrorCode
    {
        PrevOperandNotPresent,
        NextOperandNotPresent,
        BinOperationUndefined,
        InvalidArgument,
        TooLowOperands,
        CantParse,
        InvalidCloseBracer,
        InvalidOperationPosition,
        CallEmptyBuild
    }

    public interface ILogger
    {
        void LogWarning(string inText);
        void LogError(string inText);
    }
}
