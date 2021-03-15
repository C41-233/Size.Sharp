using System;

namespace Size.Sharp
{
    public partial class MemoryVisitor
    {

        /// <summary>
        /// 把基本类型合并到其父路径，不再单独OnVisitValue
        /// 基本类型包括primitive type、enum、pointer
        /// </summary>
        public bool MergePrimitiveType { get; set; } = true;

        public void Add(object root, string name = "<root>")
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            queue.Enqueue(VisitContext.CreateObject(name, root));
        }

        public void Add(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsGenericTypeDefinition)
            {
                return;
            }

            queue.Enqueue(VisitContext.CreateStatic(type));
        }

        protected virtual void OnVisitObject(string path, Type type, long size, object value)
        {
        }

        protected virtual void OnVisitValue(string path, Type type, long size)
        {
        }

        protected virtual void OnVisitType(string path, Type type, long size)
        {
        }

        protected virtual void OnVisitPath(string path, string oldPath)
        {
        }

    }

}
