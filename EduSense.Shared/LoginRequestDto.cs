using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EduSense.Shared
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }= string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }= string.Empty;
    }
}
