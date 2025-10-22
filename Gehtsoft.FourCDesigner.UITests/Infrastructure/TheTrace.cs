using System;
using System.Security.Cryptography.X509Certificates;

namespace Gehtsoft.FourCDesigner.UITests.Infrastructure
{
    public static class TheTrace
    {
        public static bool Enable { get; set; } = false;
        public static bool Timing { get; set; } = false;

        private static DateTime? mTime = null;

        public static void Trace(string text)
        {
            if (!Enable)
                return;
            if (Timing)
            {
                if (mTime != null)
                    Console.Write("[{0}]", (DateTime.Now - mTime.Value).TotalMilliseconds);
                mTime = DateTime.Now;
            }
            Console.Write("{0}", text);
        }
        
        public static void Trace(string format, params object[] args)
        {
            Trace(string.Format(format, args));
        }
    }
}