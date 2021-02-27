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
        protected override void VisitObject(string path, Type type, long size)
        {
            Console.WriteLine($"{path} | {type.GetTypeNameString()} | {size}");
        }

        protected override void VisitInternalValueType(string path, Type type, long size)
        {
            Console.WriteLine($"{path} | {type.GetTypeNameString()} | {size}");
        }
    }
}
