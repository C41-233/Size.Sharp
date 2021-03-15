using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Size.Sharp.Core;

namespace Size.Sharp
{

    public abstract partial class MemoryVisitor
    {

        public int VisitCount => VisitObjectCount + VisitValueCount;
        public int VisitObjectCount { get; private set; }
        public int VisitValueCount { get; private set; }
        public long VisitSize { get; private set; }


        private readonly Queue<VisitContext> queue = new Queue<VisitContext>();
        private readonly Stopwatch watch = new Stopwatch();
        private readonly Dictionary<object, string> visits = new Dictionary<object, string>(ReferenceComparer.Instance);

        public bool MoveNext(long timeout = 0)
        {
            watch.Restart();
            try
            {
                while (queue.Count > 0)
                {
                    if (timeout > 0 && watch.ElapsedMilliseconds >= timeout)
                    {
                        return true;
                    }
                    var parse = queue.Dequeue();
                    Parse(parse.Path, parse.Value, parse.Type, parse.VisitType);
                }
                return false;
            }
            finally
            {
                watch.Stop();
            }
        }

        private const long CLRSize = 4 + 4 + 4;

        private void Parse(string path, object value, Type type, VisitType visitType)
        {
            if (visitType == VisitType.Static)
            {
                ParseStatic(path, type);
                return;
            }

            if (visitType == VisitType.Fix)
            {
                ParseFix(path, type);
                return;
            }

            if (visitType == VisitType.Cascade)
            {
                ParseCascade(path, type);
                return;
            }

            // visitType == VisitType.Object

            if (ReferenceEquals(value, this))
            {
                return;
            }

            if (visits.TryGetValue(value, out var oldPath))
            {
                OnVisitPath(path, oldPath);
                return;
            }

            if (type.IsValueType)
            {
                VisitValueCount++;
            }
            else
            {
                VisitObjectCount++;
            }
            visits.Add(value, path);

            if (value is string str)
            {
                ParseString(path, str);
                return;
            }

            if (value is Array array)
            {
                ParseArray(path, array, type);
                return;
            }

            ParseObject(path, value, type);
        }

        private void ParseStatic(string parent, Type type)
        {
            long size = 0;
            foreach (var field in Reflect.GetFields(type, BindingFlags.Static))
            {
                var path = parent + '.' + field.Name;
                if (Reflect.TryGetFixSize(field.FieldType, out var fixSize))
                {
                    if (MergePrimitiveType)
                    {
                        size += fixSize;
                        VisitValueCount++;
                    }
                    else
                    {
                        queue.Enqueue(VisitContext.CreateFix(path, field.FieldType));
                    }
                }
                else
                {
                    var child = field.GetValue(null);
                    if (child != null)
                    {
                        queue.Enqueue(VisitContext.CreateObject(path, child));
                    }

                    size += IntPtr.Size;
                }
            }

            VisitSize += size;
            OnVisitType(parent, type, size);
        }

        private void ParseFix(string path, Type type)
        {
            var size = Reflect.GetFixSize(type);
            VisitSize += size;
            VisitValueCount++;
            OnVisitValue(path, type, size);
        }

        private void ParseCascade(string parent, Type type)
        {
            long size = 0;
            foreach (var field in Reflect.GetFields(type, BindingFlags.Instance))
            {
                if (MergePrimitiveType)
                {
                    size += Reflect.GetFixSize(field.FieldType);
                    VisitValueCount++;
                }
                else
                {
                    var path = parent + '.' + field.Name;
                    queue.Enqueue(VisitContext.CreateFix(path, field.FieldType));
                }
            }

            VisitSize += size;
            VisitValueCount++;
            OnVisitValue(parent, type, size);
        }

        private void ParseString(string path, string value)
        {
            long size = CLRSize + sizeof(int);
            if (MergePrimitiveType)
            {
                size += sizeof(char) * value.Length;
                VisitValueCount += value.Length;
            }
            else
            {
                for (var i = 0; i < value.Length; i++)
                {
                    queue.Enqueue(VisitContext.CreateFix(path + '.' + i, typeof(char)));
                }
            }
            VisitSize += size;
            OnVisitObject(path, typeof(string), size, value);
        }

        private struct VisitArrayContext
        {
            public bool FixSize;
            public Type ElementType;
            public long ElementSize;
        }

        private void ParseArray(string path, Array array, Type type)
        {
            var size = CLRSize + sizeof(long);

            var elementType = type.GetElementType();
            var count = array.Length;
            var fixSize = Reflect.TryGetFixSize(elementType, out var elementSize);

            var merge = false;
            if (fixSize)
            {
                size += elementSize * count;
                merge = MergePrimitiveType;
            }
            else
            {
                size += IntPtr.Size * count;
            }

            if (merge)
            {
                VisitValueCount += count;
                VisitSize += size;
            }

            OnVisitObject(path, type, size, array);

            if (merge)
            {
                return;
            }

            var rank = type.GetArrayRank();
            var indexes = new int[rank];
            var parseContext = new VisitArrayContext
            {
                ElementType = elementType,
                FixSize = fixSize,
                ElementSize = elementSize,
            };
            ParseArrayRank(ref parseContext, path, array, 0, indexes);
        }

        private void ParseArrayRank(ref VisitArrayContext context, string parent, Array array, int dimension, int[] indexes)
        {
            var lower = array.GetLowerBound(dimension);
            var upper = array.GetUpperBound(dimension);

            if (dimension == indexes.Length - 1)
            {
                if (context.FixSize)
                {
                    for (var i = lower; i <= upper; i++)
                    {
                        var path = parent + '.' + i;
                        OnVisitValue(path, context.ElementType, context.ElementSize);
                    }
                }
                else
                {
                    for (var i = lower; i <= upper; i++)
                    {
                        var path = parent + '.' + i;
                        indexes[dimension] = i;
                        var value = array.GetValue(indexes);
                        if (value != null)
                        {
                            queue.Enqueue(VisitContext.CreateObject(path, value));
                        }
                    }
                }
            }
            else
            {
                for (var i = lower; i <= upper; i++)
                {
                    var path = parent + '.' + i;
                    indexes[dimension] = i;
                    ParseArrayRank(ref context, path, array, dimension + 1, indexes);
                }
            }
        }

        private void ParseObject(string parent, object root, Type type)
        {
            var size = type.IsValueType ? 0 : CLRSize;
            foreach (var field in Reflect.GetFields(type, BindingFlags.Instance))
            {
                var path = parent + '.' + field.Name;
                if (Reflect.TryGetFixSize(field.FieldType, out var fixSize))
                {
                    if (MergePrimitiveType)
                    {
                        size += fixSize;
                        VisitValueCount++;
                    }
                    else
                    {
                        queue.Enqueue(VisitContext.CreateFix(path, field.FieldType));
                    }
                }
                else if (Reflect.IsCascadeValueType(field.FieldType))
                {
                    queue.Enqueue(VisitContext.CreateCascade(path, field.FieldType));
                }
                else
                {
                    var child = field.GetValue(root);
                    if (child != null)
                    {
                        queue.Enqueue(VisitContext.CreateObject(path, child));
                    }

                    size += IntPtr.Size;
                }
            }

            VisitSize += size;
            if (type.IsValueType)
            {
                OnVisitValue(parent, type, size);
            }
            else
            {
                OnVisitObject(parent, type, size, root);
            }
        }

    }

}
