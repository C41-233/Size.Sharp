using System;
using System.Collections.Generic;
using System.Reflection;

namespace Size.Sharp.Core
{

    internal static class Reflect
    {

        private static readonly Dictionary<Type, long> sizes = new Dictionary<Type, long>();

        static Reflect()
        {
            sizes[typeof(bool)] = sizeof(bool);
            sizes[typeof(char)] = sizeof(char);
            sizes[typeof(byte)] = sizeof(byte);
            sizes[typeof(sbyte)] = sizeof(sbyte);
            sizes[typeof(short)] = sizeof(short);
            sizes[typeof(ushort)] = sizeof(ushort);
            sizes[typeof(int)] = sizeof(int);
            sizes[typeof(uint)] = sizeof(uint);
            sizes[typeof(long)] = sizeof(long);
            sizes[typeof(ulong)] = sizeof(ulong);
            sizes[typeof(float)] = sizeof(float);
            sizes[typeof(double)] = sizeof(double);
            sizes[typeof(decimal)] = sizeof(decimal);
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
            return sizes.TryGetValue(type, out size);
        }

        public static long GetFixSize(Type type)
        {
            TryGetFixSize(type, out var size);
            return size;
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

    }

}
