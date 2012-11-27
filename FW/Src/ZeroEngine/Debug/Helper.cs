using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

namespace ZeroEngine.Debug
{
    class Helper
    {
        public static void Log(string s)
        {
            System.Diagnostics.Debug.Write(s + "\n");
        }

        public static void Assert(bool condition, string s)
        {
            System.Diagnostics.Debug.Assert(condition, "[ASSERTION]", s);
        }

        public static void Warning(bool condition, string s)
        {
            System.Diagnostics.Trace.Assert(condition, "[WARNING]", s);
        }
    }
}
