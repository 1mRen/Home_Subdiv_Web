using System.Threading.Tasks;
using Home_Sbdv.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace Home_Sbdv.Services
{
    public interface IAccountService
    {
        Task<ServiceResult> RegisterUserAsync(RegistrationViewModel model);
        Task<ServiceResult> VerifyEmailAsync(string token, string email);
        Task<ServiceResult> ResendVerificationEmailAsync(string email);
        Task<ServiceResult<string>> AuthenticateUserAsync(LoginViewModel model, bool rememberMe);
        Task<ServiceResult> SendPasswordResetLinkAsync(string email);
        Task<ServiceResult> ValidatePasswordResetTokenAsync(string token, string email);
        Task<ServiceResult> ResetPasswordAsync(Home_Sbdv.Models.ResetPasswordViewModel model);
    }
}