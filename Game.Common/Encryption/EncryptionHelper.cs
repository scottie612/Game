using System;
using System.Security.Cryptography;
using System.Text;

namespace Game.Common.Encryption
{
    public static class EncryptionHelper
    {
        #region RSA
        public static RSA FromXML(string xmlKey)
        {
            var rsa = RSA.Create();
            rsa.FromXmlString(xmlKey);
            return rsa;
        }

        public static RSA GenerateKeyPair()
        {
            return RSA.Create();
        }

        public static string GetPublicKey(RSA rsa)
        {
            return rsa.ToXmlString(false);
        }

        public static string GetPrivateKey(RSA rsa)
        {
            return rsa.ToXmlString(true);
        }

        public static string Encrypt(string publicKey, string data)
        {
            using (var rsa = RSA.Create())
            {
                rsa.FromXmlString(publicKey);
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var encryptedData = rsa.Encrypt(dataBytes, RSAEncryptionPadding.Pkcs1);

                return Convert.ToBase64String(encryptedData);
            }
        }

        public static string Decrypt(string privateKey, string data)
        {
            using (var rsa = RSA.Create())
            {
                rsa.FromXmlString(privateKey);
                var dataBytes = Convert.FromBase64String(data);
                var decryptedData = rsa.Decrypt(dataBytes, RSAEncryptionPadding.Pkcs1);

                return Encoding.UTF8.GetString(decryptedData);
            }
        }

        public static string Sign(string privateKey, string data)
        {
            using (var rsa = RSA.Create())
            {
                rsa.FromXmlString(privateKey);
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                return Convert.ToBase64String(signature);
            }
        }

        public static bool ValidateSignature(string publicKey, string data, string signature)
        {
            using (var rsa = RSA.Create())
            {
                rsa.FromXmlString(publicKey);
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var signatureBytes = Convert.FromBase64String(signature);
                var isValid = rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                return isValid;
            }
        }

        #endregion
        #region AES

        public static string GenerateAesKey(int keySize = 256)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = keySize;
                aes.GenerateKey();
                return Convert.ToBase64String(aes.Key);
            }
        }

        public static string EncryptAes(string key, string data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(key);
                aes.GenerateIV(); // Generate a random IV

                using (var encryptor = aes.CreateEncryptor())
                {
                    var dataBytes = Encoding.UTF8.GetBytes(data);
                    var encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

                    // Combine IV and encrypted data
                    var combinedBytes = new byte[aes.IV.Length + encryptedBytes.Length];
                    Buffer.BlockCopy(aes.IV, 0, combinedBytes, 0, aes.IV.Length);
                    Buffer.BlockCopy(encryptedBytes, 0, combinedBytes, aes.IV.Length, encryptedBytes.Length);

                    return Convert.ToBase64String(combinedBytes);
                }
            }
        }

        public static string DecryptAes(string key, string encryptedData)
        {
            var combinedBytes = Convert.FromBase64String(encryptedData);

            using (var aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(key);

                // Extract IV (first 16 bytes)
                var iv = new byte[16];
                Buffer.BlockCopy(combinedBytes, 0, iv, 0, iv.Length);
                aes.IV = iv;

                // Extract encrypted data
                var encryptedBytes = new byte[combinedBytes.Length - iv.Length];
                Buffer.BlockCopy(combinedBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

                using (var decryptor = aes.CreateDecryptor())
                {
                    var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
        #endregion
    }
}
