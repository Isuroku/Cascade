﻿using System.Collections.Generic;

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
        bool _only_pure_world;

        public ETokenType GetTokenType() { return _token_type; }
        public string GetText() { return _template; }

        public CTokenTemplate(ETokenType token_type, string template, bool only_pure_world)
        {
            _token_type = token_type;
            _template = template;
            _only_pure_world = only_pure_world;
        }

        public CTokenTemplate(ETokenType token_type, string template)
            :this(token_type, template, false)
        {

        }

        public bool CheckPassed(string inLine, int inCharIndex)
        {
            int rest_len = inLine.Length - inCharIndex;
            if (_template.Length > rest_len)
                return false;

            bool diff = false;
            for(int i = 0; i < _template.Length && !diff; ++i)
                diff = _template[i] != inLine[inCharIndex + i];

            if(!diff && _only_pure_world && _template.Length < rest_len)
            {
                char char_after_world = inLine[inCharIndex + _template.Length];
                diff = char.IsLetterOrDigit(char_after_world);
            }

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
            _templates.Add(new CTokenTemplate(ETokenType.True, "true", true));
            _templates.Add(new CTokenTemplate(ETokenType.True, "True", true));
            _templates.Add(new CTokenTemplate(ETokenType.False, "false", true));
            _templates.Add(new CTokenTemplate(ETokenType.False, "False", true));
            _templates.Add(new CTokenTemplate(ETokenType.AddKey, "+"));
            _templates.Add(new CTokenTemplate(ETokenType.OverrideKey, "^"));
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
                line = line.Substring(0, comment_pos.Item2).TrimEnd(' ').TrimEnd('\t');

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
            Tuple<ETokenType, string> tt = Utils.GetTokenType(curr_pos, world_start_pos, line);
            out_lst.Add(new CToken(tt.Item1, tt.Item2, lnum, world_start_pos + tab_shift));
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
