
namespace CascadeParser
{
    public enum ETokenType
    {
        Word,
        Int,
        UInt,
        Float,
        True,
        False,
        Comment,
        //OpenBrace, //{
        //CloseBrace,
        //OpenSqBracket, //[
        //CloseSqBracket,
        Comma, //,
        Colon, //:
        RecordDivider, //--
        Sharp, //#
        AddKey, //+
        //OverrideKey, //^
    }
}
