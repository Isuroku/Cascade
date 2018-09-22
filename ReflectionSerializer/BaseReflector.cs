using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ReflectionSerializer
{
    public abstract class BaseReflector : IReflectionProvider
    {
        public virtual T GetSingleAttributeOrDefault<T>(MemberInfo memberInfo) where T : Attribute, new()
        {
            object[] attributes = memberInfo.GetCustomAttributes(typeof(T), false);
            return attributes.Length == 0 ? new T() : attributes[0] as T;
        }

        static readonly BindingFlags CollectMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        //public virtual IEnumerable<MemberInfo> GetSerializableMembers(Type type)
        //{
        //    return type.GetProperties(CollectMembers)
        //        .Where(p => p.GetGetMethod() != null && p.GetSetMethod() != null && p.GetGetMethod().GetParameters().Length == 0)
        //        .Cast<MemberInfo>()
        //        .Union(type.GetFields(CollectMembers)
        //        //.Where(f => f.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false) == null)
        //        .Cast<MemberInfo>());
        //}

        public virtual IEnumerable<MemberInfo> GetSerializableMembers(Type type)
        {
            List<MemberInfo> lst = new List<MemberInfo>();

            PropertyInfo[] properties = type.GetProperties(CollectMembers);
            for(int i = 0; i < properties.Length; i++)
            {
                PropertyInfo p = properties[i];
                if (p.GetGetMethod() != null && p.GetSetMethod() != null && p.GetGetMethod().GetParameters().Length == 0)
                    lst.Add(p);
            }

            FieldInfo[] fields = type.GetFields(CollectMembers);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo f = fields[i];
                if (!f.IsDefined(typeof(CompilerGeneratedAttribute), false))
                    lst.Add(f);
            }

            return lst;
        }

        public abstract object Instantiate(Type type);
        public abstract object GetValue(MemberInfo member, object instance);
        public abstract void SetValue(MemberInfo member, object instance, object value);
        public abstract MethodHandler GetDelegate(MethodBase method);
    }
}
