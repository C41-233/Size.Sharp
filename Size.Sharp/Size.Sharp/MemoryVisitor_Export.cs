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
            queue.Enqueue(new VisitContext(name, root));
        }

        protected virtual void VisitObject(string path, Type type, long size)
        {
        }

        protected virtual void VisitInternalValueType(string path, Type type, long size)
        {
        }

    }

}
