using System.ComponentModel.DataAnnotations;

namespace EduSense.Shared
{
    public class SetInitialPasswordRequestDto
    {
        [Required(ErrorMessage = "UserId krävs.")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Token krävs.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Användarnamn krävs.")]
        [MinLength(3, ErrorMessage = "Användarnamnet måste vara minst 3 tecken långt.")]
        [MaxLength(50, ErrorMessage = "Användarnamnet får vara högst 50 tecken långt.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lösenord krävs.")]
        [MinLength(8, ErrorMessage = "Lösenordet måste vara minst 8 tecken långt.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}