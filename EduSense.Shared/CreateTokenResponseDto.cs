using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.Shared
{
    public class CreateTokenResponseDto
    {
        
        public DateTime TokenExpiresAt { get; set; }

        public CreateTokenResponseDto(DateTime tokenExpiresAt)
        {
           
            TokenExpiresAt = tokenExpiresAt;
        }
    }
}
