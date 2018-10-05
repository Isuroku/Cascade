
namespace CascadeParser
{
    public interface ILogPrinter
    {
        void LogError(string inText);
        void LogWarning(string inText);
        void Trace(string inText);
    }

    internal class CLoger: ILogger
    {
        ILogPrinter _printer;

        public ILogPrinter LogPrinter { get { return _printer; } }

        int _error_count;
        public int ErrorCount { get { return _error_count; } }

        public CLoger(ILogPrinter printer)
        {
            _printer = printer;
        }

        public void LogWarning(EErrorCode inErrorCode, CToken inToken)
        {
            string text = string.Format("{0}. Token {1}. Position {2}", inErrorCode, inToken, inToken.Position);
            _printer.LogWarning(text);
        }

        public void LogError(EErrorCode inErrorCode, string inText, int inLineNumber)
        {
            _error_count++;
            string text = string.Format("{0}. [{1}]: {2}", inErrorCode, inLineNumber, inText);
            _printer.LogError(text);
        }

        public void LogError(EErrorCode inErrorCode, CKey inKey)
        {
            _error_count++;
            string text = string.Format("{0}. {1}", inErrorCode, inKey);
            _printer.LogError(text);
        }

        public void LogError(EErrorCode inErrorCode, CToken inToken)
        {
            _error_count++;
            string text = string.Format("{0}. Token {1}. Position {2}", inErrorCode, inToken, inToken.Position);
            _printer.LogError(text);
        }

        public void LogError(EErrorCode inErrorCode, CTokenLine inLine)
        {
            _error_count++;
            string text = string.Format("{0}. Line {1}.", inErrorCode, inLine);
            _printer.LogError(text);
        }

        public void Trace(string inText)
        {
            _printer.Trace(inText);
        }
    }
}
