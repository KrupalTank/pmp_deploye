using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using PlacementMentorshipPortal.Models;
using PlacementMentorshipPortal.Services;

using System.Security.Claims;

namespace PlacementMentorshipPortal.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IConfiguration config;
        private readonly GmailEmailSender emailsender;
        private readonly SelectListService sls;

        public AuthenticationController(ApplicationDbContext context, IConfiguration config, GmailEmailSender emailSender, SelectListService sls)
        {
            this.context = context;
            this.config = config;
            this.emailsender = emailSender;
            this.sls = sls;
        }

        [Route("Authentication/Login")]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {

                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                if (User.IsInRole("Coordinator"))
                {
                    // Note: Ensure you actually have a CoordinatorController with an Index action
                    return RedirectToAction("Index", "Coordinator");
                }
            }

            return View();
        }

        [HttpPost]
        [Route("Authentication/LoginMethod")]
        public async Task<IActionResult> LoginMethod(string username, string pwd)
        {
            string role = "";
            int userTid = 0;
            string displayName = "";

            // 1. Check for Hardcoded Admin
            if (username == config["Admin:UserName"] && pwd == config["Admin:PWD"])
            {
                role = "Admin";
                displayName = "System Administrator";
                // Fetch Admin's Tid from DB (assuming Admin is an inactive entry)
                var adminUser = await context.Coordinators.FirstOrDefaultAsync(c => c.Uid == username);
                userTid = adminUser?.Tid ?? 0;
            }
            else
            {
                // 2. Search for Coordinator in DB
                var user = await context.Coordinators
                    .FirstOrDefaultAsync(c => c.Uid == username && c.Pwd == pwd && c.Active == true);

                if (user != null)
                {
                    role = "Coordinator";
                    userTid = user.Tid;
                    displayName = user.Tname;
                }
            }

            if (!string.IsNullOrEmpty(role))
            {
                // 3. Create Claims (Storing Tid and Role)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, displayName),
                    new Claim(ClaimTypes.NameIdentifier, userTid.ToString()), // Stores Tid
                    new Claim(ClaimTypes.Role, role) // Stores Role
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                TempData["Success"] = $"Welcome, {displayName}!";

                if(role == "Coordinator")
                    return RedirectToAction("Index", "Coordinator");
                else if(role == "Admin")
                    return RedirectToAction("Index", "Admin");
            }

            TempData["Error"] = "Invalid Username or Password.";
            return RedirectToAction("Login");
        }

        [Route("Authentication/Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Route("Authentication/ForgotPassword")]
        public ActionResult ForgotPassword()
        {
            return View(); 
        }


        [HttpPost]
        [Route("Authentication/ForgotPasswordMethod")]
        public async Task<IActionResult> ForgotPasswordMethod(string email)
        {
            var coordinator = await context.Coordinators.FirstOrDefaultAsync(c => c.Contact == email);
            if (coordinator == null)
            {
                TempData["Error"] = "Account Not Found.";
                return View("ForgotPassword");
            }

            // Generate OTP
            string otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("ResetOTP", otp);
            HttpContext.Session.SetString("ResetEmail", email);

            // Prepare the MimeMessage for your existing SendAsync method
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Placement Portal", config["Smtp:Username"]));
            message.To.Add(new MailboxAddress(coordinator.Tname, email));
            message.Subject = "Your Password Reset OTP";
            message.Body = new TextPart("plain")
            {
                Text = $"Hello {coordinator.Tname},\n\nYour OTP for resetting your password is: {otp}\n\nThis code is valid for a single use."
            };

            try
            {
                // Call your existing method!
                await emailsender.SendAsync(message);
              
                return RedirectToAction("VerifyOtp");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Email service error. Please try again later.";
                // Log ex.Message for debugging
                return View("ForgotPassword");
            }
        }

        [HttpGet]
        [Route("Authentication/VerifyOtp")]
        public IActionResult VerifyOtp() => View();

        [HttpPost]
        [Route("Authentication/VerifyOtp")]
        public IActionResult VerifyOtp(string userOtp)
        {
            string sessionOtp = HttpContext.Session.GetString("ResetOTP");

            if (sessionOtp != null && sessionOtp == userOtp)
            {
                // Clear OTP so it can't be used again, but keep the Email for the next step
                HttpContext.Session.Remove("ResetOTP");
                return RedirectToAction("ResetPassword");
            }

            TempData["Error"] = "Invalid OTP. Please try again.";
            return View();
        }

        [HttpGet]
        [Route("Authentication/ResetPassword")]
       
        public IActionResult ResetPassword()
        {
            bool isForgotPasswordFlow = !string.IsNullOrEmpty(HttpContext.Session.GetString("ResetEmail"));
            bool isLoggedIdCoordinator = User.IsInRole("Coordinator");

            // If neither condition is met, they shouldn't be here
            if (!isForgotPasswordFlow && !isLoggedIdCoordinator)
            {
                return RedirectToAction("ForgetPassword");
            }

            return View();
        }

        [HttpPost]
        [Route("Authentication/ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = "";

            // Step 1: Determine which email to use
            if (User.IsInRole("Coordinator"))
            {
                // Get the logged-in Coordinator's Tid from claims
                // (Assuming you have a helper like TpcId() or can parse User.Claims)
                var tid = sls.TpcId();
                
                var loggedInUser = await context.Coordinators.FindAsync(tid);
                email = loggedInUser.Contact;
            }
            else
            {
                // Fallback to the OTP session email
                email = HttpContext.Session.GetString("ResetEmail");
            }

            // Step 2: Update the password in the database
            var coordinator = await context.Coordinators.FirstOrDefaultAsync(c => c.Contact == email);

            if (coordinator != null)
            {
                coordinator.Pwd = model.NewPassword;
                await context.SaveChangesAsync();

                // Security: Clear session and force a fresh login with the new password
                HttpContext.Session.Clear();
                TempData["Success"] = "Password updated! Please login with your new credentials.";
                await sls.AddLog("UPDATE", "Coordinator", $"Password reset for {coordinator.Tname} ({email})");
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login");
            }

            return RedirectToAction("ForgetPassword");
        }

    }
}
