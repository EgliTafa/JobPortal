using JobPortal.Models;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using NETCore.MailKit.Core;

namespace JobPortal.Controllers
{
    [Route("accounts/")]
    public class AccountController : Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signManager;
        private RoleManager<IdentityRole> _roleManager;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        private IEmailService _emailService;
        public AccountController(UserManager<User> userManager,
            SignInManager<User> signManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, ILogger<AccountController> logger, IWebHostEnvironment hostEnvironment, IEmailService emailService)
        {
            _userManager = userManager;
            _signManager = signManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
            _emailService = emailService;
            webHostEnvironment = hostEnvironment;
        }

        [HttpGet]
        [Route("employer/register")]
        public IActionResult EmployerRegister()
        {
            return View();
        }

        [HttpPost]
        [Route("employer/register")]
        public async Task<IActionResult> EmployerRegister(IFormFile upload,
            [Bind("FirstName", "LastName", "Email", "Password", "ConfirmPassword","ImagePath", "PhoneNumber","Gender","Description")]
            EmployerRegisterViewModel model)
        {
            if (upload != null && upload.Length > 0)
            {
                var fileName = Path.GetFileName(upload.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                var updatedFilePath = filePath.Substring(filePath.IndexOf("/img"));

                model.ImagePath = updatedFilePath;

                using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileSrteam);
                }
            }


            var email = _context.Users.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());
            Random rnd = new Random();
            string randomNumber = rnd.Next(1, 100000).ToString();

            if (email != null)
            {
                ModelState.AddModelError("Email", "Adresa email ekziston. Ju lutem përdorni një adresë tjetër!");
            }

            if (ModelState.IsValid && model != null)
            {
                string usr = model.FirstName + randomNumber;
                usr = usr.Replace(" ", "");
                var user = new User
                {
                    UserName = usr,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Description = model.Description,
                    PhoneNumber = model.PhoneNumber,
                    ImagePath = model.ImagePath
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    bool checkRole = await _roleManager.RoleExistsAsync("Employer");
                    if (!checkRole)
                    {
                        var role = new IdentityRole();
                        role.Name = "Employer";
                        await _roleManager.CreateAsync(role);

                        await _userManager.AddToRoleAsync(user, "Employer");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "Employer");
                    }

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token = token }, Request.Scheme);

                    _logger.Log(LogLevel.Warning, confirmationLink);

                    if (_signManager.IsSignedIn(User) && User.IsInRole("Employer"))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    await _emailService.SendAsync(user.Email, "Confirm Your Email", $"<a href=\"{confirmationLink}\">Confirm Email Link</a>", true);
                    return View("EmailVerification");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View();
        }

        [HttpGet]
        [Route("employee/register")]
        public IActionResult EmployeeRegister()
        {
            return View();
        }

        [HttpPost]
        [Route("employee/register")]
        public async Task<IActionResult> EmployeeRegister(IFormFile upload,
            [Bind("FirstName", "LastName", "Email", "Password", "ConfirmPassword","ImagePath", "PhoneNumber","Gender","Description")]
            EmployeeRegisterViewModel model)
        {
            if (upload != null && upload.Length > 0)
            {
                var fileName = Path.GetFileName(upload.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                var updatedFilePath = filePath.Substring(filePath.IndexOf("/img"));

                model.ImagePath = updatedFilePath;

                using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileSrteam);
                }
            }


            var email = _context.Users.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());
            Random rnd = new Random();
            string randomNumber = rnd.Next(1, 100000).ToString();

            if (email != null)
            {
                ModelState.AddModelError("Email", "Adresa email ekziston. Ju lutem përdorni një adresë tjetër!");
            }

            if (ModelState.IsValid && model != null)
            {
                string usr = model.FirstName + randomNumber;
                usr = usr.Replace(" ", "");

                var user = new User
                {
                    UserName = usr,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Description = model.Description,
                    PhoneNumber = model.PhoneNumber,
                    ImagePath = model.ImagePath
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    bool checkRole = await _roleManager.RoleExistsAsync("Employee");
                    if (!checkRole)
                    {
                        var role = new IdentityRole();
                        role.Name = "Employee";
                        await _roleManager.CreateAsync(role);

                        await _userManager.AddToRoleAsync(user, "Employee");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "Employee");
                    }

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token = token }, Request.Scheme);

                    _logger.Log(LogLevel.Warning, confirmationLink);

                    if (_signManager.IsSignedIn(User) && User.IsInRole("Employee"))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    await _emailService.SendAsync(user.Email, "Confirm Your Email", $"<a href=\"{confirmationLink}\">Confirm Email Link</a>", true);
                    return View("EmailVerification");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View();
        }

        [AllowAnonymous]
        [Route("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {userId} is invalid";
                return View("NotFound");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View();
            }

            ViewBag.ErrorTitle = "Email cannot be confirmed";
            return View("Error");
        }


        [HttpGet]
        [Route("login")]
        public IActionResult Login(string returnUrl = "")
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }

                var userName = user.UserName;

                var result = await _signManager.PasswordSignInAsync(userName,
                    model.Password, model.RememberMe, lockoutOnFailure: false);


                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        //[Authorize(Roles = "Employee")]
        [HttpGet]
        [Route("employee/edit-profile")]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            return View(user);
        }

        //[Authorize(Roles = "Employee")]
        [HttpPost]
        [Route("employee/update-profile")]
        public async Task<IActionResult> UpdateProfile(IFormFile upload, [FromForm] User model)
        {
            if (upload != null && upload.Length > 0 && upload.ContentType.Contains("image"))
            {
                var fileName = Path.GetFileName(upload.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                var updatedFilePath = filePath.Substring(filePath.IndexOf("/img"));

                model.ImagePath = updatedFilePath;

                using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileSrteam);
                }
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Gender = model.Gender;
                user.Description = model.Description;
                user.PhoneNumber = model.PhoneNumber;
                user.ImagePath = model.ImagePath;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToActionPermanent("EditProfile", "Account");
        }

        [HttpGet]
        [Route("change-password")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Route("change-password")]

        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // ChangePasswordAsync changes the user password
                var result = await _userManager.ChangePasswordAsync(user,
                    model.Password, model.NewPassword);
                // The new password did not meet the complexity rules or
                // the current password is incorrect. Add these errors to
                // the ModelState and rerender ChangePassword view
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                // Upon successfully changing the password refresh sign-in cookie
                await _signManager.RefreshSignInAsync(user);
                return View("ChangePasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        [Route("forgot-password")]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [Route("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await _userManager.FindByEmailAsync(model.Email);
                // If the user is found AND Email is confirmed
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Generate the reset password token
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    // Build the password reset link
                    var passwordResetLink = Url.Action("ResetPassword", "Account",
                            new { email = model.Email, token = token }, Request.Scheme);

                    // Log the password reset link
                    _logger.Log(LogLevel.Warning, passwordResetLink);
                    await _emailService.SendAsync(user.Email, "Reset Your Password", $"<a href=\"{passwordResetLink}\">Reset Password Link</a>", true);
                    // Send the user to Forgot Password Confirmation view
                    return View("ForgotPasswordConfirmation");
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        [Route("reset-password")]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            // If password reset token or email is null, most likely the
            // user tried to tamper the password reset link
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        [Route("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // reset the user password
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        return View("ResetPasswordConfirmation");
                    }
                    // Display validation errors. For example, password reset token already
                    // used to change the password or password complexity rules not met
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist
                return View("ResetPasswordConfirmation");
            }
            // Display validation errors if model state is not valid
            return View(model);
        }

    }
}