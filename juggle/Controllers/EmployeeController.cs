using juggle.Models;
using System;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace juggle.Controllers
{
    public class EmployeeController : Controller
    {
        /// <summary>
        /// Indexes this instance.
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
        /// Employees this instance.
        /// </summary>
        /// <returns></returns>
        public ActionResult Employee()
        {
            if (Session["User_Id"] != null && Session["User_Roll_Id"].ToString() == "2")
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    var userId = Convert.ToInt32(Session["User_Id"]);
                    var id = (from a in dbcon.tbl_employee_info
                              where a.user_id == userId
                              orderby a.emp_id descending
                              select new
                              {
                                  Empname = a.emp_firstname + " " + a.emp_lastname,
                                  a.emp_id
                              }).ToList();
                    return View(id);

                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }

        // Create employee
        /// <summary>
        /// Creates the employee.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public PartialViewResult Create_Employee()
        {
            jugglecontext dbcon = new jugglecontext();
            int userid = Convert.ToInt32(Session["User_Id"]);
            var content = from p in dbcon.tbl_worktype
                          where p.user_id == userid
                          select new { p.work_id, p.name };

            var attribute = from p in dbcon.tbl_attribute_data
                            where p.user_id == userid
                            select new { p.attribute_id, p.attribute_name };


            var x = content.ToList().Select(c => new SelectListItem
            {
                Text = c.name.ToString(),
                Value = c.work_id.ToString(),
            }).ToList();

            ViewBag.emp_qualifiedservicetypeslist = x;


            var attri = attribute.ToList().Select(c => new SelectListItem
            {
                Text = c.attribute_name.ToString(),
                Value = c.attribute_id.ToString(),
            }).ToList();

            ViewBag.attributelist = attri;
            return PartialView();
        }

        /// <summary>
        /// Creates the employee.
        /// </summary>
        /// <param name="employee">The employee.</param>
        /// <returns></returns>
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create_Employee(tbl_employee_info employee)
        {
            using (jugglecontext dbcon = new jugglecontext())
            {
                try
                {
                    string multipleattribute = Request.Form["attribute_id_multiple"].ToString();
                    string multipleService = Request.Form["emp_qualifiedservicetypes_multiple"].ToString();
                    var getButton = HttpContext.Request.Form["Create"];
                    tbl_employee_info empl = new tbl_employee_info();
                    empl.emp_firstname = employee.emp_firstname.Trim();
                    empl.emp_lastname = employee.emp_lastname.Trim();
                    empl.emp_contactinfo = employee.emp_contactinfo.Trim();
                    empl.emp_qualifiedservicetypes = multipleService;
                    empl.created_date = DateTime.Now;
                    empl.user_id = Convert.ToInt32(Session["User_Id"]);
                    empl.emp_code = autogenerateid();
                    empl.attribute_id = multipleattribute;
                    empl.emp_googlecalendarID = employee.emp_googlecalendarID;
                    empl.emp_note = employee.emp_note;
                    empl.emp_transportion = employee.emp_transportion;
                    empl.emp_password = dbcon.Encrypt(employee.emp_password);
                    dbcon.tbl_employee_info.Add(empl);
                    dbcon.SaveChanges();
                    return RedirectToAction("Employee", "Employee");
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

                    return View(employee);
                }

            }

        }

        /// <summary>
        /// Employees the edit.
        /// </summary>
        /// <param name="emp_id">The emp identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Employee_Edit(Int32 emp_id)
        {
            int userid = Convert.ToInt32(Session["User_Id"]);
            using (jugglecontext dbcon = new jugglecontext())
            {
                jugglecontext dbcon1 = new jugglecontext();

                var content = from p in dbcon.tbl_worktype
                              where p.user_id == userid
                              select new { p.work_id, p.name };

                var attribute = from p in dbcon.tbl_attribute_data
                                where p.user_id == userid
                                select new { p.attribute_id, p.attribute_name };

                var getmultipledata = (from a in dbcon.tbl_employee_info
                                       where a.emp_id == emp_id
                                       select a
                                      ).ToList();

                string multipleattribute = getmultipledata[0].attribute_id.ToString();
                string multipleservicetype = getmultipledata[0].emp_qualifiedservicetypes.ToString();

                tbl_employee_info employee = dbcon.tbl_employee_info.Where(x => x.emp_id == emp_id).FirstOrDefault();
                tbl_employee_info empl = new tbl_employee_info();
                empl.emp_id = employee.emp_id;
                empl.emp_firstname = employee.emp_firstname;
                empl.emp_lastname = employee.emp_lastname;
                empl.emp_qualifiedservicetypes = employee.emp_qualifiedservicetypes;
                empl.emp_contactinfo = employee.emp_contactinfo;
                empl.emp_code = autogenerateid();
                empl.user_id = employee.user_id;
                empl.emp_transportion = employee.emp_transportion;
                empl.created_date = employee.created_date;
                empl.emp_note = employee.emp_note;
                empl.attribute_id = employee.attribute_id;
                empl.emp_googlecalendarID = employee.emp_googlecalendarID;
                empl.emp_note = employee.emp_note;
                empl.emp_password = dbcon.Decrypt(employee.emp_password);

                var emp_list = content.ToList().Select(c => new SelectListItem
                {
                    Text = c.name.ToString(),
                    Value = c.work_id.ToString(),
                }).ToList();

                ViewBag.emp_qualifiedservicetypeslist = emp_list;

                var attri = attribute.ToList().Select(c => new SelectListItem
                {
                    Text = c.attribute_name.ToString(),
                    Value = c.attribute_id.ToString(),
                }).ToList();

                ViewBag.attributelist = attri;
                return View(empl);
            }

        }
        /// <summary>
        /// Employees the edit.
        /// </summary>
        /// <param name="employee">The employee.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Employee_Edit(tbl_employee_info employee)
        {
            if (Session["User_Id"] != null)
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    try
                    {
                        string multipleattribute = Request.Form["attribute_id_multiple"].ToString();
                        string multipleservice = Request.Form["emp_qualifiedservicetypes_multiple"].ToString();
                        tbl_employee_info empl = new tbl_employee_info();
                        empl.emp_id = employee.emp_id;
                        empl.emp_firstname = employee.emp_firstname;
                        empl.emp_lastname = employee.emp_lastname;
                        empl.emp_qualifiedservicetypes = multipleservice;
                        empl.user_id = employee.user_id;
                        empl.created_date = employee.created_date;
                        empl.emp_code = autogenerateid();
                        empl.updated_date = DateTime.Now;
                        empl.emp_contactinfo = employee.emp_contactinfo;
                        empl.attribute_id = multipleattribute;
                        empl.emp_transportion = employee.emp_transportion;
                        empl.emp_googlecalendarID = employee.emp_googlecalendarID;
                        empl.emp_note = employee.emp_note;
                        empl.emp_password = dbcon.Encrypt(employee.emp_password);

                        dbcon.Entry(empl).State = System.Data.Entity.EntityState.Modified;
                        dbcon.SaveChanges();
                        return RedirectToAction("Employee", "Employee");
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
                    }
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            return View();

        }

        /// <summary>
        /// Employees the delete.
        /// </summary>
        /// <param name="emp_id">The emp identifier.</param>
        /// <returns>returns on the view</returns>
        [HttpPost]
        public ActionResult employee_delete(Int32 emp_id)
        {
            using (jugglecontext dbcon = new jugglecontext())
            {
                if (Session["User_Id"] != null)
                {
                    // when employee delete at that time his/her assigned appointment will be upassign
                    SqlConnection connection = new SqlConnection(dbcon.connectionString());
                    var command = new SqlCommand("[updatedata]", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@emp_id", emp_id);
                    command.Parameters.AddWithValue("@startdate", 0);
                    command.Parameters.AddWithValue("@enddate", 0);
                    command.Parameters.AddWithValue("@StartTime", 0);
                    command.Parameters.AddWithValue("@endTime", 0);
                    command.Parameters.AddWithValue("@user_id", Convert.ToInt32(Session["User_Id"]));
                    command.Parameters.AddWithValue("@attribute_id", 0);
                    command.Parameters.AddWithValue("@client_id", 0);
                    command.Parameters.AddWithValue("@appointment_id", 0);
                    command.Parameters.AddWithValue("@StatementType", "emp_data");
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();

                    tbl_employee_info objEmpl = dbcon.tbl_employee_info.Find(emp_id);
                    dbcon.tbl_employee_info.Remove(objEmpl);
                    dbcon.SaveChanges();
                }
                return RedirectToAction("Employee", "Employee");
            }

        }

        /// <summary>
        /// Auto genreated employee Code
        /// </summary>
        /// <returns></returns>
        public decimal autogenerateid()
        {
            string allowedChars = "";
            decimal autogenid = 0;
            allowedChars += "1,2,3,4,5,6,7,8,9,0";
            char[] sep = { ',' };
            string[] arr = allowedChars.Split(sep);
            string otp = "";
            string temp = "";
            Random rand = new Random();
            for (int i = 0; i < Convert.ToInt32(6); i++)
            {
                temp = arr[rand.Next(0, arr.Length)];
                otp += temp;
            }
            autogenid = Convert.ToDecimal(otp);
            return autogenid;
        }

        /// <summary>
        /// check the email is already exist or not.
        /// </summary>
        /// <param name="emp_googlecalendarID">The emp googlecalendar identifier.</param>
        /// <param name="emp_id">The emp identifier.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult doesemailexist(string emp_googlecalendarID, int emp_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                int userid = Convert.ToInt32(Session["User_Id"]);
                var does_client_email = (from a in dbcon.tbl_client
                                         where a.client_email == emp_googlecalendarID && a.user_id == userid
                                         select a).ToList();
                if (does_client_email.Count > 0)
                {
                    return Json(string.Format("{0} already exists.", emp_googlecalendarID));
                }
                return dbcon.tbl_employee_info.Any(x => x.emp_googlecalendarID == emp_googlecalendarID.Trim() && x.emp_id != emp_id && x.user_id == userid)
                      ? Json(string.Format("{0} already exists.", emp_googlecalendarID),
                                                JsonRequestBehavior.AllowGet)
                      : Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);

            }

        }

        /// <summary>
        /// check the phone is already exist or not
        /// </summary>
        /// <param name="emp_contactinfo">The emp contactinfo.</param>
        /// <param name="emp_id">The emp identifier.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult doesphonenoexist(string emp_contactinfo, int emp_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                int userid = Convert.ToInt32(Session["User_Id"]);

                var does_client_phone = (from a in dbcon.tbl_client
                                         where a.client_contact_info == emp_contactinfo && a.user_id == userid
                                         select a).ToList();
                if (does_client_phone.Count > 0)
                {
                    return Json(string.Format("{0} already exists.", emp_contactinfo));
                }

                return dbcon.tbl_employee_info.Any(x => x.emp_contactinfo == emp_contactinfo.Trim() && x.emp_id != emp_id && x.user_id == userid)
                      ? Json(string.Format("{0} already exists.", emp_contactinfo),
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