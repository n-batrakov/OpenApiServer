using ITExpert.OpenApiServer.Configuration;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ITExpert.OpenApiServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
