using CascadeParser;
using System;

namespace ReflectionSerializer
{
    /// <summary>
    /// Qualifies the serialization of a public property or field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DataMemberAttribute : Attribute
    {
        /// <summary>
        /// Override the serialized name of this value.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ignore this value when serializing.
        /// </summary>
        public bool Ignore { get; set; }

        //public bool Required { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CascadeIgnoreAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class NonSerializedAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CascadePropertyAttribute : Attribute
    {
        public string Name { get; set; }

        public object Default { get; set; }

        public CascadePropertyAttribute() { }
        public CascadePropertyAttribute(string inName)
        {
            Name = inName;
        }
    }

    public enum MemberSerialization { OptOut, OptIn, Fields, All }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CascadeObjectAttribute : Attribute
    {
        public MemberSerialization MemberSerialization { get; set; }

        public CascadeObjectAttribute() { }
        public CascadeObjectAttribute(MemberSerialization inMemberSerialization) { MemberSerialization = inMemberSerialization; }
    }


    public interface CascadeConverter
    {
        bool CanConvert(Type objectType);
        object ReadKey(IKey key, ILogger inLogger);
        void WriteKey(IKey key, object instance, ILogger inLogger);
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CascadeConverterAttribute : Attribute
    {
        public CascadeConverter CustomConverter { get; set; }

        public CascadeConverterAttribute(Type inCustomConverter)
        {

            if(!inCustomConverter.IsClass)
                throw new ArgumentException("Type must be inherited from class CascadeConverter");

            //if(!inCustomConverter.IsAssignableFrom(typeof(CascadeConverter)))
            //    throw new ArgumentException("Type must be inherited from class CascadeConverter");

            Type[] intfs = inCustomConverter.GetInterfaces();
            if(intfs == null || intfs.Length == 0)
                throw new ArgumentException("Type must be inherited from class CascadeConverter");

            if(!Array.Exists(intfs, i => i == typeof(CascadeConverter)))
                throw new ArgumentException("Type must be inherited from class CascadeConverter");

            //if (bt != typeof(CascadeConverter))
            //    throw new ArgumentException("Type must be inherited from class CascadeConverter");

            CustomConverter = (CascadeConverter)Activator.CreateInstance(inCustomConverter);
        }
    }
}
