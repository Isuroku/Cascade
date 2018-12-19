using System;
using System.Collections.Generic;

namespace MathExpressionParser
{
    internal class CTokenTemplate
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
            for (int i = 0; i < _template.Length && !diff; ++i)
                diff = _template[i] != inLine[inCharIndex + i];

            return !diff;
        }
    }

    public static class CTokenFinder
    {
        static CTokenTemplate[] _templates = new CTokenTemplate[] 
        {
            new CTokenTemplate(ETokenType.OpenBrace, "("),
            new CTokenTemplate(ETokenType.CloseBrace, ")"),
            new CTokenTemplate(ETokenType.Sum, "+"),
            new CTokenTemplate(ETokenType.Diff, "-"),
            new CTokenTemplate(ETokenType.Mult, "*"),
            new CTokenTemplate(ETokenType.Div, "/"),
            new CTokenTemplate(ETokenType.Power, "^"),
        };

        public static CToken[] GetTokens(string inSentense)
        {
            if (string.IsNullOrEmpty(inSentense))
                return new CToken[0];

            string line = inSentense.TrimEnd(' ').TrimEnd('\t');

            List<CToken> lst = new List<CToken>();

            int world_pos = 0;
            int world_len = 0;
            int i = 0;
            while (i < line.Length)
            {
                bool space = line[i] == ' ';
                CTokenTemplate tmp = Array.Find(_templates, tt => tt.CheckPassed(line, i));

                bool world_break = space || tmp != null;
                if (world_break)
                {
                    world_len = i - world_pos;
                    if (world_len > 0)
                        AddWorld(i, world_pos, line, lst);

                    if (tmp == null)
                        world_pos = i + 1;
                    else
                        world_pos = i + tmp.GetText().Length;
                }

                if (tmp == null)
                    i++;
                else
                {
                    lst.Add(new CToken(tmp.GetTokenType(), tmp.GetText(), i));
                    i += tmp.GetText().Length;
                }
            }

            world_len = i - world_pos;
            if (world_len > 0)
                AddWorld(i, world_pos, line, lst);

            return lst.ToArray();
        }

        static void AddWorld(int curr_pos, int world_start_pos, string line, List<CToken> out_lst)
        {
            string out_word;
            ETokenType tt = GetTokenType(curr_pos, world_start_pos, line, out out_word);
            out_lst.Add(new CToken(tt, out_word, world_start_pos));
        }

        internal static ETokenType GetTokenType(int curr_pos, int world_start_pos, string line, out string out_word)
        {
            int len = curr_pos - world_start_pos;
            out_word = line.Substring(world_start_pos, len);

            bool only_digit = true;
            int point_count = 0;
            bool minus_was = false;
            for (int i = 0; i < out_word.Length && only_digit; ++i)
            {
                char c = out_word[i];

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
                else if (out_word.Length <= 20)
                {
                    //long:  -9223372036854775808   to 9223372036854775807
                    //ulong: 0                      to 18446744073709551615
                    if (!minus_was && out_word.Length == 20)
                        tt = ETokenType.UInt;
                    else
                        tt = ETokenType.Int;
                }
            }
            return tt;
        }
    }
}
