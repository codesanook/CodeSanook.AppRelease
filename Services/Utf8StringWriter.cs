using System.IO;
using System.Text;

namespace CodeSanook.AppRelease.Services
{
    public class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter(StringBuilder stringBuilder) : base(stringBuilder)
        {
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
