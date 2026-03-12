using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementMentorshipPortal.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PlacementMentorshipPortal.Controllers
{
    public partial class AdminController
    {
        // GET: HomeController1
        [Route("Admin")]
        [Route("Admin/Index")]
        public async Task<ActionResult> Index()
        {
            // Fetch all StudentCounts for calculations
            var allIntakes = await context.StudentCounts
                .Include(sc => sc.YearNavigation)
                .Include(sc => sc.BranchNavigation)
                .ToListAsync();

            // 1) Placements vs Year (Total Placements per Year)
            var placementsByYear = await context.Studentsplaceds
                .GroupBy(sp => sp.Yid)
                .Select(g => new { Yid = g.Key, Count = g.Count() })
                .Join(context.Years, g => g.Yid, y => y.Yid, (g, y) => new { Year = y.Year1, Count = g.Count })
                .OrderBy(x => x.Year)
                .ToListAsync();

            var yearLabels = placementsByYear.Select(x => x.Year.ToString()).ToList();
            var yearData = placementsByYear.Select(x => x.Count).ToList();

            // Year-wise Percentages
            var yearPercentages = placementsByYear.Select(y => {
                var totalEligible = allIntakes.Where(i => i.YearNavigation?.Year1 == y.Year).Sum(i => i.Count);
                return totalEligible > 0 ? Math.Round((double)y.Count / totalEligible * 100, 2) : 0;
            }).ToList();

            // 2) Placements vs Branch (Total Placements per Branch across all years)
            var placementsByBranch = await context.Studentsplaceds
                .GroupBy(sp => sp.Bid)
                .Select(g => new { Bid = g.Key, Count = g.Count() })
                .Join(context.Branches, g => g.Bid, b => b.Bid, (g, b) => new { Branch = b.Bname, Count = g.Count })
                .OrderBy(x => x.Branch)
                .ToListAsync();

            var branchLabels = placementsByBranch.Select(x => x.Branch).ToList();
            var branchData = placementsByBranch.Select(x => x.Count).ToList();

            // Branch-wise Percentages
            var branchPercentages = placementsByBranch.Select(b => {
                var totalEligible = allIntakes.Where(i => i.BranchNavigation?.Bname == b.Branch).Sum(i => i.Count);
                return totalEligible > 0 ? Math.Round((double)b.Count / totalEligible * 100, 2) : 0;
            }).ToList();

            // 3) Placements in current year vs branch
            var currentYearEntity = await context.Years.FirstOrDefaultAsync(y => y.Year1 == DateTime.Now.Year);
            int currentYid;
            int currentYearNumber;

            if (currentYearEntity != null)
            {
                currentYid = currentYearEntity.Yid;
                currentYearNumber = currentYearEntity.Year1;
            }
            else
            {
                var lastYear = await context.Years.OrderByDescending(y => y.Year1).FirstOrDefaultAsync();
                currentYid = lastYear?.Yid ?? -1;
                currentYearNumber = lastYear?.Year1 ?? DateTime.Now.Year;
            }

            List<string> currentYearBranchLabels = new List<string>();
            List<int> currentYearBranchData = new List<int>();
            List<double> currentYearPercentages = new List<double>();

            if (currentYid != -1)
            {
                var placementsCurrentYearByBranch = await context.Studentsplaceds
                    .Where(sp => sp.Yid == currentYid)
                    .GroupBy(sp => sp.Bid)
                    .Select(g => new { Bid = g.Key, Count = g.Count() })
                    .Join(context.Branches, g => g.Bid, b => b.Bid, (g, b) => new { Branch = b.Bname, Count = g.Count })
                    .OrderBy(x => x.Branch)
                    .ToListAsync();

                currentYearBranchLabels = placementsCurrentYearByBranch.Select(x => x.Branch).ToList();
                currentYearBranchData = placementsCurrentYearByBranch.Select(x => x.Count).ToList();

                // Current Year Percentages
                currentYearPercentages = placementsCurrentYearByBranch.Select(cb => {
                    var intake = allIntakes.FirstOrDefault(i => i.BranchNavigation?.Bname == cb.Branch && i.YearNavigation?.Year1 == currentYearNumber);
                    return (intake != null && intake.Count > 0) ? Math.Round((double)cb.Count / intake.Count * 100, 2) : 0;
                }).ToList();
            }

            ViewBag.Branches = await context.Branches.OrderBy(b => b.Bname).ToListAsync();
            ViewBag.Years = await context.Years.OrderByDescending(y => y.Year1).ToListAsync();

            var recentLogs = await context.Audits.Include(a => a.TidNavigation)
                .OrderByDescending(a => a.Time)
                .Take(10)
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                YearLabels = yearLabels,
                YearData = yearData,
                YearPercentages = yearPercentages, // NEW
                BranchLabels = branchLabels,
                BranchData = branchData,
                BranchPercentages = branchPercentages, // NEW
                CurrentYear = currentYearNumber,
                CurrentYearBranchLabels = currentYearBranchLabels,
                CurrentYearBranchData = currentYearBranchData,
                CurrentYearBranchPercentages = currentYearPercentages, // NEW
                RecentActivities = recentLogs
            };

            return View(model);
        }

        [HttpGet]
        [Route("admin/getplacementsbybranch/{bid:int}")]
        public async Task<IActionResult> GetPlacementsByBranch(int bid)
        {
            var data = await context.Studentsplaceds
                .Where(sp => sp.Bid == bid)
                .GroupBy(sp => sp.Yid)
                .Select(g => new { Yid = g.Key, Count = g.Count() })
                .Join(context.Years, g => g.Yid, y => y.Yid, (g, y) => new { Year = y.Year1, Count = g.Count })
                .OrderBy(x => x.Year).ToListAsync();

            // Fetch intakes for this specific branch to calculate percentage per year
            var intakes = await context.StudentCounts.Where(sc => sc.Bid == bid).Include(sc => sc.YearNavigation).ToListAsync();

            var labels = data.Select(x => x.Year.ToString()).ToList();
            var counts = data.Select(x => x.Count).ToList();
            var percentages = data.Select(x => {
                var intake = intakes.FirstOrDefault(i => i.YearNavigation?.Year1 == x.Year);
                return (intake != null && intake.Count > 0) ? Math.Round((double)x.Count / intake.Count * 100, 2) : 0;
            }).ToList();

            return Json(new { labels, counts, percentages });
        }

        // Add this new JSON endpoint to the controller (place near other GET endpoints)
        [HttpGet]
        [Route("admin/getplacementsbyyear/{year:int}")]
        public async Task<IActionResult> GetPlacementsByYear(int year)
        {
            var yearEntity = await context.Years.FirstOrDefaultAsync(y => y.Year1 == year);
            if (yearEntity == null)
                return Json(new { labels = new string[0], counts = new int[0], percentages = new double[0] });

            // 1. Get placement counts per branch
            var data = await context.Studentsplaceds
                .Where(sp => sp.Yid == yearEntity.Yid)
                .GroupBy(sp => sp.Bid)
                .Select(g => new { Bid = g.Key, Count = g.Count() })
                .Join(context.Branches, g => g.Bid, b => b.Bid, (g, b) => new { Bid = b.Bid, Branch = b.Bname, Count = g.Count })
                .OrderBy(x => x.Branch)
                .ToListAsync();

            // 2. Get eligible student counts for this year
            var intakes = await context.StudentCounts.Where(sc => sc.Yid == yearEntity.Yid).ToListAsync();

            var labels = data.Select(x => x.Branch).ToList();
            var counts = data.Select(x => x.Count).ToList();

            // 3. Calculate percentages
            var percentages = data.Select(x => {
                var intake = intakes.FirstOrDefault(i => i.Bid == x.Bid);
                return (intake != null && intake.Count > 0) ? Math.Round((double)x.Count / intake.Count * 100, 2) : 0;
            }).ToList();

            return Json(new { labels, counts, percentages });
        }


        [HttpGet]
        [Route("admin/getpackagestats/{year:int?}")]
        public async Task<IActionResult> GetPackageStats(int? year)
        {
            var query = context.Studentsplaceds.AsQueryable();

            if (year.HasValue && year.Value > 0)
            {
                var yearEntity = await context.Years.FirstOrDefaultAsync(y => y.Year1 == year.Value);
                if (yearEntity != null)
                {
                    query = query.Where(sp => sp.Yid == yearEntity.Yid);
                }
            }

            var data = await query
                .GroupBy(sp => sp.Bid)
                .Select(g => new
                {
                    Bid = g.Key,
                    // Cast or handle nullable decimals here
                    Average = g.Average(sp => sp.Package),
                    Highest = g.Max(sp => sp.Package)
                })
                .Join(context.Branches,
                      g => g.Bid,
                      b => b.Bid,
                      (g, b) => new { Branch = b.Bname, g.Average, g.Highest })
                .OrderBy(x => x.Branch)
                .ToListAsync();

            return Json(new
            {
                labels = data.Select(x => x.Branch),
                // Use ?? 0 to convert decimal? to decimal before rounding
                average = data.Select(x => Math.Round(x.Average ?? 0, 2)),
                highest = data.Select(x => x.Highest ?? 0)
            });
        }

        [HttpGet]
        [Route("admin/getcompanyplacementstats/{year:int?}")]
        public async Task<IActionResult> GetCompanyPlacementStats(int? year)
        {
            if (year == null || year == 0)
            {
                // DEFAULT: Show all companies from the table and their total placements across all years
                var data = await context.Companies
                    .GroupJoin(context.Studentsplaceds,
                        c => c.Cid,
                        sp => sp.Cid,
                        (c, placements) => new { Company = c.Cname, Count = placements.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                return Json(new { labels = data.Select(x => x.Company), counts = data.Select(x => x.Count) });
            }
            else
            {
                // FILTERED: Show ONLY companies that recruited students in the selected year
                var yearEntity = await context.Years.FirstOrDefaultAsync(y => y.Year1 == year);
                if (yearEntity == null) return Json(new { labels = new string[0], counts = new int[0] });

                var data = await context.Studentsplaceds
                    .Where(sp => sp.Yid == yearEntity.Yid)
                    .GroupBy(sp => sp.Cid)
                    .Select(g => new { Cid = g.Key, Count = g.Count() })
                    .Join(context.Companies,
                          g => g.Cid,
                          c => c.Cid,
                          (g, c) => new { Company = c.Cname, g.Count })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                return Json(new { labels = data.Select(x => x.Company), counts = data.Select(x => x.Count) });
            }
        }


        [HttpGet]
        [Route("admin/getauditlogs")]
        public async Task<IActionResult> GetAuditLogs(DateTime? start, DateTime? end)
        {
            // 1. We must Include Navigation property to see the Coordinator's Name
            var query = context.Audits
                .Include(a => a.TidNavigation)
                .AsQueryable();

            if (start.HasValue && end.HasValue)
            {
                DateTime startDate = DateTime.SpecifyKind(start.Value.Date, DateTimeKind.Utc);
                DateTime endDate = DateTime.SpecifyKind(end.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(a => a.Time >= startDate && a.Time <= endDate);
            }
            else
            {
                query = query.OrderByDescending(a => a.Time).Take(10);
            }

            // 2. Select exactly what JavaScript needs
            var logs = await query
                .OrderByDescending(a => a.Time)
                .Select(a => new {
                    a.Action,
                    a.Detail,
                    // Fetch name from navigation property
                    DoneBy = a.TidNavigation != null ? a.TidNavigation.Tname : "System",
                    // Pre-format the time string
                    DisplayTime = a.Time.HasValue ? a.Time.Value.ToString("MMM dd, HH:mm") : "Time Unknown"
                })
                .ToListAsync();

            return Json(logs);
        }

        //fo report generation.


        // Inside partial class AdminController
        [HttpGet]
        [Route("admin/generateannualreport/{year:int}")]
        public async Task<IActionResult> GenerateAnnualReport(int year, bool summary, bool growth, bool branch, bool corp, bool details)
        {
            // 1. Fetch Year with full Navigation
            var yearEntity = await context.Years
                .Include(y => y.Studentsplaceds).ThenInclude(sp => sp.BidNavigation)
                .Include(y => y.Studentsplaceds).ThenInclude(sp => sp.CidNavigation)
                .FirstOrDefaultAsync(y => y.Year1 == year);

            if (yearEntity == null || !yearEntity.Studentsplaceds.Any())
            {
                TempData["Error"] = "No placement data found for the selected year.";
                return RedirectToAction("Index");
            }

            // 2. Prepare Data and Calculations
            var students = yearEntity.Studentsplaceds.ToList();
            var totalPlaced = students.Count;
            var avgPackage = students.Average(s => s.Package ?? 0);
            var maxPackage = students.Max(s => s.Package ?? 0);
            var allIntakes = await context.StudentCounts.Where(sc => sc.Yid == yearEntity.Yid).ToListAsync();

            // 3. Generate PDF
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);

                    // --- HEADER (ALWAYS INCLUDED) ---
                    page.Header().Column(headerCol =>
                    {
                        headerCol.Item().ShowOnce().Row(row =>
                        {
                            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Logo", "bvm.png");
                            if (System.IO.File.Exists(logoPath)) row.ConstantItem(80).Image(logoPath);

                            row.RelativeItem().PaddingLeft(10).Column(col =>
                            {
                                col.Item().Text("Birla Vishvakarma Mahavidyalaya").FontSize(20).SemiBold().FontColor(Colors.Indigo.Medium);
                                col.Item().Text("An Autonomous Institution").FontSize(10).SemiBold().FontColor(Colors.Red.Medium);
                                col.Item().Text("Managed by Charutar Vidya mandal, Affiliated With GTU.").FontSize(10).FontColor(Colors.Red.Medium);
                                col.Item().Text($"Annual Placement Report {year}").FontSize(16).SemiBold();
                            });
                        });
                        headerCol.Item().SkipOnce().Row(row => {
                            row.RelativeItem().BorderBottom(1).PaddingBottom(5).Row(r => {
                                r.RelativeItem().Text("BVM Engineering College").FontSize(10).SemiBold();
                                r.RelativeItem().AlignRight().Text($"Report {year}").FontSize(10);
                            });
                        });
                    });


                    // ---------------- CONTENT ----------------
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        // ================= Executive Summary =================
                        if (summary)
                        {
                            col.Item().Text("Executive Summary")
                                .FontSize(16).SemiBold().FontColor(Colors.Indigo.Darken2);

                            col.Item().PaddingTop(10).Row(row =>
                            {
                                void KPI(string title, string value)
                                {
                                    row.RelativeItem().Background(Colors.Grey.Lighten4)
                                        .Padding(12).CornerRadius(8)
                                        .Column(k =>
                                        {
                                            k.Item().Text(title).FontSize(10).FontColor(Colors.Grey.Darken1);
                                            k.Item().Text(value).FontSize(16).SemiBold().FontColor(Colors.Indigo.Medium);
                                        });
                                }

                                KPI("Total Students Placed", totalPlaced.ToString());
                                KPI("Average Package", $"{avgPackage:N2} LPA");
                                KPI("Highest Package", $"{maxPackage:N2} LPA");
                            });
                        }

                        // ================= Growth Section =================
                        if (growth)
                        {
                            var prevYear = context.Years.Include(y => y.Studentsplaceds)
                                .FirstOrDefault(y => y.Year1 == year - 1);

                            if (prevYear != null && prevYear.Studentsplaceds.Any())
                            {
                                var prevPlaced = prevYear.Studentsplaceds.Count;
                                var growthRate = ((double)(totalPlaced - prevPlaced) / prevPlaced) * 100;

                                col.Item().PaddingTop(20).Background(Colors.Indigo.Lighten5)
                                    .Padding(12).CornerRadius(6)
                                    .Text(t =>
                                    {
                                        t.Span("Year-over-Year Performance: ").SemiBold();
                                        t.Span($"Placement change compared to {year - 1}: ");
                                        t.Span($"{growthRate:F1}%")
                                            .SemiBold()
                                            .FontColor(growthRate >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                                    });
                            }
                        }

                        // ================= Branch Analysis =================
                        if (branch)
                        {
                            col.Item().PaddingTop(25)
                                .Text("Branch-wise Placement Efficiency")
                                .FontSize(14).SemiBold();

                            col.Item().PaddingTop(8).Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(3);
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                });

                                table.Header(h =>
                                {
                                    h.Cell().Element(HeaderStyle).Text("Branch");
                                    h.Cell().Element(HeaderStyle).Text("Eligible");
                                    h.Cell().Element(HeaderStyle).Text("Placed");
                                    h.Cell().Element(HeaderStyle).Text("Ratio %");

                                    static IContainer HeaderStyle(IContainer container) =>
                                        container.Background(Colors.Indigo.Lighten4)
                                            .Padding(6)
                                            .DefaultTextStyle(x => x.SemiBold());
                                });

                                int rowIndex = 0;

                                foreach (var bCount in allIntakes)
                                {
                                    var placed = students.Count(s => s.Bid == bCount.Bid);
                                    var ratio = bCount.Count > 0 ? (double)placed / bCount.Count * 100 : 0;

                                    var bg = rowIndex++ % 2 == 0
                                        ? Colors.White
                                        : Colors.Grey.Lighten5;

                                    table.Cell().Background(bg).Padding(5).Text(bCount.BranchNavigation?.Bname ?? "N/A");
                                    table.Cell().Background(bg).Padding(5).Text(bCount.Count.ToString());
                                    table.Cell().Background(bg).Padding(5).Text(placed.ToString());
                                    table.Cell().Background(bg).Padding(5).Text($"{ratio:F1}%").SemiBold();
                                }
                            });
                        }

                        // POINT 4: Corporate Insights (Modern Table-based Grid)
                        if (corp)
                        {
                            col.Item().PaddingTop(20).Text("Our Recruiting Partners").FontSize(14).SemiBold();

                            var recruitingCompanies = students
                                .Select(s => s.CidNavigation)
                                .Where(c => c != null)
                                .DistinctBy(c => c.Cid)
                                .OrderBy(c => c.Cname)
                                .ToList();

                            col.Item().PaddingTop(10).Table(table =>
                            {
                                // Define 6 equal columns to replace the deprecated Grid(6)
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                foreach (var company in recruitingCompanies)
                                {
                                    table.Cell().Padding(5).Column(c =>
                                    {
                                        var logoFileName = company.Logo ?? "default_company.png";
                                        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Logo", logoFileName);

                                        if (System.IO.File.Exists(logoPath))
                                        {
                                            // Using FitArea() to respect aspect ratio
                                            c.Item().Height(40).AlignCenter().Image(logoPath).FitArea();
                                        }
                                        else
                                        {
                                            c.Item().Height(40).AlignCenter().AlignMiddle().Text(company.Cname).FontSize(8).Italic().FontColor(Colors.Grey.Medium);
                                        }

                                        c.Item().AlignCenter().Text(company.Cname).FontSize(7);
                                    });
                                }
                            });
                        }


                        // ================= Detailed Students =================
                        if (details)
                        {
                            foreach (var group in students.GroupBy(s => s.BidNavigation?.Bname))
                            {
                                col.Item().PaddingTop(25)
                                    .Text($"{group.Key} - Placed Students")
                                    .FontSize(12).SemiBold();

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(c =>
                                    {
                                        c.ConstantColumn(25);
                                        c.RelativeColumn(3);
                                        c.RelativeColumn(4);
                                        c.RelativeColumn(2);
                                        c.RelativeColumn(1);
                                    });

                                    table.Header(h =>
                                    {
                                        h.Cell().Element(HeaderStyle).Text("#");
                                        h.Cell().Element(HeaderStyle).Text("Student");
                                        h.Cell().Element(HeaderStyle).Text("Email");
                                        h.Cell().Element(HeaderStyle).Text("Company");
                                        h.Cell().Element(HeaderStyle).Text("LPA");

                                        static IContainer HeaderStyle(IContainer container) =>
                                            container.Background(Colors.Indigo.Lighten4)
                                                .Padding(6)
                                                .DefaultTextStyle(x => x.SemiBold());

                                      
                                    });

                                    int i = 1;
                                    int r = 0;

                                    foreach (var s in group)
                                    {
                                        var bg = r++ % 2 == 0
                                            ? Colors.White
                                            : Colors.Grey.Lighten5;

                                        table.Cell().Background(bg).Padding(4).Text(i++.ToString());
                                        table.Cell().Background(bg).Padding(4).Text(s.Sname);
                                        table.Cell().Background(bg).Padding(4).Text(s.Contact ?? "N/A");
                                        table.Cell().Background(bg).Padding(4).Text(s.CidNavigation?.Cname ?? "N/A");
                                        table.Cell().Background(bg).Padding(4).Text(s.Package?.ToString("0.00") ?? "0.00");
                                    }
                                });
                            }
                        }
                    });

                    // ---------------- FOOTER ----------------
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken1));

                        text.Span("Birla Vishvakarma Mahavidyalaya | ");
                        text.Span($"Generated on {DateTime.Now:dd MMM yyyy} | Page ");
                        text.CurrentPageNumber();
                    });
                });
            });

            return File(document.GeneratePdf(), "application/pdf", $"BVM_Placement_Report_{year}.pdf");
        }

        // Helper Style
        static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1);

    }
}