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

        [HttpGet("Login/2FA")]
        public async Task<IActionResult> Get2FA(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return View("~/Views/Token/InvalidToken.cshtml");
            }

            var userId = await _loginService.GetUserIdByToken(token);

            if (userId == null)
            {
                return View("~/Views/Token/InvalidToken.cshtml");
            }

            if (!await _loginService.Verify2FAToken(userId.Value, token))
            {
                return View("~/Views/Token/InvalidToken.cshtml");
            }

            return View("~/Views/Token/Success.cshtml");
        }

        [HttpGet("Login/2FA/CheckStatus")]
        public async Task<IActionResult> Check2FAStatus(int userId)
        {
            var isVerified = await _loginService.IsTokenVerified(userId);

            if (isVerified)
            {
                await _authService.SignInAsync(HttpContext, userId);
                return Json(new { success = true, verified = true });
            }

            return Json(new { success = true, verified = false });
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

            //2FA Checking
            if (await _loginService.Has2FAAsync(model.Email!))
            {
                await _loginService.Send2FACode(user.Id);
                TempData["userId"] = user.Id;

                return View("~/Views/Token/TokenSent.cshtml");
            }

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
