using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectionSerializer
{
    public class CachedReflector : BaseReflector
    {
        readonly Dictionary<PointerPair, Attribute> attributeCache = new Dictionary<PointerPair, Attribute>();
        readonly Dictionary<IntPtr, MemberInfo[]> memberCache = new Dictionary<IntPtr, MemberInfo[]>();
        readonly Dictionary<IntPtr, Func<object>> constructorCache = new Dictionary<IntPtr, Func<object>>();
        readonly Dictionary<IntPtr, Func<object, object>> getterCache = new Dictionary<IntPtr, Func<object, object>>();
        readonly Dictionary<IntPtr, Action<object, object>> setterCache = new Dictionary<IntPtr, Action<object, object>>();
        readonly Dictionary<IntPtr, MethodHandler> methodCache = new Dictionary<IntPtr, MethodHandler>();

        T GetSingleAttributeOrDefault<T>(PropertyInfo propertyInfo) where T : Attribute, new()
        {
            Type attributeType = typeof(T);
            Attribute attribute;
            var key = new PointerPair(propertyInfo.GetGetMethod().MethodHandle.Value, attributeType.TypeHandle.Value);
            if (!attributeCache.TryGetValue(key, out attribute))
                attributeCache.Add(key, attribute = base.GetSingleAttributeOrDefault<T>(propertyInfo));
            return attribute as T;
        }

        T GetSingleAttributeOrDefault<T>(FieldInfo fieldInfo) where T : Attribute, new()
        {
            Type attributeType = typeof(T);
            Attribute attribute;
            var key = new PointerPair(fieldInfo.FieldHandle.Value, attributeType.TypeHandle.Value);
            if (!attributeCache.TryGetValue(key, out attribute))
                attributeCache.Add(key, attribute = base.GetSingleAttributeOrDefault<T>(fieldInfo));
            return attribute as T;
        }

        public override T GetSingleAttributeOrDefault<T>(MemberInfo memberInfo)
        {
            return memberInfo is PropertyInfo ?
                GetSingleAttributeOrDefault<T>(memberInfo as PropertyInfo) :
                GetSingleAttributeOrDefault<T>(memberInfo as FieldInfo);
        }

        public override IEnumerable<MemberInfo> GetSerializableMembers(Type type)
        {
            MemberInfo[] properties;
            if (!memberCache.TryGetValue(type.TypeHandle.Value, out properties))
                memberCache.Add(type.TypeHandle.Value, properties = base.GetSerializableMembers(type).ToArray());
            return properties;
        }

        public override object Instantiate(Type type)
        {
            Func<object> constructor;
            if (!constructorCache.TryGetValue(type.TypeHandle.Value, out constructor))
                constructorCache.Add(type.TypeHandle.Value, constructor = EmitHelper.CreateParameterlessConstructorHandler(type));
            return constructor();
        }

        public override object GetValue(MemberInfo memberInfo, object instance)
        {
            Func<object, object> getter;
            if (memberInfo is PropertyInfo)
            {
                var propertyInfo = memberInfo as PropertyInfo;
                var key = propertyInfo.GetGetMethod().MethodHandle.Value;
                if (!getterCache.TryGetValue(key, out getter))
                    getterCache.Add(key, getter = EmitHelper.CreatePropertyGetterHandler(propertyInfo));
            }
            else if (memberInfo is FieldInfo)
            {
                var fieldInfo = memberInfo as FieldInfo;
                if (!getterCache.TryGetValue(fieldInfo.FieldHandle.Value, out getter))
                    getterCache.Add(fieldInfo.FieldHandle.Value, getter = EmitHelper.CreateFieldGetterHandler(fieldInfo));
            }
            else
                throw new NotImplementedException();

            return getter(instance);
        }

        public override void SetValue(MemberInfo memberInfo, object instance, object value)
        {
            Action<object, object> setter;
            if (memberInfo is PropertyInfo)
            {
                var propertyInfo = memberInfo as PropertyInfo;
                var key = propertyInfo.GetSetMethod().MethodHandle.Value;
                if (!setterCache.TryGetValue(key, out setter))
                    setterCache.Add(key, setter = EmitHelper.CreatePropertySetterHandler(propertyInfo));
            }
            else if (memberInfo is FieldInfo)
            {
                var fieldInfo = memberInfo as FieldInfo;
                if (!setterCache.TryGetValue(fieldInfo.FieldHandle.Value, out setter))
                    setterCache.Add(fieldInfo.FieldHandle.Value, setter = EmitHelper.CreateFieldSetterHandler(fieldInfo));
            }
            else
                throw new NotImplementedException();

            setter(instance, value);
        }

        public override MethodHandler GetDelegate(MethodBase method)
        {
            MethodHandler handler;
            var key = method.MethodHandle.Value;
            if (!methodCache.TryGetValue(key, out handler))
                methodCache.Add(key, handler = EmitHelper.CreateMethodHandler(method));
            return handler;
        }
    }
}
