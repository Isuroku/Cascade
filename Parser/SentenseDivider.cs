using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class CSentense
    {
        string _text;
        int _rank;
        int _line_number;

        public string Text { get { return _text; } }
        public int Rank { get { return _rank; } }
        public int LineNumber { get { return _line_number; } }

        //List<CTokenTemplate> _tokens = new List<CTokenTemplate>();

        public CSentense(string inText, int inLineNumber)
        {
            _text = inText.Trim(' ');
            _rank = GetFirstTabCount(_text);
            _text = _text.Substring(_rank);
            _line_number = inLineNumber;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", _rank, _text);
        }

        int GetFirstTabCount(string inLine)
        {
            int i = 0;
            int count = inLine.Length;
            char tab = '\t';
            while (i < count && inLine[i] == tab)
                ++i;
            return i;
        }
    }

    public class CSentenseDivider
    {
        List<CSentense> _sentenses = new List<CSentense>();

        string _text;

        public int SentenseCount { get { return _sentenses.Count; } }

        public CSentense this[int index] { get { return _sentenses[index]; } }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _sentenses.Count; ++i)
                sb.AppendLine(_sentenses[i].ToString());

            return sb.ToString();
        }

        void Clear()
        {
            _text = string.Empty;
            _sentenses.Clear();
        }

        public void ParseText(string inRawText)
        {
            Clear();

            _text = inRawText;

            string[] lines = inRawText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; ++i)
            {
                string[] lines2 = lines[i].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < lines2.Length; ++j)
                    _sentenses.Add(new CSentense(lines2[j], i));
            }
        }
    }
}
