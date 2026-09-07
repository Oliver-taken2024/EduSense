using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.DAL.Models
{
    public class CreateTokenRespondModel
    {
        public string Token { get; }
        public DateTime TokenValidationTime { get; }

        public string RefreshToken { get; }

        public DateTime RefreshTokenValidationTime { get; }

        public bool IsExpired => DateTime.UtcNow > TokenValidationTime;

        public CreateTokenRespondModel(string token, DateTime tokenValidationTime, string refreshToken, DateTime refreshTokenValidationTime)
        {
            Token = token;
            TokenValidationTime = tokenValidationTime;
            RefreshToken = refreshToken;
            RefreshTokenValidationTime = refreshTokenValidationTime;
        }
    }
}
