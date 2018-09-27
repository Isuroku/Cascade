using System.Collections.Generic;

namespace CascadeParser
{
    public interface ITokenTemplate
    {
        ETokenType GetTokenType();
        string GetText();
        bool CheckPassed(string inLine, int inCharIndex);
    }

    public class CTokenTemplate: ITokenTemplate
    {
        string _template;
        ETokenType _token_type;

        public ETokenType GetTokenType() { return _token_type; }
        public string GetText() { return _template; }

        public CTokenTemplate(ETokenType token_type, string template)
        {
            _token_type = token_type;
            _template = template;
        }

        public bool CheckPassed(string inLine, int inCharIndex)
        {
            if (_template.Length > inLine.Length - inCharIndex)
                return false;

            bool diff = false;
            for(int i = 0; i < _template.Length && !diff; ++i)
                diff = _template[i] != inLine[inCharIndex + i];

            return !diff;
        }
    }

    public class CTokenFinder
    {
        static CTokenFinder _instance;
        public static CTokenFinder Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CTokenFinder();
                return _instance;
            }
        }

        const int TAB_LENGTH = 4;

        public const ETokenType COMMAND_PREFIX = ETokenType.Sharp;

        List<ITokenTemplate> _templates = new List<ITokenTemplate>();

        public CTokenFinder()
        {
            _templates.Add(new CTokenTemplate(ETokenType.Comment, "//"));
            _templates.Add(new CTokenTemplate(ETokenType.Comma, ","));
            _templates.Add(new CTokenTemplate(ETokenType.Colon, ":"));
            _templates.Add(new CTokenTemplate(ETokenType.RecordDivider, "--"));
            _templates.Add(new CTokenTemplate(ETokenType.Sharp, "#"));
            _templates.Add(new CTokenTemplate(ETokenType.True, "true"));
            _templates.Add(new CTokenTemplate(ETokenType.True, "True"));
            _templates.Add(new CTokenTemplate(ETokenType.False, "false"));
            _templates.Add(new CTokenTemplate(ETokenType.False, "False"));
            _templates.Add(new CTokenTemplate(ETokenType.AddKey, "+"));
            //_templates.Add(new CTokenTemplate(ETokenType.OverrideKey, "^"));
            //_templates.Add(new CTokenTemplate(ETokenType.OpenBrace, "{"));
            //_templates.Add(new CTokenTemplate(ETokenType.CloseBrace, "}"));
            //_templates.Add(new CTokenTemplate(ETokenType.OpenSqBracket, "["));
            //_templates.Add(new CTokenTemplate(ETokenType.CloseSqBracket, "]"));
        }

        public CToken[] GetTokens(CSentense inSentense)
        {
            if (string.IsNullOrEmpty(inSentense.Text))
                return new CToken[0];

            string line = inSentense.Text;
            Tuple<CToken, int> comment_pos = FindComments(inSentense);
            CToken comment = comment_pos.Item1;
            if (comment != null)
                line = line.Substring(0, comment_pos.Item2);

            List<CToken> lst = new List<CToken>();

            int tab_shift = inSentense.Rank * TAB_LENGTH;
            int lnum = inSentense.LineNumber;

            int world_pos = 0;
            int world_len = 0;
            int i = 0;
            bool open_qute = false;
            while (i < line.Length)
            {
                bool quote = line[i] == '"';
                if(quote)
                    open_qute = !open_qute;

                bool space = false;
                ITokenTemplate tmp = null;
                if (!open_qute)
                {
                    space = line[i] == ' ';
                    tmp = _templates.Find(tt => tt.CheckPassed(line, i));
                }

                bool world_break = space || quote || tmp != null;
                if (world_break)
                {
                    world_len = i - world_pos;
                    if(world_len > 0)
                        AddWorld(i, world_pos, line, lst, lnum, tab_shift);

                    if(tmp == null)
                        world_pos = i + 1;
                    else
                        world_pos = i + tmp.GetText().Length;
                }

                if (tmp == null)
                    i++;
                else
                {
                    lst.Add(new CToken(tmp.GetTokenType(), tmp.GetText(), inSentense.LineNumber, i + inSentense.Rank * TAB_LENGTH));
                    i += tmp.GetText().Length;
                }
            }

            world_len = i - world_pos;
            if (world_len > 0)
                AddWorld(i, world_pos, line, lst, lnum, tab_shift);

            if (comment != null)
                lst.Add(comment);

            return lst.ToArray();
        }

        void AddWorld(int curr_pos, int world_start_pos, string line, List<CToken> out_lst, int lnum, int tab_shift)
        {
            int len = curr_pos - world_start_pos;
            string word = line.Substring(world_start_pos, len);

            bool only_digit = true;
            int point_count = 0;
            bool minus_was = false;
            for (int i = 0; i < word.Length && only_digit; ++i)
            {
                char c = word[i];

                bool point = c == '.';
                point_count += point ? 1 : 0;

                bool minus = c == '-' && i == 0;
                if (minus)
                    minus_was = true;

                only_digit = (minus || point || char.IsDigit(c)) && point_count < 2;
            }

            ETokenType tt = ETokenType.Word;
            if (only_digit)
            {
                if (point_count > 0)
                    tt = ETokenType.Float;
                else if(word.Length <= 20)
                {
                    //long:  -9223372036854775808   to 9223372036854775807
                    //ulong: 0                      to 18446744073709551615
                    if (!minus_was && word.Length == 20)
                        tt = ETokenType.UInt;
                    else
                        tt = ETokenType.Int;
                }
            }
            out_lst.Add(new CToken(tt, word, lnum, world_start_pos + tab_shift));
        }

        Tuple<CToken, int> FindComments(CSentense inSentense)
        {
            if (string.IsNullOrEmpty(inSentense.Text))
                return new Tuple<CToken, int>(null, 0);

            CToken comment = null;
            string comm_str = GetTokenString(ETokenType.Comment);
            int pos = inSentense.Text.IndexOf(comm_str);
            if (pos == -1)
                return new Tuple<CToken, int>(null, 0);

            string scomm = inSentense.Text.Substring(pos + comm_str.Length);
            comment = new CToken(ETokenType.Comment, scomm, inSentense.LineNumber, pos + inSentense.Rank * TAB_LENGTH);

            return new Tuple<CToken, int>(comment, pos);
        }

        public string GetTokenString(ETokenType tt)
        {
            ITokenTemplate token = _templates.Find(t => t.GetTokenType() == tt);
            if (token == null)
                return string.Empty;
            return token.GetText();
        }
    }
}
