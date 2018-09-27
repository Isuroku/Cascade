
namespace CascadeParser
{
    internal interface ILogger
    {
        void LogWarning(EErrorCode inErrorCode, CToken inToken);
        void LogError(EErrorCode inErrorCode, string inText, int inLineNumber);
        void LogError(EErrorCode inErrorCode, CKey inKey);
        void LogError(EErrorCode inErrorCode, CToken inToken);
        void LogError(EErrorCode inErrorCode, CTokenLine inLine);
        void Trace(string inText);
    }
}
