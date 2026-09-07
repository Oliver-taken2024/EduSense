using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EduSense.DAL.Models
{
    public class AuthModel
    {
        public AuthModel(): this(username: "", password: "")
        {
        }

        public AuthModel(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public bool AllowInit() => Password == DateTime.Now.ToString("yyyy-MM-dd");

        public string PasswordHash(byte[] salt)
        {
            // Salt for password hash. Please note this has to be changed in other classes as well if change is required!!
            // Replace the base64 string below with your actual 24-character salt encoded as base64.
            


        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: Password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        }
        public static byte[] GenerateSalt()
        {
            return RandomNumberGenerator.GetBytes(16);
        }
    }
}
