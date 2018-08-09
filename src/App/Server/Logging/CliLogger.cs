using System;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace OpenApiServer.Server.Logging
{
    public class CliLogger : ConsoleLogger
    {
        public CliLogger()
                : this("Cli", (_, __) => true)
        {
        }

        public CliLogger(string name, Func<string, LogLevel, bool> filter)
                : base(name, filter, false)
        {
        }

        public override void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    WriteColoredLine(message, ConsoleColor.Gray);
                    break;
                case LogLevel.Debug:
                    WriteColoredLine(message, ConsoleColor.Gray);
                    break;
                case LogLevel.Information:
                    WriteColoredLine(message, System.Console.ForegroundColor);
                    break;
                case LogLevel.Warning:
                    WriteLabeledLine("Warning", message, ConsoleColor.Yellow);
                    break;
                case LogLevel.Error:
                    message = exception == null ? message : $"{message}\n{FormatException(exception)}";
                    WriteLabeledLine("Error", message, ConsoleColor.Red);
                    break;
                case LogLevel.Critical:
                    message = exception == null ? message : $"{message}\n{FormatException(exception)}";
                    WriteLabeledLine("Critical", message, ConsoleColor.Red, accentColor:true);
                    break;
                case LogLevel.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }

        private static string FormatException(Exception e) => e.Message;

        private void WriteColoredLine(string text, ConsoleColor textColor)
        {
            Console.WriteLine(text, null, textColor);
            Console.Flush();
        }

        private void WriteLabeledLine(string label,
                                             string text,
                                             ConsoleColor labelColor,
                                             bool accentColor = false)
        {
            if (accentColor)
            {
                Console.Write(label, labelColor, ConsoleColor.Black);
            }
            else
            {
                Console.Write(label, null, labelColor);
            }

            Console.Write($": {text}\n", null, null);
            Console.Flush();
        }
    }
}