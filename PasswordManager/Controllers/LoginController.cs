using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Account.Login;
using PasswordManager.Application.Account.Login;
using PasswordManager.Application.Security;
using PasswordManager.Data;
using PasswordManager.Infrastructure.Login;
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
        private readonly AppDbContext _db;

        public LoginController(ILoginService loginService, IAuthService authService, AppDbContext db)
        {
            _loginService = loginService;
            _authService = authService;
            _db = db;
        }

        [HttpGet("Login")]
        public IActionResult GetLogin()
        {
            return View("IndexLogin");
        }

        /// <summary>
        /// POST: /Account/Login
        /// Processes user login
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's master password</param>
        /// <returns>Redirects to vault on success, returns view with error on failure</returns>

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
            //user.LastLoginAt = DateTime.UtcNow;
            //await _db.SaveChangesAsync();

            await _authService.SignInAsync(HttpContext, user.Id);

            return RedirectToAction("Home", "Vault");
        }

        [Authorize]
        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutAsync(HttpContext);
            return RedirectToAction("Login", "Account");
        }
    }
}
