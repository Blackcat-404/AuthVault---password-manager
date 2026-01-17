using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Account.Email;
using PasswordManager.ViewModels;
using System.Security.Claims;

namespace PasswordManager.Controllers
{
    [Route("Account/Register")]
    public class EmailVerificationController : Controller
    {
        private readonly IEmailVerificationService _emailVerificationService;

        public EmailVerificationController(IEmailVerificationService emailVerificationService)
        {
            _emailVerificationService = emailVerificationService;
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

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user!.Id.ToString())
                };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

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
