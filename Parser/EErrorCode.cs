
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
        LocalPathEmpty,
        CantFindRootInFile,
        CantFindKey,
    }

    public enum EInternalErrorCode
    {
    }
}
