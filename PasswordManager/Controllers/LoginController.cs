using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Account.Login;
using PasswordManager.Application.Security;
using PasswordManager.ViewModels;
using System.Security.Claims;

namespace PasswordManager.Controllers
{
    [AllowAnonymous]
    [Route("Account")]
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;
        private readonly IAuthService _authService;

        public LoginController(ILoginService loginService, IAuthService authService)
        {
            _loginService = loginService;
            _authService = authService;
        }

        [HttpGet("Login")]
        public IActionResult GetLogin()
        {
            return View("IndexLogin");
        }


        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View("IndexLogin", model);

            var result = await _loginService.VerifyLoginAsync(
                new LoginUserDto
                {
                    Email = model.Email!,
                    Password = model.Password!
                });

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Key, error.Value);

                return View("IndexLogin", model);
            }

            var user = result.Value!;
            await _authService.SignInAsync(HttpContext, user.Id);

            return RedirectToAction("Home", "Vault");
        }

        [Authorize]
        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _loginService.DeleteEncryptionKey(userId);
            await _authService.SignOutAsync(HttpContext);
            return RedirectToAction("Login", "Account");
        }
    }
}
