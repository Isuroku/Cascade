
namespace Parser
{
    public enum EErrorCode
    {
        RecordDividerMustBeAloneInLine,
        ErrorCommentPosition,
        ColonErrorPos,
        WrongTokenInTail,
        SharpErrorPos,
        AloneDividerInLine,
        StrangeHeadType,
        HeadWithoutValues,
        EmptyCommand,
        UnknownCommand,
        UnknownCommandName,
        NotEvenQuoteCount,
        TooDeepRank,
        RecordBeforeRecordDividerDoesntPresent,
        CantTransferName,
    }

    public enum EInternalErrorCode
    {
    }
}
