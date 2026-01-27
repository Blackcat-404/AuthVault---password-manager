using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Account.Email;
using PasswordManager.Application.Security;
using PasswordManager.ViewModels;

namespace PasswordManager.Controllers
{
    [AllowAnonymous]
    [Route("Account/Register")]
    public class EmailVerificationController : Controller
    {
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly IAuthService _authService;

        public EmailVerificationController(IEmailVerificationService emailVerificationService, IAuthService authService)
        {
            _emailVerificationService = emailVerificationService;
            _authService = authService;
        }
         
        [HttpGet("EmailVerification")]
        public IActionResult GetEmailVerification()
        {
            var email = TempData["Email"] as string;

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Account", "Register");
            }

            return View("IndexEmailVerification", new EmailVerificationViewModel
            {
                Email = email
            });
        }

        /// <summary>
        /// POST: /Account/EmailVerification
        /// Processes email verification
        /// </summary>
        /// <param name="codeVerification">Verification code</param>
        /// <returns>Redirects to vault on success, returns view with error on failure</returns>
        [HttpPost("EmailVerification")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostEmailVerification(EmailVerificationViewModel model)
        {
            if (!ModelState.IsValid)
                return View("IndexEmailVerification", model);

            var result = await _emailVerificationService.VerifyAsync(new EmailVerificationDto
                {
                    Email = model.Email,
                    VerificationCode = model.VerificationCode
                }
            );

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Key, error.Value);
                return View("IndexEmailVerification", model);
            }

            var user = result.Value;

            await _authService.SignInAsync(HttpContext, user!.Id);

            return RedirectToAction("Home", "Vault");
        }


        /// <summary>
        /// POST: /Account/ResendVerificationCode
        /// Send new verification code
        /// </summary>
        /// <param name="codeVerification">Verification code</param>
        /// <returns>Redirects to vault on success, returns view with error on failure</returns>
        [HttpPost("ResendVerificationCode")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode([Bind("Email")] EmailVerificationViewModel model)
        {
            if (!ModelState.IsValid)
                return View("IndexEmailVerification", model);

            var result = await _emailVerificationService.ResendAsync(new EmailVerificationDto
                {
                    Email = model.Email,
                    VerificationCode = model.VerificationCode
                }
            );

            if(!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Key, error.Value);
                return View("IndexEmailVerification", model);
            }

            return View("IndexEmailVerification", model);
        }
    }
}
