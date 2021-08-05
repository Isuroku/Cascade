
namespace CascadeParser
{
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

        public static IKey CreateCopy(IKey inOther)
        {
            CKey other = inOther as CKey;
            if (other == null)
                return null;
            return (other.GetCopy() as CKey);
        }

        public static IKey CreateKey(byte[] ioBuffer, int inOffset)
        {
            var key = CKey.CreateRoot(string.Empty);
            key.BinaryDeserialize(ioBuffer, inOffset);
            return key;
        }
    }
}
