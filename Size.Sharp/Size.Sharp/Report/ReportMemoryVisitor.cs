﻿using System;
using System.IO;
using TypeName;

namespace Size.Sharp.Report
{
    public sealed class ReportMemoryVisitor : MemoryVisitor, IDisposable
    {

        private readonly StreamWriter writer;
        private readonly JsonRoot root;

        public ReportMemoryVisitor(Stream stream)
        {
            writer = new StreamWriter(stream);
            root = new JsonRoot(writer);

            writer.Write(Template.Begin);
            writer.Write("var G =");
            root.Begin();
        }

        protected override void OnVisitObject(string path, Type type, long size, object value)
        {
            root.Element();
            var jobj = new JsonObject(writer);
            jobj.BeginObject();
            jobj.Field("path", path);
            jobj.Field("type", type.GetTypeNameString());
            jobj.Field("size", size);
            jobj.EndObject();
        }

        protected override void OnVisitInternalValueType(string path, Type type, long size)
        {
            //writer.WriteLine($"{path} | {type.GetTypeNameString()} | {size}");
        }

        protected override void OnVisitPath(string path, string oldPath)
        {
            //writer.WriteLine($"{path} -> {oldPath}");
        }

        public void Dispose()
        {
            root.End();
            writer.WriteLine();
            writer.Write(Template.End);
            writer.Dispose();
        }

    }
}
