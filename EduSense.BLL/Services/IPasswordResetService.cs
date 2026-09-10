using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.BLL.Services
{
    public interface IPasswordResetService
    {
        Task<string> ForgotPasswordAsync(string email);

        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
