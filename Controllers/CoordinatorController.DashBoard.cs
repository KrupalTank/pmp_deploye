using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementMentorshipPortal.Models;

//for pdf
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer; // Only if you are using the previewer

namespace PlacementMentorshipPortal.Controllers
{
    public partial class CoordinatorController
    {
        // Update your existing Index action to provide Year lists
        [Route("Coordinator")]
        [Route("Coordinator/Index")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> Index()
        {
            // Fetch years for the filter dropdown
            ViewBag.Years = await context.Years.OrderByDescending(y => y.Year1).ToListAsync();
            return View();
        }

        // AJAX Endpoint: Returns branch-specific student placements
        [HttpGet]
        [Route("Coordinator/GetBranchPlacements")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> GetBranchPlacements(int yid)
        {
            // Retrieve the Coordinator's Branch ID via the SelectListService
            int coordinatorTid = sls.TpcId();
            int? coordinatorBid = -1;
            var c = await context.Coordinators.FindAsync(coordinatorTid);
            if (c != null)
            {
                coordinatorBid = c.Bid;
            }

            var students = await context.Studentsplaceds
                .Include(s => s.CidNavigation) // For Company Names
                .Where(s => s.Bid == coordinatorBid && s.Yid == yid)
                .OrderByDescending(s => s.Package)
                .Select(s => new {
                    sname = s.Sname,
                    contact = s.Contact ?? "N/A",
                    company = s.CidNavigation.Cname,
                    package = s.Package.HasValue ? s.Package.Value.ToString("0.00") : "0.00"
                })
                .ToListAsync();

            return Json(students);
        }

        [HttpGet]
        [Route("Coordinator/GenerateBranchReport")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> GenerateBranchReport(int yearId, bool placedStudents, bool resources, bool includeRounds, int? experienceYears)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            int coordinatorTid = sls.TpcId();
            var coord = await context.Coordinators.Include(c => c.BidNavigation).FirstOrDefaultAsync(c => c.Tid == coordinatorTid);
            if (coord == null) return NotFound();
            int bid = coord.Bid ?? 0;

            var selectedYear = await context.Years.FindAsync(yearId);
            if (selectedYear == null) return NotFound();

            // 1. Fetch ALL Students placed in this branch for this year
            var students = await context.Studentsplaceds
                .Include(s => s.CidNavigation)
                .Where(s => s.Bid == bid && s.Yid == yearId)
                .OrderByDescending(s => s.Package).ToListAsync();

            // 2. Fetch Resources
            var branchResources = resources ? await context.Resources
                .Where(r => r.Bid == bid).ToListAsync() : new List<Resource>();

            // 3. IDENTIFY ALL RECRUITING COMPANIES
            // Get unique company IDs from the students placed this year
            var recruitingCompanyIds = students.Select(s => s.Cid).Distinct().ToList();

            var companyInsights = new List<CompanyInsightViewModel>();
            int startYearValue = selectedYear.Year1 - (experienceYears.Value - 1);

            foreach (var cid in recruitingCompanyIds)
            {
                var company = await context.Companies.FindAsync(cid);
                if (company == null) continue;

                // Fetch Rounds if requested
                var rounds = includeRounds ? await context.Rounddetails.FirstOrDefaultAsync(r => r.Cid == cid) : null;

                // Fetch Experiences from the lookback period for THIS branch
                var exps = await context.Descriptions
                    .Where(d => d.Cid == cid &&
                                d.Createdat.Value.Year >= startYearValue && d.Createdat.Value.Year <= selectedYear.Year1)
                    .Select(e => e.Dtext).ToListAsync();

                companyInsights.Add(new CompanyInsightViewModel
                {
                    CompanyName = company.Cname,
                    RoundDetail = rounds?.Dtext,
                    Experiences = exps
                });
            }

            // 4. PDF Generation
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Helvetica"));

                    // ================= HEADER =================

                    page.Header().Column(headerCol =>
                    {
                        headerCol.Item().ShowOnce().Row(row =>
                        {
                            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Logo", "bvm.png");
                            if (System.IO.File.Exists(logoPath)) row.ConstantItem(80).Image(logoPath);

                            row.RelativeItem().PaddingLeft(10).Column(col =>
                            {
                                col.Item().Text("Birla Vishvakarma Mahavidyalaya").FontSize(20).Bold().FontColor(Colors.Indigo.Darken2);
                                col.Item().Text("An Autonomous Institution").SemiBold().FontColor(Colors.Red.Medium);
                                col.Item().Text("Managed by Charutar Vidya mandal, Affiliated With GTU.").FontSize(9).FontColor(Colors.Grey.Darken1);
                            });
                        });
                        headerCol.Item().SkipOnce().Column(col =>
                        {
                            col.Item().Text("PLACEMENT PREPARATION GUIDE")
                                .FontSize(18).Bold().FontColor(Colors.Indigo.Medium);

                            col.Item().Text($"{coord.BidNavigation?.Bname} | Academic Year {selectedYear.Year1}")
                                .FontSize(12).FontColor(Colors.Grey.Darken2);
                        });
                    });
                    

                    // ================= CONTENT =================
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        // ---------- SECTION 1: Placed Students ----------
                        if (placedStudents && students.Any())
                        {
                            col.Item().Text("1. Placed Student Details")
                                .FontSize(14).Bold().FontColor(Colors.Blue.Medium);

                            col.Item().PaddingTop(8).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                });

                                // Header Row
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Indigo.Lighten4)
                                        .Padding(6).Text("Student Name").Bold();

                                    header.Cell().Background(Colors.Indigo.Lighten4)
                                        .Padding(6).Text("Company").Bold();

                                    header.Cell().Background(Colors.Indigo.Lighten4)
                                        .Padding(6).AlignRight().Text("Package (LPA)").Bold();
                                });

