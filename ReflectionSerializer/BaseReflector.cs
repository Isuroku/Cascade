using System;
using System.Collections.Generic;
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


//        Derived d = New Derived();
//        Type ty = d.GetType;
//        List<FieldInfo> l = New List<FieldInfo>;
//l.AddRange(ty.GetFields(BindingFlags.Instance | BindingFlags.Public));
//While ty != Nothing
//{
//    l.AddRange(ty.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));
//    ty = ty.BaseType;
//}

//    If you want i.e.value in name fields of Base class:

//   Derived d = New Derived();
//Type ty = d.GetType;
//String s;
//While ty != Nothing
//    {
//    If(ty = typeof(Base))
//    {
//        s = (String)ty.GetField("name", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(d);
//        break;
//    }
//    ty = ty.BaseType;
//}
//Debug.WriteLine(s);

        public virtual MemberInfo[] GetSerializableMembers(Type type)
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

            return lst.ToArray();
        }

        public abstract object Instantiate(Type type);
        public abstract object GetValue(MemberInfo member, object instance);
        public abstract void SetValue(MemberInfo member, object instance, object value);
        public abstract MethodHandler GetDelegate(MethodBase method);
    }
}
