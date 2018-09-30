using CascadeParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
            Serialize(instance, instance.GetType(), inKey, 0, inLogger);
        }

        public T Deserialize<T>(IKey key, ILogger inLogger)
        {
            return (T)DeserializeInternal(null, key, typeof(T), 0, inLogger);
        }

        bool IsInheriteAccess(Type inDeclaredType)
        {
            return inDeclaredType != null && 
                inDeclaredType != typeof(object) && 
                inDeclaredType != typeof(ValueType) &&
                inDeclaredType != typeof(Enum) &&
                inDeclaredType != typeof(Array);
        }

        void Serialize(object instance, Type inDeclaredType, IKey inKey, int inInheriteDeep, ILogger inLogger)
        {
            Type base_type = inDeclaredType.BaseType;
            if (IsInheriteAccess(base_type))
                Serialize(instance, base_type, inKey, inInheriteDeep + 1, inLogger);

            Type type = instance == null ? inDeclaredType : instance.GetType();

            if (instance == null || inDeclaredType.IsAtomic())
                SerializeAtomic(instance, inDeclaredType, inKey, inLogger);
            else if (inDeclaredType.IsGenericDictionary())
                SerializeGenericDictionary(instance, inDeclaredType, inKey, inInheriteDeep, inLogger);
            else if (inDeclaredType.IsArray)
                SerializeArray(instance, inDeclaredType, inKey, inLogger);
            else if (inDeclaredType.IsGenericCollection())
                SerializeGenericCollection(instance, inDeclaredType, inKey, inInheriteDeep, inLogger);
            else
                SerializeClass(instance, inDeclaredType, inKey, inLogger);
        }

        public object DeserializeInternal(object inInstance, IKey inKey, Type inDeclaredType, int inInheriteDeep, ILogger inLogger)
        {
            object instance;

            // Atomic or null values
            if (inDeclaredType.IsAtomic())
                instance = DeserializeAtomic(inInstance, inKey, inDeclaredType, inLogger);
            // Dictionaries
            else if (inDeclaredType.IsGenericDictionary())
                instance = DeserializeDictionary(inInstance, inKey, inDeclaredType, inInheriteDeep, inLogger);
            // Arrays
            else if (inDeclaredType.IsArray)
                instance = DeserializeArray(inKey, inDeclaredType, inLogger);
            // lists and sets (any collection excluding dictionaries)
            else if (inDeclaredType.IsGenericCollection())
                instance = DeserializeGenericCollection(inInstance, inKey, inDeclaredType, inInheriteDeep, inLogger);
            // Everything else (serialized with recursive property reflection)
            else
                instance = DeserializeClass(inInstance, inKey, inDeclaredType, inLogger);

            Type base_type = inDeclaredType.BaseType;
            if (IsInheriteAccess(base_type))
                DeserializeInternal(instance, inKey, base_type, inInheriteDeep + 1, inLogger);

            return instance;
        }

        #region Atomic
        void SerializeAtomic(object instance, Type declaredType, IKey inKey, ILogger inLogger)
        {
            AddValueToKey(inKey, instance);
        }

        public object DeserializeAtomic(object inInstance, IKey inKey, Type type, ILogger inLogger)
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
        void SerializeGenericDictionary(object instance, Type declaredType, IKey inKey, int inInheriteDeep, ILogger inLogger)
        {
            var dictionary = instance as IDictionary;
            Type[] gen_args = declaredType.GetGenericArguments();
            if (gen_args.Length < 2)
            {
                inLogger.LogError(string.Format("SerializeGenericDictionary: Generic Arguments are None. Type {0}. Instance {1}", declaredType.Name, instance));
                return;
            }

            Type keyDeclaredType = gen_args[0];
            Type valueDeclaredType = gen_args[1];

            if (!keyDeclaredType.IsAtomic())
                inLogger.LogError("Dictionary must simple key.");
            else
            {
                IKey tree_key = inKey;
                if (inInheriteDeep > 0)
                    tree_key = inKey.CreateChildKey("BaseDictionary");

                foreach (var key in dictionary.Keys)
                {
                    IKey child = tree_key.CreateChildKey(key.ToString());
                    Serialize(dictionary[key], valueDeclaredType, child, 0, inLogger);
                }
            }
        }

        public object DeserializeDictionary(object inInstance, IKey inKey, Type declaredType, int inInheriteDeep, ILogger inLogger)
        {
            Type[] gen_args = declaredType.GetGenericArguments();
            if (gen_args.Length < 2)
            {
                inLogger.LogError(string.Format("DeserializeDictionary: Generic Arguments are None. Type {0}", declaredType.Name));
                return inInstance;
            }

            object instance = inInstance;
            // Instantiate if necessary
            if (instance == null)
                instance = _reflectionProvider.Instantiate(declaredType);

            var dictionary = instance as IDictionary;
            
            Type keyDeclaredType = gen_args[0];
            Type valueDeclaredType = gen_args[1];

            IKey tree_key = inKey;
            if (inInheriteDeep > 0)
                tree_key = inKey.GetChild("BaseDictionary");

            for (int i = 0; i < tree_key.GetChildCount(); ++i)
            {
                IKey sub_key = tree_key.GetChild(i);

                object dic_key;
                if (!ReflectionHelper.StringToAtomicValue(sub_key.GetName(), keyDeclaredType, out dic_key))
                {
                    inLogger.LogError(string.Format("SubKey {0} for dictionary with key type {1} can't convert value {2}",
                        tree_key, keyDeclaredType.Name, sub_key.GetName()));
                }
                else
                {
                    object dic_value = DeserializeInternal(null, sub_key, valueDeclaredType, 0, inLogger);

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
                    Serialize(value, declaredItemType, child, 0, inLogger);
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
                        obj_value = DeserializeInternal(null, sub_key, declaredItemType, 0, inLogger);
                    }

                    multi_dim_array.SetValue(obj_value, indexer.Current);
                }
            }

            return instance;
        }
        #endregion Array

        #region GenericCollection
        void SerializeGenericCollection(object instance, Type type, IKey inKey, int inInheriteDeep, ILogger inLogger)
        {
            var collection = instance as IEnumerable;

            Type[] gen_args = type.GetGenericArguments();
            if(gen_args.Length == 0)
            {
                inLogger.LogError(string.Format("SerializeGenericCollection: Generic Arguments are None. Type {0}. Instance {1}", type.Name, instance));
                return;
            }
            
            Type declaredItemType = gen_args[0];
            bool atomic_member = declaredItemType.IsAtomic();

            IKey tree_key = inKey;
            if (inInheriteDeep > 0)
                tree_key = inKey.CreateChildKey("BaseCollection");

            foreach (var item in collection)
            {
                if (atomic_member)
                    AddValueToKey(tree_key, item);
                else
                {
                    IKey child = tree_key.CreateArrayKey();
                    Serialize(item, declaredItemType, child, 0, inLogger);
                }
            }
        }

        public object DeserializeGenericCollection(object inInstance, IKey inKey, Type type, int inInheriteDeep, ILogger inLogger)
        {
            Type[] gen_args = type.GetGenericArguments();
            if (gen_args.Length == 0)
            {
                inLogger.LogError(string.Format("DeserializeGenericCollection: Generic Arguments are None. Type {0}", type.Name));
                return inInstance;
            }

            Type declaredItemType = type.GetGenericArguments()[0];
            bool is_atomic_elems = declaredItemType.IsAtomic();

            bool isHashSet = type.IsHashSet();

            object instance = inInstance;
            if (instance == null)
                instance = _reflectionProvider.Instantiate(type);

            instance = instance as IEnumerable;

            MethodHandler addToHashSet = null;
            if (isHashSet)
                addToHashSet = _reflectionProvider.GetDelegate(type.GetMethod("Add"));

            int element_count = is_atomic_elems ? inKey.GetValuesCount() : inKey.GetChildCount();

            IKey tree_key = inKey;
            if (inInheriteDeep > 0)
                tree_key = inKey.GetChild("BaseCollection");

            for (int i = 0; i < element_count; i++)
            {
                object obj_value;
                if (is_atomic_elems)
                {
                    string str_value = tree_key.GetValueAsString(i);
                    if (!ReflectionHelper.StringToAtomicValue(str_value, declaredItemType, out obj_value))
                    {
                        inLogger.LogError(string.Format("Key {0} for collection with element type {1} can't convert value {2}",
                            tree_key, declaredItemType.Name, str_value));
                    }
                }
                else
                {
                    IKey sub_key = tree_key.GetChild(i);
                    obj_value = DeserializeInternal(null, sub_key, declaredItemType, 0, inLogger);
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
            public string ChangedName;
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
            {
                prms.Name = memberInfo.Name;
                prms.ChangedName = ChangeFieldName(memberInfo);
            }
            else
                prms.ChangedName = prms.Name;

            return prms;
        }

        string ChangeFieldName(MemberInfo memberInfo)
        {
            string name = memberInfo.Name;
            string[] parts = name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return name;

            var sb = new StringBuilder();
            List<char> lst = new List<char>();
            for(int i = 0; i < parts.Length; ++i)
            {
                string p = parts[i];
                for (int c = 0; c < p.Length; ++c)
                {
                    char ch = p[c];

                    if(Char.IsLetter(ch))
                    {
                        if (c == 0 && Char.IsLower(ch))
                            ch = Char.ToUpper(ch);
                        //else if (c != 0 && Char.IsUpper(ch))
                        //    ch = Char.ToLower(ch);
                    }

                    lst.Add(ch);
                }
                sb.Append(new string(lst.ToArray()));
                lst.Clear();
            }

            return sb.ToString();
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


                IKey child = inKey.CreateChildKey(member_params.ChangedName);

                Type real_type = value.GetType();
                Type member_type = memberInfo.GetMemberType();

                if (member_params.Converter != null && member_params.Converter.CanConvert(real_type))
                    member_params.Converter.WriteKey(child, value, inLogger);
                else
                    Serialize(value, real_type, child, 0, inLogger);

                // Write the runtime type if different (except nullables since they get unboxed)
                if (real_type != member_type && !member_type.IsNullable())
                {
                    IKey obj_type_key = child.CreateChildKey("RealObjectType");
                    obj_type_key.AddValue(real_type.FullName);
                    obj_type_key.AddValue(real_type.Assembly.FullName);
                }
            }

            if (inKey.GetChildCount() == 0)
                inKey.AddValue("default");
        }

        public object DeserializeClass(object inInstance, IKey inKey, Type type, ILogger inLogger)
        {
            IKey type_key = inKey.GetChild("RealObjectType");
            if (type_key != null)
            {
                string type_name = type_key.GetValueAsString(0);
                string assembly_name = type_key.GetValueAsString(1);
                
                try
                {
                    Assembly assembly = Assembly.Load(assembly_name);
                    Type obj_type = assembly.GetType(type_name, true);
                    if (obj_type != null)
                        type = obj_type;
                }
                catch(Exception ex)
                {
                    inLogger.LogError(string.Format("Cant take type from RealObjectType {0}. Exception: {1}", type_name, ex.Message));
                }
            }

            object instance = inInstance;
            if (instance == null)
                instance = _reflectionProvider.Instantiate(type);

            MemberInfo[] member_infos = _reflectionProvider.GetSerializableMembers(type);
            foreach (MemberInfo memberInfo in member_infos)
            {
                SCustomMemberParams member_params = GetMemberParams(memberInfo);

                Type memberType = memberInfo.GetMemberType();

                IKey sub_key = inKey.GetChild(member_params.ChangedName);
                if(sub_key == null)
                    sub_key = inKey.GetChild(member_params.Name);

                if (sub_key != null)
                {
                    object readValue;
                    if (member_params.Converter != null)
                        readValue = member_params.Converter.ReadKey(sub_key, inLogger);
                    else
                        readValue = DeserializeInternal(null, sub_key, memberType, 0, inLogger);

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
