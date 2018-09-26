using System;
using System.Collections;
using System.Reflection;

namespace ReflectionSerializer
{
    public interface ILogger
    {
        void LogWarning(string inText);
        void LogError(string inText);
        void Trace(string inText);
    }

    public interface IKey
    {
        IKey CreateChildKey(string name);
        IKey CreateArrayKey();
        void AddValue(long v);
        void AddValue(ulong v);
        void AddValue(decimal v);
        void AddValue(bool v);
        void AddValue(string v);

        string GetName();

        int GetChildCount();
        IKey GetChild(int index);
        IKey GetChild(string name);

        int GetValuesCount();
        string GetValueAsString(int index);
    }

    public class CKeySerializer
    {
        readonly IReflectionProvider _reflectionProvider;

        public CKeySerializer(IReflectionProvider reflectionProvider)
        {
            _reflectionProvider = reflectionProvider;
        }

        public void Serialize(object instance, IKey inKey, ILogger inLogger)
        {
            Serialize(instance, typeof(object), inKey, inLogger);
        }

        void Serialize(object instance, Type declaredType, IKey inKey, ILogger inLogger)
        {
            Type type = instance == null ? declaredType : instance.GetType();

            if (instance == null || type.IsAtomic())
            {
                AddValueToKey(inKey, instance);
            }
            else if (type.IsGenericDictionary())
            {
                var dictionary = instance as IDictionary;
                Type[] genericArguments = dictionary.GetType().GetGenericArguments();
                Type keyDeclaredType = genericArguments[0];
                Type valueDeclaredType = genericArguments[1];

                if (!keyDeclaredType.IsAtomic())
                    inLogger.LogError("Dictionary must simple key.");
                else
                {
                    foreach (var key in dictionary.Keys)
                    {
                        IKey child = inKey.CreateChildKey(key.ToString());
                        Serialize(dictionary[key], valueDeclaredType, child, inLogger);
                    }
                }
            }
            else if (type.IsGenericCollection())
            {
                var collection = instance as IEnumerable;
                Type declaredItemType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];

                if (declaredItemType.IsAtomic())
                {
                    foreach (var item in collection)
                        AddValueToKey(inKey, item);
                }
                else
                {
                    foreach (var item in collection)
                    {
                        IKey child = inKey.CreateArrayKey();
                        Serialize(item, declaredItemType, child, inLogger);
                    }
                }
            }
            else
            {
                MemberInfo[] member_infos = _reflectionProvider.GetSerializableMembers(type);
                foreach (MemberInfo memberInfo in member_infos)
                {
                    var memberAttr = _reflectionProvider.GetSingleAttributeOrDefault<SerializationAttribute>(memberInfo);
                    // Make sure we want it serialized
                    if (memberAttr.Ignore)
                        continue;

                    Type memberType = memberInfo.GetMemberType();
                    object value = _reflectionProvider.GetValue(memberInfo, instance);

                    // Optional properties are skipped when serializing a default or null value
                    //if (!memberAttr.Required && (value == null || ReflectionHelper.IsDefault(value, _reflectionProvider)))
                    if (value == null || ReflectionHelper.IsDefault(value, _reflectionProvider))
                        continue;

                    // If no property name is defined, use the short type name
                    string memberName = memberAttr.Name ?? memberInfo.Name;
                    IKey child = inKey.CreateChildKey(memberName);
                    Serialize(value, memberType, child, inLogger);
                }

                if(inKey.GetChildCount() == 0)
                    inKey.AddValue("default");
            }

            // Write the runtime type if different (except nullables since they get unboxed)
            if (!declaredType.IsNullable() && type != declaredType)
            {
                inKey.CreateChildKey("RealObjectType").AddValue(type.FullName);
            }
        }

        void AddValueToKey(IKey key, object instance)
        {
            if (instance == null)
                key.AddValue(0);
            else if (ReflectionHelper.IsFLOAT(instance))
                key.AddValue(Convert.ToDecimal(instance));
            else if (ReflectionHelper.IsINT(instance))
                key.AddValue(Convert.ToInt64(instance));
            else if (ReflectionHelper.IsUINT(instance))
                key.AddValue(Convert.ToUInt64(instance));
            else
                key.AddValue(instance.ToString());
        }

        public T Deserialize<T>(IKey key, ILogger inLogger)
        {
            return (T)DeserializeInternal(key, typeof(T), inLogger);
        }

        public object DeserializeInternal(IKey key, Type declaredType, ILogger inLogger)
        {
            Type type = declaredType;
            object instance;

