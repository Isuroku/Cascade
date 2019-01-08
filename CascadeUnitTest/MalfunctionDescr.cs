using CascadeSerializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CascadeUnitTest
{
    public enum EMalfunctionType
    {
        Undefined,
        SoulDegradation,
        Speed,
        ParasiticInput,
        Switch
    }

    public enum EMalfunctionParams
    {
        Value,
        Pitch,
        Yaw,
        Roll,
        RandomValue,
        RandomSign,
        Period
    }

    public enum EShipAggregateType
    {
        Undefined = 0,
        Hull,
        Engine,
        Wing,
        Fuel,
        Steerer,
        Chassis,
        Locator,
        Cargo,
        RemainingShip
    }

    [CascadeObject(CascadeSerializer.MemberSerialization.Fields)]
    public class CMalfunctionDescr : CCommonDescr<string>
    {
        private EShipAggregateType _aggregate;
        public EShipAggregateType Aggregate { get { return _aggregate; } }

        private EMalfunctionType _type;
        public EMalfunctionType MalfunctionType { get { return _type; } }

        [CascadeProperty("EveryAggregatePart")]
        private bool _every_parts;
        public bool EveryAggregatePart { get { return _every_parts; } }

        private List<CConditionDescr> _conditions = new List<CConditionDescr>();
        public IEnumerable<CConditionDescr> ConditionDescrs { get { return _conditions; } }

        private string[][] _condition_aliases;
        public string[][] ConditionAliases { get { return _condition_aliases; } }

        private float _time_before_start_max;
        public float TimeBeforeStartMax { get { return _time_before_start_max; } }

        private float _time_before_start_min;
        public float TimeBeforeStartMin { get { return _time_before_start_min; } }

        private int _downstep_count;
        public int DownstepCount { get { return _downstep_count; } }

        private float _duration;
        public float Duration { get { return _duration; } }

        private bool _clear_after_dead;
        public bool ClearAfterDead { get { return _clear_after_dead; } }

        [CascadeProperty("CreatedObjects")]
        private Dictionary<string, string> _prefab_path_hook;
        public IEnumerable<KeyValuePair<string, string>> PrefabAndHooks { get { return _prefab_path_hook; } }

        private EnumDictionary<EMalfunctionParams> _params;
        public EnumDictionary<EMalfunctionParams> Params
        {
            get
            {
                if (_params == null)
                    _params = new EnumDictionary<EMalfunctionParams>();
                return _params;
            }
        }
    }

    [CascadeObject(CascadeSerializer.MemberSerialization.Fields)]
    public class CShipMalfunctionsDescr : CCommonDescr<string>, IEnumerable<CMalfunctionDescr>
    {
        public static string GetDataPath() { return "Ships/Malfunctions/"; }
        public static string GetFilePatternCscd() { return "*Malfunctions*.cscd"; }

        private List<CMalfunctionDescr> _malfunctions = new List<CMalfunctionDescr>();

        #region IEnumerable
        public IEnumerator<CMalfunctionDescr> GetEnumerator()
        {
            return _malfunctions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _malfunctions.GetEnumerator();
        }
        #endregion

        public CShipMalfunctionsDescr() { }
    }
}
