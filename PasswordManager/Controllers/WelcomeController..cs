using Microsoft.AspNetCore.Mvc;

namespace PasswordManager.Controllers
{
    /// <summary>
    /// Controller for the welcome/landing page
    /// </summary>
    public class WelcomeController : Controller
    {
        /// <summary>
        /// GET: / or /Welcome or /Welcome/Home
        /// Displays the landing page with hero section and features
        /// </summary>
        /// <returns>Landing page view</returns>
        [HttpGet]
        public IActionResult IndexWelcome()
        {
            return View();
        }
    }
}
