using Game.Server.Systems;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;

namespace Game.Server.Startup
{
    public static class SystemRegister
    {
        public static IHost AddSystems(this IHost host)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Find all types that inherit from SystemBase
            var systemTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(SystemBase)));

            foreach (var type in systemTypes)
            {
                var handler = ActivatorUtilities.CreateInstance(host.Services, type);
                
            }

            return host;
        }
    }
}
