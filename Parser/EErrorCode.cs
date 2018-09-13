
namespace Parser
{
    public enum EErrorCode
    {
        ErrorTokenPosition,
        RecordDividerMustBeAloneInLine,
        ErrorCommentPosition,
        ColonErrorPos,
        WrongTokenInTail,
        KeyMustHaveWordType,
        SharpErrorPos,
        AloneDividerInLine,
        StrangeHeadType,
        HeadWithoutValues,
        UndefinedLine,
        UnwaitedRank,
    }

    public enum EInternalErrorCode
    {
        EmptyTailAdded
    }
}
