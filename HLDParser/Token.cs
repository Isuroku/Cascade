using System.Globalization;

namespace CascadeParser
{
    public class CToken
    {
        ETokenType _token_type;
        SPosition _position;
        string _text;

        int _error_count;

        public ETokenType TokenType { get { return _token_type; } }
        public string Text { get { return _text; } }
        public int LinePosition { get { return _position.Col; } }
        public int LineNumber { get { return _position.Line; } }
        public SPosition Position { get { return _position; } }
        public bool IsErrorPresent { get { return _error_count > 0; } }
        public int ErrorCount { get { return _error_count; } }

        public CToken(ETokenType token_type, string text, int line_number, int line_position):
            this(token_type, text, new SPosition(line_number, line_position))
        {
        }

        public CToken(ETokenType token_type, string text, SPosition inPos)
        {
            _token_type = token_type;
            _text = text;
            _position = inPos;
        }

        public override string ToString()
        {
            return $"{_token_type} [{_text}]";
        }

        public long GetIntValue()
        {
            if (_token_type != ETokenType.Int)
                return 0;
            
            return long.Parse(_text);
        }

        public ulong GetUIntValue()
        {
            if (_token_type != ETokenType.UInt)
                return 0;

            return ulong.Parse(_text);
        }

        public decimal GetFloatValue()
        {
            if (_token_type != ETokenType.Float)
                return 0;

            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            return decimal.Parse(_text, NumberStyles.Any, ci);
        }

        internal void CheckInLine(CToken[] inTokensInLine, int inMyIndex, ILogger inLoger)
        {
            switch(_token_type)
            {
                case ETokenType.RecordDivider:
                {
                    if (inTokensInLine.Length > 1)
                    {
                        _error_count++;
                        inLoger.LogError(EErrorCode.RecordDividerMustBeAloneInLine, this);
                    }
                }
                break;
                case ETokenType.Comment:
                {
                    if (inTokensInLine.Length > 1)
                    {
                        _error_count++;
                        inLoger.LogError(EErrorCode.ErrorCommentPosition, this);
                    }
                }
                break;
                case ETokenType.Sharp:
                {
                    if (inMyIndex != 0)
                    {
                        _error_count++;
                        inLoger.LogError(EErrorCode.SharpErrorPos, this);
                    }
                }
                break;
            }
        }
    }
}
