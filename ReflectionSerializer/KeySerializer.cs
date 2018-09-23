using System;
using System.Collections;
using System.Reflection;

namespace ReflectionSerializer
{
    public interface IKey
    {
        IKey CreateChildKey(string name);
        IKey CreateArrayKey(int index);
        void AddValue(long v);
        void AddValue(ulong v);
        void AddValue(decimal v);
        void AddValue(bool v);
        void AddValue(string v);
    }

    public class CKeySerializer
    {
        readonly IReflectionProvider _reflectionProvider;

        public CKeySerializer(IReflectionProvider reflectionProvider)
        {
            _reflectionProvider = reflectionProvider;
        }

        public void Serialize(object instance, IKey inKey)
        {
            Serialize(instance.GetType().Name, 0, instance, typeof(object), inKey);
        }

        void Serialize(string name, int index, object instance, Type declaredType, IKey inKey)
        {
            IKey child;
            if(!string.IsNullOrEmpty(name))
                child = inKey.CreateChildKey(name);
            else
                child = inKey.CreateArrayKey(index);

            Type type = instance == null ? declaredType : instance.GetType();

            if (instance == null || type.IsAtomic())
            {
                AddValueToKey(child, instance);
            }
            else if (type.IsGenericDictionary())
            {
                var dictionary = instance as IDictionary;
                Type[] genericArguments = dictionary.GetType().GetGenericArguments();
                Type keyDeclaredType = genericArguments[0];
                Type valueDeclaredType = genericArguments[1];

                if(!keyDeclaredType.IsAtomic())
                {
                    throw new Exception("Dictionary must simple key.");
                }

                foreach (var key in dictionary.Keys)
                    Serialize(key.ToString(), 0, dictionary[key], valueDeclaredType, child);
            }
            else if (type.IsGenericCollection())
            {
                var collection = instance as IEnumerable;
                Type declaredItemType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];

                if (declaredItemType.IsAtomic())
                {
                    foreach (var item in collection)
                        AddValueToKey(child, item);
                }
                else
                {
                    int i = 0;
                    foreach (var item in collection)
                    {
                        Serialize(null, i, item, declaredItemType, child);
                        i++;
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
                    if (!memberAttr.Required && (value == null || ReflectionHelper.IsDefault(value, _reflectionProvider)))
                        continue;

                    // If no property name is defined, use the short type name
                    string memberName = memberAttr.Name ?? memberInfo.Name;
                    Serialize(memberName, 0, value, memberType, child);
                }
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
    }
}
