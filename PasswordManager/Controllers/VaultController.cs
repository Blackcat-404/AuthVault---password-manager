using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;
using PasswordManager.Application.Vault;
using PasswordManager.Domain.Entities;
using PasswordManager.ViewModels.Vault;
using PasswordManager.ViewModels.Vault.VaultItems;
using System.Security.Claims;

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
        private readonly IVaultSettingsService _vaultSettingsService;
        private readonly IGetItemService _vaultGetItemService;
        private readonly IUpdateItemFieldService _updateItemFieldService;
        private readonly IDeleteItemService _deleteItemService;
        private readonly IAddItemService _addItemService;
        private readonly IAddFolderService _addFolderService;
        private readonly IGetFolderService _getFolderService;
        private readonly IGetAllFoldersService _getAllFoldersService;
        private readonly IDeleteFolderService _deleteFolderService;

        public VaultController(IVaultHomeService vaultHomeService,
                                IVaultSidebarService vaultSidebarService,
                                IVaultSettingsService vaultSettingsService,
                                IGetItemService vaultGetItemService,
                                IUpdateItemFieldService updateItemFieldService,
                                IDeleteItemService deleteItemService,
                                IAddItemService addItemService,
                                IAddFolderService addFolderService,
                                IGetFolderService getFolderService,
                                IGetAllFoldersService getAllFoldersService,
                                IDeleteFolderService deleteFolderService)
        {
            _vaultHomeService = vaultHomeService;
            _vaultSidebarService = vaultSidebarService;
            _vaultSettingsService = vaultSettingsService;
            _vaultGetItemService = vaultGetItemService;
            _updateItemFieldService = updateItemFieldService;
            _deleteItemService = deleteItemService;
            _addItemService = addItemService;
            _addFolderService = addFolderService;
            _getFolderService = getFolderService;
            _getAllFoldersService = getAllFoldersService;
            _deleteFolderService = deleteFolderService;
        }


        [HttpGet]
        public async Task<IActionResult> Home()
        {

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Account", "Login");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var model = await _vaultHomeService.GetHomeDataAsync(userId);

            return View("IndexVault", model);
        }


        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var model = await _vaultSettingsService.GetSettingsDataAsync(userId);

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> AddItem(string type = "login")
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var model = new AddItemViewModel
            {
                ItemType = type,
                Sidebar = await _vaultSidebarService.GetSidebarDataAsync(userId),
                Folders = await _getAllFoldersService.GetAllFoldersAsync(userId)
            };
            // Pass item type to view to show appropriate form

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(AddItemViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (model.ItemType == "login")
            {
                var dto = new LoginItemDto
                {
                    Title = model.Title,
                    Password = model.LoginItem!.Password,
                    Note = model.LoginItem.Note,
                    UserId = userId,
                    FolderId = model.FolderId,
                    CreatedAt = DateTime.UtcNow,
                    Login = model.LoginItem.Login!,
                    WebURL = model.LoginItem.WebURL,
                };

                await _addItemService.AddLoginAsync(dto);
            }
            else if (model.ItemType == "card")
            {
                var dto = new CardItemDto
                {
                    Title = model.Title,
                    CardNumber = model.CardItem!.CardNumber,
                    CardholderName = model.CardItem!.CardholderName,
                    ExpireMonth = model.CardItem.ExpireMonth,
                    ExpireYear = model.CardItem.ExpireYear,
                    Note = model.CardItem.Note,
                    UserId = userId,
                    FolderId = model.FolderId,
                    CreatedAt = DateTime.UtcNow,
                };

                await _addItemService.AddCardAsync(dto);
            }
            else
            {
                var dto = new NoteItemDto
                {
                    Title = model.Title,
                    Content = model.NoteItem!.Content,
                    UserId = userId,
                    FolderId = model.FolderId,
                    CreatedAt = DateTime.UtcNow,
                };

                await _addItemService.AddNoteAsync(dto);
            }
            
            return RedirectToAction("Home");
        }



        [HttpGet]
        public async Task<IActionResult> ViewItem(int id, string type)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            VaultItemViewModel? item = null;
            IReadOnlyDictionary<int, string> folders = await _getAllFoldersService.GetAllFoldersAsync(userId);

            switch (type?.ToLower())
            {
                case "login":
                    
                    item = await _vaultGetItemService.GetLoginItemAsync(userId, id);
                    break;
                case "card":
                    
                    item = await _vaultGetItemService.GetCardItemAsync(userId, id);
                    break;
                case "note":
                    
                    item = await _vaultGetItemService.GetNoteItemAsync(userId, id);
                    break;
            }

            if (item == null)
            {
                return NotFound();
            }

            var model = new ViewItemViewModel
            {
                Sidebar = await _vaultSidebarService.GetSidebarDataAsync(userId),
                Item = item,
                Folders = folders
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateField(int id, string itemType, string fieldName, string fieldValue)
        {

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            switch (itemType?.ToLower())
            {
                case "login":
                    await _updateItemFieldService.UpdateLoginFieldAsync(userId, id, fieldName, fieldValue);
                    break;
                case "card":
                    await _updateItemFieldService.UpdateCardFieldAsync(userId, id, fieldName, fieldValue);
                    break;
                case "note":
                    await _updateItemFieldService.UpdateNoteFieldAsync(userId, id, fieldName, fieldValue);
                    break;
                default:
                    return Json(new { success = false, message = "Invalid item type" });
            }
            
            return Json(new { success = true, message = "Field updated successfully" });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id, string type)
        {

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            switch (type?.ToLower())
            {
                case "login":
                    await _deleteItemService.DeleteLoginItemAsync(userId, id);
                    break;
                case "card":
                    await _deleteItemService.DeleteCardItemAsync(userId, id);
                    break;
                case "note":
                    await _deleteItemService.DeleteNoteItemAsync(userId, id);
                    break;
            }

            return RedirectToAction("Home");
        }


        [HttpGet]
        public async Task<IActionResult> Folder(int folderId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var model = await _getFolderService.GetFolderAsync(userId, folderId);

            return View("IndexVault", model);
        }


        [HttpGet]
        public async Task<IActionResult> AddFolder()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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

            var dto = new FolderDto
            {
                Name = model.Name,
                Description = model.Description,
                Color = model.Color,
                UserId = userId
            };

            await _addFolderService.AddFolderAsync(dto);

            return RedirectToAction(nameof(Home));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFolder(int folderId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _deleteFolderService.DeleteFolderAsync(userId, folderId);
                return RedirectToAction(nameof(Home));
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Folder not found or access denied");
                return NotFound();
            }
        }



        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            return View("Home");
        }
    }
}