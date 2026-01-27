using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Vault;
using PasswordManager.Infrastructure.Settings;
using PasswordManager.ViewModels.Vault;
using System.Security.Claims;

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

        #region 2FA
        [HttpGet("Settings")]
        public async Task<IActionResult> GetSettings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var model = await _vaultSettingsService.GetSettingsDataAsync(userId);

            return View("Settings", model);
        }

        [HttpGet("Settings/2FA/Email")]
        public async Task<IActionResult> Get2FAEmail()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var model = await _vaultSettingsService.Get2FAEmailAsync(userId);

            return View("FAEmail", model);
        }

        [HttpGet("Settings/2FA/Code")]
        public async Task<IActionResult> Get2FACode()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var model = await _vaultSettingsService.Get2FACodeAsync(userId);

            return View("FACode", model);
        }

        [HttpPost("Settings")]
        public async Task<IActionResult> PostFAEmailChange()
        {
            return RedirectToAction("Get2FAEmail");
        }

        [HttpPost("Settings/2FA/Email")]
        public async Task<IActionResult> PostSendCode(FAuthenticationEmailViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _settingsService.Add2FAAsync(userId,model.Email);

            TempData["2FA_Email"] = model.Email;
            return RedirectToAction("Get2FACode");
        }

        [HttpPost("Settings/2FA/Code")]
        public async Task<IActionResult> PostVerifyCode(FAuthenticationCodeViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var email = TempData["2FA_Email"] as string;

            if (email == null)
            {
                return RedirectToAction("Get2FAEmail");
            }

            var isValid = await _settingsService.Verify2FACodeAsync(userId, model.Code, email);
            if (!isValid)
            {
                return RedirectToAction("Get2FACode");
            }

            return RedirectToAction("GetSettings");
        }
        #endregion


    }
}
