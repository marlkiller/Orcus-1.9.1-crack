#if LOGCONSOLE
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Orcus
{
    public class ConsolePrefixWriter : TextWriter
    {
        private readonly TextWriter _originalOut;
        private readonly Stopwatch _stopwatch;

        public ConsolePrefixWriter()
        {
            _originalOut = Console.Out;
            _stopwatch = Stopwatch.StartNew();
        }

        public override Encoding Encoding => _originalOut.Encoding;

        public override void WriteLine(string message)
        {
            _originalOut.WriteLine("[{0:G}] {1}", _stopwatch.Elapsed, message);
        }

        public override void Write(string message)
        {
            _originalOut.Write("[{0:G}] {1}", _stopwatch.Elapsed, message);
        }
    }
}
#endif