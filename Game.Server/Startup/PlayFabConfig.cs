using Game.Common.Encryption;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlayFab;
using System.Threading.Tasks;

namespace Game.Server.Startup
{
    public static class PlayFabConfig
    {
        public static IServiceCollection ConfigurePlayFab(this IServiceCollection services, IConfiguration config)
        {
            PlayFabSettings.staticSettings.TitleId = config.GetValue<string>("TitleID");
            PlayFabSettings.staticSettings.DeveloperSecretKey = config.GetValue<string>("DeveloperSecretKey");

            ///Set Public Key in PlayFab Title Data
            Task.Run(async () =>
            {
                var result = await PlayFabServerAPI.SetTitleDataAsync(new PlayFab.ServerModels.SetTitleDataRequest
                {
                    Key = "PublicKey",
                    Value = EncryptionHelper.GetPublicKey()

                });
            }).Wait();

            return services;
        }
    }
}
