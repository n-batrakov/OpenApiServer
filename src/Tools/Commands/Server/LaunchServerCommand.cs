using System.Collections.Generic;

using ITExpert.OpenApi.Server.Configuration;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Logging;

namespace ITExpert.OpenApi.Tools.Commands.Server
{
    public class LaunchServerCommand
    {
        private int Port { get; }
        private string SpecsDirectory { get; }
        private LogLevel LogLevel { get; }

        public LaunchServerCommand(LaunchServerCommandOptions options)
        {
            Port = options.Port == default ? 5000 : options.Port;
            SpecsDirectory = options.SpecsDirectory ?? ".";
            LogLevel = options.Verbose ? LogLevel.Information : LogLevel.Error;
        }

        public int Execute()
        {
            WebHost.CreateDefaultBuilder()
                   .UseStartup<Startup>()
                   .UseConfiguration(GetServerConfiguration(SpecsDirectory))
                   .ConfigureLogging(x => x.SetMinimumLevel(LogLevel))
                   .UseUrls($"http://*:{Port}")
                   .Build()
                   .Run();
            return 0;
        }

        private static IConfiguration GetServerConfiguration(string specsDirectory)
        {
            var data = new Dictionary<string, string>
                       {
                               ["specs"] = specsDirectory
                       };
            var source = new MemoryConfigurationSource { InitialData = data };
            return new ConfigurationBuilder().Add(source).Build();
        }
    }
}