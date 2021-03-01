using System.IO;

namespace Size.Sharp.Report
{
    internal struct JsonObject
    {

        private readonly StreamWriter writer;
        private bool split;

        public JsonObject(StreamWriter writer)
        {
            this.writer = writer;
            split = false;
        }

        public void BeginObject()
        {
            writer.Write('{');
        }

        public void EndObject()
        {
            writer.Write('}');
        }

        public void Field(string name, string value)
        {
            Split();
            WriteToken(name);
            writer.Write('"');
            writer.Write(value);
            writer.Write('"');
        }

        public void Field(string name, long value)
        {
            Split();
            WriteToken(name);
            writer.Write(value);
        }

        private void WriteToken(string name)
        {
            writer.Write('"');
            writer.Write(name);
            writer.Write('"');
            writer.Write(':');
        }

        private void Split()
        {
            if (split)
            {
                writer.Write(',');
            }
            else
            {
                split = true;
            }
        }
    }
}
