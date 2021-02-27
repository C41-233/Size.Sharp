using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Size.Sharp
{

    public abstract partial class MemoryVisitor
    {

        private struct VisitContext
        {
            public readonly string Path;
            public readonly object Value;
            public readonly Type Type;

            public VisitContext(string path, object value)
            {
                Path = path;
                Value = value;
                Type = Value.GetType();
            }

            public VisitContext(string path, Type type)
            {
                Path = path;
                Value = null;
                Type = type;
            }

        }

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
                    Parse(parse.Path, parse.Value, parse.Type);
                }
                return false;
            }
            finally
            {
                watch.Stop();
            }
        }

        private const long CLRSize = 4 + 4 + 4;

        private void ParseStatic(string parent, Type type)
        {
            foreach (var field in Reflect.GetFields(type, BindingFlags.Static))
            {
                var path = parent + '.' + field.Name;
                if (Reflect.IsFixSize(field.FieldType))
                {
                    queue.Enqueue(new VisitContext(path, field.FieldType));
                }
                else
                {
                    var child = field.GetValue(null);
                    if (child != null)
                    {
                        queue.Enqueue(new VisitContext(path, child));
                    }

                    VisitSize += IntPtr.Size;
                }
            }
        }

        private void Parse(string path, object value, Type type)
        {
            if (ReferenceEquals(value, this))
            {
                return;
            }

            if (value == null)
            {
                VisitValueCount++;
                ParseFix(path, type);
                return;
            }

            if (visits.TryGetValue(value, out var oldPath))
            {
                VisitPath(path, oldPath);
                return;
            }

            VisitObjectCount++;
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

        private void ParseFix(string path, Type type)
        {
            var size = Reflect.GetFixSize(type);
            VisitSize += size;
            VisitInternalValueType(path, type, size);
        }

        private void ParseString(string path, string value)
        {
            const long size = CLRSize + sizeof(int);
            VisitSize += size;
            VisitObject(path, typeof(string), size);
            for (var i = 0; i < value.Length; i++)
            {
                queue.Enqueue(new VisitContext(path + '[' + i + ']', typeof(char)));
            }
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
            var rank = type.GetArrayRank();

            if (!fixSize)
            {
                size += count * IntPtr.Size;
            }

            VisitObject(path, type, size);

            var indexes = new int[rank];
            var parseContext = new VisitArrayContext
            {
                ElementSize = elementSize,
                ElementType = elementType,
                FixSize = fixSize,
            };
            ParseArrayRank(ref parseContext, path, array, 0, indexes);
        }

        private void ParseArrayRank(ref VisitArrayContext context, string parent, Array array, int dimension, int[] indexes)
        {
            var lower = array.GetLowerBound(dimension);
            var upper = array.GetUpperBound(dimension);
            for (var i=lower; i<=upper; i++)
            {
                indexes[dimension] = i;
                var path = parent + '[' + i + ']';
                if (dimension == indexes.Length - 1)
                {
                    ParseArrayElement(ref context, path, array, indexes);
                }
                else
                {
                    ParseArrayRank(ref context, path, array, dimension + 1, indexes);
                }
            }
        }

        private void ParseArrayElement(ref VisitArrayContext context, string path, Array array, int[] indexes)
        {
            if (context.FixSize)
            {
                VisitInternalValueType(path, context.ElementType, context.ElementSize);
            }
            else
            {
                var value = array.GetValue(indexes);
                if (value != null)
                {
                    queue.Enqueue(new VisitContext(path, value));
                }
            }
        }

        private void ParseObject(string parent, object root, Type type)
        {
            var size = type.IsValueType ? 0 : CLRSize;
            foreach (var field in Reflect.GetFields(type, BindingFlags.Instance))
            {
                var path = parent + '.' + field.Name;
                if (Reflect.IsFixSize(field.FieldType))
                {
                    queue.Enqueue(new VisitContext(path, field.FieldType));
                }
                else
                {
                    var child = field.GetValue(root);
                    if (child != null)
                    {
                        queue.Enqueue(new VisitContext(path, child));
                    }

                    size += IntPtr.Size;
                }
            }

            VisitSize += size;
            VisitObject(parent, type, size);
        }

    }

}
