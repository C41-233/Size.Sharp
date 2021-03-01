using System.IO;

namespace Size.Sharp.Report
{
    internal class JsonRoot
    {

        private readonly StreamWriter writer;
        private bool split;

        public JsonRoot(StreamWriter writer)
        {
            this.writer = writer;
            split = false;
        }

        public void Begin()
        {
            writer.Write('[');
        }

        public void End()
        {
            writer.Write(']');
        }

        public void Element()
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
