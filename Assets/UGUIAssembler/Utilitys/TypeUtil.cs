using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace UGUIAssembler
{
    /// <summary>
    /// 类型工具
    /// </summary>
    public static class TypeUtil
    {

        internal static Type GetSubTypeDeepth(Type type, string memberName)
        {
            var names = memberName.Split(new char[] { '.' });
            Type memberType = type;

            for (int i = 0; i < names.Length; i++)
            {
                var members = memberType.GetMember(names[i], BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (members == null || members.Length == 0)
                {
                    return null;
                }
                else
                {
                    var member = members[0];

                    if (member is FieldInfo)
                    {
                        var fieldInfo = (member as FieldInfo);
                        memberType = fieldInfo.FieldType;
                    }
                    else if (member is PropertyInfo)
                    {
                        var propertyInfo = (member as PropertyInfo);
                        memberType = propertyInfo.PropertyType;
                    }
                }
            }
            return memberType;
        }

        /// <summary>
        ///  Get Member Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Instance"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        public static object GetMemberValue(object Instance, MemberInfo temp)
        {
            if (temp == null)
            {
                return null;
            }

            if (temp is FieldInfo)
            {
                return (temp as FieldInfo).GetValue(Instance);
            }
            else if (temp is PropertyInfo)
            {
                return (temp as PropertyInfo).GetValue(Instance, null);
            }
            else
            {
                return (temp as MethodInfo).Invoke(Instance, null);
            }
        }

        /// <summary>
        /// Set Member Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Instance"></param>
        /// <param name="temp"></param>
        /// <param name="value"></param>
        public static void SetMemberValue(object Instance, MemberInfo temp, object value)
        {
            if (temp == null)
            {
                return;
            }

            if (temp is FieldInfo)
            {
                (temp as FieldInfo).SetValue(Instance, value);
            }
            else if (temp is PropertyInfo)
            {
                (temp as PropertyInfo).SetValue(Instance, value, null);
            }
            else
            {
                (temp as MethodInfo).Invoke(Instance, new object[] { value });
            }
        }

        /// <summary>
        /// 深度获取对象
        /// </summary>
        /// <param name="Instance"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static MemberInfo GetDeepMember(ref object Instance, string memberName)
        {
            var names = memberName.Split(new char[] { '.' });
            Type type = Instance.GetType();
            MemberInfo member = null;
            for (int i = 0; i < names.Length; i++)
            {
                var members = type.GetMember(names[i], BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (members == null || members.Length == 0)
                {
                    return null;
                }
                else
                {
                    member = members[0];

                    if (member is FieldInfo)
                    {
                        var fieldInfo = (member as FieldInfo);
                        type = fieldInfo.FieldType;

                        if (i < names.Length - 1)
                        {
                            Instance = fieldInfo.GetValue(Instance);
                            if (Instance != null)
                            {
                                type = Instance.GetType();
                            }
                        }
                    }
                    else if (member is PropertyInfo)
                    {
                        var propertyInfo = (member as PropertyInfo);
                        type = propertyInfo.PropertyType;

                        if (i < names.Length - 1)
                        {
                            Instance = propertyInfo.GetValue(Instance, null);
                            if (Instance != null)
                            {
                                type = Instance.GetType();
                            }
                        }
                    }
                }
            }
            return member;
        }

        public static bool IsInnerStructure(Type type)
        {
            if (typeof(IConvertible).IsAssignableFrom(type)) return false;

            if(type.IsValueType)
            {
                if (type.Assembly.FullName.Contains("UnityEngine"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断是否是资源类型
        /// </summary>
        /// <param name="type"></param>
        public static bool IsAssetsType(Type type)
        {
            if (type == typeof(Sprite)) return true;
            if (type == typeof(Texture)) return true;
            if (type == typeof(Texture2D)) return true;
            if (type == typeof(AudioClip)) return true;
            if (type == typeof(Material)) return true;
            if (type == typeof(Shader)) return true;
            return false;
        }
    }
}