                                // Data Rows
                                foreach (var s in students)
                                {
                                    table.Cell().BorderBottom(0.5f)
                                        .Padding(6).Text(s.Sname);

                                    table.Cell().BorderBottom(0.5f)
                                        .Padding(6).Text(s.CidNavigation?.Cname);

                                    table.Cell().BorderBottom(0.5f)
                                        .Padding(6).AlignRight()
                                        .Text($"{s.Package} LPA")
                                        .SemiBold()
                                        .FontColor(Colors.Indigo.Medium);
                                }
                            });

                            col.Item().PaddingTop(15);
                        }

                        // ---------- SECTION 2: Resources ----------
                        if (resources && branchResources.Any())
                        {
                            col.Item().Text("2. Branch Preparation Resources")
                                .FontSize(14).Bold().FontColor(Colors.Blue.Medium);

                            foreach (var r in branchResources)
                            {
                                col.Item().PaddingTop(5).Row(row =>
                                {
                                    row.ConstantItem(10).Text("•").FontColor(Colors.Indigo.Medium);
                                    row.RelativeItem().Text($"{r.Details}")
                                        .SemiBold();
                                });

                                col.Item().PaddingLeft(15)
                                    .Text(r.Rlink)
                                    .FontSize(9)
                                    .FontColor(Colors.Blue.Medium)
                                    .Underline();
                            }

                            col.Item().PaddingTop(15);
                        }

                        // ---------- SECTION 3: Company Insights ----------
                        if (companyInsights.Any())
                        {
                            col.Item().Text("3. Recruiting Companies & Placement Insights")
                                .FontSize(15).Bold().FontColor(Colors.Blue.Medium);

                            foreach (var insight in companyInsights)
                            {
                                col.Item().PaddingTop(15).Column(company =>
                                {
                                    // Company Title Box
                                    company.Item().Background(Colors.Grey.Lighten3)
                                        .Padding(8)
                                        .Text(insight.CompanyName)
                                        .FontSize(13)
                                        .Bold()
                                        .FontColor(Colors.Indigo.Darken2);

                                    // Placement Procedure
                                    if (includeRounds)
                                    {
                                        company.Item().PaddingTop(6)
                                            .Text("Placement Procedure")
                                            .SemiBold()
                                            .FontColor(Colors.Grey.Darken2);

                                        if (!string.IsNullOrWhiteSpace(insight.RoundDetail))
                                        {
                                            var rounds = insight.RoundDetail
                                                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                                            foreach (var round in rounds)
                                            {
                                                company.Item().PaddingLeft(10)
                                                    .Text($"• {round.Trim()}")
                                                    .FontSize(10);
                                            }
                                        }
                                        else
                                        {
                                            company.Item().PaddingLeft(10)
                                                .Text("Not Available")
                                                .Italic()
                                                .FontSize(10)
                                                .FontColor(Colors.Grey.Medium);
                                        }
                                    }

                                    // Student Advice
                                    if (insight.Experiences.Any())
                                    {
                                        company.Item().PaddingTop(6)
                                            .Text("Student Advice")
                                            .SemiBold()
                                            .FontColor(Colors.Grey.Darken2);

                                        foreach (var exp in insight.Experiences)
                                        {
                                            var lines = exp
                                                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                                            foreach (var line in lines)
                                            {
                                                company.Item().PaddingLeft(10)
                                                    .Text($"• {line.Trim()}")
                                                    .FontSize(10);
                                            }
                                        }
                                    }
                                });
                            }
                        }
                    });

                    // ================= FOOTER =================
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().PaddingTop(5).AlignCenter().Text(text =>
                        {
                            var style = text.Span("").FontSize(9).FontColor(Colors.Grey.Darken1);

                            text.Span("Birla Vishvakarma Mahavidyalaya | Generated on ")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);

                            text.Span($"{DateTime.Now:dd MMM yyyy} | Page ")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);

                            text.CurrentPageNumber()
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });

            return File(document.GeneratePdf(), "application/pdf", $"Branch_Prep_Guide_{selectedYear.Year1}.pdf");
        }

        // Helper Model inside the same file 
        public class CompanyInsightViewModel
        {
            public string CompanyName { get; set; }
            public string RoundDetail { get; set; }
            public List<string> Experiences { get; set; }
        }
    }
}
