using Microsoft.AspNetCore.Mvc;

namespace PasswordManager.Controllers
{
    /// <summary>
    /// Controller for user authentication (login, register, logout)
    /// </summary>
    public class AccountController : Controller
    {
        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// POST: /Account/Login
        /// Processes user login
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's master password</param>
        /// <returns>Redirects to vault on success, returns view with error on failure</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password";
                return View();
            }

            // Temporary: redirect without authentication for testing UI
            return RedirectToAction("Index", "Vault");
        }

        /// <summary>
        /// GET: /Account/Register
        /// Displays the registration page
        /// </summary>
        /// <returns>Register view</returns>
        [HttpGet]
        public IActionResult Register()
        {

            return View();
        }

        /// <summary>
        /// POST: /Account/Register
        /// Processes new user registration
        /// </summary>
        /// <param name="name">User's full name</param>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's master password</param>
        /// <param name="passwordConfirm">Password confirmation</param>
        /// <param name="acceptTerms">Terms of service acceptance</param>
        /// <returns>Redirects to login on success, returns view with error on failure</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string name, string email, string password, string passwordConfirm, bool acceptTerms = false)
        {

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please fill in all required fields";
                return View();
            }

            if (password != passwordConfirm)
            {
                ViewBag.Error = "Passwords do not match";
                return View();
            }

            if (!acceptTerms)
            {
                ViewBag.Error = "You must accept the terms of service";
                return View();
            }

            ViewBag.Success = "Account created successfully! Please log in.";
            return RedirectToAction("Login");
        }

        /// <summary>
        /// GET/POST: /Account/Logout
        /// Logs out the current user
        /// </summary>
        /// <returns>Redirects to welcome page</returns>
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            return RedirectToAction("Index", "Welcome");
        }

        /// <summary>
        /// GET: /Account/ForgotPassword
        /// Displays the forgot password page
        /// </summary>
        /// <returns>Forgot password view</returns>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            
            return View();
        }
    }
}