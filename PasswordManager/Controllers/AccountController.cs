using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Enums;
using PasswordManager.Infrastructure.Email;
using PasswordManager.Infrastructure.Login;
using PasswordManager.Infrastructure.Security;
using PasswordManager.ViewModels;
using System.Security.Claims;

namespace PasswordManager.Controllers
{
    /// <summary>
    /// Controller for user authentication (login, register, logout)
    /// </summary>
    public class AccountController : Controller
    {

        private readonly EmailService _emailService;
        private readonly AppDbContext _appDbContext;
        private readonly EmailVerificationService _emailVerificationService;
        private readonly LoginService _loginService;

        public AccountController(EmailService emailService, AppDbContext appDbContext, EmailVerificationService emailVerificationService, LoginService loginService)
        {
            _emailService = emailService;
            _appDbContext = appDbContext;
            _emailVerificationService = emailVerificationService;
            _loginService = loginService;
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
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = await _loginService.VerifyLoginAsync(
                    model.Email!,
                    model.Password!);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));

                return RedirectToAction("Index", "Vault");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.Email), ex.Message);
                return View(model);
            }
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
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Condition where the mail that sends the code hasn't been entered. Change example@gmail.com
            /*if (model.Email == "example@gmail.com") 
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Are you serious, dude?"
                );

                return View(model);
            }*/

            var loginExists = await _appDbContext.Users.AnyAsync(
                u => u.Login == model.Name && 
                u.EmailVerificationStatus == EmailVerificationStatus.Verified
            );
            if (loginExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "This login is already taken"
                );

                return View(model);
            }

            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            int verificationCode = VerificationCodeGenerator.Generate();
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(5);

            if (user != null && user.EmailVerificationStatus == EmailVerificationStatus.Verified)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "An account with this email already exists"
                );
                return View(model);
            }

            if (!model.AcceptTerms)
            {
                ModelState.AddModelError(
                    nameof(model.AcceptTerms),
                    "You must accept the terms of use"
                );
                return View(model);
            }

            if (user == null)
            {
                var userNew = new User
                {
                    Login = model.Name!,
                    Email = model.Email!,
                    PasswordHash = PasswordHasher.Hash(model.Password!),
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
                user.Login = model.Name!;
                user.PasswordHash = PasswordHasher.Hash(model.Password!);
                user.EmailVerificationCode = verificationCode;
                user.EmailVerificationExpiresAt = expiresAt;
            }

            string bodystr = "Hello, " + model.Name + "\nYour verification code is: " + verificationCode +
                "\n\nThis code expires in 5 minutes\n" +
                "If you did not register, please ignore this email.";

            await _appDbContext.SaveChangesAsync();
            await _emailService.SendAsync(
                    to: model.Email!,
                    subject: "Email verification code",
                    body: bodystr
            );

            return RedirectToAction("EmailVerification", new { email = model.Email });
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


        //EmailStuff

        [HttpGet]
        public IActionResult EmailVerification(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }

            return View(new EmailVerificationViewModel
            {
                Email = email
            });
        }

        /// <summary>
        /// POST: /Account/EmailVerification
        /// Processes email verification
        /// </summary>
        /// <param name="codeVerification">Verification code</param>
        /// <returns>Redirects to vault on success, returns view with error on failure</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailVerification(EmailVerificationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _emailVerificationService.VerifyAsync(model.Email, model.VerificationCode);
                return RedirectToAction("Login");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    nameof(model.VerificationCode), 
                    ex.Message
                );
                return View(model);
            }    
        }

        /// <summary>
        /// POST: /Account/ResendVerificationCode
        /// Send new verification code
        /// </summary>
        /// <param name="codeVerification">Verification code</param>
        /// <returns>Redirects to vault on success, returns view with error on failure</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode([Bind("Email")] EmailVerificationViewModel model)
        {
            if (!ModelState.IsValid)
                return View("EmailVerification", model);

            try
            {
                await _emailVerificationService.ResendAsync(model.Email);
                ViewBag.Success = "Verification code sent again";

                return View("EmailVerification", model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("EmailVerification", model);
            }
        }
    }
}