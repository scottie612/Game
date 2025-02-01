using Game.Common;
using Game.Server.Options;
using Game.Server.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Game.Server
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder.ConfigureEncryption();

            builder.ConfigureServices((hostContext, services) =>
            {
                services.AddLogging(logging =>
                {
                    logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

                services.Configure<ServerOptions>(hostContext.Configuration.GetSection("ServerOptions"));
                services.Configure<EncryptionOptions>(hostContext.Configuration.GetSection("EncryptionOptions"));
                services.ConfigurePlayFab(hostContext.Configuration.GetSection("PlayFabOptions"));
                services.AddSingleton<PacketDispatcher>();
                services.AddSingleton<GameWorld>();
                services.AddHostedService<Engine>();
            });

            var app = builder.Build();

            app.AddSystems();
            app.Run();
        }
    }
}
