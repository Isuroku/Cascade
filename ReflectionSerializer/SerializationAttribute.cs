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
    public class JsonIgnoreAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class NonSerializedAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class JsonPropertyAttribute : Attribute
    {
        public string Name { get; set; }

        public object Default { get; set; }

        public JsonPropertyAttribute() { }
        public JsonPropertyAttribute(string inName)
        {
            Name = inName;
        }
    }

    public enum MemberSerialization { OptOut, OptIn, Fields, All }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class JsonObjectAttribute : Attribute
    {
        public MemberSerialization MemberSerialization { get; set; }

        public JsonObjectAttribute() { }
        public JsonObjectAttribute(MemberSerialization inMemberSerialization) { MemberSerialization = inMemberSerialization; }
    }


    public interface JsonConverter
    {
        bool CanConvert(Type objectType);
        object ReadKey(IKey key, ILogger inLogger);
        void WriteKey(IKey key, object instance, ILogger inLogger);
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class JsonConverterAttribute : Attribute
    {
        public JsonConverter CustomConverter { get; set; }

        public JsonConverterAttribute(Type inCustomConverter)
        {

            if(!inCustomConverter.IsClass)
                throw new ArgumentException("Type must be inherited from class JsonConverter");

            //if(!inCustomConverter.IsAssignableFrom(typeof(JsonConverter)))
            //    throw new ArgumentException("Type must be inherited from class JsonConverter");

            Type[] intfs = inCustomConverter.GetInterfaces();
            if(intfs == null || intfs.Length == 0)
                throw new ArgumentException("Type must be inherited from class JsonConverter");

            if(!Array.Exists(intfs, i => i == typeof(JsonConverter)))
                throw new ArgumentException("Type must be inherited from class JsonConverter");

            //if (bt != typeof(JsonConverter))
            //    throw new ArgumentException("Type must be inherited from class JsonConverter");

            CustomConverter = (JsonConverter)Activator.CreateInstance(inCustomConverter);
        }
    }
}
