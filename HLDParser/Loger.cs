
namespace HLDParser
{
    public enum ELogLevel
    {
        InternalError,
        Error,
        Warning,
        Info
    }

    public interface ILogPrinter
    {
        void AddLogToConsole(string inText, ELogLevel inLogLevel);
    }

    internal class CLoger: ILogger
    {
        ILogPrinter _printer;

        int _error_count;
        public int ErrorCount { get { return _error_count; } }

        public CLoger(ILogPrinter printer)
        {
            _printer = printer;
        }

        public void LogWarning(EErrorCode inErrorCode, CToken inToken)
        {
            string text = string.Format("{0}. Token {1}. Position {2}", inErrorCode, inToken, inToken.Position);
            _printer.AddLogToConsole(text, ELogLevel.Warning);
        }

        public void LogError(EErrorCode inErrorCode, string inText, int inLineNumber)
        {
            _error_count++;
            string text = string.Format("{0}. [{1}]: {2}", inErrorCode, inLineNumber, inText);
            _printer.AddLogToConsole(text, ELogLevel.Error);
        }

        public void LogError(EErrorCode inErrorCode, CKey inKey)
        {
            _error_count++;
            string text = string.Format("{0}. {1}", inErrorCode, inKey);
            _printer.AddLogToConsole(text, ELogLevel.Error);
        }

        public void LogError(EErrorCode inErrorCode, CToken inToken)
        {
            _error_count++;
            string text = string.Format("{0}. Token {1}. Position {2}", inErrorCode, inToken, inToken.Position);
            _printer.AddLogToConsole(text, ELogLevel.Error);
        }

        public void LogError(EErrorCode inErrorCode, CTokenLine inLine)
        {
            _error_count++;
            string text = string.Format("{0}. Line {1}.", inErrorCode, inLine);
            _printer.AddLogToConsole(text, ELogLevel.Error);
        }

        public void Trace(string inText)
        {
            _printer.AddLogToConsole(inText, ELogLevel.Info);
        }
    }
}
