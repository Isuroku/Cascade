using System;

namespace CascadeParser
{
    public enum EKeyOpResult { OK, AlreadyPresent, DublicateName, UnnativeKey, NotFound }
    public interface IKey
    {
        IKey CreateChildKey(string name);
        IKey CreateArrayKey();

        EKeyOpResult AddChild(IKey inNewChild);
        EKeyOpResult RemoveChild(IKey inChild);
        EKeyOpResult InsertChild(int inIndexPos, IKey inChild);

        void SortKeys(Comparison<IKey> comparison);

        bool UpInParent();
        bool DownInParent();

        EKeyOpResult SwapChild(IKey inChild1, IKey inChild2);

        bool SetName(string name);

        string GetName();
        bool IsArrayKey();
        string Comments { get; }

        bool IsEmpty { get; }

        int GetIndexInParent();

        int GetChildCount();
        IKey GetChild(int index);
        IKey GetChild(string name);

        int GetValuesCount();
        IKeyValue GetValue(int index);
        string GetValueAsString(int index);

        IKeyValue AddValue(bool v);
        IKeyValue AddValue(byte v);
        IKeyValue AddValue(short v);
        IKeyValue AddValue(ushort v);
        IKeyValue AddValue(int v);
        IKeyValue AddValue(uint v);
        IKeyValue AddValue(long v);
        IKeyValue AddValue(ulong v);
        IKeyValue AddValue(float v);
        IKeyValue AddValue(double v);
        IKeyValue AddValue(string v);
        IKeyValue AddValue(DateTime v);
        IKeyValue AddValue(TimeSpan v);
        IKeyValue AddValue(Variant v);

        void RemoveValueAt(int index);

        void ClearValues();

        string SaveToString();
        string SaveChildsToString();

        IKey FindKey(string key_path);
        IKey Parent { get; }

        IKey CreateCopy();

        string GetPath();

        int GetMemorySize();
        int BinarySerialize(byte[] ioBuffer, int inOffset);
        int AddValue(byte[] ioBuffer, int inOffset);
    }
}
