using System.IO;
using System.Text;

namespace Size.Sharp.Report
{
    internal static class Template
    {

        public static readonly string Begin;
        public static readonly string End;

        static Template()
        {
            var body = ReadTemplate("template.html");
            var lines = body.Split('\n');
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                var trim = line.Trim();
                if (trim == "//@@")
                {
                    Begin = sb.ToString();
                    sb.Clear();
                }
                else if (trim.StartsWith("//@"))
                {
                    var name = trim.Substring("//@".Length);
                    sb.AppendLine(ReadTemplate(name));
                }
                else if(trim.StartsWith("/*@") && trim.EndsWith("*/"))
                {
                    var name = trim.Substring("/*@".Length);
                    name = name.Substring(0, name.Length - "*/".Length);
                    sb.AppendLine(ReadTemplate(name));
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            End = sb.ToString();
        }

        private static string ReadTemplate(string name)
        {
            var input = typeof(ReportMemoryVisitor).Assembly.GetManifestResourceStream(typeof(Template), "template." + name);
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var reader = new StreamReader(input))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
