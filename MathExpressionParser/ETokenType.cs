
namespace MathExpressionParser
{
    public enum ETokenType
    {
        Word,
        Int,
        UInt,
        Float,
        Comma, // ,
        OpenBrace, //(
        CloseBrace, //)
        Sum, //+
        Diff, //-
        Mult, // *
        Div, // /
        Power, //^
    }
}
