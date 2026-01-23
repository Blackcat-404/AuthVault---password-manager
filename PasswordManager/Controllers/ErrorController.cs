using Microsoft.AspNetCore.Mvc;

namespace PasswordManager.Controllers
{
    public class ErrorController : Controller
    {
        [Route("/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        [Route("/Error/{statusCode}")]
        public IActionResult HandleErrorCode(int statusCode)
        {
            if (statusCode == 404)
                return View("NotFound");

            return View("Error");
        }
    }
}