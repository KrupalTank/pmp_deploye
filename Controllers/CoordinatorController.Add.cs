using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlacementMentorshipPortal.Models;

namespace PlacementMentorshipPortal.Controllers
{
    public partial class CoordinatorController // Must be partial!
    {
        [Route("Coordinator/AddCompany")]
        [Authorize(Roles = "Coordinator")]
        public ActionResult AddCompany()
        {
            //ViewBag.TID = sls.TidList();
            return View();
        }

        [HttpPost]
        [Route("Coordinator/AddCompanyMethod")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> AddCompanyMethod(CompanyWithImage obj)
        {
            obj.Tid = sls.TpcId();


            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Model I Not Valid.";
                return RedirectToAction(nameof(AddCompany));
            }

            var ext = Path.GetExtension(obj.LogoPath.FileName);
            var size = obj.LogoPath.Length;

            if (obj.LogoPath == null)
            {
                TempData["Error"] = "Please Provide Logo Of Company.";
                return View("AddCompany", obj);

            }
            else if (!(ext.Equals(".png") || ext.Equals(".jpg") || ext.Equals(".jpeg")))
            {
                TempData["Error"] = "Please Upload Logo From Following Types :-\nPNG, JPG, JPEG.";
                return View("AddCompany", obj);

            }
            else if (size > 1000000)
            {
                TempData["Error"] = "Upload Logo Of At Most 1MB";
                return View("AddCompany", obj);
            }

            string filename = "", folder = "", filepath = "";
            folder = Path.Combine(env.WebRootPath, "Logo");
            filename = Guid.NewGuid().ToString() + "_" + obj.LogoPath.FileName;
            filepath = Path.Combine(folder, filename);
            obj.LogoPath.CopyTo(new FileStream(filepath, FileMode.Create));

            Company c = new Company()
            {
                Tid = obj.Tid,
                Cname = obj.Cname,
                Logo = filename
            };

            await context.Companies.AddAsync(c);
            await context.SaveChangesAsync();
            TempData["Success"] = "Company Added Successfully.";

            await sls.AddLog("ADD", "Company", obj.Cname + " is Added.");

            return RedirectToAction(nameof(AddCompany));
        }



        // GET: CoordinatorController/Details/5
        [Route("Coordinator/AddRoundDetails")]
        [Authorize(Roles = "Coordinator")]
        public ActionResult AddRoundDetails()
        {
            //ViewBag.TID = sls.TidList();
            ViewBag.CID = sls.CidList();
            return View();
        }

