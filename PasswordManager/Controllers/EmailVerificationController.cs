using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Account.Email;

namespace PasswordManager.Controllers
{
    [AllowAnonymous]
    [Route("Account/Register")]
    public class EmailVerificationController : Controller
    {
        private readonly IEmailVerificationService _emailVerificationService;
        public EmailVerificationController(IEmailVerificationService emailVerificationService) 
        { 
            _emailVerificationService = emailVerificationService;
        }

        [HttpGet("EmailVerificationLinkSent")]
        public IActionResult GetEmailVerificationLinkSent()
        {
            return View("~/Views/Token/TokenSent.cshtml");
        }

        [HttpGet("VerifyEmail")]
        public async Task<IActionResult> GetVerifyEmail(string token)
        {
            bool IsValidToken = await _emailVerificationService.VerifyTokenAsync(token);
            if (!IsValidToken)
            {
                return View("~/Views/Token/InvalidToken.cshtml");
            }

            await _emailVerificationService.VerifyEmailAsync(token);
            return View("~/Views/Token/Success.cshtml");
        }
    }
}