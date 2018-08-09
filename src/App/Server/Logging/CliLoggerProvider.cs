using System;
using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using OpenApiServer.Cli.Run;

namespace OpenApiServer.Server.Logging
{
    public static class CliLoggerExtensions
    {
        public static ILogger CreateOpenApiLogger(this ILoggerFactory loggerFactory) =>
                loggerFactory.CreateLogger(CliLoggerProvider.OpenApiLoggerName);
    }

    public class CliLoggerProvider : ILoggerProvider
    {
        public static readonly string OpenApiLoggerName = "OpenApi";

        private LogLevel LogLevel { get; }
        private ServerVerbosityLevel VerbosityLevel { get; }
        private ConcurrentDictionary<string, ILogger> Loggers { get; }

        public CliLoggerProvider(ServerVerbosityLevel level)
        {
            VerbosityLevel = level;
            LogLevel = ConvertToLogLevel(level);
            Loggers = new ConcurrentDictionary<string, ILogger>();
        }

        public ILogger CreateLogger(string categoryName) =>
                Loggers.GetOrAdd(categoryName, LoggerFactory);

        private ILogger LoggerFactory(string name)
        {
            if (VerbosityLevel > ServerVerbosityLevel.Normal)
            {
                return new ConsoleLogger(name, LogFilter, includeScopes: false);
            }

            return name == OpenApiLoggerName
                           ? new CliLogger(name, LogFilter)
                           : new CliLogger(name, (_, level) => level >= LogLevel.Error);
        }

        public void Dispose()
        {
        }

        private static LogLevel ConvertToLogLevel(ServerVerbosityLevel level)
        {
            switch (level)
            {
                case ServerVerbosityLevel.Quiet:
                    return LogLevel.None;
                case ServerVerbosityLevel.Minimal:
                    return LogLevel.Warning;
                case ServerVerbosityLevel.Normal:
                    return LogLevel.Information;
                case ServerVerbosityLevel.Detailed:
                    return LogLevel.Information;
                case ServerVerbosityLevel.Diagnostic:
                    return LogLevel.Debug;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool LogFilter(string category, LogLevel level) =>
                level >= LogLevel;
    }
}