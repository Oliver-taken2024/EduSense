using System;

namespace EduSense.Shared
{
    public class InviteUserRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Admin" eller "Analyst"
    }
}