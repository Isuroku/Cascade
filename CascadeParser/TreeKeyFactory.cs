
namespace CascadeParser
{
    public interface IKey
    {
        IKey CreateChildKey(string name);
        IKey CreateArrayKey();

        void SetName(string name);

        void AddValue(bool v);
        void AddValue(byte v);
        void AddValue(short v);
        void AddValue(ushort v);
        void AddValue(int v);
        void AddValue(uint v);
        void AddValue(long v);
        void AddValue(ulong v);
        void AddValue(float v);
        void AddValue(double v);
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

        EValueType ValueType { get; }

        string ToString();
        bool ToBool();
        byte ToByte();
        short ToShort();
        ushort ToUShort();
        int ToInt();
        uint ToUInt();
        long ToLong();
        ulong ToULong();
        float ToFloat();
        double ToDouble();
    }

    public static class IKeyFactory
    {
        public static IKey CreateKey(string inName)
        {
            return CKey.CreateRoot(inName);
        }

        public static IKey CreateArrayKey(IKey inParent)
        {
            return CKey.CreateArrayKey(inParent as CKey);
        }
    }
}
