using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementMentorshipPortal.Models;

namespace PlacementMentorshipPortal.Controllers
{
    public partial class CoordinatorController
    {
        [Route("Coordinator/DeleteStudent/{id:int}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            // 1. Find the student record
            var student = await context.Studentsplaceds
                .Include(s => s.BidNavigation)
                .Include(s => s.CidNavigation)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student != null)
            {
                // 2. Remove from database
                await sls.AddLog("DELETE", "Studentsplaced", student.Sname + " of branch "+await sls.Bname(student.Bid) + " from record of " + await sls.Cname(student.Cid) + " company");
                context.Studentsplaceds.Remove(student);
                await context.SaveChangesAsync();
                TempData["Success"] = "Placement Student record deleted successfully.";

            }
            else
            {
                TempData["Error"] = "Record not found.";
            }

            // 3. Logic to redirect to the previous page
            string returnUrl = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Fallback if Referer is missing
            return RedirectToAction("Index", "Home");
        }

        [Route("Coordinator/DeleteRoundDetail/{id:int}")]
        public async Task<IActionResult> DeleteRoundDetail(int id)
        {
            // 1. Find the student record
            var r = await context.Rounddetails.Include(r1 => r1.CidNavigation).FirstOrDefaultAsync(r => r.Id == id);

            if (r != null)
            {
                // 2. Remove from database
                await sls.AddLog("DELETE", "Rounddetail", r.Dtext + " of " + await sls.Cname(r.Cid) + " company");

                context.Rounddetails.Remove(r);
                await context.SaveChangesAsync();
                TempData["Success"] = "Round Detail deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }

            // 3. Logic to redirect to the previous page
            string returnUrl = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Fallback if Referer is missing
            return RedirectToAction("Index", "Home");
        }

        [Route("Coordinator/DeleteDescription/{id:int}")]
        public async Task<IActionResult> DeleteDescription(int id)
        {
            // 1. Find the student record
            var r = await context.Descriptions.Include(r => r.CidNavigation).FirstOrDefaultAsync(r => r.Id == id);

            if (r != null)
            {
                // 2. Remove from database
                await sls.AddLog("DELETE", "Description", r.Dtext + " of " + await sls.Cname(r.Cid) + " company");


                context.Descriptions.Remove(r);
                await context.SaveChangesAsync();
                TempData["Success"] = "Experience deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }

            // 3. Logic to redirect to the previous page
            string returnUrl = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Fallback if Referer is missing
            return RedirectToAction("Index", "Home");
        }

        [Route("Coordinator/DeleteResource/{id:int}")]
        public async Task<IActionResult> DeleteResource(int id)
        {
            // 1. Find the student record
            var r = await context.Resources.Include(r=> r.BidNavigation).FirstOrDefaultAsync(r=>r.Id == id);

            if (r != null)
            {
                // 2. Remove from database
                await sls.AddLog("DELETE", "Resource", r.Rlink + " of branch " + await sls.Bname(r.Bid));

                context.Resources.Remove(r);
                await context.SaveChangesAsync();
                TempData["Success"] = "Resource deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }

            // 3. Logic to redirect to the previous page
            string returnUrl = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Fallback if Referer is missing
            return RedirectToAction("Index", "Home");
        }

        [Route("Coordinator/DeleteSession/{id:int}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            // 1. Find the student record
            var r = await context.Sessions.Include(r => r.BidNavigation).FirstOrDefaultAsync(r => r.Id == id);

            if (r != null)
            {
                // 2. Remove from database
                await sls.AddLog("DELETE", "Session", r.Link + " of " + await sls.Bname(r.Bid) + " Branch");

                context.Sessions.Remove(r);
                await context.SaveChangesAsync();
                TempData["Success"] = "Session Detail deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }

            // 3. Logic to redirect to the previous page
            string returnUrl = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Fallback if Referer is missing
            return RedirectToAction("Index", "Home");
        }
    }
}
