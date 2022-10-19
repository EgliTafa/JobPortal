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

        public AccountController(UserManager<User> userManager,
            SignInManager<User> signManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, ILogger<AccountController> logger, IWebHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
            _signManager = signManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
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
            [Bind("FirstName", "LastName", "Email", "Password", "ConfirmPassword", "ImagePath", "PhoneNumber")]
            EmployerRegisterViewModel model)
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

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.FirstName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    ImagePath = model.ImagePath
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                //IdentityResult roleResult = await _roleManager.CreateAsync(new IdentityRole("Employee"));

                if (_context.Users.Any(u => u.Email == user.Email))
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
            [Bind("FirstName", "LastName", "Email", "Password", "ConfirmPassword","ImagePath", "PhoneNumber")]
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

            if (ModelState.IsValid && model != null)
            {
                var user = new User
                {
                    UserName = model.FirstName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
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

                    //await _signManager.SignInAsync(user, false);
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View();
        }

        [HttpGet]
        [Route("login")]
        public IActionResult Login(string returnUrl = "")
        {
            var model = new LoginViewModel {ReturnUrl = returnUrl};
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
        public async Task<IActionResult> UpdateProfile(IFormFile upload,[FromForm] User model)
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

            //            _logger.LogError(model.Gender.ToString());
            var user = await _userManager.GetUserAsync(HttpContext.User);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Gender = model.Gender;
            user.PhoneNumber = model.PhoneNumber;
            user.ImagePath = model.ImagePath;
            //user.ProfilePicture = model.ProfilePicture;
            //user.ImageLocation = model.ImageLocation;
            
            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return RedirectToActionPermanent("EditProfile", "Account");
        }

        
    }
}