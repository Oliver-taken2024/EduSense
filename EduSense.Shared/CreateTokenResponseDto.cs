using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.Shared
{
    public class CreateTokenResponseDto
    {
        public string Token { get; }
        public DateTime TokenExpiresAt { get; }

        public CreateTokenResponseDto(string token, DateTime tokenExpiresAt)
        {
            Token = token;
            TokenExpiresAt = tokenExpiresAt;
        }
    }
}
