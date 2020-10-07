using System.Globalization;

namespace MathExpressionParser
{
    public class CToken
    {
        public ETokenType TokenType { get; private set; }
        int _position;
        public string Text { get; private set; }
        public int StartLineIndex { get { return _position; } }
        public int EndLineIndex { get { return _position + Text.Length; } }

        public CToken(ETokenType token_type, string inText, int inPos)
        {
            TokenType = token_type;
            _position = inPos;
            Text = inText;
        }

        public override string ToString()
        {
            return $"{TokenType} [{Text}]";
        }

        public EBinOp GetBinOp()
        {
            switch(TokenType)
            {
				case ETokenType.And: return EBinOp.And;
				case ETokenType.Or: return EBinOp.Or;
				case ETokenType.Power: return EBinOp.Power;
                case ETokenType.Mult: return EBinOp.Mult;
                case ETokenType.Div: return EBinOp.Div;
                case ETokenType.Sum: return EBinOp.Sum;
                case ETokenType.Diff: return EBinOp.Diff;
            }
            return EBinOp.Undefined;
        }

        public double GetFloatValue()
        {
            if (TokenType != ETokenType.Float && TokenType != ETokenType.Int && TokenType != ETokenType.UInt)
                return 0;

            return double.Parse(Text, NumberStyles.Any, Utils.GetCultureInfoFloatPoint());
        }
    }
}
