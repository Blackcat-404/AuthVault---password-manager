using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Account.Login;
using PasswordManager.Application.Security;
using PasswordManager.ViewModels;

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

        [HttpGet("Login/2FA")]
        public IActionResult Get2FA()
        {
            return View("FAuthentication");
        }

        [HttpPost("Login/2FA")]
        public async Task<IActionResult> PostVerifyCodeAsync(Login2FAViewModel model)
        {
            Console.WriteLine(model.Code);
            var id = Convert.ToInt32(TempData["userId"]);
            if (!await _loginService.Verify2FACode(id,model.Code))
            {
                return RedirectToAction("GetLogin");
            }

            await _authService.SignInAsync(HttpContext, id);

            return RedirectToAction("Home", "Vault");
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

            //2FA Checking
            if (await _loginService.Has2FAAsync(model.Email!))
            {
                var u = result.Value!;
                await _loginService.Send2FACode(u.Id,model.Email!);

                TempData["userId"] = u.Id;
                return RedirectToAction("Get2FA");
            }

            Console.WriteLine("no 2FA");

            var user = result.Value!;
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
