using CascadeParser;
using CascadeSerializer;
using System;
using System.Collections.Generic;

namespace CascadeUnitTest
{
    public class CNamedIdArray
    {
        [CascadeConverter(typeof(NamedIdArrayCscdConverter))]
        public StringId[] Names;

        public void Init()
        {
            Names = new StringId[]
            {
                StringId.GetNamedId("Test1"),
                StringId.GetNamedId("Test2"),
            };
        }

        public bool Equals(CNamedIdArray other)
        {
            return Utils.IsArrayEquals(Names, other.Names);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CNamedIdArray)obj);
        }

        public override int GetHashCode()
        {
            return 1635486599 + EqualityComparer<StringId[]>.Default.GetHashCode(Names);
        }
    }

    public class NamedIdArrayCscdConverter : CascadeConverter
    {
        public bool CanConvert(Type objectType)
        {
            return objectType == typeof(StringId[]);
        }

        public object ReadKey(IKey key, ILogPrinter inLogger)
        {
            int count = key.GetValuesCount();
            var arr = new StringId[count];
            for (int i = 0; i < count; i++)
                arr[i] = StringId.GetNamedId(key.GetValueAsString(i));
            return arr;
        }

        public void WriteKey(IKey key, object instance, ILogPrinter inLogger)
        {
            StringId[] arr = (StringId[])instance;
            for (int i = 0; i < arr.Length; i++)
                key.AddValue(arr[i].name);
        }
    }
}
