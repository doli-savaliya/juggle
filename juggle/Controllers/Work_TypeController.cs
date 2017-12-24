using System;
using System.Linq;
using System.Web.Mvc;
using juggle.Models;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Data;

namespace juggle.Controllers
{
    public class Work_TypeController : Controller
    {

        /// <summary>
        /// page load event
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
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
        /// <summary>
        /// Admins this instance.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Get Works type data.
        /// </summary>
        /// <returns></returns>
        public ActionResult Work_type()
        {
            if (Session["User_Id"] != null && Session["User_Roll_Id"].ToString() == "2")
            {
                int userid = Convert.ToInt32(Session["User_Id"]);
                using (jugglecontext dbcon = new jugglecontext())
                {

                    var worktypecate = (from a in dbcon.tbl_worktype
                                        where a.user_id == userid
                                        orderby a.work_id descending
                                        select new
                                        {
                                            a.work_id,
                                            a.user_id,
                                            a.name,
                                            a.color,
                                            a.created_date,
                                            a.updated_date,
                                        }).ToList();
                    return View(worktypecate);
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }


        /// <summary>
        /// get the worktype.
        /// </summary>
        /// <returns>returns on view</returns>
        [HttpGet]
        public PartialViewResult Create_Worktype()
        {
            if (Session["User_Id"] != null)
            {
                jugglecontext dbcon = new jugglecontext();
                var content = from p in dbcon.servicetypetimes
                              select new { p.value, p.timeinminute };
                
                var x = content.ToList().Select(c => new SelectListItem
                {
                    Text = c.timeinminute.ToString(),
                    Value = c.value.ToString(),
                }).ToList();

                ViewBag.servicetypeminute = x;
                return PartialView();
            }
            else
            {
                return PartialView("Login", "Account");
            }

        }


        /// <summary>
        /// Creates the worktype.
        /// </summary>
        /// <param name="work_type">Type of the work.</param>
        /// <returns>returns on the view</returns>
        [HttpPost]
        public ActionResult Create_Worktype(juggle.Models.tbl_worktype work_type)
        {
            using (jugglecontext dbcon = new jugglecontext())
            {
                try
                {
                    tbl_worktype worktype = new tbl_worktype();
                    worktype.name = work_type.name.Trim();
                    worktype.color = work_type.color;
                    var time = Request.Form["multiple"];
                    worktype.time = time;
                    worktype.user_id = Convert.ToInt32(Session["User_Id"]);
                    worktype.created_date = DateTime.Now;
                    dbcon.tbl_worktype.Add(worktype);
                    dbcon.SaveChanges();
                    return RedirectToAction("Work_type", "Work_type");

                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                        {
                            Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                        }
                    }
                    return View(work_type);

                }

            }
        }

        /// <summary>
        /// Worktypes the edit.
        /// </summary>
        /// <param name="work_id">The work identifier.</param>
        /// <returns>returns on the view</returns>
        [HttpGet]
        public ActionResult Worktype_Edit(Int32 work_id) // banse
        {
            if (Session["User_Id"] != null)
            {
                jugglecontext dbcon1 = new jugglecontext();

                try
                {
                    using (jugglecontext dbcon = new jugglecontext())
                    {
                        tbl_worktype work_type = dbcon.tbl_worktype.Where(x => x.work_id == work_id).FirstOrDefault();
                        tbl_worktype prod = new tbl_worktype();
                        prod.work_id = work_type.work_id;
                        prod.name = work_type.name;
                        prod.color = work_type.color;
                        prod.time = work_type.time;
                        prod.user_id = work_type.user_id;
                        prod.created_date = work_type.created_date;
                        var content = from p in dbcon.servicetypetimes
                                      select new { p.value, p.timeinminute };
                        var y = content.ToList().Select(c => new SelectListItem
                        {
                            Text = c.timeinminute.ToString(),
                            Value = c.value.ToString(),
                        }).ToList();

                        ViewBag.servicetypeminute = y;

                        return View(prod);
                    }
                }
                catch 
                {
                    return View();
                }
            }
            else
            {
                return View("Login", "Account");
            }
        }


        /// <summary>
        /// Worktypes edit.
        /// </summary>
        /// <param name="work_type">Type of the work.</param>
        /// <returns>returns on the view</returns>
        [HttpPost]
        public ActionResult Worktype_Edit(juggle.Models.tbl_worktype work_type)
        {
            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    tbl_worktype worktype = new tbl_worktype();
                    worktype.name = work_type.name.Trim();
                    worktype.color = work_type.color;
                    var time = Request.Form["multiple"];
                    worktype.time = time;
                    worktype.work_id = work_type.work_id;
                    worktype.updated_date = DateTime.Now;
                    worktype.user_id = work_type.user_id;
                    worktype.created_date = work_type.created_date;
                    dbcon.Entry(worktype).State = System.Data.Entity.EntityState.Modified;
                    dbcon.SaveChanges();
                    return RedirectToAction("Work_type", "Work_type");
                }
            }
            catch 
            {
                return View();
            }
        }

