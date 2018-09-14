
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
        EmptyCommand,
        CommandMustHaveOneParam,
        UnknownCommand,
        UnknownCommandName,
        NotEvenQuoteCount,
    }

    public enum EInternalErrorCode
    {
        EmptyTailAdded
    }
}