        [HttpPost]
        [Route("Coordinator/AddRoundDetailsMethod")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> AddRoundDetailsMethod(Rounddetail obj)
        {
            obj.Tid = sls.TpcId();
            if (ModelState.IsValid)
            {
                await context.Rounddetails.AddAsync(obj);
                await context.SaveChangesAsync();
                TempData["Success"] = "Details Of Placement Rounds Added Successfully.";

                await sls.AddLog("ADD", "Rounddetail", $"detail : {obj.Dtext} for Company : {await sls.Cname(obj.Cid)}");
                return RedirectToAction(nameof(AddRoundDetails));

            }
            else
            {
                TempData["Error"] = "Details Of Placement Rounds Dose Not Added.";

            }
            ViewBag.CID = sls.CidList();
            return View("AddRoundDetails", obj);

        }

        [Route("Coordinator/AddStudents")]
        [Authorize(Roles = "Coordinator")]
        public ActionResult AddStudents()
        {
            //ViewBag.TID = sls.TidList();
            ViewBag.CID = sls.CidList();
            ViewBag.YID = sls.YidList();
            ViewBag.BID = sls.BidList();
            return View();
        }

        [HttpPost]
        [Route("Coordinator/AddStudentsMethod")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> AddStudentsMethod(Studentsplaced obj)
        {
            obj.Tid = sls.TpcId();
            if (ModelState.IsValid)
            {
                await context.Studentsplaceds.AddAsync(obj);
                await context.SaveChangesAsync();
                TempData["Success"] = "Details Of Placed Student Added Successfully.";

                await sls.AddLog("ADD", "Studentsplaced", obj.Sname + " of branch " + await sls.Bname(obj.Bid) + " of year " + await sls.Year(obj.Yid) + " for company " + await sls.Cname(obj.Cid));
                return RedirectToAction(nameof(AddStudents));
            }
            else
            {
                TempData["Error"] = "Student Is Not Added.";
            }
            ViewBag.CID = sls.CidList();
            ViewBag.YID = sls.YidList();
            ViewBag.BID = sls.BidList();
            return View("AddStudents", obj);
        }

        [Route("Coordinator/AddDescription")]
        [Authorize(Roles = "Coordinator")]
        public ActionResult AddDescription()
        {
            //ViewBag.TID = sls.TidList();
            ViewBag.CID = sls.CidList();
            return View();
        }

        [HttpPost]
        [Route("Coordinator/AddDescriptionMethod")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> AddDescriptionMethod(Description obj)
        {
            obj.Tid = sls.TpcId();
            if (ModelState.IsValid)
            {
                await context.Descriptions.AddAsync(obj);
                await context.SaveChangesAsync();
                TempData["Success"] = "Experience Shared By Student About Company Added Successfully.";

                await sls.AddLog("ADD", "Description", "text : " + obj.Dtext + " for company : " + await sls.Cname(obj.Cid));
                return RedirectToAction(nameof(AddDescription));
            }
            ViewBag.CID = sls.CidList();
            return View("AddDescription", obj);
        }

        [Route("Coordinator/AddResources")]
        [Authorize(Roles = "Coordinator")]
        public ActionResult AddResources()
        {
            //ViewBag.TID = sls.TidList();
            ViewBag.BID = sls.BidList();
            return View();
        }

        [HttpPost]
        [Route("Coordinator/AddResourcesMethod")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> AddResourcesMethod(Resource obj)
        {
            obj.Tid = sls.TpcId();
            if (ModelState.IsValid)
            {
                await context.Resources.AddAsync(obj);
                await context.SaveChangesAsync();
                TempData["Success"] = "Resources Shared By Student For Placement Preparation Added Successfully.";

                await sls.AddLog("ADD", "Resource", "detail : "+obj.Details + " link : "+obj.Rlink + " for branch : " + await sls.Bname(obj.Bid));
                return RedirectToAction(nameof(AddResources));
            }
            ViewBag.BID = sls.BidList();
            return View("AddResources", obj);
        }

        [Route("Coordinator/AddSession")]
        [Authorize(Roles = "Coordinator")]
        public ActionResult AddSession()
        {
            //ViewBag.TID = sls.TidList();
            ViewBag.BID = sls.BidList();
            return View();
        }

        [HttpPost]
        [Route("Coordinator/AddSessionMethod")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> AddSessionMethod(Session obj)
        {
            obj.Tid = sls.TpcId();
            if (ModelState.IsValid)
            {
                // Generate a purely random room name using a GUID (no input from coordinator)
                var roomName = Guid.NewGuid().ToString("N");
                var baseUrl = "https://meet.jit.si";
                var meetingUrl = $"{baseUrl}/{roomName}";

                // Set the generated meeting link on the Session entity
                obj.Link = meetingUrl;

                await context.Sessions.AddAsync(obj);
                await context.SaveChangesAsync();

                TempData["Success"] = "Session Details Added Successfully.";
                // Store the generated link so it can be shown after redirect
                TempData["GeneratedLink"] = meetingUrl;

                await sls.AddLog("ADD", "Session", "for branch : " + await sls.Bname(obj.Bid) + " link : "+obj.Link);
                return RedirectToAction(nameof(AddSession));
            }
            else
            {
                TempData["Error"] = "Session Is Not Added.";
            }
            ViewBag.BID = sls.BidList();
            return View("AddSession", obj);

        }
    }
}