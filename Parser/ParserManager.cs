using System.Collections.Generic;

namespace Parser
{
    public class CParserManager
    {
        CSentenseDivider _sentenser = new CSentenseDivider();

        List<CTokenLine> _lines = new List<CTokenLine>();
        public IReadOnlyCollection<CTokenLine> Lines { get { return _lines; } }

        CLoger _loger;

        CKey _root;
        public CKey Root { get { return _root; } }

        public CParserManager(ILogPrinter inLogPrinter)
        {
            _loger = new CLoger(inLogPrinter);
        }

        public CKey Parse(string inText)
        {
            _root = null;

            _lines.Clear();

            _sentenser.ParseText(inText, _loger);

            for (int i = 0; i < _sentenser.SentenseCount; i++)
            {
                CSentense sentense = _sentenser[i];
                CTokenLine tl = new CTokenLine();
                tl.Init(sentense, _loger);
                _lines.Add(tl);
            }

            _root = CTreeBuilder.Build(_lines, _loger);

            return _root;
        }

        public CTokenLine GetLine(int index)
        {
            return _lines[index];
        }
    }
}
