using System.Security.Cryptography;

public static class Globals 
{
    public static string PlayFabUsername { get; set; }

    public static string PlayFabUserID { get; set; } 
    
    public static string SessionTicket { get; set; }

    public static string ServerPublicKey { get; set; }

    public static RSA RSAKeypair { get; set; }
}
