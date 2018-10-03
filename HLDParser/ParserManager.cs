using System;
using System.Collections.Generic;

namespace CascadeParser
{
    public interface IParserOwner
    {
        string GetTextFromFile(string inFileName);
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

            public CSupportOwner(CParserManager owner)
            {
                _owner = owner;
            }

            public ILogger GetLogger()
            {
                return _owner._loger;
            }

            public IKey GetTree(string inFileName)
            {
                return _owner.GetTree(inFileName);
            }
        }

        List<CParsed> _parsed = new List<CParsed>();

        CSentenseDivider _sentenser = new CSentenseDivider();

        CLoger _loger;

        IParserOwner _owner;

        CSupportOwner _supporter;

        public CParserManager(IParserOwner owner, ILogPrinter inLogPrinter)
        {
            _owner = owner;
            _loger = new CLoger(inLogPrinter);
            _supporter = new CSupportOwner(this);
        }

        public IKey Parse(string inText)
        {
            return Parse(string.Empty, inText);
        }

        public IKey Parse(string inFileName, string inText)
        {
            if(!string.IsNullOrEmpty(inFileName))
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

            CKey root = CTreeBuilder.Build(lines, _supporter);

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


        public IKey GetTree(string inFileName)
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
