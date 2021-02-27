using System;
using System.Collections.Generic;
using Size.Sharp;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var root = new Root();
            var visitor = new DefaultMemoryVisitor();
            visitor.Add(root);
            while (visitor.MoveNext(1000))
            {
            }
            Console.WriteLine($"count: {visitor.VisitCount}");
            Console.WriteLine($"size: {visitor.VisitSize}");
        }
    }

    public class Root
    {
        public int i;
        public string s = "123";
        public byte[] ba = {4, 4, 4};
        public List<Type> types = new List<Type>{typeof(string)};
    }

}
