using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlacementMentorshipPortal.Models;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace PlacementMentorshipPortal.Services
{
    public class SelectListService
    {
        private readonly ApplicationDbContext context;
        private readonly IHttpContextAccessor httpContextAccessor;

        public SelectListService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
        }

        //this function will return branch name.
        public async Task<string> Bname(int? id)
        {
            if (id == null) return "null";
            var obj = await context.Branches.FindAsync(id);
            if (obj == null) return "null";
            return obj.Bname;
        }

        //this function will return company name.
        public async Task<string> Cname(int? id)
        {
            if (id == null) return "null";
            var obj = await context.Companies.FindAsync(id);
            if (obj == null) return "null";
            return obj.Cname;
        }

        public async Task<int> Year(int? id)
        {
            if (id == null) return 0;
            var obj = await context.Years.FindAsync(id);
            if (obj == null) return 0;
            return obj.Year1;
        }

        public int TpcId()
        {
            var user = httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int id))
            {
                return id;
            }
            return -1;
        }
        public SelectList TidList()
        {
            var data = context.Coordinators.Where(c => c.Active == true).ToList();

            var tidlist = new SelectList(data, "Tid", "Tname");
            return tidlist;
        }

        public SelectList CidList()
        {
            var data = context.Companies.ToList();
            var cidlist = new SelectList(data, "Cid", "Cname");
            return cidlist;
        }

        public SelectList YidList()
        {
            var year = context.Years.OrderByDescending(c => c.Year1).ToList();

            var yearlist = new SelectList(year, "Yid", "Year1");
            return yearlist;
        }

        public SelectList BidList()
        {
            var branch = context.Branches.ToList();

            var branchlist = new SelectList(branch, "Bid", "Bname");

            return branchlist;
        }

        public async Task AddLog(string action, string category, string detail)
        {

            var log = new Audit
            {
                Tid = TpcId(), // Get from claims
                Action = action,
                Category = category,
                Detail = detail
                //Time = DateTime.Now
            };
            context.Audits.Add(log);
            await context.SaveChangesAsync();
        }
    }
}