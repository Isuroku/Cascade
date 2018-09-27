using System.Globalization;

namespace CascadeParser
{
    public class CToken
    {
        ETokenType _token_type;
        int _line_number;
        int _line_position;
        string _text;

        int _error_count;

        public ETokenType TokenType { get { return _token_type; } }
        public string Text { get { return _text; } }
        public int LinePosition { get { return _line_position; } }
        public int LineNumber { get { return _line_number; } }
        public SPosition Position { get { return new SPosition(_line_number, _line_position); } }
        public bool IsErrorPresent { get { return _error_count > 0; } }
        public int ErrorCount { get { return _error_count; } }

        public CToken(ETokenType token_type, string text, int line_number, int line_position)
        {
            _token_type = token_type;
            _text = text;
            _line_number = line_number;
            _line_position = line_position;
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

        internal void CheckInLine(CToken[] inTokensInLine, int inMyIndex, CLoger inLoger)
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
