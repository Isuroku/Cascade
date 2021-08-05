
using System;

namespace CascadeParser
{
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
        DateTime ToDateTime();
        TimeSpan ToTimeSpan();

        int GetMemorySize();
        int BinarySerialize(byte[] ioBuffer, int inOffset);
    }
}
