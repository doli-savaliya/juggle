using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using juggle.Models;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Web.UI;

namespace juggle.Controllers
{
    public class AppointMentController : Controller
    {
        // GET: AppointMent
        public ActionResult Index()
        {
            return View();
        }

        // Get appointment
        public ActionResult Appointment() // banse
        {
            if (Session["User_Id"] != null)
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    int userid = Convert.ToInt32(Session["User_Id"]);
                    var appointment = (from a in dbcon.tbl_appointment
                                       join b in dbcon.tbl_client on a.client_id equals b.client_id
                                       join d in dbcon.tbl_worktype on a.work_id equals d.work_id
                                       where a.user_id == userid
                                       orderby a.appointment_id descending
                                       select new
                                       {
                                           a.appointment_id,
                                           a.description,
                                           a.start_date,
                                           a.end_date,
                                           a.time_range_start,
                                           a.time_range_end,
                                           client_id = b.client_firstname + " " + b.client_lastname,
                                           work_id = d.name,
                                           a.time,
                                           a.emp_id
                                       }).ToList();
                    return View(appointment);
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // create appointment
        [HttpGet]
        public PartialViewResult Create_Appointment()
        {
            jugglecontext dbcon = new jugglecontext();
            int userid = Convert.ToInt32(Session["User_Id"]);
            // get service type to fill dropdown

            var worktype = (from p in dbcon.tbl_worktype
                            where p.user_id == userid
                            select new { p.work_id, p.name }).ToList();

            List<tbl_worktype> worktypelist = new List<tbl_worktype>();
            int wokrtypeid = 0;
            int j = 0;
            if (worktype.Count > 0)
            {
                for (int i = 0; i < worktype.Count; i++)
                {
                    wokrtypeid = Convert.ToInt32(worktype[i].work_id);
                    var worktypetime = (from a in dbcon.tbl_worktype
                                        where a.work_id == wokrtypeid
                                        select a).ToList();
                    if (worktypetime.Count > 0)
                    {
                        string[] timeofworktype = worktypetime[0].time.ToString().Split(',');
                        for (j = 0; j < timeofworktype.Length; j++)
                        {
                            string strtime = null;

                            int getmin = Convert.ToInt32(timeofworktype[j]) * 15;
                            strtime = "(" + getmin.ToString() + " minutes)";

                            worktypelist.Add(new tbl_worktype() { name = worktype[i].name.ToString(), time = strtime, work_id = worktype[i].work_id });
                        }
                    }
                }
            }


            // get service type data
            var content = from p in dbcon.tbl_worktype
                          where p.user_id == userid
                          select new { p.work_id, p.name };


            var x = worktypelist.Select(c => new SelectListItem
            {
                Text = c.name.ToString() + " " + c.time.ToString(),
                Value = c.work_id.ToString(),
            }).ToList();

            ViewBag.CurrencyList = x;


            // get client data
            var client = from p in dbcon.tbl_client
                         where p.user_id == userid
                         select new { p.client_id, p.client_firstname, p.client_lastname };


            var clientx = client.ToList().Select(c => new SelectListItem
            {
                Text = c.client_firstname.ToString() + " " + c.client_lastname.ToString(),
                Value = c.client_id.ToString(),
            }).ToList();

            ViewBag.client_list = clientx;


            List<string> objDay = new List<string>();
            objDay.Add("Sunday");
            objDay.Add("Monday");
            objDay.Add("Tuesday");
            objDay.Add("Wednesday");
            objDay.Add("Thursday");
            objDay.Add("Friday");
            objDay.Add("Saturday");
            ViewBag.daylist = objDay;

            var timeinterval = from p in dbcon.tbl_time_interval
                               where p.time_interval_id != 1
                               select new { p.time_interval_id, p.time_interval };

            var timeintervalx = timeinterval.ToList().Select(c => new SelectListItem
            {
                Text = c.time_interval.ToString(),
                Value = c.time_interval_id.ToString(),
            }).ToList();

            ViewBag.timeinterlist = timeintervalx;


            var attribute = from p in dbcon.tbl_attribute_data
                            where p.user_id == userid
                            select new { p.attribute_id, p.attribute_name };

            var attri = attribute.ToList().Select(c => new SelectListItem
            {
                Text = c.attribute_name.ToString(),
                Value = c.attribute_id.ToString(),
            }).ToList();

            ViewBag.attributelist = attri;
            return PartialView();
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create_Appointment(juggle.Models.tbl_appointment appointment)
        {
            if (Session["User_Id"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            using (jugglecontext dbcon = new jugglecontext())
            {
                try
                {
                    tbl_appointment addappointment = new tbl_appointment();
                    string multipleattribute = Request.Form["attribute_idmul"].ToString();
                    string multiplelist = null;

                    if (appointment.recurring)
                    {
                        multiplelist = Request.Form["multiple"];
                    }
                    else
                    {
                        multiplelist = null;
                    }
                    addappointment.created_date = DateTime.Now;
                    addappointment.description = appointment.description;
                    addappointment.recurring = appointment.recurring;

                    if (!string.IsNullOrEmpty(multiplelist))
                    {
                        addappointment.day = multiplelist;
                    }
                    else { multiplelist = "0"; addappointment.day = "0"; }

                    //addappointment.day = appointment.day;
                    addappointment.start_date = appointment.start_date;
                    addappointment.end_date = appointment.end_date;
                    addappointment.time_range_start = appointment.time_range_start;
                    addappointment.time_range_end = appointment.time_range_end;


                    // add two extra field that store the calendar start time and end time 
                    // it is use for the event of google calendar only
                    // start

                    string startdate1 = appointment.start_date.ToString();
                    DateTime strstartdate1 = Convert.ToDateTime(startdate1);

                    string enddate1 = appointment.end_date.ToString();
                    DateTime strenddate1 = Convert.ToDateTime(enddate1);

                    DateTime cal_startdate = Convert.ToDateTime(strstartdate1.ToString("yyyy/MM/dd") + " " + appointment.time_range_start);
                    DateTime cal_enddate = Convert.ToDateTime(strenddate1.ToString("yyyy/MM/dd") + " " + appointment.time_range_end);

                    addappointment.Calendar_Start_datetime = DateTimeOffset.Parse(cal_startdate.ToString()).UtcDateTime;
                    addappointment.Calendar_End_datetime = DateTimeOffset.Parse(cal_enddate.ToString()).UtcDateTime;

                    string timezone = "West Asia Standard Time";
                    TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                    cal_startdate = TimeZoneInfo.ConvertTimeFromUtc(cal_startdate, targetZone);
                    cal_enddate = TimeZoneInfo.ConvertTimeFromUtc(cal_enddate, targetZone);

                    addappointment.Calendar_Start_datetime = cal_startdate;
                    addappointment.Calendar_End_datetime = cal_enddate;

                    // end
                    string startingtime = appointment.time_range_start.ToString();
                    string endindtime = appointment.time_range_end.ToString();

                    TimeSpan starttime = TimeSpan.Parse(startingtime);
                    TimeSpan endatime = TimeSpan.Parse(endindtime);

                    TimeSpan Difflength = endatime.Subtract(starttime);
                    int total = (Difflength.Hours * 60) + Difflength.Minutes;

                    addappointment.length = total;
                    if(!string.IsNullOrEmpty(addappointment.notes))
                    {
                        addappointment.notes = appointment.notes;
                    }
                    else
                    {
                        addappointment.notes = "N/A";
                    }
                    
                    addappointment.client_id = Convert.ToInt32(appointment.client_id);
                    addappointment.work_id = Convert.ToInt32(appointment.work_id);
                    addappointment.time_interval_id = Convert.ToInt32(appointment.time_interval_id);
                    addappointment.user_id = Convert.ToInt32(Session["User_Id"]);

                    addappointment.attribute_id = multipleattribute.ToString();
                    addappointment.time = appointment.time;
                    dbcon.tbl_appointment.Add(addappointment);
                    dbcon.SaveChanges();
                    var maxid = 0;
                    try
                    {
                        var context_latest_Id = new jugglecontext();

                        var maxappointmentid = (from a in context_latest_Id.tbl_appointment
                                                select new
                                                {
                                                    a.appointment_id
                                                }).ToList();

                        maxid = Convert.ToInt32(maxappointmentid[0].appointment_id);

                        for (int i = 0; i < maxappointmentid.Count; i++)
                        {
                            if (maxid < Convert.ToInt32(maxappointmentid[i].appointment_id))
                            {
                                maxid = Convert.ToInt32(maxappointmentid[i].appointment_id);
                            }
                        }
                    }
                    catch { }

                    if (!string.IsNullOrEmpty(multiplelist))
                    {
                        string[] values = multiplelist.Split(',');

                        if (appointment.recurring)
                        {
                            appointment.end_date = null;
                        }

                        for (int i = 0; i < values.Length; i++)
                        {
                            values[i] = values[i].Trim();

                            // add day into another table
                            SqlConnection connectioninsertday = new SqlConnection(dbcon.connectionString());
                            var commandinsertday = new SqlCommand("Appointmentday", connectioninsertday);
                            commandinsertday.CommandType = CommandType.StoredProcedure;
                            commandinsertday.Parameters.AddWithValue("@maxid", Convert.ToInt32(maxid));
                            commandinsertday.Parameters.AddWithValue("@day", Convert.ToInt32(values[i]));
                            commandinsertday.Parameters.AddWithValue("@appointmentid", Convert.ToInt32(0));
                            //commandinsertday.Parameters.AddWithValue("@date", dating);
                            commandinsertday.Parameters.AddWithValue("@type", "insertday");
                            connectioninsertday.Open();
                            SqlDataReader rdr = commandinsertday.ExecuteReader();
                            connectioninsertday.Close();
                        }
                    }

                    return RedirectToAction("AppointMent", "AppointMent");
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
                    return View(appointment);
                }

            }
        }



        //Edit appointment
        [HttpGet]
        public ActionResult appointment_edit(Int32 appointment_id) // banse
        {
            if (Session["User_Id"] == null)
            {
                return RedirectToAction("Login", "Account");
                //return Redirect(Request.UrlReferrer.ToString());
            }

            if (Session["User_Id"] != null)
            {
                jugglecontext dbcon1 = new jugglecontext();
                int userid = Convert.ToInt32(Session["User_Id"]);
                var get_appo_ids = (from a in dbcon1.tbl_appointment
                                    where a.appointment_id == appointment_id
                                    select a).ToList();
                int client_id = Convert.ToInt32(get_appo_ids[0].client_id);
                var work_id = Convert.ToInt32(get_appo_ids[0].work_id);
                var emp_id = Convert.ToInt32(get_appo_ids[0].emp_id);
                var timeintervalid = Convert.ToInt32(get_appo_ids[0].time_interval_id);
                int timeofapp = Convert.ToInt32(get_appo_ids[0].time);

                var client = from p in dbcon1.tbl_client
                             where p.user_id == userid
                             select new { p.client_id, p.client_firstname, p.client_lastname };

                //bind Client dropdown
                var clientx = client.ToList().Select(c => new SelectListItem
                {
                    Text = c.client_firstname.ToString() + " " + c.client_lastname,
                    Value = c.client_id.ToString(),
                    Selected = (c.client_id == client_id)
                }).ToList();

                ViewBag.client_list = clientx;

                var Employee = from p in dbcon1.tbl_employee_info
                               where p.user_id == userid
                               select new { p.emp_id, p.emp_firstname, p.emp_lastname };

                //get timeinterval
                var timeinterval = from p in dbcon1.tbl_time_interval
                                   where p.time_interval_id != 1
                                   select new { p.time_interval_id, p.time_interval };

                var timeintervalx = timeinterval.ToList().Select(c => new SelectListItem
                {
                    Text = c.time_interval.ToString(),
                    Value = c.time_interval_id.ToString(),
                    Selected = (c.time_interval_id == timeintervalid)
                }).ToList();

                ViewBag.timeinterlist = timeintervalx;


                var get_multiplelist = (from a in dbcon1.tbl_appointment
                                        where a.appointment_id == appointment_id
                                        select a).ToList();
                string edittime = get_multiplelist[0].day.ToString();


                var attribute = from p in dbcon1.tbl_attribute_data
                                where p.user_id == userid
                                select new { p.attribute_id, p.attribute_name };



                using (jugglecontext dbcon = new jugglecontext())
                {
                    tbl_appointment appointment = dbcon.tbl_appointment.Where(x => x.appointment_id == appointment_id).FirstOrDefault();
                    tbl_appointment editappointment = new tbl_appointment();
                    editappointment.description = appointment.description;
                    editappointment.attribute_id = appointment.attribute_id;
                    editappointment.recurring = appointment.recurring;
                    editappointment.start_date = appointment.start_date;
                    editappointment.end_date = appointment.end_date;
                    editappointment.day = edittime;
                    editappointment.time_range_start = appointment.time_range_start;
                    editappointment.time_range_end = appointment.time_range_end;
                    editappointment.Calendar_Start_datetime = appointment.Calendar_Start_datetime;
                    editappointment.Calendar_End_datetime = appointment.Calendar_End_datetime;
                    editappointment.updated_date = DateTime.Now;
                    editappointment.notes = appointment.notes;
                    editappointment.length = appointment.length;
                    editappointment.appointment_id = appointment.appointment_id;
                    editappointment.client_id = Convert.ToInt32(appointment.client_id);
                    editappointment.emp_id = Convert.ToInt32(appointment.emp_id);
                    editappointment.work_id = Convert.ToInt32(appointment.work_id);
                    editappointment.user_id = Convert.ToInt32(Session["User_Id"]);
                    editappointment.time_interval_id = Convert.ToInt32(appointment.time_interval_id);
                    editappointment.time = Convert.ToInt32(appointment.time);

                    var attri = attribute.ToList().Select(c => new SelectListItem
                    {
                        Text = c.attribute_name.ToString(),
                        Value = c.attribute_id.ToString(),
                        //Selected = (c.attribute_id == appointment.attribute_id)
                    }).ToList();

                    ViewBag.attributelist = attri;

                    /////////// ------------------

                    // get service type to fill dropdown

                    var worktype = (from p in dbcon1.tbl_worktype
                                    where p.user_id == userid
                                    select new { p.work_id, p.name }).ToList();

                    List<tbl_worktype> worktypelist = new List<tbl_worktype>();
                    int wokrtypeid = 0;
                    int j = 0;
                    if (worktype.Count > 0)
                    {
                        for (int i = 0; i < worktype.Count; i++)
                        {
                            wokrtypeid = Convert.ToInt32(worktype[i].work_id);
                            var worktypetime = (from a in dbcon1.tbl_worktype
                                                where a.work_id == wokrtypeid
                                                select a).ToList();
                            if (worktypetime.Count > 0)
                            {
                                string[] timeofworktype = worktypetime[0].time.ToString().Split(',');
                                for (j = 0; j < timeofworktype.Length; j++)
                                {
                                    string strtime = null;
                                    string color = null;
                                    int getmin = Convert.ToInt32(timeofworktype[j]) * 15;
                                    strtime = "(" + getmin.ToString() + " minutes)";
                                    color = getmin.ToString();
                                    worktypelist.Add(new tbl_worktype() { name = worktype[i].name.ToString(), time = strtime, work_id = worktype[i].work_id, color = color.ToString() });
                                }
                            }
                        }
                    }

                    var tie = worktypelist.Select(c => new SelectListItem
                    {
                        Text = c.name.ToString() + " " + c.time.ToString(),
                        Value = c.work_id.ToString(),
                        Selected = c.work_id == 5 //&& timeofapp == Convert.ToInt32(c.color)
                    }).ToList();

                    ViewBag.CurrencyList = tie;

                    ////////////-------------
                    var getworktypename = (from a in dbcon1.tbl_worktype
                                           where a.work_id == work_id
                                           select a).ToList();

                    ViewBag.hdnFlag = getworktypename[0].name + " (" + timeofapp + " minutes)";// "testing (15 minutes)";
                    ViewBag.currentdate = DateTime.Now.ToString("MM/dd/yyyy");
                    return View(editappointment);
                }

            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        //edit appointent
        [HttpPost]
        public ActionResult appointment_edit(juggle.Models.tbl_appointment appointment)
        {

            if (Session["User_Id"] == null)
            {
                return RedirectToAction("Appointment", "Appointment");
            }
            if (Session["User_Id"] != null)
            {
                try
                {
                    using (jugglecontext dbcon = new jugglecontext())
                    {

                        tbl_appointment editappointment = new tbl_appointment();
                        string multipleattribute = Request.Form["attribute_iddata"].ToString();
                        editappointment.appointment_id = appointment.appointment_id;
                        editappointment.description = appointment.description;
                        editappointment.recurring = appointment.recurring;
                        editappointment.start_date = appointment.start_date;
                        editappointment.end_date = appointment.end_date;
                        editappointment.time = appointment.time;
                        string multiplevalue = Request.Form["multiple"];

                        if (appointment.recurring)
                        {
                            //appointment.end_date = null;
                        }

                        if (!string.IsNullOrEmpty(multiplevalue))
                        {
                            editappointment.day = multiplevalue;
                        }
                        else { multiplevalue = "0"; editappointment.day = "0"; }

                        editappointment.updated_date = DateTime.Now;
                        if (!string.IsNullOrEmpty(appointment.notes))
                        {
                            editappointment.notes = appointment.notes;
                        }
                        else
                        {
                            editappointment.notes = "N/A";
                        }
                     //   editappointment.notes = appointment.notes;
                        editappointment.time_range_start = appointment.time_range_start;
                        editappointment.time_range_end = appointment.time_range_end;

                        // add two extra field that store the calendar start time and end time 
                        // it is use for the event of google calendar only
                        // start

                        string startdate1 = appointment.start_date.ToString();
                        DateTime strstartdate1 = Convert.ToDateTime(startdate1);

                        string enddate1 = appointment.end_date.ToString();
                        DateTime strenddate1 = Convert.ToDateTime(enddate1);

                        DateTime cal_startdate = Convert.ToDateTime(strstartdate1.ToString("yyyy/MM/dd") + " " + appointment.time_range_start);
                        DateTime cal_enddate = Convert.ToDateTime(strenddate1.ToString("yyyy/MM/dd") + " " + appointment.time_range_end);

                        editappointment.Calendar_Start_datetime = DateTimeOffset.Parse(cal_startdate.ToString()).UtcDateTime;
                        editappointment.Calendar_End_datetime = DateTimeOffset.Parse(cal_enddate.ToString()).UtcDateTime;

                        string timezone = "West Asia Standard Time";
                        TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                        cal_startdate = TimeZoneInfo.ConvertTimeFromUtc(cal_startdate, targetZone);
                        cal_enddate = TimeZoneInfo.ConvertTimeFromUtc(cal_enddate, targetZone);

                        editappointment.Calendar_Start_datetime = cal_startdate;
                        editappointment.Calendar_End_datetime = cal_enddate;

                        // end

                        // get time into total minutes 
                        string startingtime = appointment.time_range_start.ToString();
                        string endindtime = appointment.time_range_end.ToString();

                        TimeSpan starttime = TimeSpan.Parse(startingtime);
                        TimeSpan endatime = TimeSpan.Parse(endindtime);

                        TimeSpan Difflength = endatime.Subtract(starttime); // difference between start time and end time
                        int total = (Difflength.Hours * 60) + Difflength.Minutes; // convert difference in minutes

                        editappointment.length = total;

                        editappointment.client_id = Convert.ToInt32(appointment.client_id);
                        editappointment.attribute_id = multipleattribute.ToString();
                        editappointment.emp_id = Convert.ToInt32(appointment.emp_id);
                        editappointment.work_id = Convert.ToInt32(appointment.work_id);
                        editappointment.time_interval_id = Convert.ToInt32(appointment.time_interval_id);
                        editappointment.user_id = Convert.ToInt32(Session["User_Id"]);
                        dbcon.Entry(editappointment).State = System.Data.Entity.EntityState.Modified;
                        dbcon.SaveChanges();

                        // delete the appointment already inserted days and again insert into another table
                        //SqlConnection appointmentconnection = new SqlConnection(dbcon.connectionString());
                        //var appointmentcommand = new SqlCommand("appooint_day", appointmentconnection);
                        //appointmentcommand.CommandType = CommandType.StoredProcedure;
                        //appointmentcommand.Parameters.AddWithValue("@appointment_id", Convert.ToInt32(appointment.appointment_id));
                        //appointmentcommand.Parameters.AddWithValue("@type", "deleteappointmentday");
                        //appointmentconnection.Open();
                        //SqlDataReader appointmentreader = appointmentcommand.ExecuteReader();
                        //appointmentconnection.Close();

                        SqlConnection connectiondeleteday = new SqlConnection(dbcon.connectionString());
                        var commanddeleteday = new SqlCommand("Appointmentday", connectiondeleteday);
                        commanddeleteday.CommandType = CommandType.StoredProcedure;
                        commanddeleteday.Parameters.AddWithValue("@maxid", Convert.ToInt32(0));
                        commanddeleteday.Parameters.AddWithValue("@day", Convert.ToInt32(0));
                        commanddeleteday.Parameters.AddWithValue("@appointmentid", Convert.ToInt32(appointment.appointment_id));
                        commanddeleteday.Parameters.AddWithValue("@type", "deleteday");
                        connectiondeleteday.Open();
                        SqlDataReader rdrdelete = commanddeleteday.ExecuteReader();
                        connectiondeleteday.Close();

                        if (!string.IsNullOrEmpty(multiplevalue))
                        {
                            string[] values = multiplevalue.Split(',');
                            for (int i = 0; i < values.Length; i++)
                            {
                                values[i] = values[i].Trim();

                                SqlConnection connectionupdateday = new SqlConnection(dbcon.connectionString());
                                var commandupdateday = new SqlCommand("Appointmentday", connectionupdateday);
                                commandupdateday.CommandType = CommandType.StoredProcedure;
                                commandupdateday.Parameters.AddWithValue("@maxid", Convert.ToInt32(appointment.appointment_id));
                                commandupdateday.Parameters.AddWithValue("@day", Convert.ToInt32(values[i]));
                                commandupdateday.Parameters.AddWithValue("@appointmentid", Convert.ToInt32(0));
                                commandupdateday.Parameters.AddWithValue("@type", "updateday");
                                connectionupdateday.Open();
                                SqlDataReader rdrupdate = commandupdateday.ExecuteReader();
                                connectiondeleteday.Close();
                            }
                        }
                        return RedirectToAction("Appointment", "Appointment");
                    }
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
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        /// <summary>
        /// delete worktype
        /// </summary>
        /// <param name="appointment_id"></param>
        /// <returns></returns>
        /// 

        [HttpPost]
        public ActionResult appointment_delete(Int32 appointment_id)
        {
            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    if (Session["User_Id"] != null)
                    {
                        tbl_appointment objEmp = dbcon.tbl_appointment.Find(appointment_id);
                        dbcon.tbl_appointment.Remove(objEmp);
                        dbcon.SaveChanges();
                        return RedirectToAction("Appointment", "Appointment");
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }

                }
            }
            catch
            {
                return View();
            }
        }


        //// check whether the start time is past time or not of current date
        [AllowAnonymous]
        public JsonResult checktimerangestart(string time_range_start = null, string start_date = null, string end_date = null)
        {
            try
            {
                string timezone = Session["timezone"].ToString();
                //TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time");
                TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                DateTime newDT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, targetZone);
                DateTime today = newDT;
                //DateTime today = DateTime.Now;
                string dating = today.ToString("MM/dd/yyyy");
                TimeSpan time24 = TimeSpan.Parse(time_range_start);

                // convert time start into specific format 
                DateTime startstart = Convert.ToDateTime(time_range_start);
                string test = startstart.ToString("h:mm tt", CultureInfo.CurrentCulture);

                DateTime dt;
                jugglecontext dbcon = new jugglecontext();

                // check whether the time start is given time slot or not (EX: 8:00 AM, 10:00 AM,12:00 AM)

                var gettimeintervaltime = (from a in dbcon.tbl_time_interval
                                           where a.time_start == test
                                           select a).ToList();
                if (gettimeintervaltime.Count <= 0)
                {
                    return Json(string.Format("Select only Hopper Start time"), JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(start_date))
                {
                    dt = Convert.ToDateTime(dating + " " + time_range_start);
                }
                else
                {
                    dt = Convert.ToDateTime(start_date + " " + time_range_start);
                }

                if (dt < today)
                {
                    return Json(string.Format("This time slot has already passed."), JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json("Invalid Time", JsonRequestBehavior.AllowGet);
            }
        }

        //// check whether the start time is past time or not of current date
        [AllowAnonymous]
        public JsonResult checktimerangeend(string time_range_end = null, string time_range_start = null, string start_date = null, string end_date = null, string work_id = null, int time = 0)
        {
            try
            {

                string timezone = Session["timezone"].ToString();
                //TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time");
                TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                DateTime newDT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, targetZone);

                //DateTime today = DateTime.Now;
                DateTime today = newDT;

                string dating = today.ToString("MM/dd/yyyy");

                if (string.IsNullOrEmpty(time_range_start))
                {
                    return Json(string.Format("Please Select Start Time"), JsonRequestBehavior.AllowGet);
                }

                jugglecontext dbcon = new jugglecontext();
                int worktypeid = 0;
                if (!string.IsNullOrEmpty(work_id))
                {
                    worktypeid = Convert.ToInt32(work_id);
                }

                TimeSpan timestart = TimeSpan.Parse(time_range_start);
                TimeSpan timeend = TimeSpan.Parse(time_range_end);
                TimeSpan duration = timeend - timestart;
                DateTime startstart = Convert.ToDateTime(time_range_start);
                DateTime dt = Convert.ToDateTime(start_date + " " + time_range_start);
                bool checktimeofworktype = false;
                TimeSpan timing = TimeSpan.FromMinutes(time);

                if (duration >= timing)
                {
                    checktimeofworktype = true;
                }

                if (timeend < timestart)
                {
                    return Json(string.Format("Start time cannot be greater than end time."), JsonRequestBehavior.AllowGet);
                }

                // convert time end into specific format 
                var result = Convert.ToDateTime(time_range_end);
                string test = result.ToString("h:mm tt", CultureInfo.CurrentCulture);

                // check whether the time start is given time slot or not (EX: 10:00 AM, 12:00 PM,2:00 PM)
                var gettimeintervaltime = (from a in dbcon.tbl_time_interval
                                           where a.time_end == test
                                           select a).ToList();
                if (gettimeintervaltime.Count <= 0)
                {
                    return Json(string.Format("Select only Hopper End time"), JsonRequestBehavior.AllowGet);
                }


                if (dt < today)
                {
                    return Json(string.Format("This time slot has already passed."), JsonRequestBehavior.AllowGet);
                }
                if (timestart > timeend && time_range_end != "00:00")
                {
                    return Json(string.Format("This time slot has already passed."), JsonRequestBehavior.AllowGet);
                }
                if (timestart == timeend)
                {
                    return Json(string.Format("End time cannot be the same as start time."), JsonRequestBehavior.AllowGet);
                }
                if (checktimeofworktype == false)
                {
                    return Json(string.Format("Duration can not be less than service type time."), JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json("Invalid Time", JsonRequestBehavior.AllowGet);
            }
        }

    }

}
