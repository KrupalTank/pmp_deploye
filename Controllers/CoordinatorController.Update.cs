using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementMentorshipPortal.Models;

namespace PlacementMentorshipPortal.Controllers
{
    public partial class CoordinatorController
    {

        [Route("Coordinator/EditCompany/{id:int}")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> EditCompany(int id)
        {
            var obj = await context.Companies.Where(c => c.Cid == id).FirstOrDefaultAsync();
            CompanyWithImage c = new CompanyWithImage()
            {
                Tid = obj.Tid,
                Cid = obj.Cid,
                Cname = obj.Cname,
                Logo = obj.Logo
            };
            return View(c);
        }

        [Route("Coordinator/EditCompanyMethod")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> EditCompanyMethod(CompanyWithImage model)
        {
            model.Tid = sls.TpcId();
            var companyInDb = await context.Companies.FindAsync(model.Cid);
            if (companyInDb == null) return NotFound();

            // 2. Handle the Logo Logic
            if (model.LogoPath != null) // If user uploaded a NEW file
            {
                var ext = Path.GetExtension(model.LogoPath.FileName);
                var size = model.LogoPath.Length;
                if (!(ext.Equals(".png") || ext.Equals(".jpg") || ext.Equals(".jpeg")))
                {
                    TempData["Error"] = "Please Upload Logo From Following Types :-\nPNG, JPG, JPEG.";
                    return RedirectToAction(nameof(Profile));
                }
                else if (size > 1000000)
                {
                    TempData["Error"] = "Upload Logo Of At Most 1MB";
                    return RedirectToAction(nameof(Profile));
                }

                string filename = "", folder = "", filepath = "";
                folder = Path.Combine(env.WebRootPath, "Logo");
                filename = Guid.NewGuid().ToString() + "_" + model.LogoPath.FileName;
                filepath = Path.Combine(folder, filename);

                string olderpath = Path.Combine(folder, companyInDb.Logo);
                if (System.IO.File.Exists(olderpath))
                {
                    System.IO.File.Delete(olderpath);
                }

                await model.LogoPath.CopyToAsync(new FileStream(filepath, FileMode.Create));
                companyInDb.Logo = filename;
            }

            // 3. Update other fields
            companyInDb.Cname = model.Cname;
            companyInDb.Tid = model.Tid;
            context.Companies.Update(companyInDb);
            await context.SaveChangesAsync();

            TempData["Success"] = "Company Updated Successfully.";
            await sls.AddLog("UPDATE", "Company", " Comapny : " + model.Cname);


            return RedirectToAction(nameof(Profile));
        }

        [Route("Coordinator/EditStudent/{id:int}")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> EditStudent(int id)
        {
            var obj = await context.Studentsplaceds.Where(c => c.Id == id).FirstOrDefaultAsync();
            ViewBag.Bid = sls.BidList();
            ViewBag.Cid = sls.CidList();
            ViewBag.Yid = sls.YidList();
            return View(obj);
        }

        [Route("Coordinator/EditStudentMethod")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> EditStudentMethod(Studentsplaced obj)
        {
            obj.Tid = sls.TpcId();
            if (ModelState.IsValid)
            {
                try
                {
                    var exist = await context.Studentsplaceds.AnyAsync(s => s.Id != obj.Id && s.Contact == obj.Contact);
                    if (exist)
                    {
                        TempData["Error"] = "Email of this student already exist.";
                        return View("EditStudent", obj);
                    }

                    context.Studentsplaceds.Update(obj);
                    await context.SaveChangesAsync();
                    TempData["Success"] = "Student Updated Successfully.";
                    await sls.AddLog("UPDATE", "Studentsplaced", " Comapny : " + await sls.Cname(obj.Cid) + " Student : " + obj.Sname + " Branch : " + await sls.Bname(obj.Bid) + " from year " + await sls.Year(obj.Yid));

                    return RedirectToAction(nameof(Profile));

                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Student can't be updated.";
                }
            }
            else
            {
                TempData["Error"] = "Student can't be updated because Model Data is Invalid.";
            }
            ViewBag.Bid = sls.BidList();
            ViewBag.Cid = sls.CidList();
            ViewBag.Yid = sls.YidList();
            return View("EditStudent", obj);
        }

        [Route("Coordinator/EditRoundDetail/{id:int}")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> EditRoundDetail(int id)
        {
            var obj = await context.Rounddetails.Where(c => c.Id == id).FirstOrDefaultAsync();
            ViewBag.Cid = sls.CidList();
            return View(obj);
        }

        [Route("Coordinator/EditRoundDetailMethod")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> EditRoundDetailMethod(Rounddetail obj)
        {
            obj.Tid = sls.TpcId();
            if (ModelState.IsValid)
            {
                try
                {
                    context.Rounddetails.Update(obj);
                    await context.SaveChangesAsync();
                    TempData["Success"] = "Placement Procedure Detail Updated Successfully.";
                    await sls.AddLog("UPDATE", "Rounddetail", " Comapny : " + await sls.Cname(obj.Cid) + " Text : " + obj.Dtext);

                    return RedirectToAction(nameof(Profile));

                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Placement Procedure Detail can't be updated.";
                }
            }
            else
            {
                TempData["Error"] = "Placement Procedure Detail can't be updated because Model Data is Invalid.";
            }
            ViewBag.Cid = sls.CidList();
            return View("EditRoundDetail", obj);
        }

        [Route("Coordinator/EditDescription/{id:int}")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> EditDescription(int id)
        {
            var obj = await context.Descriptions.Where(c => c.Id == id).FirstOrDefaultAsync();
            ViewBag.Cid = sls.CidList();
            return View(obj);
        }

        [Route("Coordinator/EditDescriptionMethod")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> EditDescriptionMethod(Description obj)
        {
            obj.Tid = sls.TpcId();
            if (ModelState.IsValid)
            {
                try
                {
                    context.Descriptions.Update(obj);
                    await context.SaveChangesAsync();
                    TempData["Success"] = "Experience Updated Successfully.";
                    await sls.AddLog("UPDATE", "Description", " Comapny : " + await sls.Cname(obj.Cid) + " Text : " + obj.Dtext);

                    return RedirectToAction(nameof(Profile));

                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Experience can't be updated.";
                }
            }
            else
            {
                TempData["Error"] = "Experience can't be updated because Model Data is Invalid.";
            }
            ViewBag.Cid = sls.CidList();
            return View("EditDescription", obj);
        }

        [Route("Coordinator/EditResource/{id:int}")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> EditResource(int id)
        {
            var obj = await context.Resources.Where(c => c.Id == id).FirstOrDefaultAsync();
            ViewBag.Bid = sls.BidList();
            return View(obj);
        }

        [Route("Coordinator/EditResourceMethod")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> EditResourceMethod(Resource obj)
        {
            obj.Tid = sls.TpcId();
            if (ModelState.IsValid)
            {
                try
                {
                    context.Resources.Update(obj);
                    await context.SaveChangesAsync();
                    TempData["Success"] = "Resource Updated Successfully.";
                    await sls.AddLog("UPDATE", "Resource", " Branch : " + await sls.Bname(obj.Bid) + " Link : " + obj.Rlink);

                    return RedirectToAction(nameof(Profile));

                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Resource can't be updated.";
                }
            }
            else
            {
                TempData["Error"] = "Resource can't be updated because Model Data is Invalid.";
            }
            ViewBag.Bid = sls.BidList();
            return View("EditResource", obj);
        }

        [Route("Coordinator/EditSession/{id::int}")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> EditSession(int id)
        {
            var obj = await context.Sessions.Where(c => c.Id == id).FirstOrDefaultAsync();
            ViewBag.Bid = sls.BidList();
            return View(obj);
        }

        [Route("Coordinator/EditSessionMethod")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> EditSessionMethod(Session obj)
        {
            obj.Tid = sls.TpcId();
            if (ModelState.IsValid)
            {
                var roomName = Guid.NewGuid().ToString("N");
                var baseUrl = "https://meet.jit.si";
                var meetingUrl = $"{baseUrl}/{roomName}";

                // Set the generated meeting link on the Session entity
                obj.Link = meetingUrl;
                try
                {
                    context.Sessions.Update(obj);
                    await context.SaveChangesAsync();
                    TempData["Success"] = @"Session Updated Successfully with Meeting Url : " + meetingUrl.ToString();
                    await sls.AddLog("UPDATE", "Session", " Link : " + obj.Link + " Student : " + " for Branch : " + await sls.Bname(obj.Bid));

                    return RedirectToAction(nameof(Profile));

                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Session can't be updated.";
                }
            }
            else
            {
                TempData["Error"] = "Session can't be updated because Model Data is Invalid.";
            }
            ViewBag.Bid = sls.BidList();
            return View("EditSession", obj);
        }

    }
}
