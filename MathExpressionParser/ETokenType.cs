using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpressionParser
{
    public enum ETokenType
    {
        Word,
        Int,
        UInt,
        Float,
        OpenBrace, //(
        CloseBrace, //)
        Sum, //+
        Diff, //-
        Mult, // *
        Div, // /
        Power, //^
    }
}
