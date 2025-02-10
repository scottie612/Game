using Game.Common.Encryption;
using UnityEngine;

public static class Preloader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute() 
    {
        PlayFab.Internal.PlayFabWebRequest.SkipCertificateValidation();

        var publicKeyFile = Resources.Load<TextAsset>("Encryption/public");
        var rsa = EncryptionHelper.FromXML(publicKeyFile.text);
        
        Globals.ServerPublicKey = EncryptionHelper.GetPublicKey(rsa);
        
    }   
}

