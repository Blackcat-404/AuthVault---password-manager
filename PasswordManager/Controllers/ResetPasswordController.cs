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
            var result = await _resetPasswordService.ValidateTokenAsync(token);
            if (!result.Success)
                return View("InvalidToken");


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

            var result = await _resetPasswordService.ResetPasswordAsync(model.Token,model.NewPassword);
            if (!result.Success)
                return View("InvalidToken");

            return RedirectToAction("Login", "Account");
        }
    }
}
