using System;
using System.Reflection;
using System.Collections;
using CascadeParser;

namespace ReflectionSerializer
{
    public class Serializer
    {
        readonly IReflectionProvider reflectionProvider;

        public Serializer(IReflectionProvider reflectionProvider)
        {
            this.reflectionProvider = reflectionProvider;
        }

        public SerializedObject Serialize(object instance, ILogPrinter inLogger)
        {
            return SerializeInternal(instance.GetType().Name, instance, typeof(object), inLogger);
        }

        SerializedObject SerializeInternal(string name, object instance, Type declaredType, ILogPrinter inLogger)
        {
            SerializedObject child;
            Type type = instance == null ? declaredType : instance.GetType();

            // Atomic values
            if (instance == null || type.IsAtomic())
                child = new SerializedAtom { Name = name, Value = instance, Type = type };

            // Dictionaries
            else if (type.IsGenericDictionary())
            {
                var dictionary = instance as IDictionary;
                Type[] genericArguments = dictionary.GetType().GetGenericArguments();
                Type keyDeclaredType = genericArguments[0];
                Type valueDeclaredType = genericArguments[1];

                child = new SerializedAggregate { Name = name, Type = type };
                var childAggregation = child as SerializedAggregate;
                foreach (var key in dictionary.Keys)
                    childAggregation.Children.Add(
                        SerializeInternal(null, key, keyDeclaredType, inLogger),
                        SerializeInternal(null, dictionary[key], valueDeclaredType, inLogger));
            }

            // Arrays, lists and sets (any collection excluding dictionaries)
            else if (type.IsGenericCollection())
            {
                var collection = instance as IEnumerable;
                Type declaredItemType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];

                child = new SerializedCollection { Name = name, Type = type };
                var childCollection = child as SerializedCollection;
                foreach (var item in collection)
                    childCollection.Items.Add(SerializeInternal(null, item, declaredItemType, inLogger));
            }

            // Everything else (serialized with recursive property reflection)
            else
            {
                child = new SerializedAggregate { Name = name, Type = type };
                var childAggregation = child as SerializedAggregate;

                MemberInfo[] member_infos = reflectionProvider.GetSerializableMembers(type);
                foreach (MemberInfo memberInfo in member_infos)
                {
                    var memberAttr = reflectionProvider.GetSingleAttributeOrDefault<DataMemberAttribute>(memberInfo);
                    // Make sure we want it serialized
                    if (memberAttr.Ignore)
                        continue;

                    Type memberType = memberInfo.GetMemberType();
                    object value = reflectionProvider.GetValue(memberInfo, instance);

                    // Optional properties are skipped when serializing a default or null value
                    //if (!memberAttr.Required && (value == null || IsDefault(value)))
                    if (value == null || IsDefault(value, inLogger))
                        continue;

                    // If no property name is defined, use the short type name
                    string memberName = memberAttr.Name ?? memberInfo.Name;
                    childAggregation.Children.Add(memberName, SerializeInternal(memberName, value, memberType, inLogger));
                }
            }

            // Write the runtime type if different (except nullables since they get unboxed)
            if (!declaredType.IsNullable() && type != declaredType)
                child.Type = type;

            return child;
        }

        public T Deserialize<T>(SerializedObject instance, ILogPrinter inLogger)
        {
            return (T)DeserializeInternal(instance, typeof(T), null, inLogger);
        }

        public object DeserializeInternal(SerializedObject serialized, Type declaredType, object existingInstance, ILogPrinter inLogger)
        {
            Type type = declaredType;
            object instance = existingInstance;

            // Atomic or null values
            if (serialized is SerializedAtom)
                // The current value is replaced; they're immutable
                instance = (serialized as SerializedAtom).Value;

            // Dictionaries
            else if (type.IsGenericDictionary())
            {
                // Instantiate if necessary
                if (instance == null)
                    instance = reflectionProvider.Instantiate(type, inLogger);

                var dictionary = instance as IDictionary;
                Type[] genericArguments = dictionary.GetType().GetGenericArguments();
                Type keyDeclaredType = genericArguments[0];
                Type valueDeclaredType = genericArguments[1];

                var serializedAggregation = serialized as SerializedAggregate;
                foreach (var key in serializedAggregation.Children.Keys)
                    // Dictionaries always contain atoms as keys
                    SafeAddToDictionary(dictionary,
                        DeserializeInternal(key as SerializedObject, keyDeclaredType, null, inLogger),
                        DeserializeInternal(serializedAggregation.Children[key], valueDeclaredType, null, inLogger));
            }

