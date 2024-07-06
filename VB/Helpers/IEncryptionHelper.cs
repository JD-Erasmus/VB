using System;

namespace VB.Helpers
{
    public interface IEncryptionHelper
    {
        /// <summary>
        /// Encrypts the provided plain text string.
        /// </summary>
        /// <param name="plainText">The text to be encrypted.</param>
        /// <returns>The encrypted string.</returns>
        /// <exception cref="ArgumentException">Thrown when the input is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the encryption key is not set or invalid.</exception>
        string EncryptString(string plainText);

        /// <summary>
        /// Decrypts the provided cipher text string.
        /// </summary>
        /// <param name="cipherText">The text to be decrypted.</param>
        /// <returns>The decrypted string.</returns>
        /// <exception cref="ArgumentException">Thrown when the input is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the encryption key is not set or invalid.</exception>
        /// <exception cref="FormatException">Thrown when the cipher text is not in the correct format.</exception>
        string DecryptString(string cipherText);
    }
}