using Hangfire;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using PlacementMentorshipPortal.Models;
//for mail
using PlacementMentorshipPortal.Services;
//for pdf
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer; // Only if you are using the previewer
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PlacementMentorshipPortal.Controllers
{
    [Authorize(Roles = "Admin, Coordinator")]
    public partial class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly GmailEmailSender _emailSender;
        private readonly IConfiguration _config;
        private readonly SelectListService sls; //for Select List Sercices.
        private readonly IWebHostEnvironment env;


        public CoordinatorController(ApplicationDbContext context, GmailEmailSender emailSender, IConfiguration config, SelectListService sls, IWebHostEnvironment env)
        {
            this.context = context;
            _emailSender = emailSender;
            _config = config;
            this.sls = sls;
            this.env = env;
        }

        

        // GET: CoordinatorController
        //[Route("Coordinator")]
        //[Route("Coordinator/Index")]
        //[Authorize(Roles = "Coordinator")]
        //public ActionResult Index()
        //{
        //    return View();
        //}

        

        [Route("Coordinator/ViewSessions")]
        [Authorize(Roles = "Coordinator")]
        public IActionResult ViewSessions()
        {
            var data = context.Sessions.Include(s => s.TidNavigation).Include(s => s.BidNavigation).ToList();
            return View(data);
        }

        // Add/replace these members inside CoordinatorController

        [Route("Coordinator/SendMail")]

        public ActionResult SendMail()
        {
            ViewBag.BID = sls.BidList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Coordinator")]
        [Route("Coordinator/SendMailMethod")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMailMethod(SendMailViewModel model)
        {
            // 1. Validation and Setup
            if (!ModelState.IsValid)
            {
                ViewBag.BID = sls.BidList();
                return View("SendMail", model);
            }

            var branch = await context.Branches.FindAsync(model.Bid);
            if (branch == null) return NotFound();

            var studentEmails = await context.Students
                .Where(s => s.Bname.ToLower() == branch.Bname.ToLower() && !string.IsNullOrEmpty(s.Mail))
                .Select(s => s.Mail).Distinct().ToListAsync();

            if (!studentEmails.Any())
            {
                TempData["Error"] = "No students found for this branch.";
                return RedirectToAction(nameof(SendMail));
            }

            // 2. Serialize Attachments to DTOs
            var attachmentDtos = new List<AttachmentDto>();
            if (model.Attachments != null)
            {
                foreach (var file in model.Attachments)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    attachmentDtos.Add(new AttachmentDto
                    {
                        FileName = file.FileName,
                        Data = ms.ToArray(),
                        ContentType = file.ContentType
                    });
                }
            }

            // 3. ENQUEUE JOB: Hand off to Hangfire
            // This happens in the database, allowing the controller to return immediately
            BackgroundJob.Enqueue(() => SendBulkEmailJob(
                studentEmails,
                model.Subject ?? "Placement Update",
                model.Content ?? "",
                attachmentDtos,
                branch.Bname
            ));

            TempData["Success"] = $"Mail campaign for {branch.Bname} has been queued. You can safely close this page.";
            return RedirectToAction(nameof(SendMail));
        }

        // This method is executed by the Hangfire background worker
        [AutomaticRetry(Attempts = 3)]
        public async Task SendBulkEmailJob(List<string> studentEmails, string subject, string content, List<AttachmentDto> attachments, string branchName)
        {
            var fromName = _config["Smtp:FromName"] ?? "BVM Placement Portal";
            var fromEmail = _config["Smtp:FromEmail"] ?? _config["Smtp:Username"];

            foreach (var recipientEmail in studentEmails)
            {
                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(fromName, fromEmail));
                    message.To.Add(MailboxAddress.Parse(recipientEmail));
                    message.Subject = subject;

                    var builder = new BodyBuilder { HtmlBody = content, TextBody = content };

                    // Re-attach files from the DTO for each message
                    foreach (var att in attachments)
                    {
                        builder.Attachments.Add(att.FileName, att.Data, ContentType.Parse(att.ContentType));
                    }

                    message.Body = builder.ToMessageBody();
                    await _emailSender.SendAsync(message);

                    // "Polite" delay to stay within Gmail SMTP limits
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    // Individual failures are caught so the whole loop doesn't crash
                    Console.WriteLine($"Background mailer error for {recipientEmail}: {ex.Message}");
                }
            }
        }

        // Data Transfer Object for persistent storage in Hangfire tables
        public class AttachmentDto
        {
            public string FileName { get; set; } = null!;
            public byte[] Data { get; set; } = null!;
            public string ContentType { get; set; } = null!;
        }

        [Route("Coordinator/Profile")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> Profile()
        {
            CoordinatorProfileViewModel obj = new CoordinatorProfileViewModel();

            int tidInt = sls.TpcId();

            obj.companies = await context.Companies.Where(c => c.Tid == tidInt).ToListAsync();

            obj.resources = await context.Resources.Where(r => r.Tid == tidInt).Include(r => r.BidNavigation).ToListAsync();

            obj.descriptions = await context.Descriptions.Where(d => d.Tid == tidInt).Include(d => d.CidNavigation).ToListAsync();

            obj.rounddetails = await context.Rounddetails.Where(rd => rd.Tid == tidInt).Include(rd => rd.CidNavigation).ToListAsync();

            obj.sessions = await context.Sessions.Where(s => s.Tid == tidInt).Include(s => s.BidNavigation).ToListAsync();

            obj.studentsplaced = await context.Studentsplaceds.Where(sp => sp.Tid == tidInt)
                .Include(sp => sp.BidNavigation).Include(sp=>sp.CidNavigation).Include(sp=>sp.YidNavigation).ToListAsync();

            return View(obj);
        }
        
    }
}
