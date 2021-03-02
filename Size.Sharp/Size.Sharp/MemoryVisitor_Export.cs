using System;

namespace Size.Sharp
{
    public partial class MemoryVisitor
    {

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

        protected virtual void OnVisitInternalValueType(string path, Type type, long size)
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
