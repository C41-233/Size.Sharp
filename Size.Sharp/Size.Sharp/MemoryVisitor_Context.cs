using System;
using TypeName;

namespace Size.Sharp
{
    public partial class MemoryVisitor
    {

        private enum VisitType
        {
            Object,
            Fix,
            Static,
        }

        private struct VisitContext
        {
            public string Path;
            public object Value;
            public Type Type;
            public VisitType VisitType;

            public static VisitContext CreateObject(string path, object value)
            {
                return new VisitContext
                {
                    Path = string.Intern(path),
                    Value = value,
                    Type = value.GetType(),
                    VisitType = VisitType.Object,
                };
            }

            public static VisitContext CreateFix(string path, Type type)
            {
                return new VisitContext
                {
                    Path = string.Intern(path),
                    Type = type,
                    VisitType = VisitType.Fix,
                };
            }

            public static VisitContext CreateStatic(Type type)
            {
                return new VisitContext
                {
                    Path = string.Intern(type.GetTypeNameString()),
                    Type = type,
                    VisitType = VisitType.Static,
                };
            }

        }

    }
}
