using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlayFab;

namespace Game.Server.Startup
{
    public static class PlayFabConfig
    {
        public static IServiceCollection ConfigurePlayFab(this IServiceCollection services, IConfiguration config)
        {
            PlayFabSettings.staticSettings.TitleId = config.GetValue<string>("TitleID");
            PlayFabSettings.staticSettings.DeveloperSecretKey = config.GetValue<string>("DeveloperSecretKey");

            return services;
        }
    }
}
