
namespace Parser
{
    public enum ETokenType
    {
        Word,
        Int,
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
    }
}
