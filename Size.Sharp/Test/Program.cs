using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Size.Sharp;
using TypeName;

namespace Test
{
    public class Program
    {

        private static Root root = new Root();

        public static void Main(string[] args)
        {
            root.root = root;
            var visitor = new TypeCollectVisitor();
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in ass.GetTypes())
                {
                    visitor.Add(type);
                }
            }
            while (visitor.MoveNext())
            {
            }
            Console.WriteLine($"count: {visitor.VisitCount}");
            Console.WriteLine($"object count: {visitor.VisitObjectCount}");
            Console.WriteLine($"value count: {visitor.VisitValueCount}");
            Console.WriteLine($"size: {visitor.VisitSize}");

            foreach (var kv in visitor.dic.OrderByDescending(kv => kv.Value))
            {
                Console.WriteLine($"{kv.Key.GetTypeNameString()} | {kv.Value}");
            }
        }
    }

    public class Root
    {
        public int i;
        public string s = "123";
        public byte[] ba = {4, 4, 4};
        public List<Type> types = new List<Type>{typeof(string)};
        public Root root;
    }

}
