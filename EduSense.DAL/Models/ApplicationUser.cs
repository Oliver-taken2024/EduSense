using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace EduSense.DAL.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(30)]
        public string? DisplayName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
