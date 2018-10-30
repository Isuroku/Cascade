
namespace CascadeParser
{
    public interface IKey
    {
        IKey CreateChildKey(string name);
        IKey CreateArrayKey();

        void SetName(string name);

        void AddValue(long v);
        void AddValue(int v);
        void AddValue(ulong v);
        void AddValue(uint v);
        void AddValue(decimal v);
        void AddValue(float v);
        void AddValue(bool v);
        void AddValue(string v);

        string GetName();
        bool IsArrayKey();
        string Comments { get; }

        bool IsEmpty { get; }

        int GetChildCount();
        IKey GetChild(int index);
        IKey GetChild(string name);

        int GetValuesCount();
        IKeyValue GetValue(int index);
        string GetValueAsString(int index);

        string SaveToString();

        IKey FindKey(string key_path);
        IKey Parent { get; }

        string GetPath();
    }

    public interface IKeyValue
    {
        IKey Parent { get; }
        string Comments { get; }

        string GetValueAsString();

        float GetValueAsFloat();
        decimal GetValueAsDecimal();
        int GetValueAsInt();
        long GetValueAsLong();
        uint GetValueAsUInt();
        ulong GetValueAsULong();
        bool GetValueAsBool();
    }

    public static class IKeyFactory
    {
        public static IKey CreateKey(string inName)
        {
            return CKey.CreateRoot(inName);
        }
    }
}
