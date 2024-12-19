using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace Game.EntityComponentSystem
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Engine>();

                })
                .Build();
            await host.RunAsync();
        }
    }
}
