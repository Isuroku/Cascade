using System;
using System.Collections.Generic;
using System.Text;

namespace HLDParser
{
    public class CTokenLine
    {
        CSentense _sentense;

        CToken[] _tokens;
        public CToken[] Tokens { get { return _tokens; } }

        CToken _comments;

        CToken _head;
        public CToken Head { get { return _head; } }
        public bool IsHeadEmpty { get { return _head == null; } }

        CToken[] _tail;
        public CToken[] Tail { get { return _tail; } }
        public bool IsTailEmpty { get { return _tail == null || _tail.Length == 0; } }
        public int TailLength { get { return _tail == null ? 0 : _tail.Length; } }

        ECommands _command;
        public ECommands Command { get { return _command; } }
        CCommandParams _command_params = new CCommandParams();
        internal CCommandParams CommandParams { get { return _command_params; } }

        int _error_count;
        public int ErrorCount { get { return _error_count; } }

        public int LineNumber { get { return _sentense.LineNumber; } }

        public int Rank { get { return _sentense.Rank; } }

        public SPosition Position { get { return new SPosition(LineNumber, 0); } }

        EKeyAddingMode _addition_mode = EKeyAddingMode.AddUnique;
        public EKeyAddingMode AdditionMode { get { return _addition_mode; } }

        internal CTokenLine()
        {
        }

        internal void Init(CSentense sentense, CLoger inLoger)
        {
            _sentense = sentense;
            BuildTokens(sentense, inLoger);
            FindHeadAndTail(inLoger);
            FindCommandParams(inLoger);
        }

        void BuildTokens(CSentense sentense, CLoger inLoger)
        {
            CToken[] tokens = CTokenFinder.Instance.GetTokens(sentense);

            if (tokens.Length > 0 && tokens[tokens.Length - 1].TokenType == ETokenType.Comment)
            {
                _tokens = new CToken[tokens.Length - 1];
                Array.Copy(tokens, _tokens, _tokens.Length);
                _comments = tokens[tokens.Length - 1];
            }
            else
                _tokens = tokens;

            _error_count += Check(inLoger);
        }

        void FindHeadAndTail(CLoger inLoger)
        {
            if (_tokens.Length == 0)
                return;

            if (_tokens[0].TokenType == ETokenType.Sharp)
                return;

            if (_tokens[0].TokenType == ETokenType.RecordDivider)
                return;

            int cur_index = 0;
            CToken toc = _tokens[cur_index];
            if (toc.TokenType == ETokenType.AddKey)
            {
                _addition_mode = EKeyAddingMode.Add;
                cur_index++;
            }
            //if (toc.TokenType == ETokenType.OverrideKey)
            //{
            //    _addition_mode = EKeyAddingMode.Override;
            //    cur_index++;
            //}

            int token_count = _tokens.Length - cur_index;
            if (cur_index == 0 && token_count == 1 && Utils.IsDataType(toc.TokenType))
            {//written only one value
                _tail = new CToken[1] { _tokens[0] };
                return;
            }

            if(token_count < 2)
            {
                inLoger.LogError(EErrorCode.CantResolveLine, this);
                return;
            }

            toc = _tokens[cur_index + 1];
            if(toc.TokenType == ETokenType.Colon)
            {
                _head = _tokens[cur_index];
                cur_index += 2;
                if (_head.TokenType != ETokenType.Word)
                    inLoger.LogError(EErrorCode.StrangeHeadType, _head);
            }
            
            List<CToken> lst = new List<CToken>();
            for(int i = cur_index; i < _tokens.Length; ++i)
            {
                if(Utils.IsDataType(_tokens[i].TokenType))
                    lst.Add(_tokens[i]);
            }
            _tail = lst.ToArray();
        }

        void FindCommandParams(CLoger inLoger)
        {
            if (_tokens.Length == 0)
                return;

            if (_tokens[0].TokenType != ETokenType.Sharp)
                return;

            if (_tokens.Length < 2)
            {
                inLoger.LogError(EErrorCode.UnknownCommand, this);
                return;
            }

            ECommands[] coms = (ECommands[])Enum.GetValues(typeof(ECommands));
            for (int i = 0; i < coms.Length && _command == ECommands.None; ++i)
            {
                if (string.Equals(coms[i].ToString(), _tokens[1].Text, StringComparison.InvariantCultureIgnoreCase))
                    _command = coms[i];
            }

            if (_command == ECommands.None)
            {
                inLoger.LogError(EErrorCode.UnknownCommandName, this);
                return;
            }


            int curr_index = 2;
            while(curr_index < _tokens.Length)
            {
                bool triplet = false;
                if(_tokens.Length - curr_index >= 3)
                {
                    int colon_index = curr_index + 1;
                    triplet = _tokens[colon_index].TokenType == ETokenType.Colon;
                    if(triplet)
                    {
                        _command_params.Add(_tokens[curr_index].Text, _tokens[curr_index + 2].Text);
                        curr_index += 3;
                    }
                }

                if(!triplet)
                {
                    _command_params.Add(_tokens[curr_index].Text, string.Empty);
                    curr_index += 1;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[Line {0}, Rank {1}] ", Position.Line.ToString("D4"), Rank);
            for (int i = 0; i < _sentense.Rank; ++i)
                sb.Append('\t');

            for (int j = 0; j < _tokens.Length; j++)
            {
                sb.Append(_tokens[j]);
                sb.Append(" ");
            }

            if(_comments != null)
                sb.Append(_comments);

            return sb.ToString();
        }

        public bool IsRecordDivider()
        {
            return _tokens.Length == 1 && _tokens[0].TokenType == ETokenType.RecordDivider;
        }

        public bool IsCommandLine()
        {
            return _command != ECommands.None;
        }

        public bool IsEmpty()
        {
            return _tokens.Length == 0;
        }

        int Check(CLoger inLoger)
        {
            int ecount = 0;
            for(int i = 0; i < _tokens.Length; ++i)
            {
                _tokens[i].CheckInLine(_tokens, i, inLoger);
                ecount += _tokens[i].ErrorCount;
            }

            return ecount;
        }
    }
}
