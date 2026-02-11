using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Account.ForgotPassword;
using PasswordManager.ViewModels;

namespace PasswordManager.Controllers
{
    [Route("Account")]
    public class ForgotPasswordController : Controller
    {
        private readonly IResetPasswordService _resetPasswordService;
        public ForgotPasswordController(IResetPasswordService resetPasswordService)
        {
            _resetPasswordService = resetPasswordService;
        }

        [HttpGet("ForgotPassword")]
        public IActionResult GetForgotPassword()
        {
            return View("IndexForgotPassword");
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> PostForgotPassword(ForgotPasswordViewModel model)
        {
            Console.WriteLine("Here");
            if (!ModelState.IsValid)
                return View(model);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            await _resetPasswordService.CreateResetTokenAsync(new ForgotPasswordDto
            {
                Email = model.Email,
                BaseUrl = baseUrl
            });

            ModelState.AddModelError(nameof(model.Email), "If your email address has been confirmed and entered correctly, " +
                "and you have not sent the link within 30 minutes, then this link has been sent to you.");

            return View("IndexForgotPassword",model);
        }
    }
}
