using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Account.Email;
using PasswordManager.ViewModels;

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

        [HttpGet("EmailVerification")]
        public IActionResult GetEmailVerification()
        {
            var email = TempData["Email"] as string;

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("GetRegister", "Register");

            TempData.Keep("Email");

            return View("IndexEmailVerification", new EmailVerificationViewModel
            {
                Email = email
            });
        }


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
            });

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Key, error.Value);
                return View("IndexEmailVerification", model);
            }

            return RedirectToAction("GetLogin", "Login");
        }


        [HttpPost("ResendVerificationCode")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode([Bind("Email")] EmailVerificationViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
                return RedirectToAction("GetRegister", "Register");

            var result = await _emailVerificationService.ResendAsync(new EmailVerificationDto
            {
                Email = model.Email
            });

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Key, error.Value);
                return View("IndexEmailVerification", model);
            }

            TempData.Keep("Email");

            return View("IndexEmailVerification", model);
        }
    }
}