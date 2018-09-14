using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
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
        string[] _command_params;
        public string[] CommandParams { get { return _command_params; } }

        int _error_count;
        public int ErrorCount { get { return _error_count; } }

        public int LineNumber { get { return _sentense.LineNumber; } }

        public int Rank { get { return _sentense.Rank; } }

        public SPosition Position { get { return new SPosition(LineNumber, 0); } }


        public CTokenLine()
        {
        }

        public void Init(CSentense sentense, CLoger inLoger)
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

            if (_tokens.Length == 1)
            {
                if (IsDataType(_tokens[0].TokenType))
                    _tail = new CToken[1] { _tokens[0] };
                else if(_tokens[0].TokenType != ETokenType.RecordDivider)
                    inLoger.LogError(EErrorCode.AloneDividerInLine, _tokens[0]);
                return;
            }

            int start_for_tail = 0;
            if(_tokens[1].TokenType == ETokenType.Colon)
            {
                _head = _tokens[0];
                start_for_tail = 2;
                if (_tokens[0].TokenType != ETokenType.Word)
                    inLoger.LogError(EErrorCode.StrangeHeadType, _tokens[0]);
            }

            List<CToken> lst = new List<CToken>();
            for(int i = start_for_tail; i < _tokens.Length; ++i)
            {
                if(IsDataType(_tokens[i].TokenType))
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

            List<string> lst = new List<string>();
            for (int i = 2; i < _tokens.Length; ++i)
            {
                if (IsDataType(_tokens[i].TokenType))
                    lst.Add(_tokens[i].Text);
            }
            _command_params = lst.ToArray();
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

        public static bool IsDataType(ETokenType inType)
        {
            return inType == ETokenType.Word || inType == ETokenType.Float || inType == ETokenType.Int || 
                inType == ETokenType.True || inType == ETokenType.False;
        }
    }
}
