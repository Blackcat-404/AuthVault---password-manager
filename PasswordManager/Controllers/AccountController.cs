using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Enums;
using PasswordManager.Infrastructure.Email;
using PasswordManager.Infrastructure.Security;
using PasswordManager.ViewModels;

namespace PasswordManager.Controllers
{
    /// <summary>
    /// Controller for user authentication (login, register, logout)
    /// </summary>
    public class AccountController : Controller
    {

        private readonly EmailService _emailService;
        private readonly AppDbContext _appDbContext;

        public AccountController(EmailService emailService,AppDbContext appDbContext)
        {
            _emailService = emailService;
            _appDbContext = appDbContext;
        }

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
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
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
        public async Task<IActionResult> Register(RegisterViewModel model)
        {

            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                ViewBag.Error = "Please fill in all required fields";
                return View();
            }

            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View();
            }

            if (!model.AcceptTerms)
            {
                ViewBag.Error = "You must accept the terms of service";
                return View();
            }

            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            int verificationCode = VerificationCodeGenerator.Generate();
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(5);

            if (user != null && user.EmailVerificationStatus == EmailVerificationStatus.Verified)
            {
                ViewBag.Error = "An account with this email already exists.";
                return View();
            }

            if (user == null)
            {
                var userNew = new User
                {
                    Login = model.Name,
                    Email = model.Email,
                    PasswordHash = PasswordHasher.Hash(model.Password),
                    EmailVerificationStatus = EmailVerificationStatus.NotVerified,
                    EmailVerificationCode = verificationCode,
                    EmailVerificationExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    LastLoginAt = null,
                    CreatedAt = DateTime.UtcNow
                };

                _appDbContext.Users.Add(userNew);
            }
            else
            {
                user.Login = model.Name;
                user.PasswordHash = PasswordHasher.Hash(model.Password);
                user.EmailVerificationCode = verificationCode;
                user.EmailVerificationExpiresAt = expiresAt;
            }

            string bodystr = "Hello " + model.Name + "\nYour verification code is: " + verificationCode + 
                "\n\nThis code expires in 5 minutes\n" +
                "If you did not register, please ignore this email.";

            await _appDbContext.SaveChangesAsync();
            await _emailService.SendAsync(
                    to: model.Email,
                    subject: "Email verification code",
                    body:bodystr
            );

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