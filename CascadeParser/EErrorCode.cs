
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
        DublicateCommandParam,
        DublicateKeyName,
        NotEvenQuoteCount,
        RecordBeforeRecordDividerDoesntPresent,
        CantTransferName,
        PathEmpty,
        CantFindInsertFile,
        CantFindKey,
        ElementWithNameAlreadyPresent,
        CantResolveLine,
        KeyMustHaveParent,
        CantChangeName,
        CantAddComment,
        NextArrayKeyNameAlreadySetted,
        NextArrayKeyNameMissParent,
        NextLineCommentMissParent,

        LineMustHaveHead
    }
}
