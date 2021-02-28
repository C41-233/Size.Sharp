using System;
using System.IO;

namespace Size.Sharp.Report
{
    public sealed class ReportMemoryVisitor : MemoryVisitor, IDisposable
    {

        private static readonly string[] TemplateLines;

        static ReportMemoryVisitor()
        {
            var input = typeof(ReportMemoryVisitor).Assembly.GetManifestResourceStream(typeof(ReportMemoryVisitor), "template.html");
            string body;
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var reader = new StreamReader(input))
            {
                body = reader.ReadToEnd();
            }

            TemplateLines = body.Split('\n');
            for (var i = 0; i < TemplateLines.Length; i++)
            {
                var trim = TemplateLines[i].Trim();
                if (trim == "//@@")
                {
                    TemplateLines[i] = null;
                }
                else if(trim.StartsWith("@"))
                {
                    
                }
            }
        }

        private readonly StreamWriter writer;
        private int currentLineIndex;

        public ReportMemoryVisitor(Stream stream)
        {
            writer = new StreamWriter(stream);
            WriteNext();
            writer.Write("var G =[");
        }

        protected override void OnVisitObject(string path, Type type, long size, object value)
        {
            writer.Write("{");
            writer.Write($"\"path\": \"{path}\"");
            writer.Write("}");
            //writer.WriteLine($"{path} | {type.GetTypeNameString()} | {size}");
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
            writer.Write("]");
            writer.WriteLine();
            WriteNext();
            writer.Dispose();
        }

        private void WriteNext()
        {
            while (currentLineIndex < TemplateLines.Length)
            {
                var line = TemplateLines[currentLineIndex];
                currentLineIndex++;
                if (line == null)
                {
                    return;
                }
                writer.WriteLine(line);
            }
        }


    }
}
