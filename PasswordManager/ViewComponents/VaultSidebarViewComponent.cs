using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Vault;
using System.Security.Claims;

namespace PasswordManager.ViewComponents
{
    public class VaultSidebarViewComponent : ViewComponent
    {
        private readonly IVaultSidebarService _vaultService;

        public VaultSidebarViewComponent(IVaultSidebarService vaultService)
        {
            _vaultService = vaultService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = int.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var sidebarModel = await _vaultService.GetSidebarDataAsync(userId);

            return View("_VaultSidebar", sidebarModel);
        }
    }

}
