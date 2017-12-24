using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using juggle.Models;
using System.Data.Entity.Validation;

namespace juggle.Controllers
{
    public class WorkTypeController : Controller
    {
        // GET: WorkType
        public ActionResult Index()
        {
            return View();
        }


        // GET: Home check user
        public ActionResult Admin()
        {
            if (Session["User_Id"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // Get work type
        public ActionResult Worktype()
        {
            if (Session["User_Id"] != null)
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                   List<tbl_worktype> worktype = dbcon.tbl_worktype.OrderByDescending(x => x.name).ToList<tbl_worktype>();
                   return View(worktype);
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }


        //Create worktype
        [HttpGet]
        public PartialViewResult Create_Worktype()
        {
            jugglecontext dbcon = new jugglecontext();
            var content = from p in dbcon.tbl_worktype_category
                          select new { p.worktypecat_id, p.worktypecategory_name };


            var x = content.ToList().Select(c => new SelectListItem
            {
                Text = c.worktypecategory_name.ToString(),
                Value = c.worktypecat_id.ToString(),
            }).ToList();

            ViewBag.CurrencyList = x;
            return PartialView();
        }


        [HttpPost]
        public ActionResult Create_Worktype(juggle.Models.tbl_worktype work_type)
        {

            using (jugglecontext dbcon = new jugglecontext())
            {
                try
                {

                    tbl_worktype worktype = new tbl_worktype();
                    worktype.name = work_type.name.Trim();
                    worktype.color = work_type.color.Trim();
                    worktype.worktypecat_id = Convert.ToInt32(work_type.worktypecat_id);
                    worktype.user_id = Convert.ToInt32(Session["User_Id"]);
                    worktype.created_date = DateTime.Now;
                    dbcon.tbl_worktype.Add(worktype);
                    dbcon.SaveChanges();
                    return RedirectToAction("Worktype", "Worktype");

                }
                catch (Exception ex)
                {
                    return View(work_type);
                }

            }
        }


        // remote validationd for name of Worktype to check duplications

        [AllowAnonymous]
        public JsonResult doesNameExist(string name, int work_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                return dbcon.tbl_worktype.Any(x => x.name == name.Trim() && x.work_id != work_id)
                      ? Json(string.Format("{0} already exists.", name),
                                                JsonRequestBehavior.AllowGet)
                      : Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("error", JsonRequestBehavior.AllowGet);

            }

        }

        [AllowAnonymous]
        public JsonResult doescolorExist(string color, int work_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                return dbcon.tbl_worktype.Any(x => x.color == color && x.work_id != work_id)
                      ? Json(string.Format("color already exist"),
                                                JsonRequestBehavior.AllowGet)
                      : Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("error", JsonRequestBehavior.AllowGet);

            }
        }



        //Edit worktype
        [HttpGet]
        public ActionResult Worktype_Edit(Int32 work_id) // banse
        {

            jugglecontext dbcon1 = new jugglecontext();
            var content = from p in dbcon1.tbl_worktype
                          select new { p.work_id, p.name };

            var wname = content.ToList().Select(c => new SelectListItem
            {
                Text = c.name.ToString(),
                Value = c.work_id.ToString(),
                Selected = (c.work_id == work_id)
            }).ToList();
            ViewBag.CurrencyList = wname;

            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    tbl_worktype work_type = dbcon.tbl_worktype.Where(x => x.work_id == work_id).FirstOrDefault();
                    tbl_worktype prod = new tbl_worktype();
                    prod.work_id = work_type.work_id;
                    prod.name = work_type.name;
                    prod.color = work_type.color;
                    prod.worktypecat_id = work_type.worktypecat_id;
                    prod.user_id = work_type.user_id;
                    prod.created_date = work_type.created_date;
                    return View(prod);
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }


        [HttpPost]
        public ActionResult Worktype_Edit(juggle.Models.tbl_worktype work_type)
        {
            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    tbl_worktype worktype = new tbl_worktype();
                    worktype.name = work_type.name;
                    worktype.color = work_type.color;
                    worktype.work_id = work_type.work_id;
                    worktype.updated_date = DateTime.Now;
                    worktype.user_id = work_type.user_id;
                    worktype.worktypecat_id = work_type.worktypecat_id;
                    worktype.created_date = work_type.created_date;

                    dbcon.Entry(worktype).State = System.Data.Entity.EntityState.Modified;
                    dbcon.SaveChanges();
                    return RedirectToAction("Worktype", "Worktype");
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        /// <summary>
        /// delete worktype
        /// </summary>
        /// <param name="work_id"></param>
        /// <returns></returns>
        /// 

        [HttpPost]
        public ActionResult Worktype_delete(Int32 work_id)
        {
            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    if (Session["User_Id"] != null)
                    {
                        tbl_worktype objEmp = dbcon.tbl_worktype.Find(work_id);
                        dbcon.tbl_worktype.Remove(objEmp);
                        dbcon.SaveChanges();
                    }
                    return RedirectToAction("Worktype", "Worktype");
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }


        [HttpPost]
        public void getworktype(Int32 work_id)
        {
            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    tbl_worktype objEmp = dbcon.tbl_worktype.Find(work_id);
                }
                ViewBag.Countries = new List<string>
                {
                        "India",
                        "US",
                        "UK",
                        "Canada"
                };
            }
            catch (Exception ex)
            {
            }
        }



        public ActionResult worktypeparent()
        {
            return View();
        }


        //Create worktype
        [HttpGet]
        public PartialViewResult Create_Worktype_parent()
        {
            return PartialView();
        }
    }


}