
namespace CascadeParser
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
        PathEmpty,
        CantFindRootInFile,
        CantFindKey,
        ElementWithNameAlreadyPresent,
        CantResolveLine,
        KeyMustHaveParent,
        CantChangeName,
        CantAddComment,
        NextArrayKeyNameAlreadySetted,
        NextArrayKeyNameMissParent,
        NextLineCommentMissParent,
    }
}
