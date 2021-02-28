using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Size.Sharp;
using Size.Sharp.Report;
using TypeName;

namespace Test
{
    public class Program
    {

        private static Root root = new Root();

        public static void Main(string[] args)
        {
            root.root = root;
            var visitor = new ReportMemoryVisitor(new FileStream("output.html", FileMode.Create));
            visitor.Add(root);
            while (visitor.MoveNext())
            {
            }
            visitor.Dispose();
        }
    }

    public class Root
    {
        public int i;
        public string s = "123";
        public byte[] ba = {4, 4, 4};
        public List<Type> types = new List<Type>{typeof(string)};
        public Root root;
        public event Action e;
    }

}
