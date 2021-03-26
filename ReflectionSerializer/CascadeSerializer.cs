using CascadeParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace CascadeSerializer
{
    public enum EReflectorType
    {
        Cached = 0,
        Direct
    }

    public class CCascadeSerializer
    {
        readonly IReflectionProvider _reflectionProvider;

        CParserManager _parser;
        public CParserManager Parser { get { return _parser; } }

        string _debug_file_name;
        string _debug_text;

        public CCascadeSerializer(EReflectorType inReflectorType, CParserManager parser)
        {
            switch (inReflectorType)
            {
                case EReflectorType.Cached:  _reflectionProvider = new CachedReflector(); break;
                case EReflectorType.Direct: _reflectionProvider = new DirectReflector(); break;
            }
            _parser = parser;
        }

        public CCascadeSerializer(CParserManager parser, EReflectorType inReflectorType = EReflectorType.Cached) 
            : this(inReflectorType, parser)
        {
            
        }

        public CCascadeSerializer(IParserOwner owner, EReflectorType inReflectorType = EReflectorType.Cached) 
            : this(inReflectorType, null)
        {
            _parser = new CParserManager(owner);
        }

        public CCascadeSerializer(EReflectorType inReflectorType = EReflectorType.Cached) : this(inReflectorType, null)
        {

        }

        public IKey SerializeToKey(object instance, string inRootName, ILogPrinter inLogger)
        {
            IKey root = IKeyFactory.CreateKey(inRootName);
            Serialize(instance, root, inLogger);
            return root;
        }

        //public string SerializeToCascade(object instance, string inRootName, ILogPrinter inLogger)
        public string SerializeToCascade(object instance, ILogPrinter inLogger)
        {
            //IKey key = SerializeToKey(instance, string.Empty, inLogger);
            IKey key = IKeyFactory.CreateArrayKey(null);
            Serialize(instance, key, inLogger);
            string text = key.SaveToString();
            return text;
        }

        public void Serialize(object instance, IKey inKey, ILogPrinter inLogger)
        {
            Type type = instance.GetType();
            Serialize(instance, type, inKey, 0, inLogger);
            //if (inKey.GetChildCount() == 1 && type.IsClass && string.IsNullOrEmpty(inKey.GetName()))
            //    inKey.SetName(type.Name);
        }

        public T Deserialize<T>(string text, ILogPrinter inLogger)
        {
            return Deserialize<T>(string.Empty, text, inLogger, null);
        }

        public T Deserialize<T>(string text, ILogPrinter inLogger, object inContextData)
        {
            return Deserialize<T>(string.Empty, text, inLogger, inContextData);
        }

        public T Deserialize<T>(string file_name, string text, ILogPrinter inLogger, object inContextData)
        {
            if(_parser == null)
            {
                LogError(inLogger, "Cascade Parser doesnt present!");
                return default(T);
            }
            _debug_file_name = file_name;
            _debug_text = text;
            IKey key = _parser.Parse(file_name, text, inLogger, inContextData);
            return Deserialize<T>(key, inLogger);
        }

        public T Deserialize<T>(IKey key, ILogPrinter inLogger)
        {
            return (T)DeserializeInternal(null, key, typeof(T), 0, 0, inLogger);
        }

        public void DeserializeToObject<T>(IKey key, T inInstance, ILogPrinter inLogger)
        {
            DeserializeInternal(inInstance, key, typeof(T), 0, 0, inLogger);
        }

        bool IsInheriteAccess(Type inDeclaredType)
        {
            return inDeclaredType != null && 
                inDeclaredType != typeof(object) && 
                inDeclaredType != typeof(ValueType) &&
                inDeclaredType != typeof(Enum) &&
                inDeclaredType != typeof(Array);
        }

        void Serialize(object instance, Type inDeclaredType, IKey inKey, int inInheriteDeep, ILogPrinter inLogger)
        {
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
                SerializeClass(instance, inDeclaredType, inKey, inInheriteDeep, inLogger);

            Type base_type = inDeclaredType.BaseType;
            if (IsInheriteAccess(base_type))
                Serialize(instance, base_type, inKey, inInheriteDeep + 1, inLogger);

            //if (inKey.GetChildCount() == 0 && inInheriteDeep == 0)
            //    inKey.AddValue("default");
        }

        object DeserializeInternal(object inInstance, IKey inKey, Type inDeclaredType, int inInheriteDeep, int inStructDeep, ILogPrinter inLogger)
        {
            object instance;

            // Atomic or null values
            if (inDeclaredType.IsAtomic())
                instance = DeserializeAtomic(inInstance, inKey, inDeclaredType, inStructDeep, inLogger);
            // Dictionaries
            else if (inDeclaredType.IsGenericDictionary())
                instance = DeserializeDictionary(inInstance, inKey, inDeclaredType, inInheriteDeep, inStructDeep, inLogger);
            // Arrays
            else if (inDeclaredType.IsArray)
                instance = DeserializeArray(inKey, inDeclaredType, inStructDeep, inLogger);
            // lists and sets (any collection excluding dictionaries)
            else if (inDeclaredType.IsGenericCollection())
                instance = DeserializeGenericCollection(inInstance, inKey, inDeclaredType, inInheriteDeep, inStructDeep, inLogger);
            // Everything else (serialized with recursive property reflection)
            else
                instance = DeserializeClass(inInstance, inKey, inDeclaredType, inStructDeep, inLogger);

            Type base_type = inDeclaredType.BaseType;
            if (IsInheriteAccess(base_type))
                DeserializeInternal(instance, inKey, base_type, inInheriteDeep + 1, inStructDeep, inLogger);

            return instance;
        }

        #region Atomic
        void SerializeAtomic(object instance, Type declaredType, IKey inKey, ILogPrinter inLogger)
        {
            AddValueToKey(inKey, instance);
        }

        void LogError(ILogPrinter inLogger, string inText)
        {
            string pt = string.Empty;
            if(!string.IsNullOrEmpty(_debug_text))
            {
                if (_debug_text.Length > 30)
                    pt = _debug_text.Substring(0, 30);
                else
                    pt = _debug_text;
            }
            inLogger.LogError(string.Format("{2}: [File: {0}. Text: {1}]", _debug_file_name, _debug_text, inText));
        }

        object DeserializeAtomic(object inInstance, IKey inKey, Type type, int inStructDeep, ILogPrinter inLogger)
        {
            object instance;
            IKey key = inKey;

            if (key.GetValuesCount() == 0)
            {
                instance = ReflectionHelper.GetDefaultValue(type, _reflectionProvider, inLogger);
            }
            else if (key.GetValuesCount() > 1)
            {
                var sb = new StringBuilder();
                sb.Append("\"");
                for (int i = 0; i < key.GetValuesCount(); ++i)
                {
                    if(i > 0)
                        sb.Append(",");
                    sb.Append(key.GetValueAsString(i));
                }
                sb.Append("\"");

                LogError(inLogger, string.Format("Need one value for type {1}. Key: {0} [{3}]; Values: {2} ", key, type.Name, sb, key.GetPath()));
                instance = ReflectionHelper.GetDefaultValue(type, _reflectionProvider, inLogger);
            }
            else
            {
                string key_value = key.GetValueAsString(0);
                if (!ReflectionHelper.StringToAtomicValue(key_value, type, out instance, _reflectionProvider, inLogger))
                {
                    LogError(inLogger, string.Format("Key {0} [{3}] with value {1} can't convert value to type {2}", key, key_value, type.Name, key.GetPath()));
                    instance = ReflectionHelper.GetDefaultValue(type, _reflectionProvider, inLogger);
                }
            }
            return instance;
        }
        #endregion Atomic

        #region Dictionary
        void SerializeGenericDictionary(object instance, Type declaredType, IKey inKey, int inInheriteDeep, ILogPrinter inLogger)
        {
            var dictionary = instance as IDictionary;
            Type[] gen_args = declaredType.GetGenericArguments();
            if (gen_args.Length < 2)
            {
                LogError(inLogger, string.Format("SerializeGenericDictionary: Generic Arguments are None. Type {0}. Instance {1}", declaredType.Name, instance));
                return;
            }

            Type keyDeclaredType = gen_args[0];
            Type valueDeclaredType = gen_args[1];

            if (!keyDeclaredType.IsAtomic())
                LogError(inLogger, "Dictionary must simple key.");
            else
            {
                IKey tree_key = inKey;
                if (tree_key.GetChildCount() > 0)
                    tree_key = inKey.CreateChildKey("BaseDictionary");

                foreach (var key in dictionary.Keys)
                {
                    IKey child = tree_key.CreateChildKey(key.ToString());
                    Serialize(dictionary[key], valueDeclaredType, child, 0, inLogger);
                }
            }
        }

        object DeserializeDictionary(object inInstance, IKey inKey, Type declaredType, int inInheriteDeep, int inStructDeep, ILogPrinter inLogger)
        {
            Type[] gen_args = declaredType.GetGenericArguments();
            if (gen_args.Length < 2)
            {
                LogError(inLogger, string.Format("DeserializeDictionary: Generic Arguments are None. Type {0}", declaredType.Name));
                return inInstance;
            }

            object instance = inInstance;
            // Instantiate if necessary
            if (instance == null)
                instance = _reflectionProvider.Instantiate(declaredType, inLogger);

            var dictionary = instance as IDictionary;
            
            Type keyDeclaredType = gen_args[0];
            Type valueDeclaredType = gen_args[1];

            IKey tree_key = inKey.GetChild("BaseDictionary");
            if (tree_key == null)
                tree_key = inKey;

            for (int i = 0; i < tree_key.GetChildCount(); ++i)
            {
                IKey sub_key = tree_key.GetChild(i);

                object dic_key;
                if (!ReflectionHelper.StringToAtomicValue(sub_key.GetName(), keyDeclaredType, out dic_key, _reflectionProvider, inLogger))
                {
                    LogError(inLogger, string.Format("SubKey {0} [{3}] for dictionary with key type {1} can't convert value {2}",
                        tree_key, keyDeclaredType.Name, sub_key.GetName(), tree_key.GetPath()));
                }
                else
                {
                    object dic_value = DeserializeInternal(null, sub_key, valueDeclaredType, 0, inStructDeep + 1, inLogger);

                    if (dictionary.Contains(dic_key))
                        dictionary.Remove(dic_key);
                    dictionary.Add(dic_key, dic_value);
                }
            }
            return instance;
        }
        #endregion Dictionary

        #region Array
        IKey GetOrCreateKeyByArrayIndex(IKey inParent, int[] indicies, int[] lengthes, bool atomic_elems)
        {
            IKey key = inParent;
            int keys_length = indicies.Length - 1; //length of key chaine
            //indicies.Length - 1 - last array to one key

            for (int i = 0; i < keys_length; i++)
            {
                int index = indicies[i];
                int length = lengthes[i];
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

        void SerializeArray(object instance, Type type, IKey inKey, ILogPrinter inLogger)
        {
            //if (string.IsNullOrEmpty(inKey.GetName()))
                //inKey.SetName("Values");

            Type declaredItemType = type.GetElementType();
            bool atomic_member = declaredItemType.IsAtomic();

            Array multi_dim_array = instance as Array;

            CMultiArrayIndexer indexer = new CMultiArrayIndexer(multi_dim_array);
            while (indexer.MoveNext())
            {
                object value = multi_dim_array.GetValue(indexer.Current);
                IKey dim_child = GetOrCreateKeyByArrayIndex(inKey, indexer.Current, indexer.Lengthes, atomic_member);
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

            bool all = true;
            for (int i = 0; i < inKey.GetChildCount() && all; ++i)
                if (!inKey.GetChild(i).IsArrayKey())
                    all = false;

            return all;
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
            else if(lst.Count == 0 && ck.GetChildCount() > 0)
                lst.Add(1);
            return lst.ToArray();
        }

        object DeserializeArray(IKey inKey, Type type, int inStructDeep, ILogPrinter inLogger)
        {
            if(inKey.IsEmpty)
                return null;

            IKey key = inKey;

            Array multi_dim_array = null;
            Type declaredItemType = type.GetElementType();

            bool is_atomic_elems = declaredItemType.IsAtomic();
            bool is_array_elems = declaredItemType.IsArray;

            int[] dims;
            if(is_array_elems)
                dims = new int[] { key.GetChildCount() };
            else
                dims = FindArrayDimension(key, is_atomic_elems);

            if (dims.Length == 0)
                return null;

            object instance = Array.CreateInstance(declaredItemType, dims);

            multi_dim_array = instance as Array;

            CMultiArrayIndexer indexer = new CMultiArrayIndexer(multi_dim_array);

            while (indexer.MoveNext())
            {
                IKey dim_child = GetKeyByArrayIndex(key, indexer.Current);
                if (dim_child == null)
                    LogError(inLogger, string.Format("Cant get value for multi array index {0}", indexer));
                else
                {
                    object obj_value;
                    int last_index = indexer.Current[indexer.Current.Length - 1];
                    if (is_atomic_elems)
                    {
                        string str_value;
                        if (dim_child.GetValuesCount() == 0)
                            str_value = string.Empty;
                        else
                            str_value = dim_child.GetValueAsString(last_index);
                        if (!ReflectionHelper.StringToAtomicValue(str_value, declaredItemType, out obj_value, _reflectionProvider, inLogger))
                        {
                            LogError(inLogger, string.Format("Key {0} [{3}] for collection with element type {1} can't convert value {2}",
                                key, declaredItemType.Name, str_value, key.GetPath()));
                        }
                    }
                    else
                    {
                        IKey child = dim_child.GetChild(last_index);
                        obj_value = DeserializeInternal(null, child, declaredItemType, 0, inStructDeep + 1, inLogger);
                    }

                    multi_dim_array.SetValue(obj_value, indexer.Current);
                }
            }

            return instance;
        }
        #endregion Array

        #region GenericCollection
        void SerializeGenericCollection(object instance, Type type, IKey inKey, int inInheriteDeep, ILogPrinter inLogger)
        {
            var collection = instance as IEnumerable;

            Type[] gen_args = type.GetGenericArguments();
            if(gen_args.Length == 0)
            {
                LogError(inLogger, string.Format("SerializeGenericCollection: Generic Arguments are None. Type {0}. Instance {1}", type.Name, instance));
                return;
            }
            
            Type declaredItemType = gen_args[0];
            bool atomic_member = declaredItemType.IsAtomic();

            IKey tree_key = inKey;
            if (tree_key.GetValuesCount() > 0 || tree_key.GetChildCount() > 0)
                tree_key = inKey.CreateChildKey("BaseCollection");

            if (atomic_member)
            {
                foreach (var item in collection)
                    AddValueToKey(tree_key, item);
            }
            else
            {
                foreach (var item in collection)
                {
                    IKey child = tree_key.CreateArrayKey();
                    Serialize(item, declaredItemType, child, 0, inLogger);
                }
            }
        }

        struct SCollect
        {
            object _instance;
            MethodHandler _addToHashSet;
            IList _list;

            public SCollect(object inInstance, Type type, IReflectionProvider provider)
            {
                _instance = inInstance;

                bool isHashSet = type.IsHashSet();

                if (isHashSet)
                {
                    _addToHashSet = provider.GetDelegate(type.GetMethod("Add"));
                    _list = null;
                }
                else
                {
                    _addToHashSet = null;
                    _list = inInstance as IList;
                }
            }

            public void AddValue(object value)
            {
                if (_addToHashSet != null)
                    _addToHashSet(_instance, value);
                else
                    _list.Add(value);
            }
        }

        object DeserializeGenericCollection(object inInstance, IKey inKey, Type type, int inInheriteDeep, int inStructDeep, ILogPrinter inLogger)
        {
            Type[] gen_args = type.GetGenericArguments();
            if (gen_args.Length == 0)
            {
                LogError(inLogger, string.Format("DeserializeGenericCollection: Generic Arguments are None. Type {0}", type.Name));
                return inInstance;
            }

            Type declaredItemType = gen_args[0];
            bool is_atomic_elems = declaredItemType.IsAtomic();

            object instance = inInstance;
            if (instance == null)
                instance = _reflectionProvider.Instantiate(type, inLogger);

            instance = instance as IEnumerable;

            SCollect collect = new SCollect(instance, type, _reflectionProvider);

            IKey tree_key = inKey.GetChild("BaseCollection");
            if (tree_key == null)
                tree_key = inKey;

            if(is_atomic_elems)
            {
                int element_count = tree_key.GetValuesCount();
                for (int i = 0; i < element_count; i++)
                {
                    object obj_value;
                    string str_value = tree_key.GetValueAsString(i);
                    if (!ReflectionHelper.StringToAtomicValue(str_value, declaredItemType, out obj_value, _reflectionProvider, inLogger))
                    {
                        LogError(inLogger, string.Format("Key {0} [{3}] for collection with element type {1} can't convert value {2}",
                            tree_key, declaredItemType.Name, str_value, tree_key.GetPath()));
                    }

                    collect.AddValue(obj_value);
                }
            }
            else if (tree_key.GetChildCount() > 0)
            {
                if (AllChildsAreArray(tree_key))
                {
                    int element_count = tree_key.GetChildCount();

                    for (int i = 0; i < element_count; i++)
                    {
                        IKey sub_key = tree_key.GetChild(i);
                        object obj_value = DeserializeInternal(null, sub_key, declaredItemType, 0, inStructDeep + 1, inLogger);
                        collect.AddValue(obj_value);
                    }
                }
                else
                {
                    object obj_value = DeserializeInternal(null, tree_key, declaredItemType, 0, inStructDeep + 1, inLogger);
                    collect.AddValue(obj_value);
                }
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
                //if (string.IsNullOrEmpty(prms.Name))
                //{
                //    DataMemberAttribute dm = attributes[i] as DataMemberAttribute;
                //    if (dm != null && !string.IsNullOrEmpty(dm.Name))
                //        prms.Name = dm.Name;
                //}

                {
                    DefaultMemberAttribute dm = attributes[i] as DefaultMemberAttribute;
                    if (dm != null && !string.IsNullOrEmpty(dm.MemberName))
                        prms.Name = dm.MemberName;
                }

                {
                    DefaultValueAttribute dm = attributes[i] as DefaultValueAttribute;
                    if (prms.DefaultValue == null && dm != null)
                        prms.DefaultValue = dm.Value;
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

        void SerializeClass(object instance, Type type, IKey inKey, int inInheriteDeep, ILogPrinter inLogger)
        {
            MethodInfo mi = type.GetMethod("SerializationToCscd", new Type[] { typeof(CascadeParser.IKey), typeof(CascadeParser.ILogPrinter) });
            if(mi != null)
            {
                if (string.IsNullOrEmpty(inKey.GetName()))
                    inKey.SetName("Value");

                mi.Invoke(instance, new object[] { inKey, inLogger });
                return;
            }

            MemberInfo[] member_infos = _reflectionProvider.GetSerializableMembers(type);
            foreach (MemberInfo memberInfo in member_infos)
            {
                object value = _reflectionProvider.GetValue(memberInfo, instance);
                if (value == null)
                    continue;

                SCustomMemberParams member_params = GetMemberParams(memberInfo);

                if (member_params.DefaultValue != null)
                {
                    if(member_params.DefaultValue.GetType() != value.GetType())
                    {
                        LogError(inLogger, string.Format("DefaultValue and member {2} of class {3} have difference types: {0} and {1}",
                            member_params.DefaultValue.GetType().Name,
                            value.GetType().Name,
                            member_params.Name,
                            type.Name));
                    }
                    else if (member_params.DefaultValue.Equals(value))
                        continue;
                }
                else if (ReflectionHelper.IsDefault(value, _reflectionProvider, inLogger))
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
        }

        static readonly Type[] SpecCtorTypes = new[] { typeof(IKey), typeof(ILogPrinter) };

        object DeserializeClass(object inInstance, IKey inKey, Type type, int inStructDeep, ILogPrinter inLogger)
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
                    LogError(inLogger, string.Format("Cant take type from RealObjectType {0}. Exception: {1}", type_name, ex.Message));
                }
            }

            object instance = inInstance;
            bool was_inited = false;
            if (instance == null)
            {
                ConstructorInfo ctor_info = type.GetConstructor(SpecCtorTypes);
                if (ctor_info != null)
                {
                    instance = ctor_info.Invoke(new object[] { inKey, inLogger });
                    was_inited = true;
                }
                else
                    instance = _reflectionProvider.Instantiate(type, inLogger);
            }

            if (instance != null && !was_inited)
            {
                //MethodInfo mi = type.GetMethod("DeserializationFromCscd", new Type[] { typeof(CascadeParser.IKey), typeof(CascadeParser.ILogPrinter) });
                MethodInfo mi = type.GetMethod("DeserializationFromCscd", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi != null)
                {
                    if (inKey != null && !inKey.IsEmpty)
                    {
                        IKey key = inKey;
                        mi.Invoke(instance, new object[] { key, inLogger });
                    }
                }
                else
                {
                    MemberInfo[] member_infos = _reflectionProvider.GetSerializableMembers(type);
                    foreach (MemberInfo memberInfo in member_infos)
                    {
                        SCustomMemberParams member_params = GetMemberParams(memberInfo);

                        Type memberType = memberInfo.GetMemberType();

                        IKey sub_key = inKey.GetChild(member_params.ChangedName);
                        if (sub_key == null)
                            sub_key = inKey.GetChild(member_params.Name);

                        if (sub_key != null)
                        {
                            object readValue;
                            if (member_params.Converter != null)
                                readValue = member_params.Converter.ReadKey(sub_key, inLogger);
                            else
                                readValue = DeserializeInternal(null, sub_key, memberType, 0, inStructDeep + 1, inLogger);

                            // This dirty check is naive and doesn't provide performance benefits
                            //if (memberType.IsClass && readValue != currentValue && (readValue == null || !readValue.Equals(currentValue)))
                            _reflectionProvider.SetValue(memberInfo, instance, readValue, inLogger);
                        }
                        else if (member_params.DefaultValue != null)
                            _reflectionProvider.SetValue(memberInfo, instance, member_params.DefaultValue, inLogger);
                        else if (memberType.IsClass || memberType.IsStruct())
                        {
                            object already_exists_member = _reflectionProvider.GetValue(memberInfo, instance);
                            if (already_exists_member != null)
                            {
                                //for set default values inside this object
                                already_exists_member = DeserializeInternal(already_exists_member, IKeyFactory.CreateKey(string.Empty), memberType, 0, inStructDeep + 1, inLogger);
                                if(already_exists_member != null)
                                    _reflectionProvider.SetValue(memberInfo, instance, already_exists_member, inLogger);
                            }
                        }
                    }
                }

                mi = type.GetMethod("OnDeserializedMethod", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi != null)
                {
                    var context = new StreamingContext(StreamingContextStates.Other);
                    mi.Invoke(instance, new object[] { context });
                }
            }

            return instance;
        }
        #endregion Class

        void AddValueToKey(IKey key, object instance)
        {
            if (instance == null)
                key.AddValue(0);

            else if (instance is bool)
                key.AddValue(Convert.ToBoolean(instance));

            else if (instance is float)
                key.AddValue(Convert.ToSingle(instance));
            else if (instance is double)
                key.AddValue(Convert.ToDouble(instance));

            else if (instance is byte)
                key.AddValue(Convert.ToByte(instance));
            else if (instance is short)
                key.AddValue(Convert.ToInt16(instance));
            else if (instance is ushort)
                key.AddValue(Convert.ToUInt16(instance));
            else if (instance is int)
                key.AddValue(Convert.ToInt32(instance));
            else if (instance is uint)
                key.AddValue(Convert.ToUInt32(instance));
            else if (instance is long)
                key.AddValue(Convert.ToInt64(instance));
            else if (instance is ulong)
                key.AddValue(Convert.ToUInt64(instance));
            else
                key.AddValue(instance.ToString());
        }
    }
}
