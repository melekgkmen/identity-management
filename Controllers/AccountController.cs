using IdentityApp.Models;
using IdentityApp.ViewModels.Account;
using IdentityApp.ViewModels.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUser> _userManager;
        private RoleManager<AppRole> _roleManager;
        private SignInManager<AppUser> _singInManager;
        private IEmailSender _emailsender;
        public AccountController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _singInManager = signInManager;
            _emailsender = emailSender;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AccountLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    await _singInManager.SignOutAsync();

                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", "confirm your account");
                        return View(model);
                    }

                    var result = await _singInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                        await _userManager.SetLockoutEndDateAsync(user, null);

                        return RedirectToAction("Index", "Home");
                    }

                    if (result.IsLockedOut)
                    {
                        var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);
                        var timeLeft = lockoutDate.Value - DateTime.UtcNow;
                        ModelState.AddModelError("", $"Your account has been locked,Please {timeLeft.Minutes}try in a minute");
                    }
                    else
                    {
                        ModelState.AddModelError("", "incorrect password");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No account found with this email address");
                }
            }

            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new AppUser { UserName = model.UserName, Email = model.Email, FullName = model.FullName };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var url = Url.Action("ConfirmEmail", "Account", new { user.Id, token });

                await _emailsender.SendEmailAsync(user.Email, "Account Confirmation", $"Please click on the link to confirm your email account<a href='http://localhost:5097{url}'>tıklayınız. </a>");

                TempData["message"] = "Click on the confirmation text in your email account.";
                return RedirectToAction("Login", "Account");
            }

            foreach (IdentityError err in result.Errors)
                ModelState.AddModelError("", err.Description);

            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string Id, string token)
        {
            if (Id == null || token == null)
            {
                TempData["message"] = "invalid token information";
                return View();
            }

            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                TempData["message"] = "User not found";
                return View();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                TempData["message"] = "your account has been confirmed";
                return RedirectToAction("Login", "Account");
            }

            TempData["message"] = "User not found";
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> Logout()
        {
            await _singInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData["message"] = "enter your email address";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                TempData["message"] = "There are no records matching the email address";
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var url = Url.Action("ResetPassword", "Account", new { user.Id, token });

            await _emailsender.SendEmailAsync(Email, "password reset", $"Click on the link to reset your password <a href='http://localhost:5097{url}'>click here.</a>");

            TempData["message"] = "You can reset your password with the link sent to your email address.";

            return View();
        }

        public IActionResult ResetPassword(string Id, string token)
        {
            if (Id == null || token == null)
                return RedirectToAction("Login");

            var model = new AccountResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(AccountResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Email);
            if (User == null)
            {
                TempData["message"] = "There are no users matching this email address.";
                return RedirectToAction("Login");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
            {
                TempData["message"] = "Your password has been changed";
                return RedirectToAction("Login");
            }

            foreach (IdentityError err in result.Errors)
                ModelState.AddModelError("", err.Description);

            return View(model);
        }
    }
}