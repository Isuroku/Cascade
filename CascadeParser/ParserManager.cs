using System;
using System.Collections.Generic;
using System.IO;

namespace CascadeParser
{
    public interface IParserOwner
    {
        string GetTextFromFile(string inFileName, object inContextData);
    }

    public class CParserManager
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

        class CSupportOwner: ITreeBuildSupport
        {
            CParserManager _owner;
            CLoger _loger;
            object _context_data;

            public CSupportOwner(CParserManager owner, ILogPrinter inLogger, object inContextData, string inFileName)
            {
                _owner = owner;
                _loger = new CLoger(inLogger, inFileName);
                _context_data = inContextData;
            }

            public ILogger GetLogger()
            {
                return _loger;
            }

            public IKey GetTree(string inFileName)
            {
                return _owner.GetTree(inFileName, _context_data, _loger.LogPrinter);
            }
        }

        List<CParsed> _parsed = new List<CParsed>();

        CSentenseDivider _sentenser = new CSentenseDivider();

        IParserOwner _owner;

        public CParserManager(IParserOwner owner)
        {
            _owner = owner;
        }

        public void ClearParsed()
        {
            _parsed.Clear();
        }

        public IKey Parse(string inText, ILogPrinter inLogger, object inContextData)
        {
            return Parse(string.Empty, inText, inLogger, inContextData);
        }

        public IKey Parse(string inFileName, string inText, ILogPrinter inLogger, object inContextData)
        {
            if(!string.IsNullOrEmpty(inFileName))
                _parsed.RemoveAll(p => string.Equals(p.FileName, inFileName));

            var supporter = new CSupportOwner(this, inLogger, inContextData, inFileName);

            _sentenser.ParseText(inText, supporter.GetLogger());

            List<CTokenLine> lines = new List<CTokenLine>();
            for (int i = 0; i < _sentenser.SentenseCount; i++)
            {
                CSentense sentense = _sentenser[i];
                CTokenLine tl = new CTokenLine();
                tl.Init(sentense, supporter.GetLogger());
                lines.Add(tl);
            }

            string root_name = Path.GetFileNameWithoutExtension(inFileName);
            CKey root = CTreeBuilder.Build(root_name, lines, supporter);

            _parsed.Add(new CParsed(root, lines, inFileName));

            return root;
        }

        public CTokenLine[] GetLineByRoot(IKey inRoot)
        {
            CParsed parsed = _parsed.Find(p => p.Root == inRoot);
            if (parsed == null)
                return null;
            return parsed.Lines.ToArray();
        }


        internal IKey GetTree(string inFileName, object inContextData, ILogPrinter inLogger)
        {
            CParsed parsed = _parsed.Find(p => string.Equals(p.FileName, inFileName, StringComparison.InvariantCultureIgnoreCase));
            if (parsed != null)
                return parsed.Root;

            string text = _owner.GetTextFromFile(inFileName, inContextData);
            if (string.IsNullOrEmpty(text))
                return null;

            return Parse(inFileName, text, inLogger, inContextData);
        }
    }
}
