using Microsoft.AspNetCore.Mvc;
using PasswordManager.ViewModels;
using PasswordManager.Application.Account.ForgotPassword;

namespace PasswordManager.Controllers
{
    [Route("Account/ForgotPassword")]
    public class ResetPasswordController : Controller
    {
        private readonly IResetPasswordService _resetPasswordService;
        public ResetPasswordController(IResetPasswordService resetPasswordService) 
        { 
            _resetPasswordService = resetPasswordService;
        }

        [HttpGet("ResetPassword")]
        public async Task<IActionResult> GetResetPassword(string token)
        {
            bool IsValideToken = await _resetPasswordService.ValidateTokenAsync(token);
            if (!IsValideToken)
                return View("~/Views/Token/InvalidToken.cshtml");

            return View("IndexResetPassword", new ResetPasswordViewModel
            {
                Token = token
            });
        }

        [HttpPost("ResetPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View("IndexResetPassword", model);

            bool result = await _resetPasswordService.ResetPasswordAsync(model.Token,model.NewPassword);
            if (!result)
                return View("~/Views/Token/InvalidToken.cshtml");

            return RedirectToAction("Login", "Account");
        }
    }
}
