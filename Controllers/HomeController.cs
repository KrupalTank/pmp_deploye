using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlacementMentorshipPortal.Models;
using PlacementMentorshipPortal.Services;

namespace PlacementMentorshipPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ApplicationDbContext context;
        private readonly SelectListService sls; //for Select List Sercices.


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, SelectListService sls) { 
            this.logger = logger;
            this.context = context;
            this.sls = sls;
        }
        
        [Route("")]
        [Route("Home")]
        [Route("Home/Index")]
        [HttpGet]
        public IActionResult Index(string? SelectedBranch, string? SelectedYear)
        {
            ViewData["Branch"] = sls.BidList();
            ViewData["Year"] = sls.YidList();
            if (SelectedBranch == null && SelectedYear == null)
            {
                var data = context.Companies.ToList();
                return View(data);
            }
            int bid, yid;
            try
            {
                bid = Convert.ToInt32(SelectedBranch);
                yid = Convert.ToInt32(SelectedYear);
            }
            catch (Exception ex)
            {
                bid = yid = 0;
            }
            if (bid == 0 && yid == 0)
            {
                var data = context.Companies.ToList();
                return View(data);
            }
            else if (bid != 0 && yid != 0)
            {
                var uniquecid = context.Studentsplaceds.Where(s => s.Bid == bid && s.Yid == yid).Select(s => s.Cid).Distinct().ToList();
                var data = context.Companies.Where(c => uniquecid.Contains(c.Cid)).ToList();
                return View(data);
            }
            else if (bid != 0 && yid == 0)
            {
                var uniquecid = context.Studentsplaceds.Where(s => s.Bid == bid).Select(s => s.Cid).Distinct().ToList();
                var data = context.Companies.Where(c => uniquecid.Contains(c.Cid)).ToList();
                return View(data);
            }
            else
            {
                var uniquecid = context.Studentsplaceds.Where(s => s.Yid == yid).Select(s => s.Cid).Distinct().ToList();
                var data = context.Companies.Where(c => uniquecid.Contains(c.Cid)).ToList();
                return View(data);
            }

        }


        [Route("Home/Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }


        [Route("Home/Coordinators")]
        public IActionResult Coordinators()
        {
            var data = context.Coordinators.Where(c => c.Active == true).Include(c => c.BidNavigation).Include(c => c.YidNavigation).OrderBy(c => c.Bid).ToList();
            return View(data);
        }

        [Route("Home/Resources")]
        [HttpGet]
        public IActionResult Resources(string? SelectedBranch)
        {
            ViewData["Branch"] = sls.BidList();
            if(SelectedBranch == null || Convert.ToInt32(SelectedBranch) == 0)
            {
                var data = context.Resources.Include(r => r.BidNavigation).ToList();
                return View(data);
            }
            else
            {
                int bid = Convert.ToInt32(SelectedBranch);
                var data = context.Resources.Where(r => r.Bid == bid).Include(r => r.BidNavigation).ToList();
                return View(data);

            }
        }

        [Route("Home/CompanyProfile/{id}")]
        public async Task<IActionResult> CompanyProfile(int id)
        {
            CompanyProfileViewModel model = new CompanyProfileViewModel();
            model.company = await context.Companies.FirstOrDefaultAsync(c => c.Cid == id);
            if (model.company == null)
            {
                return NotFound();
            }

            model.studentsplaced = await context.Studentsplaceds.Where(s => s.Cid == id).Include(s => s.BidNavigation).Include(s => s.YidNavigation).ToListAsync();
            model.rounddetail = context.Rounddetails.FirstOrDefault(c => c.Cid == id);
            model.experience = await context.Descriptions.Where(d => d.Cid == id).ToListAsync();
            return View(model);
        }


        [Route("Home/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // GET: Home/Statistics
        [Route("Home/Statistics")]
        public async Task<IActionResult> Statistics()
        {
            // Fetch branches for the dropdown filter
            ViewBag.Branches = await context.Branches.OrderBy(b => b.Bname).ToListAsync();

            // Fetch years for the summary
            ViewBag.Years = await context.Years.OrderByDescending(y => y.Year1).ToListAsync();

            return View();
        }

        // AJAX Endpoint: Returns branch-specific trends
        // Endpoint for Chart 1: Branch Trends (Historical)
        [HttpGet]
        [Route("Home/GetBranchStats/{bid:int}")]
        public async Task<IActionResult> GetBranchStats(int bid)
        {
            var placementData = await context.Studentsplaceds
                .Where(sp => sp.Bid == bid)
                .GroupBy(sp => sp.YidNavigation.Year1)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .OrderBy(x => x.Year)
                .ToListAsync();

            var intakeData = await context.StudentCounts
                .Where(sc => sc.Bid == bid)
                .Include(sc => sc.YearNavigation)
                .ToListAsync();

            var labels = placementData.Select(x => x.Year.ToString()).ToList();
            var counts = placementData.Select(x => x.Count).ToList();
            var percentages = placementData.Select(x => {
                var intake = intakeData.FirstOrDefault(i => i.YearNavigation.Year1 == x.Year);
                return (intake != null && intake.Count > 0) ? Math.Round((double)x.Count / intake.Count * 100, 2) : 0;
            }).ToList();

            return Json(new { labels, counts, percentages });
        }

        // Endpoint for Chart 2: Company Breakdown (Specific Year/Branch)
        [HttpGet]
        [Route("Home/GetCompanyBreakdown")]
        public async Task<IActionResult> GetCompanyBreakdown(int bid, int yid)
        {
            // Filter placements by both Branch and Year
            var baseQuery = context.Studentsplaceds.Where(sp => sp.Bid == bid && sp.Yid == yid);

            // 1. Calculate Summary Stats for the Cards
            var totalPlaced = await baseQuery.CountAsync();

            // Safety check: if no one is placed, return zeroed data immediately
            if (totalPlaced == 0)
            {
                return Json(new { totalPlaced = 0, avgPackage = 0, maxPackage = 0, companyData = new List<object>() });
            }

            var avgPackage = await baseQuery.AverageAsync(sp => sp.Package ?? 0);
            var maxPackage = await baseQuery.MaxAsync(sp => sp.Package ?? 0);

            // 2. Get Company Breakdown for the Chart
            var companyData = await baseQuery
                .GroupBy(sp => sp.CidNavigation.Cname)
                .Select(g => new
                {
                    CompanyName = g.Key,
                    Count = g.Count(),
                    AvgPackage = Math.Round(g.Average(sp => sp.Package) ?? 0, 2)
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // 3. Return the exact object the JavaScript is looking for [cite: 90, 92, 93]
            return Json(new
            {
                totalPlaced,
                avgPackage = Math.Round(avgPackage, 2),
                maxPackage = Math.Round(maxPackage, 2),
                companyData
            });
        }
    }
}
