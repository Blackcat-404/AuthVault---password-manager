using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Account.Login;
using PasswordManager.ViewModels;
using System.Security.Claims;

namespace PasswordManager.Controllers
{
    [Route("Account")]
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
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

            var result = await _loginService.VerifyLoginAsync(new LoginUserDto
                {
                    Email = model.Email!,
                    Password = model.Password!,
                }
            );

            if(!result.Success)
            {
                foreach(var error in result.Errors)
                    ModelState.AddModelError(error.Key, error.Value);
                return View("IndexLogin", model);
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, result.Value!.Id.ToString()),
                };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToAction("Home", "Vault");
        }
    }
}