            // Arrays, lists and sets (any collection excluding dictionaries)
            else if (type.IsGenericCollection())
            {
                bool isArray = type.IsArray;
                bool isHashSet = type.IsHashSet();
                var serializedCollection = serialized as SerializedCollection;

                Type declaredItemType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];

                // Instantiate if necessary
                if (instance == null)
                    if (isArray)
                        instance = Array.CreateInstance(declaredItemType, serializedCollection.Items.Count);
                    else
                        instance = reflectionProvider.Instantiate(declaredType, inLogger) as IEnumerable;

                MethodHandler addToHashSet = null;
                if (isHashSet)
                    addToHashSet = reflectionProvider.GetDelegate(type.GetMethod("Add"));

                int valueIndex = 0;
                foreach (var item in serializedCollection.Items)
                {
                    object value = DeserializeInternal(item, declaredItemType, null, inLogger);

                    if (isArray)
                        (instance as IList)[valueIndex++] = value;
                    else if (isHashSet)
                        // Potential problem if set already contains key...
                        addToHashSet(instance, value);
                    else if (instance is IList)
                        (instance as IList).Add(value);
                    else
                        throw new NotImplementedException();
                }
            }

            // Everything else (serialized with recursive property reflection)
            else
            {
                bool mustInstantiate = instance == null;

                if (serialized.Type != null && declaredType != serialized.Type)
                {
                    type = serialized.Type;
                    mustInstantiate = true;
                }

                if (mustInstantiate)
                    instance = reflectionProvider.Instantiate(type, inLogger);

                var serializedAggregation = serialized as SerializedAggregate;

                foreach (MemberInfo memberInfo in reflectionProvider.GetSerializableMembers(type))
                {
                    var memberAttr = reflectionProvider.GetSingleAttributeOrDefault<DataMemberAttribute>(memberInfo);
                    if (memberAttr.Ignore)
                        continue;

                    Type memberType = memberInfo.GetMemberType();
                    string name = memberAttr.Name ?? memberInfo.Name;

                    // Checking if it's a class before doing GetValue doesn't speed up the process
                    object currentValue = reflectionProvider.GetValue(memberInfo, instance);
                    bool valueFound = serializedAggregation.Children.ContainsKey(name);

                    if (valueFound)
                    {
                        var readValue = DeserializeInternal(serializedAggregation[name], memberType, currentValue, inLogger);
                        // This dirty check is naive and doesn't provide performance benefits
                        //if (memberType.IsClass && readValue != currentValue && (readValue == null || !readValue.Equals(currentValue)))
                        reflectionProvider.SetValue(memberInfo, instance, readValue);
                    }
                }
            }

            return instance;
        }

        void SafeAddToDictionary(IDictionary dictionary, object key, object value)
        {
            if (dictionary.Contains(key))
                dictionary.Remove(key);
            dictionary.Add(key, value);
        }

        bool IsDefault(object value, ILogPrinter inLogger)
        {
            if (value is string)
                return (string)value == string.Empty;
            if (value is char)
                return (char)value == '\0';
            if (value is int || value is long || value is uint || value is float || value is double ||
                value is decimal || value is short || value is sbyte || value is byte || value is ushort)
                return Convert.ToInt32(value) == 0;
            if (value is bool)
                return (bool)value == false;
            if (value is ICollection)
                return (value as ICollection).Count == 0;
            if (value is Array)
                return (value as Array).Length == 0;

            Type type = value.GetType();
            if (type.IsHashSet())
                return (int)reflectionProvider.GetValue(type
                    .GetProperty("Count", BindingFlags.ExactBinding | ReflectionHelper.PublicInstanceMembers, null, typeof(int), Type.EmptyTypes, null),
                    value) == 0;
            if (type.IsEnum)
                return (int)value == 0;

            return value.Equals(reflectionProvider.Instantiate(type, inLogger));
        }
    }
}