            // Atomic or null values
            if (type.IsAtomic())
            {
                if (key.GetValuesCount() == 0)
                {
                    instance = ReflectionHelper.GetDefaultValue(type, _reflectionProvider);
                }
                else if (key.GetValuesCount() > 1)
                {
                    inLogger.LogError(string.Format("Need one value for type {1}. Key: {0} ", key, type.Name));
                    instance = ReflectionHelper.GetDefaultValue(type, _reflectionProvider);
                }
                else
                {
                    string key_value = key.GetValueAsString(0);
                    if (!ReflectionHelper.StringToAtomicValue(key_value, type, out instance))
                    {
                        inLogger.LogError(string.Format("Key {0} with value {1} can't convert value to type {2}", key, key_value, type.Name));
                        instance = ReflectionHelper.GetDefaultValue(type, _reflectionProvider);
                    }
                }
            }
            // Dictionaries
            else if (type.IsGenericDictionary())
            {
                // Instantiate if necessary
                instance = _reflectionProvider.Instantiate(type);

                var dictionary = instance as IDictionary;
                Type[] genericArguments = dictionary.GetType().GetGenericArguments();
                Type keyDeclaredType = genericArguments[0];
                Type valueDeclaredType = genericArguments[1];

                for(int i = 0; i < key.GetChildCount(); ++i)
                {
                    IKey sub_key = key.GetChild(i);

                    object dic_key;
                    if (!ReflectionHelper.StringToAtomicValue(sub_key.GetName(), keyDeclaredType, out dic_key))
                    {
                        inLogger.LogError(string.Format("SubKey {0} for dictionary with key type {1} can't convert value {2}", 
                            key, keyDeclaredType.Name, sub_key.GetName()));
                    }
                    else
                    {
                        object dic_value = DeserializeInternal(sub_key, valueDeclaredType, inLogger);

                        if (dictionary.Contains(dic_key))
                            dictionary.Remove(dic_key);
                        dictionary.Add(dic_key, dic_value);
                    }
                }
            }
            // Arrays, lists and sets (any collection excluding dictionaries)
            else if (type.IsGenericCollection())
            {
                bool isArray = type.IsArray;
                bool isHashSet = type.IsHashSet();

                Type declaredItemType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                bool is_atomic_elems = declaredItemType.IsAtomic();
                int element_count = is_atomic_elems ? key.GetValuesCount() : key.GetChildCount();

                if (isArray)
                    instance = Array.CreateInstance(declaredItemType, element_count);
                else
                    instance = _reflectionProvider.Instantiate(declaredType) as IEnumerable;

                MethodHandler addToHashSet = null;
                if (isHashSet)
                    addToHashSet = _reflectionProvider.GetDelegate(type.GetMethod("Add"));

                for(int i = 0; i < element_count; i++)
                {
                    object obj_value;
                    if (is_atomic_elems)
                    {
                        string str_value = key.GetValueAsString(i);
                        if (!ReflectionHelper.StringToAtomicValue(str_value, declaredItemType, out obj_value))
                        {
                            inLogger.LogError(string.Format("Key {0} for collection with element type {1} can't convert value {2}",
                                key, declaredItemType.Name, str_value));
                        }
                    }
                    else
                    {
                        IKey sub_key = key.GetChild(i);
                        obj_value = DeserializeInternal(sub_key, declaredItemType, inLogger);
                    }

                    if (isArray)
                        (instance as IList)[i] = obj_value;
                    else if (isHashSet)
                        // Potential problem if set already contains key...
                        addToHashSet(instance, obj_value);
                    else if (instance is IList)
                        (instance as IList).Add(obj_value);
                    else
                        throw new NotImplementedException();
                }
            }
            // Everything else (serialized with recursive property reflection)
            else
            {
                IKey type_key = key.GetChild("RealObjectType");
                if (type_key != null)
                {
                    string type_name = type_key.GetValueAsString(0);
                    Type obj_type = Type.GetType(type_name, false);
                    if (obj_type != null)
                        type = obj_type;
                }

                instance = _reflectionProvider.Instantiate(type);

                if (key.GetChildCount() != 0)
                {
                    foreach (MemberInfo memberInfo in _reflectionProvider.GetSerializableMembers(type))
                    {
                        var memberAttr = _reflectionProvider.GetSingleAttributeOrDefault<SerializationAttribute>(memberInfo);
                        if (memberAttr.Ignore)
                            continue;

                        Type memberType = memberInfo.GetMemberType();
                        string name = memberAttr.Name ?? memberInfo.Name;

                        IKey sub_key = key.GetChild(name);
                        //if (sub_key == null)
                        //{
                        //    if (memberAttr.Required)
                        //    {
                        //        inLogger.LogError(string.Format("Key {0} doesn't have sub_key with name {1}",
                        //            key, name));
                        //    }
                        //}
                        //else
                        if (sub_key != null)
                        {
                            var readValue = DeserializeInternal(sub_key, memberType, inLogger);
                            // This dirty check is naive and doesn't provide performance benefits
                            //if (memberType.IsClass && readValue != currentValue && (readValue == null || !readValue.Equals(currentValue)))
                            _reflectionProvider.SetValue(memberInfo, instance, readValue);
                        }
                    }
                }
            }


            return instance;
        }
    }
}
