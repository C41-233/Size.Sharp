﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Size.Sharp;

namespace Test
{
    class TypeCollectVisitor : MemoryVisitor
    {

        public Dictionary<Type, int > dic = new Dictionary<Type, int>();

        protected override void OnVisitObject(string path, Type type, long size, object value)
        {
            if (dic.TryGetValue(type, out var count))
            {
                dic[type] = count + 1;
            }
            else
            {
                dic[type] = 1;
            }
        }

        protected override void OnVisitInternalValueType(string path, Type type, long size)
        {
            if (dic.TryGetValue(type, out var count))
            {
                dic[type] = count + 1;
            }
            else
            {
                dic[type] = 1;
            }
        }

    }
}
