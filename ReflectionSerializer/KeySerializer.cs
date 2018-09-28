using CascadeParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ReflectionSerializer
{
    public interface ILogger
    {
        void LogWarning(string inText);
        void LogError(string inText);
        void Trace(string inText);
    }

    public class CCascadeSerializer
    {
        readonly IReflectionProvider _reflectionProvider;

        public CCascadeSerializer(IReflectionProvider reflectionProvider)
        {
            _reflectionProvider = reflectionProvider;
        }

        public CCascadeSerializer(): this(new CachedReflector())
        {
            
        }

        public IKey SerializeToKey(object instance, string inRootName, ILogger inLogger)
        {
            IKey root = IKeyFactory.CreateKey(inRootName);
            Serialize(instance, root, inLogger);
            return root;
        }

        public string SerializeToCascade(object instance, string inRootName, ILogger inLogger)
        {
            IKey key = SerializeToKey(instance, inRootName, inLogger);
            string text = key.SaveToString();
            return text;
        }

        public void Serialize(object instance, IKey inKey, ILogger inLogger)
        {
            Serialize(instance, instance.GetType(), inKey, inLogger);
        }

        public T Deserialize<T>(IKey key, ILogger inLogger)
        {
            return (T)DeserializeInternal(key, typeof(T), inLogger);
        }

        void Serialize(object instance, Type declaredType, IKey inKey, ILogger inLogger)
        {
            Type type = instance == null ? declaredType : instance.GetType();

            if (instance == null || type.IsAtomic())
                SerializeAtomic(instance, type, inKey, inLogger);
            else if (type.IsGenericDictionary())
                SerializeGenericDictionary(instance, type, inKey, inLogger);
            else if (type.IsArray)
                SerializeArray(instance, type, inKey, inLogger);
            else if (type.IsGenericCollection())
                SerializeGenericCollection(instance, type, inKey, inLogger);
            else
                SerializeClass(instance, type, inKey, inLogger);

            // Write the runtime type if different (except nullables since they get unboxed)
            if (!declaredType.IsNullable() && type != declaredType)
                inKey.CreateChildKey("RealObjectType").AddValue(type.FullName);
        }

        public object DeserializeInternal(IKey inKey, Type declaredType, ILogger inLogger)
        {
            Type type = declaredType;
            object instance;

            // Atomic or null values
            if (type.IsAtomic())
                instance = DeserializeAtomic(inKey, type, inLogger);
            // Dictionaries
            else if (type.IsGenericDictionary())
                instance = DeserializeDictionary(inKey, type, inLogger);
            // Arrays
            else if (type.IsArray)
                instance = DeserializeArray(inKey, type, inLogger);
            // lists and sets (any collection excluding dictionaries)
            else if (type.IsGenericCollection())
                instance = DeserializeGenericCollection(inKey, type, inLogger);
            // Everything else (serialized with recursive property reflection)
            else
                instance = DeserializeClass(inKey, type, inLogger);

            return instance;
        }

        #region Atomic
        void SerializeAtomic(object instance, Type declaredType, IKey inKey, ILogger inLogger)
        {
            AddValueToKey(inKey, instance);
        }

        public object DeserializeAtomic(IKey inKey, Type type, ILogger inLogger)
        {
            object instance;
            if (inKey.GetValuesCount() == 0)
            {
                instance = ReflectionHelper.GetDefaultValue(type, _reflectionProvider);
            }
            else if (inKey.GetValuesCount() > 1)
            {
                inLogger.LogError(string.Format("Need one value for type {1}. Key: {0} ", inKey, type.Name));
                instance = ReflectionHelper.GetDefaultValue(type, _reflectionProvider);
            }
            else
            {
                string key_value = inKey.GetValueAsString(0);
                if (!ReflectionHelper.StringToAtomicValue(key_value, type, out instance))
                {
                    inLogger.LogError(string.Format("Key {0} with value {1} can't convert value to type {2}", inKey, key_value, type.Name));
                    instance = ReflectionHelper.GetDefaultValue(type, _reflectionProvider);
                }
            }
            return instance;
        }
        #endregion Atomic

        #region Dictionary
        void SerializeGenericDictionary(object instance, Type declaredType, IKey inKey, ILogger inLogger)
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

        public object DeserializeDictionary(IKey inKey, Type type, ILogger inLogger)
        {
            // Instantiate if necessary
            object instance = _reflectionProvider.Instantiate(type);

            var dictionary = instance as IDictionary;
            Type[] genericArguments = dictionary.GetType().GetGenericArguments();
            Type keyDeclaredType = genericArguments[0];
            Type valueDeclaredType = genericArguments[1];

            for (int i = 0; i < inKey.GetChildCount(); ++i)
            {
                IKey sub_key = inKey.GetChild(i);

                object dic_key;
                if (!ReflectionHelper.StringToAtomicValue(sub_key.GetName(), keyDeclaredType, out dic_key))
                {
                    inLogger.LogError(string.Format("SubKey {0} for dictionary with key type {1} can't convert value {2}",
                        inKey, keyDeclaredType.Name, sub_key.GetName()));
                }
                else
                {
                    object dic_value = DeserializeInternal(sub_key, valueDeclaredType, inLogger);

                    if (dictionary.Contains(dic_key))
                        dictionary.Remove(dic_key);
                    dictionary.Add(dic_key, dic_value);
                }
            }
            return instance;
        }
        #endregion Dictionary

        #region Array
        IKey GetOrCreateKeyByArrayIndex(IKey inParent, int[] indicies)
        {
            IKey key = inParent;
            //indicies.Length - 1 - last array to one key
            for (int i = 0; i < indicies.Length - 1; i++)
            {
                int index = indicies[i];
                while (index >= key.GetChildCount())
                    key.CreateArrayKey();

                key = key.GetChild(index);
            }
            return key;
        }

        IKey GetKeyByArrayIndex(IKey inParent, int[] indicies)
        {
            IKey key = inParent;
            //indicies.Length - 1 - last array to one key
            for (int i = 0; i < indicies.Length - 1; i++)
            {
                int index = indicies[i];
                if (index >= key.GetChildCount())
                    return null;
                key = key.GetChild(index);
            }
            return key;
        }

        void SerializeArray(object instance, Type type, IKey inKey, ILogger inLogger)
        {
            Type declaredItemType = type.GetElementType();
            bool atomic_member = declaredItemType.IsAtomic();

            Array multi_dim_array = instance as Array;

            CMultiArrayIndexer indexer = new CMultiArrayIndexer(multi_dim_array);
            while (indexer.MoveNext())
            {
                object value = multi_dim_array.GetValue(indexer.Current);
                IKey dim_child = GetOrCreateKeyByArrayIndex(inKey, indexer.Current);
                if (atomic_member)
                    AddValueToKey(dim_child, value);
                else
                {
                    IKey child = dim_child.CreateArrayKey();
                    Serialize(value, declaredItemType, child, inLogger);
                }
            }
        }

        bool AllChildsAreArray(IKey inKey)
        {
            if (inKey.GetChildCount() == 0)
                return false;

            for (int i = 0; i < inKey.GetChildCount(); ++i)
                if (!inKey.GetChild(i).IsArrayKey())
                    return false;
            return true;
        }

        int[] FindArrayDimension(IKey inKey, bool IsAtomicElementType)
        {
            List<int> lst = new List<int>();
            IKey ck = inKey;
            while (ck.GetChildCount() > 0 && ck.GetValuesCount() == 0 && AllChildsAreArray(ck))
            {
                lst.Add(ck.GetChildCount());
                ck = ck.GetChild(0);
            }
            if (IsAtomicElementType)
                lst.Add(ck.GetValuesCount());
            return lst.ToArray();
        }

        public object DeserializeArray(IKey inKey, Type type, ILogger inLogger)
        {
            Array multi_dim_array = null;
            Type declaredItemType = type.GetElementType();

            bool is_atomic_elems = declaredItemType.IsAtomic();

            int[] dims = FindArrayDimension(inKey, is_atomic_elems);

            object instance = Array.CreateInstance(declaredItemType, dims);

            multi_dim_array = instance as Array;

            CMultiArrayIndexer indexer = new CMultiArrayIndexer(multi_dim_array);

            while (indexer.MoveNext())
            {
                IKey dim_child = GetKeyByArrayIndex(inKey, indexer.Current);
                if (dim_child == null)
                    inLogger.LogError(string.Format("Cant get value for multi array index {0}", indexer));
                else
                {
                    object obj_value;
                    int last_index = indexer.Current[indexer.Current.Length - 1];

                    if (is_atomic_elems)
                    {
                        string str_value = dim_child.GetValueAsString(last_index);
                        if (!ReflectionHelper.StringToAtomicValue(str_value, declaredItemType, out obj_value))
                        {
                            inLogger.LogError(string.Format("Key {0} for collection with element type {1} can't convert value {2}",
                                inKey, declaredItemType.Name, str_value));
                        }
                    }
                    else
                    {
                        IKey sub_key = dim_child.GetChild(last_index);
                        obj_value = DeserializeInternal(sub_key, declaredItemType, inLogger);
                    }

                    multi_dim_array.SetValue(obj_value, indexer.Current);
                }
            }

            return instance;
        }
        #endregion Array

        #region GenericCollection
        void SerializeGenericCollection(object instance, Type type, IKey inKey, ILogger inLogger)
        {
            var collection = instance as IEnumerable;
            Type declaredItemType = type.GetGenericArguments()[0];
            bool atomic_member = declaredItemType.IsAtomic();
            foreach (var item in collection)
            {
                if (atomic_member)
                    AddValueToKey(inKey, item);
                else
                {
                    IKey child = inKey.CreateArrayKey();
                    Serialize(item, declaredItemType, child, inLogger);
                }
            }
        }

        public object DeserializeGenericCollection(IKey inKey, Type type, ILogger inLogger)
        {
            bool isHashSet = type.IsHashSet();
            Type declaredItemType = type.GetGenericArguments()[0];
            object instance = _reflectionProvider.Instantiate(type) as IEnumerable;
            bool is_atomic_elems = declaredItemType.IsAtomic();

            MethodHandler addToHashSet = null;
            if (isHashSet)
                addToHashSet = _reflectionProvider.GetDelegate(type.GetMethod("Add"));

            int element_count = is_atomic_elems ? inKey.GetValuesCount() : inKey.GetChildCount();

            for (int i = 0; i < element_count; i++)
            {
                object obj_value;
                if (is_atomic_elems)
                {
                    string str_value = inKey.GetValueAsString(i);
                    if (!ReflectionHelper.StringToAtomicValue(str_value, declaredItemType, out obj_value))
                    {
                        inLogger.LogError(string.Format("Key {0} for collection with element type {1} can't convert value {2}",
                            inKey, declaredItemType.Name, str_value));
                    }
                }
                else
                {
                    IKey sub_key = inKey.GetChild(i);
                    obj_value = DeserializeInternal(sub_key, declaredItemType, inLogger);
                }

                if (isHashSet)
                    // Potential problem if set already contains key...
                    addToHashSet(instance, obj_value);
                else if (instance is IList)
                    (instance as IList).Add(obj_value);
                else
                    throw new NotImplementedException();
            }

            return instance;
        }
        #endregion GenericCollection

        #region Class
        struct SCustomMemberParams
        {
            public string Name;
            public object DefaultValue;
            public CascadeConverter Converter;
        }

        SCustomMemberParams GetMemberParams(MemberInfo memberInfo)
        {
            var prms = new SCustomMemberParams();
            object[] attributes = memberInfo.GetCustomAttributes(false);
            for (int i = 0; i < attributes.Length; ++i)
            {
                if (string.IsNullOrEmpty(prms.Name))
                {
                    DataMemberAttribute dm = attributes[i] as DataMemberAttribute;
                    if (dm != null && !string.IsNullOrEmpty(dm.Name))
                        prms.Name = dm.Name;
                }

                CascadePropertyAttribute jp = attributes[i] as CascadePropertyAttribute;
                if (jp != null)
                {
                    if (string.IsNullOrEmpty(prms.Name) && !string.IsNullOrEmpty(jp.Name))
                        prms.Name = jp.Name;

                    if (prms.DefaultValue == null && jp.Default != null)
                        prms.DefaultValue = jp.Default;
                }

                if (prms.Converter == null)
                {
                    CascadeConverterAttribute conv = attributes[i] as CascadeConverterAttribute;
                    if (conv != null && conv.CustomConverter != null)
                        prms.Converter = conv.CustomConverter;
                }
            }

            if (string.IsNullOrEmpty(prms.Name))
                prms.Name = memberInfo.Name;

            return prms;
        }

        void SerializeClass(object instance, Type type, IKey inKey, ILogger inLogger)
        {
            MemberInfo[] member_infos = _reflectionProvider.GetSerializableMembers(type);
            foreach (MemberInfo memberInfo in member_infos)
            {
                object value = _reflectionProvider.GetValue(memberInfo, instance);
                if (value == null)
                    continue;

                SCustomMemberParams member_params = GetMemberParams(memberInfo);

                if (member_params.DefaultValue != null)
                {
                    if (member_params.DefaultValue.Equals(value))
                        continue;
                }
                else if (ReflectionHelper.IsDefault(value, _reflectionProvider))
                    continue;


                IKey child = inKey.CreateChildKey(member_params.Name);

                Type memberType = memberInfo.GetMemberType();

                if (member_params.Converter != null && member_params.Converter.CanConvert(memberType))
                    member_params.Converter.WriteKey(child, value, inLogger);
                else
                    Serialize(value, memberType, child, inLogger);
            }

            if (inKey.GetChildCount() == 0)
                inKey.AddValue("default");
        }

        public object DeserializeClass(IKey inKey, Type type, ILogger inLogger)
        {
            IKey type_key = inKey.GetChild("RealObjectType");
            if (type_key != null)
            {
                string type_name = type_key.GetValueAsString(0);
                Type obj_type = Type.GetType(type_name, false);
                if (obj_type != null)
                    type = obj_type;
            }

            object instance = _reflectionProvider.Instantiate(type);

            MemberInfo[] member_infos = _reflectionProvider.GetSerializableMembers(type);
            foreach (MemberInfo memberInfo in member_infos)
            {
                SCustomMemberParams member_params = GetMemberParams(memberInfo);

                Type memberType = memberInfo.GetMemberType();

                IKey sub_key = inKey.GetChild(member_params.Name);
                if (sub_key != null)
                {
                    object readValue;
                    if (member_params.Converter != null)
                        readValue = member_params.Converter.ReadKey(sub_key, inLogger);
                    else
                        readValue = DeserializeInternal(sub_key, memberType, inLogger);

                    // This dirty check is naive and doesn't provide performance benefits
                    //if (memberType.IsClass && readValue != currentValue && (readValue == null || !readValue.Equals(currentValue)))
                    _reflectionProvider.SetValue(memberInfo, instance, readValue);
                }
                else if (member_params.DefaultValue != null)
                    _reflectionProvider.SetValue(memberInfo, instance, member_params.DefaultValue);
            }

            return instance;
        }
        #endregion Class

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
    }
}
