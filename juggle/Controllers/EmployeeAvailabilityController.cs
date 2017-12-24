using juggle.Models;
using System;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace juggle.Controllers
{
    public class EmployeeAvailabilityController : Controller
    {

        /// <summary>
        /// load page event
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Employees the availability.
        /// </summary>
        /// <returns></returns>
        public ActionResult EmployeeAvailability()
        {
            if (Session["User_Id"] != null)
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    int userid = Convert.ToInt32(Session["User_Id"]);
                    var employeeavailabilit = (from a in dbcon.tbl_employee_availability
                                               join b in dbcon.tbl_employee_info on a.emp_id equals b.emp_id
                                               where a.userid == userid
                                               orderby a.empavailability_id descending
                                               select new {
                                                   a.start_date,
                                                   a.end_date,
                                                   a.time_range_start,
                                                   a.time_range_end,
                                                   a.empavailability_id,
                                                   Empname = b.emp_firstname +" " + b.emp_lastname,
                                               }).ToList();
                    return View(employeeavailabilit);
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
 
        /// <summary>
        ///  add employee schedule availibility
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public PartialViewResult Create_EmployeeSchedule()
        {
            jugglecontext dbcon = new jugglecontext();
            int userid = Convert.ToInt32(Session["User_Id"]);
            var emplist = (from a in dbcon.tbl_employee_info
                           where a.user_id == userid
                           select new { a.emp_id, a.emp_firstname, a.emp_lastname });
            var empdropdown = emplist.ToList().Select(c => new SelectListItem
            {
                Text = c.emp_firstname.ToString() +" "+ c.emp_lastname.ToString(),
                Value = c.emp_id.ToString(),
            }).ToList();

            ViewBag.emp_list = empdropdown;
            return PartialView();
        }
        /// <summary>
        /// Creates the employee schedule.
        /// </summary>
        /// <param name="employee_av">The employee av.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create_EmployeeSchedule(tbl_employee_availability employee_av)
        {
            using (jugglecontext dbcon = new jugglecontext())
            {
                try
                {
                    tbl_employee_availability employee = new tbl_employee_availability();
                    employee.start_date = Convert.ToDateTime(employee_av.start_date);
                    employee.end_date = Convert.ToDateTime(employee_av.end_date);
                    employee.userid = Convert.ToInt32(Session["User_Id"]);
                    employee.time_range_start = employee_av.time_range_start;
                    employee.time_range_end = employee_av.time_range_end;
                    employee.emp_id = Convert.ToInt32(employee_av.emp_id);
                    employee.created_date = DateTime.Now;
                    dbcon.tbl_employee_availability.Add(employee);
                    dbcon.SaveChanges();



                    // when employee delete at that time his/her assigned appointment will be upassign
                    SqlConnection connection = new SqlConnection(dbcon.connectionString());
                    var command = new SqlCommand("[updateempavailibility]", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@startdate", Convert.ToDateTime(employee.start_date));
                    command.Parameters.AddWithValue("@enddate", Convert.ToDateTime(employee.end_date));
                    command.Parameters.AddWithValue("@StartTime", employee_av.time_range_start.ToString());
                    command.Parameters.AddWithValue("@endTime", employee_av.time_range_end.ToString());
                    //command.Parameters.AddWithValue("@user_id", Convert.ToInt32(Session["User_Id"]));
                    command.Parameters.AddWithValue("@emp_id", Convert.ToInt32(employee.emp_id));
                    command.Parameters.AddWithValue("@type", "edit");
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();

                    return RedirectToAction("EmployeeAvailability", "EmployeeAvailability");
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
                    return View(employee_av);
                }

            }
        }
  
        /// <summary>
        /// Get employee availibilities the edit.
        /// </summary>
        /// <param name="empavailability_id">The empavailability identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Employeeavailibility_Edit(Int32 empavailability_id)
        {

            using (jugglecontext dbcon = new jugglecontext())
            {
                jugglecontext dbcon1 = new jugglecontext();
                int userid = Convert.ToInt32(Session["User_Id"]);
                tbl_employee_availability employee = dbcon.tbl_employee_availability.Where(x => x.empavailability_id == empavailability_id).FirstOrDefault();
                tbl_employee_availability empl = new tbl_employee_availability();
                empl.start_date = Convert.ToDateTime(employee.start_date);
                empl.end_date = Convert.ToDateTime(employee.end_date);
                empl.time_range_start = employee.time_range_start;
                empl.time_range_end = employee.time_range_end;
                empl.emp_id = Convert.ToInt32(employee.emp_id);
                empl.created_date = employee.created_date;
                empl.updated_date = DateTime.Now;

                var emplist = (from a in dbcon.tbl_employee_info
                               where a.user_id == userid
                               select new { a.emp_id, a.emp_firstname, a.emp_lastname });

                var empdropdown = emplist.ToList().Select(c => new SelectListItem
                {
                    Text = c.emp_firstname.ToString() + " " + c.emp_lastname.ToString(),
                    Value = c.emp_id.ToString(),
                    Selected = (c.emp_id == employee.emp_id)
                }).ToList();

                ViewBag.emp_list = empdropdown;
                return View(empl);
            }

        }
        /// <summary>
        /// post employee availibilities the edit.
        /// </summary>
        /// <param name="employee">The employee.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Employeeavailibility_Edit(tbl_employee_availability employee)
        {
            if (Session["User_Id"] != null)
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    try
                    {
                        tbl_employee_availability empl = new tbl_employee_availability();
                        empl.emp_id = employee.emp_id;
                        empl.start_date = Convert.ToDateTime(employee.start_date);
                        empl.end_date = Convert.ToDateTime(employee.end_date);
                        empl.userid = Convert.ToInt32(Session["User_Id"]);
                        empl.time_range_start = employee.time_range_start;
                        empl.time_range_end = employee.time_range_end;
                        empl.created_date = employee.created_date;
                        empl.updated_date = DateTime.Now;
                        empl.empavailability_id = Convert.ToInt32(employee.empavailability_id);
                        dbcon.Entry(empl).State = System.Data.Entity.EntityState.Modified;
                        dbcon.SaveChanges();

                        // when employee delete at that time his/her assigned appointment will be upassign
                        SqlConnection connection = new SqlConnection(dbcon.connectionString());
                        var command = new SqlCommand("[updateempavailibility]", connection);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@startdate", Convert.ToDateTime(employee.start_date));
                        command.Parameters.AddWithValue("@enddate", Convert.ToDateTime(employee.end_date));
                        command.Parameters.AddWithValue("@StartTime",employee.time_range_start );
                        command.Parameters.AddWithValue("@endTime", employee.time_range_end);
                        command.Parameters.AddWithValue("@emp_id", Convert.ToInt32(employee.emp_id));
                        command.Parameters.AddWithValue("@type", "edit");
                        connection.Open();
                        SqlDataReader rdr = command.ExecuteReader();















                        return RedirectToAction("EmployeeAvailability", "EmployeeAvailability");
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
        /// delete Employee availibility and  work type
        /// </summary>
        /// <param name="empavailability_id">The empavailability identifier.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Employeeavailibility_delete(Int32 empavailability_id)
        {
            if (Session["User_Id"] != null)
            {
                try
                {
                    using (jugglecontext dbcon = new jugglecontext())
                    {
                        if (Session["User_Id"] != null)
                        {
                            //var empavailability_detail = (from a in dbcon.tbl_employee_availability
                            //                              where a.empavailability_id == empavailability_id
                            //                              select a).ToList();
                            //// when employee delete at that time his/her assigned appointment will be upassign
                            //SqlConnection connection = new SqlConnection(dbcon.connectionString());
                            //var command = new SqlCommand("[updatedata]", connection);
                            //command.CommandType = CommandType.StoredProcedure;
                            //command.Parameters.AddWithValue("@startdate", Convert.ToDateTime(empavailability_detail[0].start_date));
                            //command.Parameters.AddWithValue("@enddate", Convert.ToDateTime(empavailability_detail[0].end_date));
                            //command.Parameters.AddWithValue("@StartTime", empavailability_detail[0].time_range_start.ToString());
                            //command.Parameters.AddWithValue("@endTime", empavailability_detail[0].time_range_end.ToString());
                            //command.Parameters.AddWithValue("@user_id", Convert.ToInt32(Session["User_Id"]));
                            //command.Parameters.AddWithValue("@emp_id", Convert.ToInt32(empavailability_detail[0].emp_id));
                            //command.Parameters.AddWithValue("@attribute_id", 0);
                            //command.Parameters.AddWithValue("@client_id", 0);
                            //command.Parameters.AddWithValue("@appointment_id", 0);
                            //command.Parameters.AddWithValue("@StatementType", "emp_data");
                            //connection.Open();
                            //SqlDataReader rdr = command.ExecuteReader();
                            tbl_employee_availability objEmp = dbcon.tbl_employee_availability.Find(empavailability_id);
                            dbcon.tbl_employee_availability.Remove(objEmp);
                            dbcon.SaveChanges();
                        }
                        return RedirectToAction("EmployeeAvailability", "EmployeeAvailability");
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
        /// check with remote validation to check employee availability
        /// </summary>
        /// <param name="emp_id">The emp identifier.</param>
        /// <param name="empavailability_id">The empavailability identifier.</param>
        /// <param name="time_range_start">The time range start.</param>
        /// <param name="time_range_end">The time range end.</param>
        /// <param name="start_date">The start date.</param>
        /// <param name="end_date">The end date.</param>
        /// <param name="day">The day.</param>
        /// <param name="recurring">if set to <c>true</c> [recurring].</param>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult doesemployeeavailability(int emp_id, int empavailability_id = 0, string time_range_start = null, string time_range_end = null, string start_date = null, string end_date = null, string day = null, Boolean recurring = false)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                DateTime start_Date_check = Convert.ToDateTime(start_date);
                if (string.IsNullOrEmpty(end_date))
                {
                    end_date = start_date;
                }
                int userid = Convert.ToInt32(Session["User_Id"]);
                DateTime end_Date_check = Convert.ToDateTime(end_date);
                TimeSpan time_range_start_timespan = TimeSpan.Parse(time_range_start);
                TimeSpan time_range_end_timespan = TimeSpan.Parse(time_range_end);
                DateTime time_range_start_timespandate = Convert.ToDateTime(start_date + " " + time_range_start);
                DateTime time_range_end_timespandate = Convert.ToDateTime(end_date + " " + time_range_end);

                var id = (from a in dbcon.tbl_employee_availability
                          where a.emp_id == emp_id && a.empavailability_id != empavailability_id
                          select a).ToList();
      
                for(int i=0;i<id.Count;i++)
                {
                    SqlConnection connection = new SqlConnection(dbcon.connectionString());
                    var command = new SqlCommand("compare_client_Availibility", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@startdate", Convert.ToDateTime(start_date));
                    command.Parameters.AddWithValue("@enddate", Convert.ToDateTime(end_date));
                    command.Parameters.AddWithValue("@StartTime", time_range_start.ToString());
                    command.Parameters.AddWithValue("@endTime", time_range_end.ToString());
                    command.Parameters.AddWithValue("@emp_id", Convert.ToInt32(emp_id));
                    command.Parameters.AddWithValue("@empavailability_id", Convert.ToInt32(empavailability_id));
                    command.Parameters.AddWithValue("@userid", userid);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        connection.Close();
                        return (Json(string.Format("This Employee has already availability."), JsonRequestBehavior.AllowGet));
                    }
                }
                return (Json(true, JsonRequestBehavior.AllowGet));
            }
            catch(Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "doesemployeeavailability");
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        /// check whether the start time is past time or not of current date
        /// </summary>
        /// <param name="time_range_end">The time range end.</param>
        /// <param name="time_range_start">The time range start.</param>
        /// <param name="start_date">The start date.</param>
        /// <param name="end_date">The end date.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult checktimerangeend(string time_range_end = null, string time_range_start = null, string start_date = null, string end_date = null)
        {
            try
            {
                string timezone = Session["timezone"].ToString();
                TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                DateTime newDT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, targetZone);
                DateTime today = newDT;
                string dating = today.ToString("MM/dd/yyyy");
                if (string.IsNullOrEmpty(time_range_start))
                {
                    return Json(string.Format("Please Select Start Time"), JsonRequestBehavior.AllowGet);
                }
                jugglecontext dbcon = new jugglecontext();
                TimeSpan timestart = TimeSpan.Parse(time_range_start);
                TimeSpan timeend = TimeSpan.Parse(time_range_end);
                DateTime startstart = Convert.ToDateTime(time_range_start);
                DateTime dt = Convert.ToDateTime(start_date + " " + time_range_start);

                if (timestart == timeend)
                {
                    return Json(string.Format("End time cannot be the same as start time."), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "checktimerangeend");
                return Json("Invalid Time", JsonRequestBehavior.AllowGet);
            }
        }
    }
}