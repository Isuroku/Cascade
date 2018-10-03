using ReflectionSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CascadeUnitTest
{
    [CascadeObject(ReflectionSerializer.MemberSerialization.Fields)]
    public class CCharacterDescr : CCommonDescr<string>
    {
        private ECharacterType _character_type;
        public ECharacterType CharacterType { get { return _character_type; } }

        private string _default_prefab;

        public enum EMoveType { Walk = 0, Fly }

        private EMoveType _move_type;
        public EMoveType MoveType { get { return _move_type; } }

        private CMoverDescr _mover_descr;
        public CMoverDescr MoverDescr { get { return _mover_descr; } }

        private CAIBrainDescr _brain_descr;
        public CAIBrainDescr BrainDescr { get { return _brain_descr; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if(!base.Equals(obj))
                return false;

            var v = obj as CCharacterDescr;

            return Equals(_character_type, v._character_type) &&
                Equals(_default_prefab, v._default_prefab) &&
                Equals(_move_type, v._move_type) &&
                Equals(_mover_descr, v._mover_descr) &&
                Equals(_brain_descr, v._brain_descr);
        }

        public CCharacterDescr()
        {
            _default_prefab = string.Empty;
        }

        public CCharacterDescr(string inName) : base(inName)
        {
            _default_prefab = string.Empty;
        }

        public string GetDefaultPrefab()
        {
            return _default_prefab;
        }


        public static CCharacterDescr CreateTestObject()
        {
            var descr = new CCharacterDescr("TestCharDescr");

            descr._mover_descr = CMoverDescr.CreateTestObject();
            descr._brain_descr = CAIBrainDescr.CreateTestObject();

            return descr;
        }
    }

    public enum EAIPathType
    {
        HumanWay = 0,
        CarRoad,
        AirRoad,
        GlobalFlyPath,
    }

    [CascadeObject(ReflectionSerializer.MemberSerialization.Fields)]
    public class CMoverDescr
    {
        [CascadeProperty(Name = "ExactlyForwardAngle", Default = 3f)]
        private float _exactly_forward;
        public float ExactlyForwardAngle { get { return _exactly_forward; } }

        [CascadeProperty(Default = 70f)]
        private float _move_angle;
        public float MoveAngle { get { return _move_angle; } }

        [CascadeProperty("AIPathType")]
        private EAIPathType _ai_path_type;
        public EAIPathType AIPathType { get { return _ai_path_type; } }

        [CascadeProperty(Default = 50f)]
        private float _ignor_pathfinder_distance;
        public float IgnorPathfinderDistance { get { return _ignor_pathfinder_distance; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CMoverDescr;

            return Equals(_exactly_forward, v._exactly_forward) &&
                Equals(_move_angle, v._move_angle) &&
                Equals(_ai_path_type, v._ai_path_type) &&
                Equals(_ignor_pathfinder_distance, v._ignor_pathfinder_distance);
        }

        public static CMoverDescr CreateTestObject()
        {
            var descr = new CMoverDescr();

            descr._move_angle = 70;
            descr._exactly_forward = 3;
            descr._ignor_pathfinder_distance = 50;

            return descr;
        }
    }

    public enum ECharacterType
    {
        Simple,
        Passenger
    }

    [CascadeObject(ReflectionSerializer.MemberSerialization.Fields)]
    public abstract class CCommonDescr<T>
    {
        protected T _name;
        public T Name { get { return GetName(); } }

        [CascadeIgnore]
        private int _index;
        public int Index { get { return _index; } }

        protected CCommonDescr()
        {
            _name = default(T);
        }

        protected CCommonDescr(T inName)
        {
            _name = inName;
        }

        public void SetIndex(int inIndex)
        {
            _index = inIndex;
        }

        public virtual bool CheckValid(string inPath) { return true; }

        protected virtual T GetName() { return _name; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", GetType().Name, _name);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CCommonDescr<T>;

            return Equals(_name, v._name);
        }
    }

    [CascadeObject(ReflectionSerializer.MemberSerialization.Fields)]
    public class CAIBrainDescr
    {
        private string[] _person_names;
        public string[] PersonNames { get { return _person_names; } }

        private CStringGroupDescr[] _stringGroupOverrides;
        public CStringGroupDescr[] StringGroupOverrides { get { return _stringGroupOverrides; } }

        private bool _always_active;

        public CAIBehaviorConditionDescr[] Conditions;

        public CAIBehaviorDescr[] BehaviorDescrs;

        private bool _destroy_on_crash;
        public bool DestroyOnCrash { get { return _destroy_on_crash; } }

        private bool _start_invisible;
        public bool StartInvisible { get { return _start_invisible; } }

        [CascadeProperty("OnCrash_ActionDescrs")]
        private List<CAIActionDescrs> _oncrash_actionDescrs;
        public IEnumerable<CAIActionDescrs> OnCrashActions
        {
            get
            {
                if (_oncrash_actionDescrs == null)
                    _oncrash_actionDescrs = new List<CAIActionDescrs>();
                return _oncrash_actionDescrs;
            }
        }

        [CascadeProperty("LocalizationKeys")]
        private CAILocKeys _loc_keys;

        public CAILocKeys LocalizationKeys
        {
            get
            {
                if (_loc_keys == null)
                    _loc_keys = new CAILocKeys();
                return _loc_keys;
            }
        }

        private CAIBehaviorStateDescr[] _states;
        public CAIBehaviorStateDescr[] States { get { return _states; } }

        [CascadeIgnore]
        private int[] _state_indexes;
        public int[] StateIndexes { get { return _state_indexes; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CAIBrainDescr;

            return Utils.IsArrayEquals(_person_names, v._person_names) &&
                Utils.IsArrayEquals(_stringGroupOverrides, v._stringGroupOverrides) &&
                Utils.IsArrayEquals(Conditions, v.Conditions) &&
                Utils.IsArrayEquals(BehaviorDescrs, v.BehaviorDescrs) &&
                Utils.IsCollectionEquals(_oncrash_actionDescrs, v._oncrash_actionDescrs) &&
                Utils.IsCollectionEquals(_states, v._states) &&
                Equals(_always_active, v._always_active) &&
                Equals(_destroy_on_crash, v._destroy_on_crash) &&
                Equals(_start_invisible, v._start_invisible) &&
                Equals(_loc_keys, v._loc_keys);
        }


        public CAIBrainDescr()
        {
            if (_states == null)
                _states = new CAIBehaviorStateDescr[0];
        }

        internal static CAIBrainDescr CreateTestObject()
        {
            var descr = new CAIBrainDescr();

            descr._person_names = new string[] { "PersonName1", "PersonName2" };
            descr._start_invisible = true;
            descr._stringGroupOverrides = new CStringGroupDescr[]
            {
                new CStringGroupDescr("SGO1", new string[] { "sgo_text1", "sgo_text2" }),
                new CStringGroupDescr("SGO2", new string[] { "sgo_text3", "sgo_text4" })
            };
            descr.Conditions = new CAIBehaviorConditionDescr[]
            {
                CAIBehaviorConditionDescr.CreateTestObject()
            };
            descr.BehaviorDescrs = new CAIBehaviorDescr[]
            {
                CAIBehaviorDescr.CreateTestObject("behav1"),
                CAIBehaviorDescr.CreateTestObject("behav2"),
            };

            descr._oncrash_actionDescrs = new List<CAIActionDescrs>()
            {
                CAIActionDescrs.CreateTestObject()
            };
            descr._loc_keys = new CAILocKeys(new EBehaviourReportParam[] { EBehaviourReportParam.CurrentBehaviourActiveTime, EBehaviourReportParam.CurrentWorldTime });
            descr._states = new CAIBehaviorStateDescr[]
            {
                CAIBehaviorStateDescr.CreateTestObject("state1"),
                CAIBehaviorStateDescr.CreateTestObject("state2"),
            };

            return descr;
        }
    }

    [CascadeObject(ReflectionSerializer.MemberSerialization.Fields)]
    public class CConditionArrayTest
    {
        public CAIBehaviorConditionDescr[] Conditions;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CConditionArrayTest;

            return Utils.IsArrayEquals(Conditions, v.Conditions);
        }

        internal void Init()
        {
            Conditions = new CAIBehaviorConditionDescr[]
            {
                CAIBehaviorConditionDescr.CreateTestObject()
            };
        }
    }

    public enum EBehaviourReportParam
    {
        DistToSource,
        DistToDestination,
        DistSourceToDestination,
        PassangerTimeSourceToDestination,
        CurrentBehaviourActiveTime,
        CurrentBehaviourFirstActiveTime,
        DestinationMapName,
        CurrentWorldTime,
        SavedTimePeriod
    }

    [CascadeObject(ReflectionSerializer.MemberSerialization.Fields)]
    public class CAILocKeys
    {
        private string _source_target;
        public string SourceTarget { get { return _source_target ?? string.Empty; } }

        [CascadeProperty("DestinationTarget")]
        private string _dest_target;
        public string DestinationTarget { get { return _dest_target ?? string.Empty; } }

        private string _var_name;
        public string VarName { get { return _var_name ?? string.Empty; } }

        private EBehaviourReportParam[] _report_params;

        public EBehaviourReportParam[] ReportParams
        {
            get
            {
                if (_report_params == null)
                    _report_params = new EBehaviourReportParam[0];
                return _report_params;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CAILocKeys;

            return Utils.IsArrayEquals(_report_params, v._report_params) &&
                Equals(_source_target, v._source_target) &&
                Equals(_dest_target, v._dest_target) &&
                Equals(_var_name, v._var_name);
        }

        public override string ToString()
        {
            string s = string.Empty;

            if (!string.IsNullOrEmpty(_source_target))
                s += string.Format("SourceTarget: {0}; ", SourceTarget);

            if (!string.IsNullOrEmpty(_dest_target))
                s += string.Format("DestinationTarget: {0}; ", DestinationTarget);

            if (_report_params != null)
                s += string.Format("ReportParams: {0}; ", string.Join(", ", _report_params.Select(t => t.ToString()).ToArray()));

            return s;
        }

        public CAILocKeys() { }

        public CAILocKeys(EBehaviourReportParam[] report_params)
        {
            _report_params = report_params;
            _source_target = "source_target";
            _var_name = "var222";
        }
    }

    [CascadeObject(ReflectionSerializer.MemberSerialization.Fields)]
    public class CStringGroupDescr : CCommonDescr<string>
    {
        private string[] _strings;
        public string[] Strings { get { return _strings; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (!base.Equals(obj))
                return false;

            var v = obj as CStringGroupDescr;

            return Utils.IsArrayEquals(_strings, v._strings);
        }

        public CStringGroupDescr(string inName, string[] inStrings)
            : base(inName)
        {
            _strings = inStrings;
        }

        public CStringGroupDescr()
        {
        }
    }

    public enum EAIBehaviorConditionType
    {
        None,
        TargetDistance,
        OpenCargo,
        IsTakePlaceAsPassangerIn,
        InsideCity,
        IsLanded,
        IsNotDrawing,
        IsPassanger,
        InsideCityTime,
        StateTime,
        BehaviourFinished,
        MemoryFlag,
        SavedPositionDistance,
        SavedTimePeriod,
        MemoryFloat,
        PlayerIsInTrade,
        IsIdle
    }

    [CascadeObject(ReflectionSerializer.MemberSerialization.Fields)]
    public class CAIBehaviorConditionDescr
    {
        private string _alias;

        public string Alias
        {
            get
            {
                if (string.IsNullOrEmpty(_alias))
                    _alias = _condition_type.ToString();
                return _alias;
            }
        }

        private EAIBehaviorConditionType _condition_type = EAIBehaviorConditionType.None;
        public EAIBehaviorConditionType ConditionType { get { return _condition_type; } }

        protected bool _invert;
        public bool Invert { get { return _invert; } }

        protected float _count;
        public float Count { get { return _count; } }

        protected string _target_alias;
        public string TargetAlias { get { return _target_alias; } }

        protected string _object_name;
        public string ObjectName { get { return _object_name; } }

        [CascadeIgnore]
        int _index_in_list;
        public int Index { get { return _index_in_list; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CAIBehaviorConditionDescr;

            return Equals(_invert, v._invert) &&
                Equals(_count, v._count) &&
                Equals(_object_name, v._object_name) &&
                Equals(_target_alias, v._target_alias);
        }

        public CAIBehaviorConditionDescr() { }

        public CAIBehaviorConditionDescr(EAIBehaviorConditionType inCondType)
        {
            _condition_type = inCondType;
        }

        internal static CAIBehaviorConditionDescr CreateTestObject()
        {
            var descr = new CAIBehaviorConditionDescr();

            descr._alias = "BehavCond";
            descr._condition_type = EAIBehaviorConditionType.BehaviourFinished;
            descr._count = 10;
            descr._target_alias = "Target2";
            descr._object_name = "object_name_ww";

            return descr;
        }
    }

    [CascadeObject(ReflectionSerializer.MemberSerialization.Fields)]
    public class CAIBehaviorReportDescr
    {
        private string _text;
        public string Text { get { return _text ?? string.Empty; } }

        private string _source_target;
        public string SourceTarget { get { return _source_target ?? string.Empty; } }

        protected bool _source_world_target;

        [CascadeProperty("DestinationTarget")]
        private string _dest_target;
        public string DestinationTarget { get { return _dest_target ?? string.Empty; } }

        [CascadeProperty("DestinationWorldTarget")]
        protected bool _dest_world_target;

        private string _var_name;
        public string VarName { get { return _var_name ?? string.Empty; } }

        private EBehaviourReportParam[] _report_params;
        public EBehaviourReportParam[] ReportParams { get { return _report_params ?? new EBehaviourReportParam[0]; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CAIBehaviorReportDescr;

            return Utils.IsArrayEquals(_report_params, v._report_params) &&
                Equals(_text, v._text) &&
                Equals(_source_target, v._source_target) &&
                Equals(_source_world_target, v._source_world_target) &&
                Equals(_dest_target, v._dest_target) &&
                Equals(_dest_world_target, v._dest_world_target) &&
                Equals(_var_name, v._var_name);
        }

        public override string ToString()
        {
            string s = string.Empty;

            if (string.IsNullOrEmpty(Text))
                s += string.Format("Text: {0}; ", Text);

            if (_report_params != null)
                s += string.Format("ReportParams: {0}; ", string.Join(", ", _report_params.Select(t => t.ToString()).ToArray()));

            return s;
        }
    }

    public enum EAISignalType
    {
        Undefined,
        RepairMe,
        ReplaceCargo,
        TakeCargo,
        TransferCargoToStore,
        TransferCargoToShip,

        MessageReply,
        PlayerCrashed
    }

    [CascadeObject(ReflectionSerializer.MemberSerialization.Fields)]
    public class CAIBehaviorDescr
    {
        private string _behavior_name;
        public string Name { get { return _behavior_name; } }

        [CascadeIgnore]
        private int _index;
        public int Index { get { return _index; } }

        [CascadeProperty("AISignalType")]
        private EAISignalType _ai_signal;
        public EAISignalType AISignalType { get { return _ai_signal; } }

        private string _ai_signal_message_name;
        public string AISignalMessageName { get { return _ai_signal_message_name; } }

        private string _ai_signal_message_reply;
        public string AISignalMessageReply { get { return _ai_signal_message_reply; } }

        [CascadeProperty("ConditionAliases")]
        private string[,] _cond_aliases;
        public string[,] ConditionAliases { get { return _cond_aliases; } }


        [CascadeProperty("ResetBehaviorConditionAliases")]
        private string[,] _reset_behavior_cond_aliases;
        public string[,] ResetBehaviorConditionAliases { get { return _reset_behavior_cond_aliases; } }

        private CAIActionDescrs[] _action_descrs;
        public CAIActionDescrs[] Actions { get { return _action_descrs; } }

        private int _priority;
        public int Priority { get { return _priority; } }

        [CascadeProperty("PriorityWhenCurrent")]
        private int _priority_current;
        public int PriorityWhenCurrent { get { return _priority_current; } }

        private bool _no_circle;
        public bool NoCircle { get { return _no_circle; } }

        private CAIBehaviorReportDescr _report_descr;

        private string[] _enable_in_states;
        public string[] EnableInStates { get { return _enable_in_states; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CAIBehaviorDescr;

            return
                Utils.IsArrayEquals(_cond_aliases, v._cond_aliases) &&
                Utils.IsArrayEquals(_reset_behavior_cond_aliases, v._reset_behavior_cond_aliases) &&
                Utils.IsArrayEquals(_action_descrs, v._action_descrs) &&
                Utils.IsArrayEquals(_enable_in_states, v._enable_in_states) &&
                Equals(_behavior_name, v._behavior_name) &&
                Equals(_ai_signal, v._ai_signal) &&
                Equals(_ai_signal_message_name, v._ai_signal_message_name) &&
                Equals(_ai_signal_message_reply, v._ai_signal_message_reply) &&
                Equals(_priority, v._priority) &&
                Equals(_priority_current, v._priority_current) &&
                Equals(_no_circle, v._no_circle) &&
                Equals(_report_descr, v._report_descr);
        }

        public CAIBehaviorReportDescr ReportDescr
        {
            get
            {
                if (_report_descr == null)
                    _report_descr = new CAIBehaviorReportDescr();
                return _report_descr;
            }
        }

        public CAIBehaviorDescr()
        {
            _action_descrs = new CAIActionDescrs[0];
            _report_descr = new CAIBehaviorReportDescr();

            if (_enable_in_states == null)
                _enable_in_states = new string[0];
        }

        public CAIBehaviorDescr(string behaviorName, List<CAIActionDescrs> actionDescrs, int inPriority, string[,] inCondAliases)
        {
            _behavior_name = behaviorName;
            _action_descrs = actionDescrs.ToArray();
            _priority = inPriority;
            _cond_aliases = inCondAliases;
        }

        public override string ToString()
        {
            return _behavior_name;
        }

        public static CAIBehaviorDescr CreateTestObject(string name)
        {
            var descr = new CAIBehaviorDescr();

            descr._behavior_name = name;
            descr._ai_signal = EAISignalType.MessageReply;
            descr._ai_signal_message_name = "msg_name";
            descr._ai_signal_message_reply = "msg_reply";

            descr._cond_aliases = new string[,] { { "cond1", "cond2" }, { "cond3", "cond4" } };
            descr._reset_behavior_cond_aliases = new string[,] { { "cond11" } };

            descr._action_descrs = new CAIActionDescrs[]
            {
                CAIActionDescrs.CreateTestObject(),
                CAIActionDescrs.CreateTestObject()
            };
            descr._priority = 7;

            return descr;
        }
    }

    [CascadeObject(ReflectionSerializer.MemberSerialization.OptIn)]
    public class CAIBehaviorStateDescr
    {
        [CascadeProperty("StateName")]
        private string _state_name;
        public string Name { get { return _state_name; } }

        [CascadeProperty("ActionDescrs")]
        private CAIActionDescrs[] _action_descrs;
        public CAIActionDescrs[] Actions { get { return _action_descrs; } }

        [CascadeProperty("NextStates")]
        private CNextStatesDescr[] _next_states;
        public CNextStatesDescr[] NextStates { get { return _next_states; } }

        [CascadeIgnore]
        private int _state_index;
        public int Index { get { return _state_index; } }

        [CascadeObject(ReflectionSerializer.MemberSerialization.Fields)]
        public class CNextStatesDescr
        {
            private string _state_name;
            public string Name { get { return _state_name; } }


            private string[,] _condition_aliases;
            public string[,] ConditionAliases { get { return _condition_aliases; } }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if (obj.GetType() != GetType())
                    return false;

                var v = obj as CNextStatesDescr;

                return Utils.IsArrayEquals(_condition_aliases, v._condition_aliases) &&
                    _state_name.Equals(v._state_name);
            }

            public CNextStatesDescr()
            {
                if (_condition_aliases == null)
                    _condition_aliases = new string[0, 0];
            }

            public CNextStatesDescr(string state_name, string[,] condition_aliases)
            {
                _state_name = state_name;
                _condition_aliases = condition_aliases;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            var v = obj as CAIBehaviorStateDescr;

            return Utils.IsArrayEquals(_action_descrs, v._action_descrs) &&
                Utils.IsArrayEquals(_next_states, v._next_states) &&
                Equals(_state_name, v._state_name);
        }

        public CAIBehaviorStateDescr()
        {
            if (_action_descrs == null)
                _action_descrs = new CAIActionDescrs[0];

            if (_next_states == null)
                _next_states = new CNextStatesDescr[0];
        }

        public static CAIBehaviorStateDescr CreateTestObject(string name)
        {
            var descr = new CAIBehaviorStateDescr();

            descr._state_name = name;
            descr._action_descrs = new CAIActionDescrs[]
            {
                CAIActionDescrs.CreateTestObject(),
                CAIActionDescrs.CreateTestObject()
            };
            descr._next_states = new CNextStatesDescr[]
            {
                new CNextStatesDescr("next_state1", new string[,] { { "cond1", "cond2" }, { "cond3", "cond4" } }),
                new CNextStatesDescr("next_state2", new string[,] { { "cond11" } })
            };
            descr._state_index = -1;

            return descr;
        }
    }


}