        /// <summary>
        /// Worktypes delete.
        /// </summary>
        /// <param name="work_id">The work identifier.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Worktype_delete(Int32 work_id)
        {
            if (Session["User_Id"] != null)
            {
                try
                {
                    using (jugglecontext dbcon = new jugglecontext())
                    {
                        if (Session["User_Id"] != null)
                        {
                            string workid = work_id.ToString();
                            var employee_detail = (from a in dbcon.tbl_employee_info
                                                   where a.emp_qualifiedservicetypes.Contains(workid)
                                                   select a).ToList();

                            for (int i = 0; i < employee_detail.Count; i++)
                            {
                                string emp_worktype = employee_detail[i].emp_qualifiedservicetypes.ToString();
                                emp_worktype = emp_worktype.Replace(workid, "");
                                SqlConnection connection = new SqlConnection(dbcon.connectionString());
                                var command = new SqlCommand("[updatedata]", connection);
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@client_id", 0);
                                command.Parameters.AddWithValue("@attribute_id", emp_worktype);
                                command.Parameters.AddWithValue("@appointment_id", 0);
                                command.Parameters.AddWithValue("@StatementType", "worktype_data");
                                command.Parameters.AddWithValue("@emp_id", Convert.ToInt32(employee_detail[i].emp_id));
                                command.Parameters.AddWithValue("@startdate", 0);
                                command.Parameters.AddWithValue("@enddate", 0);
                                command.Parameters.AddWithValue("@StartTime", 0);
                                command.Parameters.AddWithValue("@endTime", 0);
                                command.Parameters.AddWithValue("@user_id", Convert.ToInt32(Session["User_Id"]));
                                connection.Open();
                                SqlDataReader rdr = command.ExecuteReader();
                            }

                            tbl_worktype objEmp = dbcon.tbl_worktype.Find(work_id);
                            dbcon.tbl_worktype.Remove(objEmp);
                            dbcon.SaveChanges();
                        }
                        return RedirectToAction("Work_type", "Work_type");
                    }
                }
                catch 
                {
                    return View();
                }
            }
            else
            {
                return View("Login", "Account");
            }
        }
        /// <summary>
        /// validation check name is exist or not.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="work_id">The work identifier.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult doesNameExist(string name, int work_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                int userid = Convert.ToInt32(Session["User_Id"]);
                return dbcon.tbl_worktype.Any(x => x.name == name.Trim() && x.work_id != work_id && x.user_id == userid)
                      ? Json(string.Format("{0} already exists.", name),
                                                JsonRequestBehavior.AllowGet)
                      : Json(true, JsonRequestBehavior.AllowGet);
            }
            catch 
            {
                return Json("error", JsonRequestBehavior.AllowGet);

            }

        }
        /// <summary>
        /// validation check for color exist or not.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="work_id">The work identifier.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult doescolorExist(string color, int work_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                int userid = Convert.ToInt32(Session["User_Id"]);
                return dbcon.tbl_worktype.Any(x => x.color == color && x.work_id != work_id && x.user_id == userid)
                      ? Json(string.Format("color already exist"),
                                                JsonRequestBehavior.AllowGet)
                      : Json(true, JsonRequestBehavior.AllowGet);
            }
            catch 
            {
                return Json("error", JsonRequestBehavior.AllowGet);

            }
        }

    }
}