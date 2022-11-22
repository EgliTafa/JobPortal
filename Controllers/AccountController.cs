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
            [Bind("FirstName", "LastName", "Email", "Password", "ConfirmPassword", "ImagePath", "PhoneNumber","Description","CompanyDescription")]
            EmployerRegisterViewModel model, Job job)
        {
            //Profile Picture
            if (upload != null && upload.Length > 0 && upload.ContentType.Contains("image"))
            {
                var fileName = Path.GetFileName(upload.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                var updatedFilePath = filePath.Substring(filePath.IndexOf("/"));

                model.ImagePath = updatedFilePath;
                //_context.Users.Add(model);
                //_context.SaveChanges();

                using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileSrteam);
                }
            }

            var email = _context.Users.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());

            if (email != null)
            {
                ModelState.AddModelError("Email", "Email Already Exist!");
            }

            var companyDescription = job.CompanyDescription;

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.FirstName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Description = model.Description,
                    PhoneNumber = model.PhoneNumber,
                    ImagePath = model.ImagePath
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                //IdentityResult roleResult = await _roleManager.CreateAsync(new IdentityRole("Employee"));

                if (!_context.Users.Any(u => u.Email == user.Email))
                {
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

                        //await _signManager.SignInAsync(user, false);
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
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

                var updatedFilePath = filePath.Substring(filePath.IndexOf("/"));

                model.ImagePath = updatedFilePath;
                //_context.Users.Add(model);
                //_context.SaveChanges();

                using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileSrteam);
                }
            }

            //model.User.ProfilePicture = photo.FileName;

            var email = _context.Users.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());

            if (email != null)
            {
                ModelState.AddModelError("Email", "Email Already Exist!");
            }

            if (ModelState.IsValid && model != null)
            {
                var user = new User
                {
                    UserName = model.FirstName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Description = model.Description,
                    PhoneNumber = model.PhoneNumber,
                    //ProfilePicture = model.User.ProfilePicture,
                    ImagePath = model.ImagePath
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                //IdentityResult roleResult = await _roleManager.CreateAsync(new IdentityRole("Employee"));

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

                    var emailConfirm = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var link = Url.Action(nameof(VerifyEmail), "Home", new { userId = user.Id, emailConfirm }, Request.Scheme, Request.Host.ToString());
                    await _emailService.SendAsync(user.Email, "email verify", $"<a href=\"{link}\">Verify Your Email</a>", true);
                    return RedirectToAction("EmailVerification" );
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View();
        }
        [HttpGet(Name = "VerifyEmail")]
        public async Task<IActionResult> VerifyEmail(string userId, string emailConfirm)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest();
            var result = await _userManager.ConfirmEmailAsync(user, emailConfirm);

            if (result.Succeeded)
            {
                user.EmailConfirmed = true;
                await _context.SaveChangesAsync();
                return View();
            }


            return BadRequest();
        }

        [HttpGet(Name = "EmailVerification")]
        public IActionResult EmailVerification() => View();

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

                var updatedFilePath = filePath.Substring(filePath.IndexOf("/"));

                model.ImagePath = updatedFilePath;
                //_context.Users.Add(model);
                //_context.SaveChanges();

                using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileSrteam);
                }
            }

            if (ModelState.IsValid)
            {
                //_logger.LogError(model.Gender.ToString());
                var user = await _userManager.GetUserAsync(HttpContext.User);
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Gender = model.Gender;
                user.Description = model.Description;
                user.PhoneNumber = model.PhoneNumber;
                user.ImagePath = model.ImagePath;
                //user.ProfilePicture = model.ProfilePicture;
                //user.ImageLocation = model.ImageLocation;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToActionPermanent("EditProfile", "Account");
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