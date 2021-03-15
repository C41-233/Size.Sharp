using System;
using System.Collections.Generic;
using System.Reflection;

namespace Size.Sharp.Core
{

    internal static class Reflect
    {

        private static readonly Dictionary<Type, long> fixSizes = new Dictionary<Type, long>();

        static Reflect()
        {
            fixSizes[typeof(bool)] = sizeof(bool);
            fixSizes[typeof(char)] = sizeof(char);
            fixSizes[typeof(byte)] = sizeof(byte);
            fixSizes[typeof(sbyte)] = sizeof(sbyte);
            fixSizes[typeof(short)] = sizeof(short);
            fixSizes[typeof(ushort)] = sizeof(ushort);
            fixSizes[typeof(int)] = sizeof(int);
            fixSizes[typeof(uint)] = sizeof(uint);
            fixSizes[typeof(long)] = sizeof(long);
            fixSizes[typeof(ulong)] = sizeof(ulong);
            fixSizes[typeof(float)] = sizeof(float);
            fixSizes[typeof(double)] = sizeof(double);
            fixSizes[typeof(decimal)] = sizeof(decimal);
        }

        public static bool TryGetFixSize(Type type, out long size)
        {
            if (type.IsPointer || type.IsByRef)
            {
                size = IntPtr.Size;
                return true;
            }
            if (type.IsEnum)
            {
                return TryGetFixSize(type.GetEnumUnderlyingType(), out size);
            }
            return fixSizes.TryGetValue(type, out size);
        }

        public static long GetFixSize(Type type)
        {
            TryGetFixSize(type, out var size);
            return size;
        }

        public static bool IsFixSize(Type type)
        {
            return TryGetFixSize(type, out _);
        }

        public static IEnumerable<FieldInfo> GetFields(Type type, BindingFlags flag)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | flag;
            while (type != null)
            {
                foreach (var fieldInfo in type.GetFields(flags))
                {
                    yield return fieldInfo;
                }

                type = type.BaseType;
            }
        }

        private static readonly Dictionary<Type, bool> cascadeTypes = new Dictionary<Type, bool>();

        public static bool IsCascadeValueType(Type type)
        {
            if (!type.IsValueType)
            {
                return false;
            }

            if (cascadeTypes.TryGetValue(type, out var value))
            {
                return value;
            }

            value = CreateCascade(type);
            cascadeTypes.Add(type, value);
            return value;
        }

        private static bool CreateCascade(Type type)
        {
            foreach (var field in GetFields(type, BindingFlags.Instance))
            {
                if (!IsFixSize(field.FieldType))
                {
                    return false;
                }
            }

            return true;
        }

    }

}
