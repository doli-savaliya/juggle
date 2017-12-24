using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using juggle.Models;

namespace juggle.Controllers
{
    public class Work_Type_CategoryController : Controller
    {
        // GET: Work_Type_Category
        public ActionResult Index()
        {
            if (Session["User_Id"] != null )
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // Get work type
        public ActionResult Worktype_category()
        {
            if (Session["User_Id"] != null && Session["User_Roll_Id"].ToString() == "2")
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    List<tbl_worktype_category> worktype_category = dbcon.tbl_worktype_category.OrderByDescending(x => x.worktypecategory_name).ToList<tbl_worktype_category>();
                    return View(worktype_category);
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }


        // Create worktype category
        [HttpGet]
        public PartialViewResult Create_Worktype_Category()
        {
            return PartialView();
        }


        // Create worktype category
        [HttpPost]
        public ActionResult Create_Worktype_Category(juggle.Models.tbl_worktype_category work_type_category)
        {

            using (jugglecontext dbcon = new jugglecontext())
            {
                try
                {

                    tbl_worktype_category worktype = new tbl_worktype_category();
                    worktype.worktypecategory_name = work_type_category.worktypecategory_name.Trim();
                    worktype.created_date = DateTime.Now;
                    dbcon.tbl_worktype_category.Add(worktype);
                    dbcon.SaveChanges();
                    return RedirectToAction("Worktype_category", "Work_Type_Category");
                }
                catch (Exception ex)
                {
                    return View(work_type_category);
                }

            }
        }

        //Edit worktype
        [HttpGet]
        public ActionResult Worktype_Category_Edit(Int32 worktypecat_id) // banse
        {

            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    tbl_worktype_category work_type = dbcon.tbl_worktype_category.Where(x => x.worktypecat_id == worktypecat_id).FirstOrDefault();
                    tbl_worktype_category prod = new tbl_worktype_category();
                    prod.worktypecat_id = work_type.worktypecat_id;
                    prod.worktypecategory_name = work_type.worktypecategory_name;
                    prod.created_date = work_type.created_date;
                    return View(prod);
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }



        [AllowAnonymous]
        public JsonResult doesNameExist(string worktypecategory_name, int worktypecat_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                return dbcon.tbl_worktype_category.Any(x => x.worktypecategory_name == worktypecategory_name.Trim() && x.worktypecat_id != worktypecat_id)
                      ? Json(string.Format("{0} already exists.", worktypecategory_name),
                                                JsonRequestBehavior.AllowGet)
                      : Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("error", JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public ActionResult Worktype_Category_Edit(juggle.Models.tbl_worktype_category work_type_category)
        {
            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    tbl_worktype_category worktype = new tbl_worktype_category();
                    worktype.worktypecategory_name = work_type_category.worktypecategory_name.Trim();
                    worktype.worktypecat_id = work_type_category.worktypecat_id;
                    worktype.updated_date = DateTime.Now;
                    worktype.created_date = work_type_category.created_date;
                    dbcon.Entry(worktype).State = System.Data.Entity.EntityState.Modified;
                    dbcon.SaveChanges();
                    return RedirectToAction("Worktype_category", "Work_Type_Category");
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Worktype_category_delete(Int32 worktypecat_id)
        {
            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    if (Session["User_Id"] != null)
                    {
                        tbl_worktype_category objEmp = dbcon.tbl_worktype_category.Find(worktypecat_id);
                        dbcon.tbl_worktype_category.Remove(objEmp);
                        dbcon.SaveChanges();
                    }
                    return RedirectToAction("Worktype_category", "Work_Type_Category");
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }
    }
}