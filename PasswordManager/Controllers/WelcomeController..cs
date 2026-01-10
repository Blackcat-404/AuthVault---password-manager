using Microsoft.AspNetCore.Mvc;

namespace PasswordManager.Controllers
{
    /// <summary>
    /// Controller for the welcome/landing page
    /// </summary>
    public class WelcomeController : Controller
    {
        /// <summary>
        /// GET: / or /Welcome or /Welcome/Index
        /// Displays the landing page with hero section and features
        /// </summary>
        /// <returns>Landing page view</returns>
        public IActionResult Index()
        {
            return View();
        }
    }
}