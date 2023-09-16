using System;
using System.CommandLine.Invocation;
using System.IO;

namespace HaloInfiniteResearchTools.Cli
{
    public static class CommandLineExtensions
    {
        // Kotlin-style Also method
        public static T Also<T>(this T self, Action<T> block)
        {
            block(self);
            return self;
        }

        public static void ExceptionHandler(Exception ex, InvocationContext ctx)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            switch (ex)
            {
                case IOException:
                case UnauthorizedAccessException:
                    Console.Error.WriteLine(ex.Message);
                    break;
                default:
                    Console.Error.WriteLine("Unhandled exception: {0}", ex.Message);
                    Console.Error.WriteLine(ex.StackTrace);
                    break;
            }
            Console.ResetColor();
            ctx.ExitCode = 1;
        }
    }
}
