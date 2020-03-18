using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    public static class Logger
    {
        private static DateTime startTime = DateTime.UtcNow;

        public static bool Disabled = true;

        private static string GetProcessStat()
        {        
            var startCpuUsage = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;

            var endTime = DateTime.UtcNow;
            var endCpuUsage = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
            var cntOfTh= System.Diagnostics.Process.GetCurrentProcess().Threads.Count;


            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs * 100 / (Environment.ProcessorCount * totalMsPassed);

            return 
                cntOfTh.ToString("#00") + "-" + 
                Threading.Thread.CurrentThread.ManagedThreadId.ToString("#00");
        }

        public static void Log(string message, [CallerMemberName] string memberName = default)
        {
            if (Disabled)
                return;

            var stat = GetProcessStat();

            var str = DateTime.Now.ToString("HH:mm:ss.ffff") + 
                " | " + stat + 
                (memberName != default ? " | " + memberName + " | " : " ") +
                message;
            Console.WriteLine(str);
        }

        private static void LogColored(string str, ConsoleColor fColor, ConsoleColor bColor, [CallerMemberName] string memberName = default)
        {
            if (Disabled)
                return;

            ConsoleColor fColorb = Console.ForegroundColor;
            ConsoleColor bColorb = Console.BackgroundColor;
            
            Console.ForegroundColor = fColor;
            Console.BackgroundColor = bColor;

            Log(str, memberName);

            Console.ForegroundColor = fColorb;
            Console.BackgroundColor = bColorb;
        }

        public static void Alarm(string str, [CallerMemberName] string memberName = default) =>
            LogColored(str, ConsoleColor.White, ConsoleColor.DarkRed, memberName);

        public static void Warn(string str, [CallerMemberName] string memberName = default) =>
            LogColored(str, ConsoleColor.Black, ConsoleColor.Yellow, memberName);

        public static void Info(string str, [CallerMemberName] string memberName = default) =>
            LogColored(str, ConsoleColor.Gray, ConsoleColor.Black, memberName);
    }
}
