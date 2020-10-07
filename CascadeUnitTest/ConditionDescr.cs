using CascadeSerializer;
using System.Collections;
using System.Collections.Generic;

namespace CascadeUnitTest
{
    public enum EConditionType
    {
        Undefined,

        True,

        PlayerHasFund,
        PlayerHasProduct,
        PlayerFitProduct,
        //PlayerFitUpgrade, - был реализован, но почему-то переделали - переименован в PlayerHasUpgrade
        PlayerHasUpgrade,
        PlayerPickupProduct,
        PlayerBreakProduct,
        PlayerDropProduct,
        PlayerDockedAtCity,
        PlayerLanded, //на событие посадки
        PlayerIsLanded, //на состояние - сидит на колесах
        PlayerCrashed,

        CityIsOpen,
        CityHasFund,
        CityHasPhase,

        QuestState,
        QuestStateTime,

        CharacterInsideShip,

        PlayerCargoEmpty,
        PlayerNavigatorEmpty,
        PlayerNavigatorTargetAdded,
        PlayerNavigatorTargetRemoved,
        PlayerFuel,
        PlayerAtLandingZone,
        IsPlayerInHangarCapturePoint,
        PlayerDistanceToTarget,
        PlayerCargoIsOpen,
        PlayerShipCanGetNearestCargo,
        IsPlayerShipInWindType,
        PlayerShipAggregateHealth,
        PlayerShipMalfunction,
        PlayerShipVelocity,
        PlayerShipOverSurface,
        PlayerShipAltitude,
        PlayerShipAcceleration,
        HistoryEvent,
        PlayerBuyOrSellProduct,
        PlayerHasPassenger,
        PlayerInCity,
        CharacterInCity,
        PlayerShipParam,

        PlayerHelpDone,
        PlayerInMenu,
        PlayerHasPlaneModeUpgrade,
        PlayerIsInHangar,
        PlayerIsInTrade,
        PlayerShipIsCruise,
        PlayerShipName,
        PlayerShipOverload,
        IsCanOpenMap,
        PlayerShipAboveCriticalAltitude,
        PlayerShipFlapsOpen,
        PlayerShipJetpack,
        PlayerShipPlane,
        PlayerShipWheels,

        PlayerEnterCity,
        PlayerLeaveCity,
        PlayerInsideCity,
        PlayerInsideCityTime,
        PlayerOutsideCityTime,

        InputShip_ShiftActions1,
        InputShip_ShiftActions2,

        CharacterState,
        CharacterStateTime,

        IsHudActive,
        MessagesCount,
        MessageReply,
        IsWindViewActive,

        TargetDistanceFrom,
        ContainerContentIsDead,
    }

    [CascadeObject(CascadeSerializer.MemberSerialization.Fields)]
    public class CConditionDescr
    {
        protected EConditionType _condition_type;

        protected byte _group;

        protected bool _invert;

        protected float _count;

        protected string _object_name;

        protected string _subject_name;

        protected string _fund;

        protected EShipAggregateType _ship_aggregate;

        protected EMalfunctionType _malfunction;

        protected EEntityPlaces _entity_place;

        protected bool _landed;

        protected bool _resetable;

        public byte Group { get { return _group; } }
        public bool Invert { get { return _invert; } }
        public float Count { get { return _count; } }
        public string ObjectName { get { return _object_name; } }
        public string Fund { get { return _fund; } }

        public bool Resetable { get { return _resetable; } }

        public EConditionType GetConditionType() { return _condition_type; }


        public CConditionDescr() { }

        public override string ToString()
        {
            return string.Format("{0}: Group {1}, Invert {2}, Count {3}", _condition_type, _group, _invert, _count);
        }
    }


    [CascadeObject(CascadeSerializer.MemberSerialization.Fields)]
    public class CConditionDescrs : IEnumerable<CConditionDescr>
    {
        [CascadeProperty("ConditionDescrs")]
        private List<CConditionDescr> _lst = new List<CConditionDescr>();

        public IEnumerator<CConditionDescr> GetEnumerator()
        {
            return _lst.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _lst.GetEnumerator();
        }

        public CConditionDescrs()
        {
        }

        public CConditionDescrs(IEnumerable<CConditionDescr> conditions)
        {
            Add(conditions);
        }

        public bool IsEmpty()
        {
            return _lst.Count == 0;
        }

        public void Add(CConditionDescr condition)
        {
            if (condition == null)
                return;

            _lst.Add(condition);
        }

        public void Add(IEnumerable<CConditionDescr> conditions)
        {
            if (conditions == null)
                return;

            foreach (var condition in conditions)
                Add(condition);
        }

    }
}
