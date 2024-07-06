using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VB.Helpers
{
    public class EncryptionHelper : IEncryptionHelper
    {
        private readonly string _encryptionKey;
        private const int KeySize = 256;
        private const int BlockSize = 128;
        private const int IvSize = 16;

        public EncryptionHelper(IConfiguration configuration)
        {
            _encryptionKey = configuration["ENCRYPTION_KEY"]
                ?? throw new InvalidOperationException("Encryption key is not set in configuration.");
        }

        public string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("Plain text cannot be empty.", nameof(plainText));

            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.GenerateIV();
            var iv = aes.IV;

            using var deriveBytes = new Rfc2898DeriveBytes(_encryptionKey, iv, 1000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(aes.KeySize / 8);

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream();

            memoryStream.Write(iv, 0, iv.Length);

            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public string DecryptString(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentException("Cipher text cannot be empty.", nameof(cipherText));

            var cipherBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var iv = new byte[IvSize];
            Array.Copy(cipherBytes, 0, iv, 0, iv.Length);

            using var deriveBytes = new Rfc2898DeriveBytes(_encryptionKey, iv, 1000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(aes.KeySize / 8);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream(cipherBytes, iv.Length, cipherBytes.Length - iv.Length);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
    }
}