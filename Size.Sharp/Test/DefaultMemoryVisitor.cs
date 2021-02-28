using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Size.Sharp;
using TypeName;

namespace Test
{
    class DefaultMemoryVisitor : MemoryVisitor
    {
        protected override void OnVisitObject(string path, Type type, long size, object value)
        {
            Console.WriteLine($"{path} | {type.GetTypeNameString()} | {size}");
        }

        protected override void OnVisitInternalValueType(string path, Type type, long size)
        {
            Console.WriteLine($"{path} | {type.GetTypeNameString()} | {size}");
        }

        protected override void OnVisitPath(string path, string oldPath)
        {
            Console.WriteLine($"{path} -> {oldPath}");
        }
    }
}
