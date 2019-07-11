using System.IO;
using System.Text;

namespace Codesanook.AppRelease.Services {
    public class Utf8StringWriter : StringWriter {
        public Utf8StringWriter(StringBuilder stringBuilder) : base(stringBuilder) {
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}
