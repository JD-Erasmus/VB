using System;
using System.Security.Cryptography;
using System.Linq;

namespace VB.Services
{
    public class PasswordService : IPasswordService
    {
        private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NumericChars = "0123456789";
        private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        public string GeneratePassword(int length = 16, bool includeLowercase = true, bool includeUppercase = true, bool includeNumeric = true, bool includeSpecial = true)
        {
            if (length < 8)
                throw new ArgumentException("Password length must be at least 8 characters.", nameof(length));

            var charSet = string.Empty;
            if (includeLowercase) charSet += LowercaseChars;
            if (includeUppercase) charSet += UppercaseChars;
            if (includeNumeric) charSet += NumericChars;
            if (includeSpecial) charSet += SpecialChars;

            if (string.IsNullOrEmpty(charSet))
                throw new ArgumentException("At least one character set must be included.");

            var password = new char[length];

            // Ensure at least one character from each included set
            int position = 0;
            if (includeLowercase) password[position++] = LowercaseChars[RandomNumberGenerator.GetInt32(LowercaseChars.Length)];
            if (includeUppercase) password[position++] = UppercaseChars[RandomNumberGenerator.GetInt32(UppercaseChars.Length)];
            if (includeNumeric) password[position++] = NumericChars[RandomNumberGenerator.GetInt32(NumericChars.Length)];
            if (includeSpecial) password[position++] = SpecialChars[RandomNumberGenerator.GetInt32(SpecialChars.Length)];

            // Fill the rest of the password
            for (int i = position; i < length; i++)
            {
                password[i] = charSet[RandomNumberGenerator.GetInt32(charSet.Length)];
            }

            // Shuffle the password using Fisher–Yates driven by cryptographic RNG
            for (int i = password.Length - 1; i > 0; i--)
            {
                int swapIndex = RandomNumberGenerator.GetInt32(i + 1);
                (password[i], password[swapIndex]) = (password[swapIndex], password[i]);
            }

            return new string(password);
        }

        public bool ValidatePasswordStrength(string password, int requiredLength = 12, bool requireLowercase = true, bool requireUppercase = true, bool requireNumeric = true, bool requireSpecial = true)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (password.Length < requiredLength)
                return false;

            if (requireLowercase && !password.Any(char.IsLower))
                return false;

            if (requireUppercase && !password.Any(char.IsUpper))
                return false;

            if (requireNumeric && !password.Any(char.IsDigit))
                return false;

            if (requireSpecial && !password.Any(c => SpecialChars.Contains(c)))
                return false;

            return true;
        }
    }

    public interface IPasswordService
    {
        string GeneratePassword(int length = 16, bool includeLowercase = true, bool includeUppercase = true, bool includeNumeric = true, bool includeSpecial = true);
        bool ValidatePasswordStrength(string password, int requiredLength = 12, bool requireLowercase = true, bool requireUppercase = true, bool requireNumeric = true, bool requireSpecial = true);
    }
}
