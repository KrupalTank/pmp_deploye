using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlacementMentorshipPortal.Models;
using PlacementMentorshipPortal.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace PlacementMentorshipPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class AdminController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly SelectListService sls; //for Select List Sercices.

        public AdminController(ApplicationDbContext _context, SelectListService selectListService)
        {
            context = _context;
            sls = selectListService;
        }

        // GET: admin/addstudentcount
        [Route("admin/addstudentcount")]
        public IActionResult AddStudentCount()
        {
            // Provide dropdown data via SelectListService
            ViewBag.Bid = sls.BidList();
            ViewBag.Yid = sls.YidList();
            return View();
        }

        // POST: admin/addstudentcountmethod
        [HttpPost]
        [Route("admin/addstudentcountmethod")]
        public async Task<IActionResult> AddStudentCountMethod(StudentCount obj)
        {
            if (ModelState.IsValid)
            {
                // Check if a count already exists for this specific Year and Branch
                var existing = await context.StudentCounts
                    .AnyAsync(sc => sc.Yid == obj.Yid && sc.Bid == obj.Bid);

                if (existing)
                {
                    TempData["Error"] = "A student count for this branch and year already exists. Please edit the existing record instead.";
                    ViewBag.Bid = sls.BidList();
                    ViewBag.Yid = sls.YidList();
                    return View("AddStudentCount", obj);
                }

                await context.StudentCounts.AddAsync(obj);
                await context.SaveChangesAsync();

                TempData["Success"] = "Student count added successfully.";
                await sls.AddLog("ADD", "StudentCount", $"Intake for Branch {await sls.Bname(obj.Bid)} Year {await sls.Year(obj.Yid)} set to {obj.Count}");

                return RedirectToAction("AddStudentCount");
            }

            ViewBag.Bid = sls.BidList();
            ViewBag.Yid = sls.YidList();
            return View("AddStudentCount", obj);
        }

        // GET: admin/editstudentcount/5
        [Route("admin/editstudentcount/{id:int}")]
        public async Task<IActionResult> EditStudentCount(int id)
        {
            var sc = await context.StudentCounts.FindAsync(id);
            if (sc == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("Profile");
            }

            ViewBag.Bid = sls.BidList();
            ViewBag.Yid = sls.YidList();
            return View(sc);
        }

        // POST: admin/editstudentcountmethod
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin/editstudentcountmethod")]
        public async Task<IActionResult> EditStudentCountMethod(StudentCount obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verify no other record (besides this one) has the same Yid/Bid
                    var duplicate = await context.StudentCounts
                        .AnyAsync(sc => sc.Id != obj.Id && sc.Yid == obj.Yid && sc.Bid == obj.Bid);

                    if (duplicate)
                    {
                        TempData["Error"] = "Another record already exists for this branch and year.";
                        ViewBag.Bid = sls.BidList();
                        ViewBag.Yid = sls.YidList();
                        return View("EditStudentCount", obj);
                    }

                    context.StudentCounts.Update(obj);
                    await context.SaveChangesAsync();

                    TempData["Success"] = "Student count updated successfully.";
                    await sls.AddLog("UPDATE", "StudentCount", $"Intake for Branch {await sls.Bname(obj.Bid)} Year {await sls.Year(obj.Yid)} updated to {obj.Count}");

                    return RedirectToAction("Profile");
                }
                catch (DbUpdateConcurrencyException)
                {
                    TempData["Error"] = "The record was modified by another user. Please try again.";
                }
            }

            ViewBag.Bid = sls.BidList();
            ViewBag.Yid = sls.YidList();
            return View("EditStudentCount", obj);
        }

        [Route("Admin/AddYear")]
        public  ActionResult AddYear()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/addyearmethod")]
        public async Task<IActionResult> AddYearMethod(Year obj)
        {
            if (ModelState.IsValid)
            {
                bool exists = await context.Years.AnyAsync(y => y.Year1 == obj.Year1);
                if (exists)
                {
                    TempData["Error"] = "This Year already exists in the system.";
                    return View("AddYear", obj);
                }

                await context.Years.AddAsync(obj);
                await context.SaveChangesAsync();
                TempData["Success"] = "Year Added Successfully.";

                await sls.AddLog("ADD", "Year", "year : " + obj.Year1);

                return RedirectToAction("AddYear");
            }
            else
            {
                TempData["Error"] = "Year Is Not Added.";
            }
            return View("AddYear", obj);
        }
            
       

        [Route("admin/addbranch")]
        public ActionResult AddBranch()
        { 
            return View();
        }

        [HttpPost]
        [Route("admin/addbranchmethod")]
        public async Task<IActionResult> AddBranchMethod(Branch obj)
        {
            if (ModelState.IsValid)
            {
                bool exists = await context.Branches.AnyAsync(b => b.Bname.ToLower() == obj.Bname.ToLower());
                if (exists)
                {
                    TempData["Error"] = "This Branch name already exists.";
                    return RedirectToAction("AddBranch");
                }
               
                await context.Branches.AddAsync(obj);
                await context.SaveChangesAsync();
                TempData["Success"] = "Branch Added Successfully.";
                await sls.AddLog("ADD", "Branch", "Branch : " + obj.Bname);

                return RedirectToAction("AddBranch");

            }
            else
            {
                TempData["Error"] = "Branch Is Not Added.";
            }
            return View("AddBranch", obj);
        }

        [Route("admin/deactivatecoordinators")]
        //public async Task<IActionResult> DeactivateCoordinators()
        //{
        //    // Find coordinators where Active == true
        //    var activeCoordinators = await context.Coordinators
        //        .Where(c => c.Active == true)
        //        .ToListAsync();

        //    if (activeCoordinators.Count > 0)
        //    {
        //        foreach (var coord in activeCoordinators)
        //        {
        //            coord.Active = false;
        //        }

        //        await context.SaveChangesAsync();
        //        TempData["Success"] = $"{activeCoordinators.Count} coordinator(s) are deactivated.";
        //    }
        //    else
        //    {
        //        TempData["Info"] = "No active coordinators found.";
        //    }

        //    return RedirectToAction("Index");
        //}

        public async Task<IActionResult> DeactivateCoordinators()
        {
            var model = await context.Coordinators
            .Where(c => c.Active == true)
            .Select(c => new DeactivateCoordinatorsViewModel
            {
                Tid = c.Tid,
                Tname = c.Tname,
                BranchName = c.BidNavigation.Bname,
                IsSelected = false
            }).ToListAsync();

            return View(model);
        }

        [HttpPost]
        [Route("admin/DeactivateCoordinatorsMethod")]
        public async Task<IActionResult> DeactivateCoordinatorsMethod(List<DeactivateCoordinatorsViewModel> coordinators)
        {
            // Filter only those that were checked
            var selectedIds = coordinators.Where(x => x.IsSelected).Select(x => x.Tid).ToList();

            if (selectedIds.Any())
            {
                var toDeactivate = await context.Coordinators
                    .Where(c => selectedIds.Contains(c.Tid))
                    .ToListAsync();

                foreach (var coord in toDeactivate)
                {
                    coord.Active = false;
                }

                await context.SaveChangesAsync();
                TempData["Success"] = $"{toDeactivate.Count} coordinators deactivated successfully.";
                await sls.AddLog("DEACTIVATE", "Coordinator", $"{toDeactivate.Count} coordinators deactivated.");

            }

            return RedirectToAction(nameof(Index));
        }


        [Route("admin/addcoordinator")]
        public ActionResult AddCoordinator()
        {
            ViewBag.BID = sls.BidList();
            ViewBag.YID = sls.YidList();

            return View();
        }


        [Route("admin/addcoordinatormethod")]
        public async Task<IActionResult> AddCoordinatorMethod( Coordinator obj)
        {
            ViewBag.BID = sls.BidList();
            ViewBag.YID = sls.YidList();
            if (ModelState.IsValid)
            {
               
                bool emailExists = await context.Coordinators.AnyAsync(c => c.Contact == obj.Contact);
                if (emailExists)
                {
                    TempData["Error"] = "A Coordinator with this email/UID already exists.";
                    return View("AddCoordinator", obj);
                }

                bool credentialsTaken = await context.Coordinators.AnyAsync(c => c.Uid == obj.Uid );
                if (credentialsTaken)
                {
                    TempData["Error"] = "This UID and Password combination is already in use.";
                    return View("AddCoordinator", obj);
                }

                await context.Coordinators.AddAsync(obj);
                await context.SaveChangesAsync();
                TempData["Success"] = "Coordinator Added Successfully.";
                await sls.AddLog("ADD", "Coordinator", obj.Tname + " is added of branch " + await sls.Bname(obj.Bid));

                return RedirectToAction("AddCoordinator");
            }
            else
            {
                TempData["Error"] = "Coordinator Is Not Added.";
            }
            return View("AddCoordinator", obj);
        }

        [Route("admin/addstudents")]
        public ActionResult AddStudents()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin/addstudentsmethod")]
        public async Task<IActionResult> AddStudentsMethod(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file (.xlsx or .xls).";
                return RedirectToAction(nameof(AddStudents));
            }

            var added = new List<Student>();

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                ms.Position = 0;

                using var workbook = new XLWorkbook(ms);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    TempData["Error"] = "Uploaded file contains no worksheets.";
                    return RedirectToAction(nameof(AddStudents));
                }

                // Read rows. Skip header row if present.
                var rows = worksheet.RangeUsed()?.RowsUsed().ToList();
                if (rows.Count == 0)
                {
                    TempData["Error"] = "No data found in the worksheet.";
                    return RedirectToAction(nameof(AddStudents));
                }

                // Heuristic: if first row contains text like 'mail' or 'email' treat it as header and skip
                var startIndex = 0;
                var firstRowFirstCell = rows[0].Cell(1).GetString().Trim().ToLowerInvariant();
                if (firstRowFirstCell.ToLower().Contains("mail") || firstRowFirstCell.ToLower().Contains("email"))
                    startIndex = 1;

                for (int i = startIndex; i < rows.Count; i++)
                {
                    var row = rows[i];
                    var mail = row.Cell(1).GetString().Trim().ToLower();
                    var bname = row.Cell(2).GetString().Trim();
                    var entryCell = row.Cell(3);

                    if (string.IsNullOrWhiteSpace(mail) || !mail.Contains("@")) continue;
                    if (string.IsNullOrWhiteSpace(bname)) continue;

                    int entryyear = 0;
                    if (!int.TryParse(entryCell.GetString().Trim(), out entryyear))
                    {
                        if (entryCell.DataType == XLDataType.Number)
                            entryyear = (int)entryCell.GetDouble();
                    }

                    // Skip duplicates by email
                    var exists = await context.Students.AnyAsync(s => s.Mail == mail);
                    if (exists) continue;

                    added.Add(new Student { Mail = mail, Bname = bname, Entryyear = entryyear });
                }
            }

            if (added.Count > 0)
            {
                await context.Students.AddRangeAsync(added);
                await context.SaveChangesAsync();
            }

            TempData["Success"] = $"Queued import complete: {added.Count} new student(s) added.";
            await sls.AddLog("ADD", "Student", $"{added.Count} new student(s) added.");

            return RedirectToAction(nameof(AddStudents));
        }

        [Route("admin/removestudents")]
        public ActionResult RemoveStudents()
        {
            var year = context.Years.ToList();

            ViewBag.YID = new SelectList(year, "Year1", "Year1");
            return View();
        }

        [HttpPost]
        [Route("admin/removestudentsmethod")]
        public async Task<IActionResult> RemoveStudentsMethod(string SelectedYear)
        {
            if (string.IsNullOrEmpty(SelectedYear))
            {
                TempData["Error"] = "Please select a valid option.";
                return RedirectToAction(nameof(RemoveStudents));
            }
            if (SelectedYear.ToLower() == "all")
            {
                var allStudents = await context.Students.ToListAsync();

                if (allStudents.Any())
                {
                    context.Students.RemoveRange(allStudents);
                    await context.SaveChangesAsync();
                    TempData["Success"] = $"{allStudents.Count} total students have been successfully removed.";
                    await sls.AddLog("DELETE", "Student", $"{allStudents.Count} total students have been successfully removed.");

                }
            }
            else
            {
                try
                {
                    int y = Convert.ToInt32(SelectedYear);
                    var studentsByYear = await context.Students.Where(s => s.Entryyear == y).ToListAsync();

                    if (studentsByYear.Any())
                    {
                        context.Students.RemoveRange(studentsByYear);
                        await context.SaveChangesAsync();
                        TempData["Success"] = $"Successfully removed {studentsByYear.Count} students from the entry year {SelectedYear}.";
                        await sls.AddLog("DELETE", "Student", $"removed {studentsByYear.Count} students from the entry year {SelectedYear}.");

                    }
                    else
                    {
                        TempData["Error"] = $"No students found for the year {SelectedYear}.";
                    }

                }
                catch (Exception ex) { TempData["Error"] = "An error occurred while removing students: " + ex.Message; }
            }
            return RedirectToAction(nameof(RemoveStudents));

        }

        [Route("admin/profile")]
        public async Task<IActionResult> Profile()
        {
            AdminProfileViewModel obj = new AdminProfileViewModel();
            obj.branches = await context.Branches.ToListAsync();
            obj.years = await context.Years.OrderByDescending(y => y.Year1).ToListAsync();
            obj.coordinators = await context.Coordinators
                .Where(c => c.Active == true)
                .Include(c => c.BidNavigation)
                .ToListAsync();

            // Default: Show current year counts
            int currentYear = DateTime.Now.Year;
            obj.studentCounts = await context.StudentCounts
                .Include(sc => sc.BranchNavigation)
                .Include(sc => sc.YearNavigation)
                .Where(sc => sc.YearNavigation.Year1 == currentYear)
                .ToListAsync();

            return View(obj);
        }
        //for dynamic intake display based on year selection in profile page.
        [HttpGet]
        [Route("admin/getintakebyyear/{year:int}")]
        public async Task<IActionResult> GetIntakeByYear(int year)
        {
            var data = await context.StudentCounts
                .Include(sc => sc.BranchNavigation)
                .Include(sc => sc.YearNavigation)
                .Where(sc => sc.YearNavigation.Year1 == year)
                .Select(sc => new {
                    sc.Id,
                    BranchName = sc.BranchNavigation.Bname,
                    sc.Count
                })
                .ToListAsync();

            return Json(data);
        }

        //all update methods....

        [Route("admin/editbranch/{id}")]
        public async Task<IActionResult> EditBranch(int id)
        {
            var branch = await context.Branches.FirstOrDefaultAsync(b => b.Bid == id);

            if (branch == null)
            {
                TempData["Error"] = "Branch not found.";
                return RedirectToAction(nameof(Profile));
            }

            return View(branch);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/EditBranchMethod")]
        public async Task<IActionResult> EditBranchMethod(Branch obj)
        {
            if (obj == null)
            {
                TempData["Error"] = "Invalid branch data.";
                return RedirectToAction(nameof(Profile)); // Or wherever your list is
            }

            if (ModelState.IsValid)
            { 
                try
                {
                    var exist = await context.Branches.AnyAsync(b => b.Bid != obj.Bid && b.Bname.ToLower() == obj.Bname.ToLower());
                    if (exist)
                    {
                        TempData["Error"] = "Branch name already exists.";
                        return View("EditBranch", obj);
                    }

                    context.Branches.Update(obj);
                    await context.SaveChangesAsync();
                    TempData["Success"] = "Branch Updated Successfully.";
                    await sls.AddLog("UPDATE", "Branch", "Branch : " + obj.Bname + " is updated.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    TempData["Error"] = "The record was modified by another user. Please try again.";
                }
            }
            else
            {
                TempData["Error"] = "Model data is invalid.";
            }

            return RedirectToAction(nameof(Profile));
        }

        [Route("admin/edityear/{id}")]
        public async Task<IActionResult> EditYear(int id)
        {

            var year = await context.Years.FirstOrDefaultAsync(b => b.Yid == id);

            if (year == null)
            {
                TempData["Error"] = "Branch not found.";
                return RedirectToAction(nameof(Profile));
            }

            return View(year);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/EditYearMethod")]
        public async Task<IActionResult> EditYearMethod(Year obj)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    bool exists = await context.Years.AnyAsync(y => y.Year1 == obj.Year1);
                    if (exists)
                    {
                        TempData["Error"] = "This Year already exists in the system.";
                        return View("EditYear", obj);
                    }

                    context.Years.Update(obj);
                    await context.SaveChangesAsync();
                    TempData["Success"] = "Year Updated Successfully.";
                    await sls.AddLog("UPDATE", "Year", "Year : " + obj.Year1 + " is updated.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    TempData["Error"] = "The record was modified by another user. Please try again.";
                }
            }
            else
            {
                TempData["Error"] = "Model data is invalid. Update year within range.";
            }

            return RedirectToAction(nameof(Profile));
        }

        [Route("admin/editcoordinator/{id:int}")]
        public async Task<IActionResult> EditCoordinator(int id)
        {
            var c = await context.Coordinators.FirstOrDefaultAsync(b => b.Tid == id);

            if (c == null)
            {
                TempData["Error"] = "Coordinator not found.";
                return RedirectToAction(nameof(Profile));
            }
            ViewBag.Bid = sls.BidList();
            ViewBag.Yid = sls.YidList();
            return View(c);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/EditCoordinatorMethod")]
        public async Task<IActionResult> EditCoordinatorMethod(Coordinator obj)
        {
            ViewBag.Bid = sls.BidList();
            ViewBag.Yid = sls.YidList();
            if (ModelState.IsValid)
            {
                try
                {
                    var c = await context.Coordinators.FirstOrDefaultAsync(c => c.Tid != obj.Tid && c.Contact == obj.Contact);
                    if (c != null)
                    {
                        TempData["Error"] = "Email already exist";
                        return View("EditCoordinator", obj);
                    }
                    context.Coordinators.Update(obj);
                    await context.SaveChangesAsync();
                    TempData["Success"] = "Coordinator Updated Successfully.";
                    await sls.AddLog("UPDATE", "Coordinator", $"Coordinator : {obj.Tname} {obj.Contact} updated.");
                    
                    return RedirectToAction(nameof(Profile));
                }
                catch (DbUpdateConcurrencyException)
                {
                    TempData["Error"] = "The record was modified by another user. Please try again.";
                }
            }
            else
            {
                TempData["Error"] = "Model data is invalid.";
            }
            
            return View("EditCoordinator", obj);
        }

    }
}
