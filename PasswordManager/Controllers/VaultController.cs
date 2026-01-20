using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.ViewModels;
using PasswordManager.Application.Vault;
using System.Security.Claims;
using PasswordManager.ViewModels.Vault;
using PasswordManager.ViewModels.Vault.VaultItems;

namespace PasswordManager.Controllers
{
    /// <summary>
    /// Controller for managing the password vault (dashboard, CRUD operations)
    /// </summary>
    [Authorize]
    public class VaultController : Controller
    {
        private readonly IVaultHomeService _vaultHomeService;
        private readonly IVaultSidebarService _vaultSidebarService;



        public VaultController(IVaultHomeService vaultHomeService, IVaultSidebarService vaultSidebarService)
        {
            
            _vaultHomeService = vaultHomeService;
            _vaultSidebarService = vaultSidebarService;
        }


        [HttpGet]
        public async Task<IActionResult> Home()
        {

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Account", "Login");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _vaultHomeService.SeedTestDataAsync(userId);

            var model = await _vaultHomeService.GetHomeDataAsync(userId);

            return View("IndexVault", model);
        }


        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _vaultHomeService.SeedTestDataAsync(userId);

            var model = await _vaultSidebarService.GetSidebarDataAsync(userId);

            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> AddFolder()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _vaultHomeService.SeedTestDataAsync(userId);

            var model = await _vaultSidebarService.GetSidebarDataAsync(userId);

            return View(model);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFolder(FolderViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            return RedirectToAction(nameof(Home));
        }



        [HttpGet]
        public async Task<IActionResult> AddItem(string type = "login")
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _vaultHomeService.SeedTestDataAsync(userId);

            var model = new AddItemViewModel
            {
                ItemType = type,
                Sidebar = await _vaultSidebarService.GetSidebarDataAsync(userId)
            };
            // Pass item type to view to show appropriate form

            return View(model);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem()
        {
            return RedirectToAction("Home");
        }



        [HttpGet]
        public async Task<IActionResult> EditItem(int id)
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(int id, VaultItemViewModel model)
        {
            return RedirectToAction("Home");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            return RedirectToAction("Home");
        }




        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            return View("Home");
        }




        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            return View("Home");
        }




        [HttpGet]
        public async Task<IActionResult> Folder(int folderId)
        {
            return View("Home");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            return Json(new { success = false });
        }
    }
}