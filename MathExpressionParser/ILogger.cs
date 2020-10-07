

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
        CallEmptyBuild,
        CantParseFunc,
        InvalidTripletPosition
    }

    public interface ILogger
    {
        void LogWarning(string inText);
        void LogError(string inText);
    }
}
