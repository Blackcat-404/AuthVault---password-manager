using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Settings;
using PasswordManager.Application.Vault;
using PasswordManager.Infrastructure.Settings;
using PasswordManager.ViewModels.Vault;
using System.Security.Claims;
using System.Text;

namespace PasswordManager.Controllers
{
    [Route("Vault")]
    public class SettingsController : Controller
    {
        SettingsService _settingsService;
        IVaultSettingsService _vaultSettingsService;
        public SettingsController(IVaultSettingsService vaultSettingsService,SettingsService settingsService)
        {
            _vaultSettingsService = vaultSettingsService;
            _settingsService = settingsService;
        }

        [HttpGet("Settings")]
        public async Task<IActionResult> GetSettings()
        {
            var userId = GetUserID();
            var model = await _vaultSettingsService.GetSettingsDataAsync(userId);

            return View("Settings", model);
        }

        #region 2FA
        [HttpGet("Settings/2FA/Email")]
        public async Task<IActionResult> Get2FAEmail()
        {
            var userId = GetUserID();
            var model = await _vaultSettingsService.Get2FAEmailAsync(userId);

            return View("FAEmail", model);
        }

        [HttpGet("Settings/2FA/EmailVerification")]
        public async Task<IActionResult> GetEmailVerification(string token)
        {
            bool IsValidToken = await _settingsService.Verify2FAToken(token);
            if (!IsValidToken)
            {
                return View("~/Views/Token/InvalidToken.cshtml");
            }

            var email = TempData["email"] as string;
            await _settingsService.Add2FAEmailAsync(token, email!);

            return View("~/Views/Token/Success.cshtml");
        }

        [HttpGet("Settings/2FA/EmailVerificationLinkSent")]
        public IActionResult Get2FAEmailVerificationLinkSent()
        {
            return View("~/Views/Token/TokenSent.cshtml");
        }

        [HttpPost("Settings/EmailChange")]
        public IActionResult PostFAEmailChange()
        {
            return RedirectToAction("Get2FAEmail");
        }

        [HttpPost("Settings/2FA/Email")]
        public async Task<IActionResult> PostSendCode(FAuthenticationEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("FAEmail", model);
            }

            var userId = GetUserID();
            await _settingsService.Add2FAAsync(userId,model.Email);

            TempData["email"] = model.Email;
            return RedirectToAction("Get2FAEmailVerificationLinkSent");
        }

        [HttpPost("Settings/Toggle2FA")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle2FA([FromBody] Toggle2FADto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _settingsService.Set2FAStatement(userId, dto.IsEnabled);

            return RedirectToAction("GetSettings");
        }
        #endregion

        #region Change master password get/post methods
        [HttpGet("Settings/ChangeMasterPassword")]
        public async Task<IActionResult> GetChangeMasterPassword()
        {
            var userId = GetUserID();
            var model = await _vaultSettingsService.GetChangeMasterPasswordAsync(userId);

            return View("ChangeMasterPassword", model);
        }


        [HttpPost("Settings/ChangeMasterPassword")]
        public async Task<IActionResult> PostChangeMasterPassword(ChangeMasterPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ChangeMasterPassword",model);
            }

            int userId = GetUserID();
            await _settingsService.ChangeMasterPassword(userId,model.NewPassword);

            return RedirectToAction("GetSettings");
        }
        #endregion

        #region Delete account get/post methods
        [HttpGet("Settings/DeleteAccount/Password")]
        public async Task<IActionResult> GetDeleteAccountPassword()
        {
            var userId = GetUserID();
            var model = await _vaultSettingsService.GetDeleteAccountPassword(userId);

            return View("DeleteAccountPassword", model);
        }

        [HttpPost("Settings/DeleteAccount/Password")]
        public async Task<IActionResult> PostDeleteAccountPassword(DeleteAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("DeleteAccountPassword", model);
            }

            int userId = GetUserID();
            if (!await _settingsService.PasswordVerifyAsync(userId,model.Password))
            {
                ModelState.AddModelError("Password","Password is incorrect");
                return View("DeleteAccountPassword", model);
            }

            //TODO: implement "OK" page
            return RedirectToAction("GetDeleteAccountConfirmation");
        }

        [HttpGet("Settings/DeleteAccount/Confirm")]
        public async Task<IActionResult> GetDeleteAccountConfirmation()
        {
            var userId = GetUserID();
            var model = await _vaultSettingsService.GetDeleteAccountConfirmationAsync(userId);
            return View("DeleteAccountConfirmation", model);
        }

        [HttpPost("Settings/DeleteAccount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = GetUserID();

            if (await _settingsService.DeleteAccountAsync(userId))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Register", "Account");
            }
            return View("Vault","Home");
        }
        #endregion

        [HttpPost("Settings/ExportData")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostExportData(string format)
        {
            int userId = GetUserID();
            if (format == "txt")
            {
                string content = await _settingsService.ExportUserDataAsTextAsync(userId);
                var bytes = Encoding.UTF8.GetBytes(content);
                return File(bytes, "text/plain", "Data.txt");
            }
            else
            {
                string content = await _settingsService.ExportUserDataAsMarkdownAsync(userId);
                var bytes = Encoding.UTF8.GetBytes(content);
                return File(bytes, "text/plain", "Data.md");
            }
        }

        private int GetUserID()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
