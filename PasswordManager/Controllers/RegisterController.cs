using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Account.Register;
using PasswordManager.ViewModels;


namespace PasswordManager.Controllers
{
    [Route("Account")]
    public class RegisterController : Controller
    {
        readonly IRegisterService _registerService;

        public RegisterController(IRegisterService registerService)
        {
            _registerService = registerService;
        }


        /// <summary>
        /// GET: /Account/Register
        /// Displays the registration page
        /// </summary>
        /// <returns>Register view</returns>
        [HttpGet("Register")]
        public IActionResult GetRegister()
        {
            return View("IndexRegister");
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
        [HttpPost("Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostRegister(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("IndexRegister", model);
            }

            var result = await _registerService.RegisterUserAsync(new RegisterUserDto
                {
                    Name = model.Name!,
                    Email = model.Email!,
                    Password = model.Password!,
                    AcceptTerms = model.AcceptTerms
                }
            );

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Key, error.Value);

                return View("IndexRegister", model);
            }

            TempData["Email"] = model.Email;
            return RedirectToAction("EmailVerification", "Register");

        }
    }
}
