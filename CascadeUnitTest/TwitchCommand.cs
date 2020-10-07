using System;

namespace CascadeUnitTest
{
    public enum ETwitchCommandType { CreateTornado, CreateMeteors, CreateNuggets, SetNuggetName }

    [Serializable]
    public struct STwitchCommand
    {
        public ETwitchCommandType CommandType;
        public bool Active;
        public string CommandText;
        public int CooldownMin;
        public bool VotesOrBits;
        public int Count; //VotesOrBits

        public override string ToString()
        {
            string t = Active ? "active" : "disable";
            string vb = VotesOrBits ? "votes" : "bits";
            return $"{CommandType} [{t}]: {CommandText}, cooldown {CooldownMin}, {vb} [{Count}]";
        }

        public static STwitchCommand CreateDefault(ETwitchCommandType inCommandType)
        {
            return new STwitchCommand
            {
                CommandType = inCommandType,
                Active = false,
                CommandText = $"#UCmd_{inCommandType}",
                CooldownMin = 20,
                VotesOrBits = true,
                Count = 1
            };
        }

        public static STwitchCommand[] CreateDefaults()
        {
            Array arr_enums = Enum.GetValues(typeof(ETwitchCommandType));
            STwitchCommand[] arr = new STwitchCommand[arr_enums.Length];

            for (int i = 0; i < arr.Length; i++)
                arr[i] = STwitchCommand.CreateDefault((ETwitchCommandType)arr_enums.GetValue(i));

            return arr;
        }
    }
}
