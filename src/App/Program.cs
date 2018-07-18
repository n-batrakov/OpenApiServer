using System.IO;

using ITExpert.OpenApi.Server.Configuration;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITExpert.OpenApi.Server
{
    public class Program
    {
        public static void Main()
        {
            CreateHostBuilder().UseStartup<Startup>().Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder()
        {
            var builder = new WebHostBuilder();

            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder.UseKestrel(x => x.Configure());
            builder.ConfigureAppConfiguration(x => x.AddEnvironmentVariables());
            builder.ConfigureLogging(x => x.AddConsole());

            builder.UseDefaultServiceProvider(
                    (ctx, options) =>
                    {
                        options.ValidateScopes = ctx.HostingEnvironment.IsDevelopment();
                    });

            return builder;
        }
    }
}
