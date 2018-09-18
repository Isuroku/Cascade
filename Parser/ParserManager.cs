using System;
using System.Collections.Generic;

namespace Parser
{
    public interface IParserOwner
    {
        string GetTextFromFile(string inFileName);
    }

    public class CParserManager : ITreeBuildSupport
    {
        class CParsed
        {
            List<CTokenLine> _lines = new List<CTokenLine>();
            public List<CTokenLine> Lines { get { return _lines; } }

            CKey _root;
            public CKey Root { get { return _root; } }

            string _file_name;
            public string FileName { get { return _file_name; } }

            public CParsed(CKey inRoot, List<CTokenLine> inLines, string inFileName)
            {
                _lines = inLines;
                _root = inRoot;
                _file_name = inFileName;
            }
        }

        List<CParsed> _parsed = new List<CParsed>();

        CSentenseDivider _sentenser = new CSentenseDivider();

        CLoger _loger;

        IParserOwner _owner;

        public CParserManager(IParserOwner owner, ILogPrinter inLogPrinter)
        {
            _owner = owner;
            _loger = new CLoger(inLogPrinter);
        }

        public CKey Parse(string inFileName, string inText)
        {
            _parsed.RemoveAll(p => string.Equals(p.FileName, inFileName));

            _sentenser.ParseText(inText, _loger);

            List<CTokenLine> lines = new List<CTokenLine>();
            for (int i = 0; i < _sentenser.SentenseCount; i++)
            {
                CSentense sentense = _sentenser[i];
                CTokenLine tl = new CTokenLine();
                tl.Init(sentense, _loger);
                lines.Add(tl);
            }

            CKey root = CTreeBuilder.Build(lines, this);

            _parsed.Add(new CParsed(root, lines, inFileName));

            return root;
        }

        public CTokenLine[] GetLineByRoot(CKey inRoot)
        {
            CParsed parsed = _parsed.Find(p => p.Root == inRoot);
            if (parsed == null)
                return null;
            return parsed.Lines.ToArray();
        }


        //ITreeBuildSupport

        public void LogWarning(EErrorCode inErrorCode, CToken inToken)
        {
            _loger.LogWarning(inErrorCode, inToken);
        }

        public void LogError(EErrorCode inErrorCode, string inText, int inLineNumber)
        {
            _loger.LogError(inErrorCode, inText, inLineNumber);
        }

        public void LogError(EErrorCode inErrorCode, CBaseKey inKey)
        {
            _loger.LogError(inErrorCode, inKey);
        }

        public void LogError(EErrorCode inErrorCode, CToken inToken)
        {
            _loger.LogError(inErrorCode, inToken);
        }

        public void LogError(EErrorCode inErrorCode, CTokenLine inLine)
        {
            _loger.LogError(inErrorCode, inLine);
        }

        public void LogInternalError(EInternalErrorCode inErrorCode, string inDebugText)
        {
            _loger.LogInternalError(inErrorCode, inDebugText);
        }

        public void Trace(string inText)
        {
            _loger.Trace(inText);
        }

        public CKey GetTree(string inFileName)
        {
            CParsed parsed = _parsed.Find(p => string.Equals(p.FileName, inFileName, StringComparison.InvariantCultureIgnoreCase));
            if (parsed != null)
                return parsed.Root;

            string text = _owner.GetTextFromFile(inFileName);
            if (string.IsNullOrEmpty(text))
                return null;

            return Parse(inFileName, text);
        }
    }
}
