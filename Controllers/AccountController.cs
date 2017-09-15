using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GlobalEvent.Models;
using GlobalEvent.Models.AccountViewModels;
using GlobalEvent.Services;
using GlobalEvent.Data;
using GlobalEvent.Models.OwnerViewModels;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using GlobalEvent.Models.AdminViewModels;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager; 
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly string _externalCookieScheme;
        private readonly string _id;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<IdentityCookieOptions> identityCookieOptions,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ApplicationDbContext db,
            IHttpContextAccessor http)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _db = db;
            _id = _userManager.GetUserId(http.HttpContext.User);
        }

        //
        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                
                ApplicationUser currentUser = await _userManager.FindByEmailAsync(model.Email);
                
                if (result.Succeeded)
                {
                    await _db.Logs.AddAsync(await Log.New("Log In", "User Loged In", currentUser.Id, _db));
                    return RedirectToAction("Dashboard", "Admin");
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    // If user tried to log-in while being locked.
                    await _db.Logs.AddAsync(await Log.New("Log In", "Attempted to Log in", currentUser.Id, _db));
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            // prevents from registering if Owner exist
            if (_db.Users.Any(x => x.Level == "Owner"))
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // create a new user
                ApplicationUser owner = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, Level = model.Level };
                var result = await _userManager.CreateAsync(owner, model.Password);
                
                // LogIn
                if (result.Succeeded)
                {
                    
                    ApplicationUser registeredOwner = await _userManager.FindByEmailAsync(model.Email);
                
                    // Adding All Claims
                    List<PropertyInfo> properties = JackLib.PropertyAsObject(new Claims());
                    foreach(var item in properties)
                    {
                        await _userManager.AddClaimAsync(registeredOwner, new Claim(item.Name, ""));
                    }

                    // log for the owner
                    await _db.Logs.AddAsync(await Log.New("Register", $"Owner's initial registration", _id, _db));
                    
                    // sign in after adding claims
                    await _signInManager.SignInAsync(registeredOwner, isPersistent: false);
                    return RedirectToAction("Index", "Owner");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        //
        // GET: /Account/AddUser
        [HttpGet]
        [Authorize(Policy="Admin Creator")]
        public IActionResult AddUser(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new RegisterViewModel());
        }

        //
        // POST: /Account/Adduser
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy="Admin Creator")]
        public async Task<IActionResult> AddUser(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                ApplicationUser newUser = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, Level = model.Level };
                var result = await _userManager.CreateAsync(newUser, model.Password);
                
                if (result.Succeeded)
                {
                    // ==> LOG for creator
                    ApplicationUser creator = await _userManager.GetUserAsync(User);
                    await _db.Logs.AddAsync(await Log.New("Create User", $"{newUser.Level.ToUpper()}: {newUser.FirstName} {newUser.LastName}, was created", creator.Id, _db));
                    
                    // ==> LOG for created user
                    ApplicationUser created = await _userManager.FindByEmailAsync(newUser.Email);
                    await _db.Logs.AddAsync(await Log.New("Create User", $"{newUser.Level.ToUpper()} created by {creator.Level}: {creator.FirstName} {creator.LastName}", created.Id, _db));

                    return RedirectToAction("Admins", "Owner");
                }
                AddErrors(result);
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Policy="Admin Killer")]
        public async Task<IActionResult> DeleteUser(string ID = null)
        {
            if (ID == null)
            {
                ViewBag.Message = "User ID isn't correct. Couldn't delete the user.";
                return RedirectToAction("Admins", "Owner");
            }
            
            ApplicationUser user = await _userManager.FindByIdAsync(ID);
            return View(user);
        }
        
        [HttpGet]
        [Authorize(Policy="Admin Killer")]
        public async Task<IActionResult> DeleteUserOk(string ID = null)
        {
            if (ID != null)
            {                
                ApplicationUser userToDelete= await _userManager.FindByIdAsync(ID);
                
                // log for deleter
                ApplicationUser deleter = await _userManager.GetUserAsync(User);
                await _db.Logs.AddAsync(await Log.New("USER", $"{userToDelete.Level.ToUpper()}: {userToDelete.FirstName} {userToDelete.LastName}, was DELETED", deleter.Id, _db));
                
                // lof for deleted user
                await _db.Logs.AddAsync(await Log.New("USER", $"User was DELETED by {deleter.FirstName} {deleter.LastName}", userToDelete.Id, _db));

                await _userManager.DeleteAsync(userToDelete);
            }

            return RedirectToAction("Admins", "Owner");
        }

        [HttpGet]
        [Authorize(Policy="Admin Editor")]
        public async Task<IActionResult> EditAdmin (string ID = null)
        {
            if (ID == null) 
            {
                return RedirectToAction("Admins", "Owner");
            }

            // get user from db by ID
            ApplicationUser userToEdit = await _userManager.FindByIdAsync(ID);
            ViewBag.Claims = await _userManager.GetClaimsAsync(userToEdit);
            
            // create model for the view
            EditAdmin model = new EditAdmin();
            model.CopyValues(userToEdit);
            
            // all user logs
            ViewBag.Logs = await _db.Logs.Where(x => x.AdminID == userToEdit.Id).OrderByDescending(x => x.ID).Take(100).ToListAsync();
            
            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Admin Editor")]
        public async Task<IActionResult> EditAdmin (EditAdmin model)
        {
            if (!ModelState.IsValid) 
            {
                return RedirectToAction("Admins", "Owner");
            }

            // update user
            ApplicationUser userToUpdate = await _userManager.FindByIdAsync(model.Id);
            userToUpdate.FirstName = model.FirstName;
            userToUpdate.LastName = model.LastName;
            userToUpdate.Level = model.Level;
            userToUpdate.Email = model.Email;
            await _userManager.UpdateAsync(userToUpdate);

            // logs for updating user
            ApplicationUser updator = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(await Log.New("Edit User", $"{userToUpdate.Level.ToUpper()}: {userToUpdate.FirstName} {userToUpdate.LastName}, was EDITED", _id, _db));
            
            // log for updated user
            await _db.Logs.AddAsync(await Log.New("Edit User", $"User was EDITED by {updator.FirstName} {updator.LastName}", _id, _db));

            // change password if new one is provided
            if (!string.IsNullOrEmpty(model.Password)  
                && model.Password == model.ConfirmPassword)
            {
                await _userManager.RemovePasswordAsync(userToUpdate);
                await _userManager.AddPasswordAsync(userToUpdate, model.Password); 

                // log for updating user
                await _db.Logs.AddAsync(await Log.New("Edit User", $"{userToUpdate.Level.ToUpper()}: {userToUpdate.FirstName} {userToUpdate.LastName}, PASSWORD was changed", updator.Id, _db));
                
                // log for updated user
                await _db.Logs.AddAsync(await Log.New("Edit User", $"Password was changed by {updator.FirstName} {updator.LastName}", userToUpdate.Id, _db));
            }

            return RedirectToAction("Admins", "Owner");
        }

        [HttpGet]
        [Authorize(Policy = "Claims Editor")]
        public async Task<IActionResult> ChangeClaims (string ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Admins", "Owner");
            }

            // new full list of claims
            Claims claims = new Claims();
            
            // get properties for Claims
            List<PropertyInfo> properties = claims.GetType().GetProperties().ToList();
            ApplicationUser userToUpdate = await _userManager.FindByIdAsync(ID);
            IList<Claim> hasClaims = await _userManager.GetClaimsAsync(userToUpdate);
            
            // if claim exist change the value to TRUE
            foreach (var claim in hasClaims){
                foreach (var item in properties){
                    if (item.Name == claim.Type){
                        item.SetValue(claims, true);
                    }
                }
            }

            // sort properties alphabetically by Name
            ViewBag.Properties = properties.OrderBy(x => x.Name);
            ViewBag.AdminName = $"{userToUpdate.Level}: {userToUpdate.FirstName} {userToUpdate.LastName}";
            
            // user ID
            ViewBag.AID = userToUpdate.Id;
            
            return View(claims);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy = "Claims Editor")]
        public async Task<IActionResult> ChangeClaims (Claims claims, string ID = null)
        {
            if (claims == null || ID == null) 
            {
                return RedirectToAction("Admins", "Owner");
            }
            
            // get user by ID
            ApplicationUser userToUpdate = await _userManager.FindByIdAsync(ID);
            List<PropertyInfo> properties = claims.GetType().GetProperties().ToList();
            
            // remove claims before assigning new ones
            IList<Claim> hasClaims = await _userManager.GetClaimsAsync(userToUpdate);
            await _userManager.RemoveClaimsAsync(userToUpdate, hasClaims);
            
            // add new claims
            foreach(var item in properties)
            {
                if ((bool)item.GetValue(claims, null) == true)
                {
                    await _userManager.AddClaimAsync(userToUpdate, new Claim(item.Name, ""));
                }
            }

            // log for editor
            ApplicationUser editor = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(await Log.New("Edit User", $"{userToUpdate.Level.ToUpper()}: {userToUpdate.FirstName} {userToUpdate.LastName}, CLAIMS were changed", _id, _db));
            
            // log for edited
            await _db.Logs.AddAsync(await Log.New("Edit User", $"User's CLAIMS were changed by {editor.FirstName} {editor.LastName}", userToUpdate.Id, _db));

            return RedirectToAction("EditAdmin", "Account", new {ID = ID});
        }

        //
        // GET: /Account/LOCK
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LockUser (string ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Admins", "Owner");
            }
            
            ApplicationUser userToLock = await _userManager.FindByIdAsync(ID);
            ApplicationUser locker = await _userManager.GetUserAsync(User);
            
            if (userToLock.LockoutEnd > DateTime.Now)
            {
                // unlock user if locked
                userToLock.LockoutEnd = DateTime.Now.AddDays(-1);
                
                // log for locker
                await _db.Logs.AddAsync(await Log.New("Lock User", $"{userToLock.Level.ToUpper()}: {userToLock.FirstName} {userToLock.LastName}, was UNLOCKED", _id, _db));
                
                // Log for locked user
                await _db.Logs.AddAsync(await Log.New("Edit User", $"User was UNLOCKED by {locker.FirstName} {locker.LastName}", userToLock.Id, _db));
            }
            else
            {
                // lock user
                userToLock.LockoutEnd = DateTime.Now.AddYears(10);
                
                // log for locker
                await _db.Logs.AddAsync(await Log.New("Lock User", $"{userToLock.Level.ToUpper()}: {userToLock.FirstName} {userToLock.LastName}, was LOCKED and Logged Out", _id, _db));
                
                // Log for locked user
                await _db.Logs.AddAsync(await Log.New("Edit User", $"User was LOCKED by {locker.FirstName} {locker.LastName}", userToLock.Id, _db));
                
                // log out immediately
                await _userManager.UpdateSecurityStampAsync(userToLock);
            }
            await _userManager.UpdateAsync(userToLock);

            return RedirectToAction("Admins", "Owner");
        }

        //
        // POST: /Account/LockAll
        [HttpPost]
        [Authorize(Policy="Is Owner")]
        public async Task<IActionResult> LockAll (string lockall)
        {
            // check if proper string-code was submitted
            if (lockall != "lockall")
            {
                return RedirectToAction("Admins", "Owner");
            }
            
            ApplicationUser locker = await _userManager.GetUserAsync(User);
            
            // list of all users
            List<ApplicationUser> users = await _db.Users.ToListAsync();
            
            foreach (var user in users)
            {
                // lock all users but Owner
                if (user.Level != "Owner")
                {
                    user.LockoutEnd = DateTime.Now.AddYears(10);
                    
                    // lock immediately 
                    await _userManager.UpdateSecurityStampAsync(user);
                    await _userManager.UpdateAsync(user);

                    // log for locked user
                    await _db.Logs.AddAsync(await Log.New("Edit User", $"User was LOCKED by {locker.FirstName} {locker.LastName}", user.Id, _db));
                }
            }
            
            // log for locker
            await _db.Logs.AddAsync(await Log.New("Lock User", $"All users were LOCKED and Logged Out", _id, _db));

            return RedirectToAction("Admins", "Owner");
        }

        //
        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // log for logging out user
            await _db.Logs.AddAsync(await Log.New("Log Out", "User Loged Out", _id, _db));
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        //
        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                //var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                //var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                //await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                //   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                //return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/SendCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            // Generate the token and send it
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                return View("Error");
            }

            var message = "Your security code is: " + code;
            if (model.SelectedProvider == "Email")
            {
                await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
            }
            else if (model.SelectedProvider == "Phone")
            {
                await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
            }

            return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/VerifyCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }
        }

        //
        // GET /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
