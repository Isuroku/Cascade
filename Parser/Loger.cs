using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
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

    public class CLoger
    {
        ILogPrinter _printer;

        public CLoger(ILogPrinter printer)
        {
            _printer = printer;
        }

        public void LogWarning(EErrorCode inErrorCode, CToken inToken)
        {
            string text = string.Format("{0}. Token {1}. Position {2}", inErrorCode, inToken, inToken.Position);
            _printer.AddLogToConsole(text, ELogLevel.Warning);
        }

        public void LogError(EErrorCode inErrorCode, CToken inToken)
        {
            string text = string.Format("{0}. Token {1}. Position {2}", inErrorCode, inToken, inToken.Position);
            _printer.AddLogToConsole(text, ELogLevel.Error);
        }

        public void LogError(EErrorCode inErrorCode, CTokenLine inLine)
        {
            string text = string.Format("{0}. Line {1}.", inErrorCode, inLine);
            _printer.AddLogToConsole(text, ELogLevel.Error);
        }

        public void LogInternalError(EInternalErrorCode inErrorCode, string inDebugText)
        {
            string text = string.Format("{0}. {1}", inErrorCode, inDebugText);
            _printer.AddLogToConsole(text, ELogLevel.InternalError);
        }

        public void Trace(string inText)
        {
            _printer.AddLogToConsole(inText, ELogLevel.Info);
        }
    }
}
