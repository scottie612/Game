using Game.Common.Encryption;
using Game.Server.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;

namespace Game.Server.Startup
{
    public static class EncryptionConfig
    {
        public static IHostBuilder ConfigureEncryption(this IHostBuilder builder)
        {
            var keyFile = "private.txt";
            

            if (!File.Exists(keyFile)) throw new FileNotFoundException($"Private Key file not found: {keyFile}");

            var fileText = File.ReadAllText(keyFile);

            var keyPair = EncryptionHelper.FromXML(fileText);

            var dict = new Dictionary<string, string?>
            {
                ["EncryptionOptions:PrivateKey"] = EncryptionHelper.GetPrivateKey(keyPair),
                ["EncryptionOptions:PublicKey"] = EncryptionHelper.GetPublicKey(keyPair)
            };

            builder.ConfigureAppConfiguration((hostContext, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(dict);
            });
         
            return builder;
        }
    }
}
