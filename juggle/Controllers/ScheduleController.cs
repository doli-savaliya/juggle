using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using juggle.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Web.Mvc;
using Twilio;


namespace juggle.Controllers
{
    public class ScheduleController : Controller
    {

        private static string UserId = "user"; //System.Web.HttpContext.Current.User.Identity.Name
        private static string gFolder = System.Web.HttpContext.Current.Server.MapPath("/App_Data/MyGoogleStorage");

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            if (Session["User_Id"] != null)
            {
                try
                {
                    //CalendarService Service = GetCalendarService();
                    //Session["googlecal"] = Service;
                    Session.Remove("appointment_save");
                    Session["appointment_save"] = ",";
                }
                catch (Exception ex)
                {
                    Logfile.WriteCDNLog(ex.ToString(), "Index");
                }
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        /// <summary>
        /// get the timeinterval slot from the table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Timeintervaldata()
        {
            try
            {
                List<tbl_time_interval> timeinterval;
                using (jugglecontext dbcon = new jugglecontext())
                {
                    timeinterval = dbcon.tbl_time_interval.Where(time => time.time_interval_id != 1).ToList<tbl_time_interval>();

                    if (timeinterval.Count > 0)
                    {
                        return Json(new { data = timeinterval }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "Timeintervaldata");
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///  get employee detail 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult displayemployee()
        {
            try
            {
                int userId = Convert.ToInt32(Session["User_Id"]);
                using (jugglecontext dbcon1 = new jugglecontext())
                {
                    List<tbl_employee_info> emp_data = dbcon1.tbl_employee_info.Where(eemp => eemp.user_id == userId).OrderByDescending(x => x.emp_firstname).ToList<tbl_employee_info>();
                    var foundDepartments = from a in dbcon1.tbl_employee_info
                                           where a.user_id == userId
                                           select new
                                           {
                                               a.emp_id,
                                               a.emp_firstname,
                                               a.emp_lastname,
                                               a.emp_qualifiedservicetypes,
                                               a.emp_transportion
                                           };
                    if (emp_data.Count > 0)
                    {
                        return Json(new { data = foundDepartments.ToList() }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "displayemployee");
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get Appointments
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult appointment(string time)
        {
            try
            {
                List<tbl_appointment> appointment;
                string[] timing = time.Split('-');
                string time1 = timing[0].ToString();
                string time2 = timing[1].ToString();
                using (jugglecontext dbcon = new jugglecontext())
                {
                    int userId = Convert.ToInt32(Session["User_Id"]);
                    appointment = dbcon.tbl_appointment.Where(eemp => eemp.user_id == userId).ToList<tbl_appointment>();
                    if (appointment.Count > 0)
                    {
                        return Json(new { data = appointment }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "appointment");
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// post Employees detail.
        /// </summary>
        /// <param name="empdata_id">The empdata identifier.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EmployeeDetail(int empdata_id)
        {
            jugglecontext dbcon = new jugglecontext();
            List<string> empdetaildata = new List<string>();
            try
            {
                var emp_data = (from b in dbcon.tbl_employee_info
                                where b.emp_id == empdata_id
                                select new
                                {
                                    EmployeeName = b.emp_firstname + " " + b.emp_lastname,
                                    b.attribute_id,
                                    b.emp_contactinfo,
                                    b.emp_qualifiedservicetypes,
                                    b.emp_transportion,
                                    b.emp_note,
                                }).ToList();

                string[] empsplitattributeid = emp_data[0].attribute_id.ToString().Split(',');
                string[] servicetypesplit = emp_data[0].emp_qualifiedservicetypes.ToString().Split(',');
                string empsplitattributename = null;
                string allempservicetypename = null;

                for (int k = 0; k < servicetypesplit.Length; k++)
                {
                    int emp_servicetypeid = Convert.ToInt32(servicetypesplit[k]);
                    var empservicetype = (from s in dbcon.tbl_worktype
                                          where s.work_id == emp_servicetypeid
                                          select s).ToList();
                    allempservicetypename = allempservicetypename + "," + empservicetype[0].name;
                    allempservicetypename = allempservicetypename.Replace(",", ", \n");
                }
                for (int i = 0; i < empsplitattributeid.Length; i++)
                {
                    int emp_attid = Convert.ToInt32(empsplitattributeid[i]);
                    var empattribute = (from a in dbcon.tbl_attribute_data
                                        where a.attribute_id == emp_attid
                                        select a).ToList();
                    empsplitattributename = empsplitattributename + "," + empattribute[0].attribute_name;
                    empsplitattributename = empsplitattributename.Replace(",", ", \n");
                }
                empdetaildata.Add(emp_data[0].EmployeeName);
                empdetaildata.Add(empsplitattributename);
                empdetaildata.Add(emp_data[0].emp_note);
                empdetaildata.Add(emp_data[0].emp_contactinfo);
                empdetaildata.Add(emp_data[0].emp_transportion);
                empdetaildata.Add(allempservicetypename);

            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "EmployeeDetail");
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { empdetail = empdetaildata }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Rightpaneldatas data using appointmentid.
        /// </summary>
        /// <param name="appointmentid">The appointmentid.</param>
        /// <returns></returns>
        public ActionResult rightpaneldata(string appointmentid)
        {

            List<tbl_client> Caldistance = new List<tbl_client>();
            int appid = Convert.ToInt32(appointmentid);

            jugglecontext dbcon = new jugglecontext();
            List<string> rightpaneldata = new List<string>();
            try
            {
                var appointmentdetail = (from a in dbcon.tbl_appointment
                                         join b in dbcon.tbl_worktype on a.work_id equals b.work_id
                                         where a.appointment_id == appid
                                         select new
                                         {
                                             a.client_id,
                                             a.description,
                                             a.notes,
                                             Time = a.time_range_start + "-" + a.time_range_end,
                                             a.attribute_id,
                                             b.name,
                                             a.time
                                         }).ToList();
                string allappointmentattribute = null;
                string[] appsplitattributeid = appointmentdetail[0].attribute_id.ToString().Split(',');

                var appattribute = (from a in dbcon.tbl_attribute_data
                                    where appsplitattributeid.Contains(a.attribute_id.ToString())
                                    select a).ToList();
                if (appattribute.Count > 0)
                {
                    for (int i = 0; i < appattribute.Count; i++)
                    {
                        allappointmentattribute = allappointmentattribute + "," + appattribute[i].attribute_name;
                        allappointmentattribute = allappointmentattribute.Replace(",", ", \n");
                    }
                    allappointmentattribute = allappointmentattribute.TrimStart(',');
                    allappointmentattribute = allappointmentattribute.TrimEnd(',');
                }
                else
                {
                    allappointmentattribute = " -";
                }
                int custid = Convert.ToInt32(appointmentdetail[0].client_id);
                var customerdetail = (from a in dbcon.tbl_client
                                      where a.client_id == custid
                                      select a).ToList();
                string[] custsplitattributeid = customerdetail[0].attribute_id.ToString().Split(',');

                var custattribute = (from a in dbcon.tbl_attribute_data
                                     where custsplitattributeid.Contains(a.attribute_id.ToString())
                                     select a).ToList();
                string allcustattribute = null;
                for (int i = 0; i < custattribute.Count; i++)
                {
                    allcustattribute = allcustattribute + "," + custattribute[i].attribute_name;
                    allcustattribute = allcustattribute.Replace(",", ", \n");
                }
                allcustattribute = allcustattribute.TrimStart(',');
                allcustattribute = allcustattribute.TrimEnd(',');

                var employeedetail = (from a in dbcon.tbl_appointment
                                      join b in dbcon.tbl_employee_info on a.emp_id equals b.emp_id
                                      where a.appointment_id == appid
                                      select new
                                      {
                                          EmployeeName = b.emp_firstname + " " + b.emp_lastname,
                                          b.attribute_id,
                                          b.emp_note,
                                      }).ToList();

                rightpaneldata.Add(appointmentdetail[0].name.ToString());
                rightpaneldata.Add(customerdetail[0].client_firstname.ToString() + " " + customerdetail[0].client_lastname.ToString());
                rightpaneldata.Add(customerdetail[0].client_secondaryname.ToString());
                rightpaneldata.Add(customerdetail[0].client_address.ToString());
                rightpaneldata.Add(customerdetail[0].client_email.ToString());
                rightpaneldata.Add(customerdetail[0].client_contact_info.ToString());
                rightpaneldata.Add(customerdetail[0].x_lat.ToString());
                rightpaneldata.Add(customerdetail[0].y_long.ToString());
                rightpaneldata.Add(allcustattribute.ToString());

                if (employeedetail.Count > 0)
                {
                    string[] empsplitattributeid = employeedetail[0].attribute_id.ToString().Split(',');

                    var empattribute = (from a in dbcon.tbl_attribute_data
                                        where empsplitattributeid.Contains(a.attribute_id.ToString())
                                        select a).ToList();
                    string allempattribute = null;
                    for (int i = 0; i < custattribute.Count; i++)
                    {
                        allempattribute = allempattribute + "," + empattribute[i].attribute_name;
                    }
                    allempattribute = allempattribute.TrimStart(',');
                    allempattribute = allempattribute.TrimEnd(',');
                    rightpaneldata.Add(employeedetail[0].EmployeeName);
                    rightpaneldata.Add(allempattribute);
                    rightpaneldata.Add(employeedetail[0].emp_note);
                }
                else
                {
                    rightpaneldata.Add("Not Assign");
                    rightpaneldata.Add("-");
                    rightpaneldata.Add("-");
                }
                rightpaneldata.Add(appointmentdetail[0].Time.ToString());
                rightpaneldata.Add(appointmentdetail[0].notes.ToString());
                rightpaneldata.Add(appointmentdetail[0].description.ToString());
                rightpaneldata.Add(allappointmentattribute);
                rightpaneldata.Add(appointmentdetail[0].time.ToString());
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "rightpaneldata");
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { rightpanel = rightpaneldata }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Distancecalculates the specified x log.
        /// </summary>
        /// <param name="x_log">The x log.</param>
        /// <param name="y_log">The y log.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public ActionResult distancecalculate(string x_log, string y_log, string date)
        {
            try
            {
                List<tbl_client> Caldistance = new List<tbl_client>();
                Caldistance = GetAllNearestFamousPlaces(Convert.ToDouble(x_log), Convert.ToDouble(y_log), date);
                return Json(new { Caldistance = Caldistance }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// Appointment data bind for the hopper
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="date">The date.</param>
        /// <param name="timeintervalid">The timeintervalid.</param>
        /// <returns></returns>
        public ActionResult appointmentdata(string time, string date, string timeintervalid)
        {
            try
            {
                //getting appointment data on load 
                jugglecontext dbcon = new jugglecontext();

                DateTime formatdate = Convert.ToDateTime(date);
                string dating = formatdate.ToString("yyyy-MM-dd");
                SqlConnection connection = new SqlConnection(dbcon.connectionString());

                //var command2 = new SqlCommand("updateappointmenteveryday", connection);
                //command2.Parameters.AddWithValue("@date", dating);
                //command2.CommandType = CommandType.StoredProcedure;
                //connection.Open();
                //SqlDataReader rdr1 = command2.ExecuteReader();

                int tbl_timeintervalid = Convert.ToInt32(timeintervalid);
                int timeofappointmet = 0;
                var gettimefromtimeinterval = (from a in dbcon.tbl_time_interval
                                               where a.time_interval_id == tbl_timeintervalid
                                               select a).ToList();

                string[] timing = time.Split('-');
                string time1 = timing[0].ToString();
                string time2 = timing[1].ToString();

                if (gettimefromtimeinterval.Count > 0)
                {
                    time1 = gettimefromtimeinterval[0].time_start.ToString();
                    time2 = gettimefromtimeinterval[0].time_end.ToString();
                }

                List<tbl_appointment> appointmentlist = new List<tbl_appointment>();
                // SqlConnection connection = new SqlConnection(dbcon.connectionString());
                var command = new SqlCommand("AppointMentUi", connection);
                // getting single appointment
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@userId", Convert.ToInt32(Session["User_Id"]));
                command.Parameters.AddWithValue("@timestart", time1);
                command.Parameters.AddWithValue("@timeend", time2);
                command.Parameters.AddWithValue("@date", dating);
                command.Parameters.AddWithValue("@type", "notrecuring");
                connection.Open();

                SqlDataReader rdr = command.ExecuteReader();
                string desc = null;
                int appointmentidforcheck = 0;
                while (rdr.Read())
                {
                    desc = rdr["description"].ToString();
                    //hopper every day
                    appointmentidforcheck = Convert.ToInt32(rdr["appointment_id"]);
                    if (desc.Length > 17)
                    {
                        // get only 18 characters of the desc string 
                        desc = desc.Substring(0, 17) + "...";
                    }
                    //hopper every day
                    var checkineveryappointabhopper = (from a in dbcon.tbl_appointmentEveryday_hopper
                                                       where a.appointment_id == appointmentidforcheck && a.appointment_date == formatdate
                                                       select a).ToList();
                    if (checkineveryappointabhopper.Count <= 0)
                    {
                        timeofappointmet = Convert.ToInt32(rdr["time"]);
                        appointmentlist.Add(new tbl_appointment() { description = desc.ToString(), notes = rdr["color"].ToString(), appointment_id = Convert.ToInt32(rdr["appointment_id"]), time = timeofappointmet });
                    }
                }
                connection.Close();

                // getting recuring appointment
                var command1 = new SqlCommand("AppointMentUi", connection);
                command1.CommandType = CommandType.StoredProcedure;
                command1.Parameters.AddWithValue("@userId", Convert.ToInt32(Session["User_Id"]));
                command1.Parameters.AddWithValue("@timestart", time1);
                command1.Parameters.AddWithValue("@timeend", time2);
                command1.Parameters.AddWithValue("@date", dating);
                command1.Parameters.AddWithValue("@type", "recuring");
                connection.Open();
                SqlDataReader rdrrecuring = command1.ExecuteReader();
                string daynorecuring = null;
                int appointmentidforcheckrecruing = 0;
                while (rdrrecuring.Read())
                {
                    if (Convert.ToBoolean(rdrrecuring["recurring"]))
                    {
                        string[] days = rdrrecuring["day"].ToString().Split(',');
                        if (formatdate.DayOfWeek == DayOfWeek.Sunday)
                        {
                            daynorecuring = "1";
                        }
                        else if (formatdate.DayOfWeek == DayOfWeek.Monday)
                        {
                            daynorecuring = "2";
                        }
                        else if (formatdate.DayOfWeek == DayOfWeek.Tuesday)
                        {
                            daynorecuring = "3";
                        }
                        else if (formatdate.DayOfWeek == DayOfWeek.Wednesday)
                        {
                            daynorecuring = "4";
                        }
                        else if (formatdate.DayOfWeek == DayOfWeek.Thursday)
                        {
                            daynorecuring = "5";
                        }
                        else if (formatdate.DayOfWeek == DayOfWeek.Friday)
                        {
                            daynorecuring = "6";
                        }
                        else if (formatdate.DayOfWeek == DayOfWeek.Saturday)
                        {
                            daynorecuring = "7";
                        }

                        if (days.Contains(daynorecuring))
                        {
                            desc = rdrrecuring["description"].ToString();
                            //hopper every day
                            appointmentidforcheckrecruing = Convert.ToInt32(rdrrecuring["appointment_id"]);
                            if (desc.Length > 17)
                            {
                                // get only 18 characters of the desc string 
                                desc = desc.Substring(0, 17) + "...";
                            }
                            //hopper every day
                            var checkineveryappointabhopperrecuring = (from a in dbcon.tbl_appointmentEveryday_hopper
                                                                       where a.appointment_id == appointmentidforcheckrecruing && a.appointment_date == formatdate
                                                                       select a).ToList();
                            if (checkineveryappointabhopperrecuring.Count <= 0)
                            {
                                timeofappointmet = Convert.ToInt32(rdrrecuring["time"]);
                                appointmentlist.Add(new tbl_appointment() { description = desc, notes = rdrrecuring["color"].ToString(), appointment_id = Convert.ToInt32(rdrrecuring["appointment_id"]), time = timeofappointmet });
                            }

                        }
                    }
                }
                connection.Close();
                return Json(new { appointmentlist = appointmentlist }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Gets all nearest famous places.
        /// </summary>
        /// <param name="currentLatitude">The current latitude.</param>
        /// <param name="currentLongitude">The current longitude.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public List<tbl_client> GetAllNearestFamousPlaces(double currentLatitude, double currentLongitude, string date)
        {
            List<tbl_client> Caldistance = new List<tbl_client>();
            int userid = Convert.ToInt32(Session["User_Id"]);
            jugglecontext dbcon = new jugglecontext();
            var query = (from c in dbcon.tbl_client
                         join a in dbcon.tbl_appointment on c.client_id equals a.client_id
                         where a.user_id == userid
                         select new
                         {
                             c.x_lat,
                             c.y_long,
                             a.start_date,
                             a.end_date
                         }).ToList();
            DateTime startdate;
            DateTime enddate;
            DateTime checkdate;
            checkdate = Convert.ToDateTime(date);
            foreach (var place in query)
            {
                enddate = Convert.ToDateTime(place.end_date);
                startdate = Convert.ToDateTime(place.start_date);
                //      double distance = Distance(currentLatitude, currentLongitude, Convert.ToDouble(place.x_lat), Convert.ToDouble(place.y_long));
                if (checkdate >= place.start_date && checkdate <= place.end_date)
                {
                    tbl_client dist = new tbl_client();
                    dist.x_lat = place.x_lat;
                    dist.y_long = place.y_long;
                    Caldistance.Add(dist);
                }
            }
            return Caldistance;
        }


        /// <summary>
        ///update appointment means assign appointment to employee 
        /// </summary>
        /// <param name="appointmentid">The appointmentid.</param>
        /// <param name="empid">The empid.</param>
        /// <param name="timeintervalidth">The timeintervalidth.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult updateappointment(string appointmentid, string empid, string timeintervalidth)
        {
            try
            {

                int appid = Convert.ToInt32(appointmentid);

                jugglecontext dbcon = new jugglecontext();
                var getappointempid = (from a in dbcon.tbl_appointment
                                       where a.appointment_id == appid
                                       select a).ToList();
                int beforeupdateempid = Convert.ToInt32(getappointempid[0].emp_id);
                if ((Convert.ToInt32(empid) != beforeupdateempid && beforeupdateempid != 0) || empid == "0")
                {
                    sendmailwhenunassign(appointmentid);
                    sendsmswhenunassign(appointmentid);
                }

                SqlConnection connection = new SqlConnection(dbcon.connectionString());
                var command = new SqlCommand("UpdateEmpID", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@emp_id", Convert.ToInt32(empid));
                command.Parameters.AddWithValue("@timeintervalid", Convert.ToInt32(timeintervalidth));
                command.Parameters.AddWithValue("@appointment_id", Convert.ToInt32(appointmentid));
                connection.Open();
                SqlDataReader rdr = command.ExecuteReader();
                int userid = Convert.ToInt32(Session["User_Id"]);

                //Send email to client and employee at drag time
                var command1 = new SqlCommand("Emailsend_clientEmp", connection);
                command1.CommandType = CommandType.StoredProcedure;
                command1.Parameters.AddWithValue("@appointment_id", Convert.ToInt32(appointmentid));
                command1.Parameters.AddWithValue("@emp_id", Convert.ToInt32(empid));
                command1.Parameters.AddWithValue("@userid", Convert.ToInt32(Session["User_Id"]));

                SqlDataReader rdr1 = command1.ExecuteReader();
                string ClientEmail = "";
                string EmpEmial = "";
                string clientname, empname = null;
                string clientphoneno = "";
                string empphoneno = "";
                string clientaddress = "";
                string starttime = "";
                string endtime = "";
                string startdate = "";
                string enddate = "";
                while (rdr1.Read())
                {
                    ClientEmail = rdr1["ClientEmail"].ToString();
                    EmpEmial = rdr1["empEmail"].ToString();
                    empname = rdr1["empname"].ToString();
                    clientname = rdr1["clientname"].ToString();
                    clientphoneno = "+1" + rdr1["clientphoneno"].ToString();
                    empphoneno = "+1" + rdr1["empphoneno"].ToString();
                    clientaddress = rdr1["clientaddress"].ToString();
                    starttime = rdr1["starttime"].ToString();
                    endtime = rdr1["endtime"].ToString();
                    startdate = rdr1["startdate"].ToString();
                    enddate = rdr1["enddate"].ToString();
                }

                if ((!string.IsNullOrEmpty(ClientEmail) && !string.IsNullOrEmpty(EmpEmial)) && !string.IsNullOrEmpty(empname))
                {
                    try
                    {
                        //sendmail(appointmentid);
                    }
                    catch (Exception ex)
                    {
                        Logfile.WriteCDNLog(ex.ToString(), "updateappointment");
                    }

                }
                if ((!string.IsNullOrEmpty(clientphoneno) && !string.IsNullOrEmpty(empphoneno)) && !string.IsNullOrEmpty(empname))
                {
                    sendsms(appointmentid);
                }
                connection.Close();
                return Json(new { error = "true" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "updateappointment");
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// send sms using twilio using appointment id
        /// </summary>
        /// <param name="appointmentid">The appointmentid.</param>
        private void sendsms(string appointmentid)
        {
            int appointmet_id = Convert.ToInt32(appointmentid);
            using (jugglecontext dbcon = new jugglecontext())
            {
                var detail = (from a in dbcon.tbl_appointment
                              join b in dbcon.tbl_employee_info on a.emp_id equals b.emp_id
                              join c in dbcon.tbl_client on a.client_id equals c.client_id
                              join d in dbcon.tbl_worktype on a.work_id equals d.work_id
                              where a.appointment_id == appointmet_id
                              select new
                              {
                                  a.start_date,
                                  a.end_date,
                                  a.description,
                                  a.time_range_start,
                                  a.time_range_end,
                                  b.emp_googlecalendarID,
                                  empname = b.emp_firstname + " " + b.emp_lastname,
                                  clientname = c.client_firstname + " " + c.client_lastname,
                                  c.client_email,
                                  d.name,
                                  a.time,
                                  c.client_address,
                                  c.client_contact_info,
                                  b.emp_contactinfo
                              }).ToList();

                string AccountSid = ConfigurationManager.AppSettings["accountsid"];
                string AuthToken = ConfigurationManager.AppSettings["AuthToken"];

                var twilio = new TwilioRestClient(AccountSid, AuthToken);

                TimeSpan duration = TimeSpan.Parse(detail[0].time_range_end) - TimeSpan.Parse(detail[0].time_range_start);
                DateTime startdate = Convert.ToDateTime(detail[0].start_date);
                string appoinmentduration = detail[0].description + "(" + Convert.ToInt32(detail[0].time) + "Min)";
                string textmsg = "Hello " + detail[0].empname.ToString() + " " + detail[0].description + " You have been scheduled for " + detail[0].name + " for " + detail[0].clientname + " for Duration " + appoinmentduration + " from " + detail[0].time_range_start + " to " + detail[0].time_range_end + " on " + startdate.ToString("dd/MM/yyyy") + " at " + detail[0].client_address + "";
                var validPhoneNumberof_Client = detail[0].client_contact_info;

                validPhoneNumberof_Client = validPhoneNumberof_Client.Replace("+", "");
                validPhoneNumberof_Client = validPhoneNumberof_Client.Replace("(", "");
                validPhoneNumberof_Client = validPhoneNumberof_Client.Replace(")", "");
                validPhoneNumberof_Client = validPhoneNumberof_Client.Replace("-", "");
                validPhoneNumberof_Client = validPhoneNumberof_Client.Replace(" ", "");

                var validPhoneNumberof_Employee = detail[0].emp_contactinfo;
                validPhoneNumberof_Employee = validPhoneNumberof_Employee.Replace("+", "");
                validPhoneNumberof_Employee = validPhoneNumberof_Employee.Replace("(", "");
                validPhoneNumberof_Employee = validPhoneNumberof_Employee.Replace(")", "");
                validPhoneNumberof_Employee = validPhoneNumberof_Employee.Replace("-", "");
                validPhoneNumberof_Employee = validPhoneNumberof_Employee.Replace(" ", "");

                var message = twilio.SendMessage(ConfigurationManager.AppSettings["TwilioNo"], "+1" + validPhoneNumberof_Client, textmsg);
                var message1 = twilio.SendMessage(ConfigurationManager.AppSettings["TwilioNo"], "+1" + validPhoneNumberof_Employee, textmsg);
            }
        }

        /// <summary>
        /// send sms using twilio when appointment unassign
        /// </summary>
        /// <param name="appointmetid">The appointmetid.</param>
        private void sendsmswhenunassign(string appointmetid)
        {
            int appointmet_id = Convert.ToInt32(appointmetid);
            using (jugglecontext dbcon = new jugglecontext())
            {
                var detail = (from a in dbcon.tbl_appointment
                              join b in dbcon.tbl_employee_info on a.emp_id equals b.emp_id
                              join c in dbcon.tbl_client on a.client_id equals c.client_id
                              join d in dbcon.tbl_worktype on a.work_id equals d.work_id
                              where a.appointment_id == appointmet_id
                              select new
                              {
                                  a.start_date,
                                  a.end_date,
                                  a.description,
                                  a.time_range_start,
                                  a.time_range_end,
                                  b.emp_googlecalendarID,
                                  empname = b.emp_firstname + " " + b.emp_lastname,
                                  clientname = c.client_firstname + " " + c.client_lastname,
                                  c.client_email,
                                  d.name,
                                  a.time,
                                  c.client_address,
                                  c.client_contact_info,
                                  b.emp_contactinfo
                              }).ToList();

                string AccountSid = ConfigurationManager.AppSettings["accountsid"];
                string AuthToken = ConfigurationManager.AppSettings["AuthToken"];

                TimeSpan duration = TimeSpan.Parse(detail[0].time_range_end) - TimeSpan.Parse(detail[0].time_range_start);
                DateTime startdate = Convert.ToDateTime(detail[0].start_date);
                string appoinmentduration = detail[0].description + "(" + Convert.ToInt32(detail[0].time) + "Min)";
                var twilio = new TwilioRestClient(AccountSid, AuthToken);
                string textmsg = "Hello " + detail[0].empname.ToString() + " " + detail[0].description + " Appointment has been cancelled or rescheduled " + detail[0].name + " for Customer " + detail[0].clientname + " for Duration " + appoinmentduration + " from " + detail[0].time_range_start + " to " + detail[0].time_range_end + " on " + startdate.ToString("dd/MM/yyyy") + " at " + detail[0].client_address + "";

                var validPhoneNumberof_Client = detail[0].client_contact_info;
                validPhoneNumberof_Client = validPhoneNumberof_Client.Replace("+", "");
                validPhoneNumberof_Client = validPhoneNumberof_Client.Replace("(", "");
                validPhoneNumberof_Client = validPhoneNumberof_Client.Replace(")", "");
                validPhoneNumberof_Client = validPhoneNumberof_Client.Replace("-", "");
                validPhoneNumberof_Client = validPhoneNumberof_Client.Replace(" ", "");

                var validPhoneNumberof_Employee = detail[0].emp_contactinfo;
                validPhoneNumberof_Employee = validPhoneNumberof_Employee.Replace("+", "");
                validPhoneNumberof_Employee = validPhoneNumberof_Employee.Replace("(", "");
                validPhoneNumberof_Employee = validPhoneNumberof_Employee.Replace(")", "");
                validPhoneNumberof_Employee = validPhoneNumberof_Employee.Replace("-", "");
                validPhoneNumberof_Employee = validPhoneNumberof_Employee.Replace(" ", "");

                var message = twilio.SendMessage(ConfigurationManager.AppSettings["TwilioNo"], "+1" + validPhoneNumberof_Client, textmsg);
                var message1 = twilio.SendMessage(ConfigurationManager.AppSettings["TwilioNo"], "+1" + validPhoneNumberof_Employee, textmsg);
            }

        }

        /// <summary>
        ///send mail to client and employee when unassign appointment
        /// </summary>
        /// <param name="appointmetid">The appointmetid.</param>
        public void sendmailwhenunassign(string appointmetid)
        {
            MailMessage mail = new MailMessage();
            int appointmentid = Convert.ToInt32(appointmetid);
            using (jugglecontext dbcon = new jugglecontext())
            {
                var detail = (from a in dbcon.tbl_appointment
                              join b in dbcon.tbl_employee_info on a.emp_id equals b.emp_id
                              join c in dbcon.tbl_client on a.client_id equals c.client_id
                              join d in dbcon.tbl_worktype on a.work_id equals d.work_id
                              where a.appointment_id == appointmentid
                              select new
                              {
                                  a.start_date,
                                  a.end_date,
                                  a.description,
                                  a.time_range_start,
                                  a.time_range_end,
                                  b.emp_googlecalendarID,
                                  empname = b.emp_firstname + " " + b.emp_lastname,
                                  clientname = c.client_firstname + " " + c.client_lastname,
                                  c.client_email,
                                  d.name,
                                  a.time,
                                  c.client_address
                              }).ToList();

                TimeSpan duration = TimeSpan.Parse(detail[0].time_range_end) - TimeSpan.Parse(detail[0].time_range_start);
                DateTime startdate = Convert.ToDateTime(detail[0].start_date);
                string appoinmentduration = detail[0].description + "(" + Convert.ToInt32(detail[0].time) + "Min)";

                string Body = "Hello" + detail[0].empname.ToString() + " Appointment has been cancelled or rescheduled Service Type " + detail[0].name + " + for Customer " + detail[0].clientname + " for Duration " + appoinmentduration + " from " + detail[0].time_range_start + " to " + detail[0].time_range_end + " on " + startdate + " at " + detail[0].client_address + "";
                dbcon.SendMail(detail[0].client_email + ',' + detail[0].emp_googlecalendarID, "Appointment", Body);

            }
        }

        /// <summary>
        /// send mail to client and employee when drop appointment
        /// </summary>
        /// <param name="appointmetid">The appointmetid.</param>
        public void sendmail(string appointmetid, string empid, string timeintervalid, string date)
        {

            int appointmentid = Convert.ToInt32(appointmetid);
            int emp_id = Convert.ToInt32(empid);
            int timeinterval_id = Convert.ToInt32(timeintervalid);
            DateTime dating = Convert.ToDateTime(date);
            using (jugglecontext dbcon = new jugglecontext())
            {
                var details = (from a in dbcon.tbl_appointment
                               join b in dbcon.tbl_employee_info on a.emp_id equals b.emp_id
                               join c in dbcon.tbl_client on a.client_id equals c.client_id
                               join d in dbcon.tbl_worktype on a.work_id equals d.work_id
                               where a.appointment_id == appointmentid
                               select new
                               {
                                   a.start_date,
                                   a.end_date,
                                   a.description,
                                   a.Calendar_Start_datetime,
                                   a.Calendar_End_datetime,
                                   a.time_range_start,
                                   a.time_range_end,
                                   b.emp_googlecalendarID,
                                   empname = b.emp_firstname + " " + b.emp_lastname,
                                   clientname = c.client_firstname + " " + c.client_lastname,
                                   c.client_email,
                                   d.name,
                                   a.time,
                                   c.client_address,
                                   a.notes,
                                   c.client_note,
                                   d.color,
                                   a.recurring,
                                   a.day
                               }).ToList();

                var detail = (from a in dbcon.tbl_appointmentEveryday_hopper
                              join b in dbcon.tbl_employee_info on a.emp_id equals b.emp_id
                              join c in dbcon.tbl_appointment on a.appointmentEveryday_id equals c.appointment_id
                              join d in dbcon.tbl_client on c.client_id equals d.client_id
                              join e in dbcon.tbl_worktype on c.work_id equals e.work_id
                              where a.appointment_id == appointmentid && a.emp_id == emp_id && a.timeinterval_id == timeinterval_id && a.appointment_date == dating
                              select new
                              {
                                  c.start_date,
                                  c.end_date,
                                  c.description,
                                  c.Calendar_Start_datetime,
                                  c.Calendar_End_datetime,
                                  c.time_range_start,
                                  c.time_range_end,
                                  c.time,
                                  c.notes,
                                  c.recurring,
                                  c.day,
                                  b.emp_googlecalendarID,
                                  empname = b.emp_firstname + " " + b.emp_lastname,
                                  clientname = d.client_firstname + " " + d.client_lastname,
                                  d.client_email,
                                  d.client_address,
                                  d.client_note,
                                  e.name,
                                  e.color
                              }).ToList();



                TimeSpan duration = TimeSpan.Parse(detail[0].time_range_end) - TimeSpan.Parse(detail[0].time_range_start);
                DateTime startdate = Convert.ToDateTime(detail[0].start_date);
                string appoinmentduration = detail[0].description + "(" + Convert.ToInt32(detail[0].time) + "Min)";

                string Body = "Hello " + detail[0].empname.ToString() + " you have been scheduled for  " + detail[0].name + " for Customer " + detail[0].clientname + " for Duration " + appoinmentduration + " from " + detail[0].time_range_start + " to " + detail[0].time_range_end + " on " + startdate.ToString("dd/MM/yyyy") + " at " + detail[0].client_address + "";
                dbcon.SendMail(detail[0].client_email + ',' + detail[0].emp_googlecalendarID, "Appointment", Body);

                string Start_datetime = Convert.ToDateTime(detail[0].start_date).ToString("yyyy-MM-dd") + " " + detail[0].time_range_start.ToString() + ":00.00";
                string End_datetime = Convert.ToDateTime(detail[0].end_date).ToString("yyyy-MM-dd") + " " + detail[0].time_range_end.ToString() + ":00.00";

                DateTime startdating = Convert.ToDateTime(Start_datetime);
                DateTime enddating = Convert.ToDateTime(End_datetime);

                var service = (CalendarService)Session["googlecal"];
                TimeSpan durationgoogle = TimeSpan.Parse(detail[0].time_range_end) - TimeSpan.Parse(detail[0].time_range_start);
                int duratoion = Convert.ToInt32(detail[0].time);
                Session["StartDate"] = Convert.ToDateTime(Start_datetime);
                Session["EndDate"] = Convert.ToDateTime(End_datetime);
                bool recurring = detail[0].recurring;

                DateTime cal_Startdate = Convert.ToDateTime(detail[0].Calendar_Start_datetime);
                DateTime cal_enddate = Convert.ToDateTime(detail[0].Calendar_End_datetime);

                // To show appointment on Google calendar at particular date
                string timeofenddate = cal_enddate.ToString("HH:mm");

                //if (detail[0].recurring)
                //{
                //    // if appointment is recuring then show appointment till the 3 month of the start date of the appointment start date
                //    DateTime dateIteratorAddappointmentrecuring = cal_Startdate;
                //    string currentappontmentday = detail[0].day.ToString();
                //    string recviersEmail = detail[0].emp_googlecalendarID + ';' + detail[0].client_email;
                //    int i = 0;
                //    while (dateIteratorAddappointmentrecuring < cal_enddate.AddMonths(1))
                //    {
                //        string itertorday = null;
                //        DateTime endatetoattach = Convert.ToDateTime(dateIteratorAddappointmentrecuring.ToString("yyyy/MM/dd") + " " + timeofenddate);
                //        if (Convert.ToDateTime(cal_enddate.ToString("yyyy/MM/dd")) > Convert.ToDateTime(enddating.ToString("yyyy/MM/dd")))
                //        {
                //            endatetoattach = endatetoattach.AddDays(1);
                //        }

                //        if (dateIteratorAddappointmentrecuring.DayOfWeek == DayOfWeek.Sunday)
                //        {
                //            itertorday = "1";
                //        }
                //        if (dateIteratorAddappointmentrecuring.DayOfWeek == DayOfWeek.Monday)
                //        {
                //            itertorday = "2";
                //        }
                //        if (dateIteratorAddappointmentrecuring.DayOfWeek == DayOfWeek.Tuesday)
                //        {
                //            itertorday = "3";
                //        }
                //        if (dateIteratorAddappointmentrecuring.DayOfWeek == DayOfWeek.Wednesday)
                //        {
                //            itertorday = "4";
                //        }
                //        if (dateIteratorAddappointmentrecuring.DayOfWeek == DayOfWeek.Thursday)
                //        {
                //            itertorday = "5";
                //        }
                //        if (dateIteratorAddappointmentrecuring.DayOfWeek == DayOfWeek.Friday)
                //        {
                //            itertorday = "6";
                //        }
                //        if (dateIteratorAddappointmentrecuring.DayOfWeek == DayOfWeek.Saturday)
                //        {
                //            itertorday = "7";
                //        }

                //        if (currentappontmentday.Contains(itertorday))
                //        {
                //            addeventtocalendar(detail[0].emp_googlecalendarID.ToString(), detail[0].client_email.ToString(), dateIteratorAddappointmentrecuring, endatetoattach, detail[0].empname, detail[0].name, detail[0].clientname, Convert.ToDateTime(detail[0].time_range_start), Convert.ToDateTime(detail[0].time_range_end), duratoion, detail[0].client_address, detail[0].notes, detail[0].client_note, detail[0].color, appoinmentduration, recurring);
                //            //  addeventtocalendar(detail[0].client_email.ToString(), dateIteratorAddappointmentrecuring, endatetoattach, detail[0].empname, detail[0].name, detail[0].clientname, Convert.ToDateTime(detail[0].time_range_start), Convert.ToDateTime(detail[0].time_range_end), duratoion, detail[0].client_address, detail[0].notes, detail[0].client_note, detail[0].color, appoinmentduration, recurring);
                //        }

                //        dateIteratorAddappointmentrecuring = dateIteratorAddappointmentrecuring.AddDays(1);
                //    }
                //}
                //else
                //{
                //    addeventtocalendar(detail[0].emp_googlecalendarID.ToString(), detail[0].client_email.ToString(), cal_Startdate, cal_enddate, detail[0].empname, detail[0].name, detail[0].clientname, Convert.ToDateTime(detail[0].time_range_start), Convert.ToDateTime(detail[0].time_range_end), duratoion, detail[0].client_address, detail[0].notes, detail[0].client_note, detail[0].color, appoinmentduration, recurring);
                //    //    addeventtocalendar(detail[0].emp_googlecalendarID.ToString(), detail[0].client_email.ToString(), cal_Startdate, cal_enddate, detail[0].empname, detail[0].name, detail[0].clientname, Convert.ToDateTime(detail[0].time_range_start), Convert.ToDateTime(detail[0].time_range_end), duratoion, detail[0].client_address, detail[0].notes, detail[0].client_note, detail[0].color, appoinmentduration, recurring);

                //}

                addeventtocalendar(detail[0].emp_googlecalendarID.ToString(), detail[0].client_email.ToString(), dating, dating, detail[0].empname, detail[0].name, detail[0].clientname, Convert.ToDateTime(detail[0].time_range_start), Convert.ToDateTime(detail[0].time_range_end), duratoion, detail[0].client_address, detail[0].notes, detail[0].client_note, detail[0].color, appoinmentduration, recurring);
            }

        }
        /// <summary>
        /// Gets the calendar service.
        /// </summary>
        /// <returns></returns>
        public CalendarService GetCalendarService()
        {
            CalendarService service = null;
            IAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = GetClientConfiguration().Secrets,
                    DataStore = new FileDataStore(gFolder),
                    Scopes = new[] { CalendarService.Scope.Calendar }
                });

            var uri = System.Web.HttpContext.Current.Request.Url.ToString();
            var code = System.Web.HttpContext.Current.Request["code"];
            if (code != null)
            {
                var token = flow.ExchangeCodeForTokenAsync(UserId, code,
                    uri.Substring(0, uri.IndexOf("?")), CancellationToken.None).Result;

                var oauthState = AuthWebUtility.ExtracRedirectFromState(
                    flow.DataStore, UserId, System.Web.HttpContext.Current.Request["state"]).Result;
                System.Web.HttpContext.Current.Response.Redirect(oauthState);
            }
            else
            {
                var result = new AuthorizationCodeWebApp(flow, uri, uri).AuthorizeAsync(UserId,
                    CancellationToken.None).Result;
                if (result.RedirectUri != null)
                {
                    Response.Redirect(result.RedirectUri);
                }
                else
                {
                    // The data store contains the user credential, so the user has been already authenticated.
                    service = new CalendarService(new BaseClientService.Initializer
                    {
                        ApplicationName = "Juggle",
                        HttpClientInitializer = result.Credential
                    });
                }
            }

            return service;
        }
        public GoogleClientSecrets GetClientConfiguration()
        {
            using (var stream = new FileStream(Server.MapPath("/Googlecal_json/client_secret.json"), FileMode.Open, FileAccess.Read))
            {
                return GoogleClientSecrets.Load(stream);
            }
        }


        /// <summary>
        /// Addeventtocalendars the specified client email.
        /// </summary>
        /// <param name="clientEmail">The client email.</param>
        /// <param name="empEmail">The emp email.</param>
        /// <param name="startdateandtime">The startdateandtime.</param>
        /// <param name="enddateandtime">The enddateandtime.</param>
        /// <param name="empname">The empname.</param>
        /// <param name="clientname">The clientname.</param>
        /// <param name="name">The name.</param>
        /// <param name="timerangestart">The timerangestart.</param>
        /// <param name="timerangeend">The timerangeend.</param>
        /// <param name="durationgoogle">The durationgoogle.</param>
        /// <param name="clientaddress">The clientaddress.</param>
        /// <param name="appointmentnotes">The appointmentnotes.</param>
        /// <param name="clientnotes">The clientnotes.</param>
        /// <param name="color">The color.</param>
        /// <param name="appoinmentduration">The appoinmentduration.</param>
        /// <param name="recurring">if set to <c>true</c> [recurring].</param>
        private void addeventtocalendar(string clientEmail, string empEmail, DateTime startdateandtime, DateTime enddateandtime, string empname, string clientname, string name, DateTime timerangestart, DateTime timerangeend, int durationgoogle, string clientaddress, string appointmentnotes, string clientnotes, string color, string appoinmentduration, bool recurring)
        {

            CalendarService service = GetCalendarService();

            var attendees = new List<EventAttendee>
                  {
                  new EventAttendee()
                  {
                          Email = clientEmail
                  },
                  new EventAttendee()
                  {
                          Email = empEmail
                  }
                  };

            Event newEvent = new Event();

            newEvent.Attendees = attendees;
            newEvent.Summary = appoinmentduration;
            newEvent.Description = "Appointment Note:" + appointmentnotes + "</br> " + " Customer Note:" + clientnotes + "";
            newEvent.Start = new EventDateTime();
            newEvent.Start.DateTime = Convert.ToDateTime(startdateandtime);
            newEvent.End = new EventDateTime();
            newEvent.End.DateTime = Convert.ToDateTime(enddateandtime);
            newEvent.Location = clientaddress;
            service.Events.Insert(newEvent, "primary").ExecuteAsync();
        }

        public static DateTime GetTimeZoneOffset(DateTime dt, string win32Id)
        {
            var timeUtc = dt.ToUniversalTime();
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(win32Id);
            DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);
            return cstTime;

        }

        /// <summary>
        /// Validations the specified appointmentid.
        /// </summary>
        /// <param name="appointmentid">The appointmentid.</param>
        /// <param name="empid">The empid.</param>
        /// <param name="timeintervalidth">The timeintervalidth.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult validation(string appointmentid, string empid, string timeintervalidth, string date)
        {
            try
            {
                int appid = Convert.ToInt32(appointmentid);
                int emp_id = Convert.ToInt32(empid);
                int userid = Convert.ToInt32(Session["User_Id"]);

                // declare the variables

                int timeintervalidempav = Convert.ToInt32(timeintervalidth);
                TimeSpan previousappointmentdiff = TimeSpan.Zero;
                TimeSpan nextappointmentdiff = TimeSpan.Zero;
                DateTime dating = Convert.ToDateTime(date);
                TimeSpan starttimeofcurrentappointment = TimeSpan.Zero;
                TimeSpan endtimeofcurrentappointment = TimeSpan.Zero;
                TimeSpan starttimeofnextappointment = TimeSpan.Zero;
                TimeSpan endtimeofnextappointment = TimeSpan.Zero;
                TimeSpan starttimeofpreviousappointment = TimeSpan.Zero;
                TimeSpan endtimeofpreviousappointment = TimeSpan.Zero;
                TimeSpan calculateTime = TimeSpan.Zero;


                jugglecontext dbcon = new jugglecontext();

                // get current appointment data
                var appointment_val = (from a in dbcon.tbl_appointment
                                       where a.appointment_id == appid
                                       select a).ToList();

                // get the time interval data from the timeinterval id
                var timeintervaldata = (from a in dbcon.tbl_time_interval
                                        where a.time_interval_id == timeintervalidempav
                                        select a).ToList();

                ArrayList list = new ArrayList();
                int timeintervalid = 0;

                string timeinterval = timeintervaldata[0].time_interval;
                string[] splittimeinterval = timeinterval.Split('-');

                DateTime StartTimeinterval = DateTime.Parse(splittimeinterval[0]);
                DateTime EndTimeinterval = DateTime.Parse(splittimeinterval[1]);

                string time1emp_av = StartTimeinterval.ToString("HH:mm");
                string time2emp_av = EndTimeinterval.ToString("HH:mm");

                // check whether employee is available or not in tbl_employee_availability table
                SqlConnection empconnection = new SqlConnection(dbcon.connectionString());
                var empcommand = new SqlCommand("compare_client_Availibility", empconnection);
                empcommand.CommandType = CommandType.StoredProcedure;
                empcommand.Parameters.AddWithValue("@startdate", dating);
                empcommand.Parameters.AddWithValue("@enddate", dating);
                empcommand.Parameters.AddWithValue("@StartTime", splittimeinterval[0].ToString());
                empcommand.Parameters.AddWithValue("@endTime", splittimeinterval[1].ToString());
                empcommand.Parameters.AddWithValue("@emp_id", Convert.ToInt32(emp_id));
                empcommand.Parameters.AddWithValue("@empavailability_id", Convert.ToInt32(0));
                empcommand.Parameters.AddWithValue("@userid", userid);
                empconnection.Open();
                SqlDataReader employeeava = empcommand.ExecuteReader();
                Boolean checkempav = employeeava.Read();

                // if user is log out
                if (userid == 0)
                {
                    list.Add("Login Failed");
                    list.Add("false");
                    return Json(new { data = list }, JsonRequestBehavior.AllowGet);
                }

                // check whether employee is available or not
                // if (!checkempav && emp_id != 0)
                if (checkempav && emp_id != 0) // change Swap Employee Availability to Employee Unavailability
                {
                    list.Add("Employee is Unavilable!");
                    list.Add("false");
                    return Json(new { data = list }, JsonRequestBehavior.AllowGet);
                }
                empconnection.Close();


                if (appointment_val.Count > 0)
                {
                    if (Convert.ToInt32(timeintervalidth) == appointment_val[0].time_interval_id && emp_id == appointment_val[0].emp_id)
                    {
                        //list.Add("Success");
                        //list.Add("nothing");
                        //return Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    }
                }


                if (empid == "0" && appointmentid == "0")
                {
                    list.Add("");
                    list.Add("nothing");
                    return Json(new { data = list }, JsonRequestBehavior.AllowGet);
                }


                // when unassign the appointment means drop appointment again into hopper section
                if (empid == "0")
                {
                    SqlConnection connection1 = new SqlConnection(dbcon.connectionString());
                    var command1 = new SqlCommand("Check_timeslot", connection1);
                    command1.CommandType = CommandType.StoredProcedure;
                    command1.Parameters.AddWithValue("@timestart", appointment_val[0].time_range_start);
                    command1.Parameters.AddWithValue("@timeend", appointment_val[0].time_range_end);
                    command1.Parameters.AddWithValue("@type", "unassign");
                    connection1.Open();
                    SqlDataReader rdr = command1.ExecuteReader();

                    while (rdr.Read())
                    {
                        timeintervalid = Convert.ToInt32(rdr["time_interval_id"]);
                    }
                    if (timeintervalid == Convert.ToInt32(timeintervalidth))
                    {
                        list.Add("Success");
                        list.Add("true");
                    }
                    else
                    {
                        list.Add("");
                        list.Add("false");
                    }
                    string assignsaveappointmentdata = null;
                    try
                    {
                        string getsaveappointmentsessionstr = Session["appointment_save"].ToString();
                        int index1 = getsaveappointmentsessionstr.IndexOf(appointmentid);
                        int index2 = getsaveappointmentsessionstr.IndexOf("-" + timeintervalidth);

                        string result2 = getsaveappointmentsessionstr.Remove(index1, index2 + 1);
                        assignsaveappointmentdata = result2;
                    }
                    catch (Exception ex)
                    {
                        Logfile.WriteCDNLog(ex.ToString(), "validation");
                    }

                    string temptoshowdatatosave = null;


                    ////// then we need toi remove that appointment first 

                    string sessionappointment = Session["appointment_save"].ToString();
                    string[] sessionappointmentarray = sessionappointment.Split(',');
                    List<string> sessionlist = new List<string>(sessionappointmentarray);

                    for (int i = 0; i < sessionappointmentarray.Length; i++)
                    {
                        string[] sessionappointmentarraysplit = sessionappointmentarray[i].ToString().Split('-');
                        if (sessionappointmentarraysplit[0].ToString().IndexOf(appointmentid) == 0)
                        {
                            sessionlist.RemoveAt(i);
                        }
                    }
                    sessionappointmentarray = sessionlist.ToArray();

                    Session["appointment_save"] = sessionappointmentarray;

                    ///////////  end

                    string topassintosession = appointmentid + "-" + empid + "-" + timeintervalidth;
                    Session["appointment_save"] = string.Join(",", sessionappointmentarray) + "," + topassintosession;

                    //string topunassintosession = appointmentid + "-" + empid + "-" + timeintervalidth;
                    //assignsaveappointmentdata = Session["appointment_save"] + "," + assignsaveappointmentdata + topunassintosession;
                    //Session["appointment_save"] = assignsaveappointmentdata;
                    temptoshowdatatosave = Session["appointment_save"].ToString();
                    return Json(new { data = list }, JsonRequestBehavior.AllowGet);
                }


                // assign the appointment to employee and check needed validation
                else
                {
                    SqlConnection connection = new SqlConnection(dbcon.connectionString());
                    bool validationtocheckinbetweenappointment = false;

                    // get employee data
                    var emp_val = (from a in dbcon.tbl_employee_info
                                   where a.emp_id == emp_id
                                   select a).ToList();

                    // attribute validation
                    string emp_attribute = emp_val[0].attribute_id.ToString(); // employee attribute 
                    string appoimet_attribute = appointment_val[0].attribute_id.ToString(); // current appointment attribute 
                    int clientid = Convert.ToInt32(appointment_val[0].client_id);
                    string appointmnet_desc = appointment_val[0].description.ToString();

                    // get client detail
                    var clientdetail = (from a in dbcon.tbl_client
                                        where a.client_id == clientid
                                        select a).ToList();

                    string client_attribute = clientdetail[0].attribute_id.ToString(); // client attribute 
                    Boolean checksameattribute = false;

                    // attribute validation start 
                    var stringCollectionemp = emp_attribute.Split(',');
                    var stringCollectionclient = client_attribute.Split(',');
                    var stringCollectionapp = appoimet_attribute.Split(',');

                    if (string.IsNullOrEmpty(appoimet_attribute))
                    {
                        Array.Resize(ref stringCollectionapp, stringCollectionapp.Length - 1);
                    }
                    string[] union = stringCollectionclient.Union(stringCollectionapp).ToArray();
                    string[] intercept = stringCollectionemp.Intersect(union).ToArray();

                    int count = 0;
                    foreach (string value in union)
                    {
                        foreach (string un in intercept)
                        {
                            if (value == un)
                            {
                                count++;
                            }
                        }
                    }
                    if (count == union.Length)
                    {
                        checksameattribute = true;
                    }
                    // attribute validation end 

                    // get appointment of the same employee 
                    var appointment_val_byemp = (from a in dbcon.tbl_appointment
                                                 where a.emp_id == emp_id && (a.start_date <= dating && a.end_date >= dating)
                                                 select a).ToList();

                    // get recuring appointment of the same employee 
                    var recuringappointment_val_byemp = (from a in dbcon.tbl_appointment
                                                         where a.emp_id == emp_id && a.recurring == true
                                                         select a).ToList();


                    /// commment start

                    //// get total appointment of current time block data of current employee data without recuring
                    //var get_current_emp_timeblock_withoutrecuring = (from a in dbcon.tbl_appointment
                    //                                                 where a.emp_id == emp_id && a.time_interval_id == timeintervalidempav && (a.start_date <= dating && a.end_date >= dating) && a.recurring == false
                    //                                                 select a).ToList();

                    //// get total appointment of current time block data of current employee data with recuring
                    //var get_current_emp_timeblock_withrecuring = (from a in dbcon.tbl_appointment
                    //                                              where a.emp_id == emp_id && a.time_interval_id == timeintervalidempav && a.recurring == true
                    //                                              select a).ToList();

                    // get total appointment of current time block data of current employee data with recuring


                    // get the appointment from the appointment table and everyday assign appointment table
                    var get_current_emp_timeblock_withoutrecuring = (from a in dbcon.tbl_appointmentEveryday_hopper
                                                                     join b in dbcon.tbl_appointment on a.appointment_id equals b.appointment_id
                                                                     //where a.emp_id == emp_id && a.appointment_date == dating && b.time_interval_id == timeintervalidempav
                                                                     where a.emp_id == emp_id && a.appointment_date == dating && a.timeinterval_id == timeintervalidempav
                                                                     select new
                                                                     {
                                                                         a.appointment_id,
                                                                         b.time,
                                                                         b.emp_id,
                                                                         a.timeinterval_id
                                                                     }).ToList();


                    /// commment end

                    int totaldurationofappointment = 0;
                    int statictravelbuffer = 0;
                    int fixtotalblocktime = 120;
                    string currentday = "0";
                    string dayofrecuringappointment = null;
                    try
                    {
                        // session value for the save appointment 
                        //string tempsessionstr = Session["appointment_save"].ToString();
                        //if (tempsessionstr.IndexOf("System.String[]") == 0)
                        //{
                        //    tempsessionstr=tempsessionstr.Replace("System.String[]", "");
                        //    Session["appointment_save"] = tempsessionstr;
                        //}

                        string sessionvariablefordraggedappointment = Session["appointment_save"].ToString();
                        string[] splitsessionvariablefordraggedappointment = sessionvariablefordraggedappointment.Split(',');
                        splitsessionvariablefordraggedappointment = splitsessionvariablefordraggedappointment.Distinct().ToArray();
                        //if (splitsessionvariablefordraggedappointment.Length > 0)
                        //{
                        //    for (int loopi = 0; loopi < splitsessionvariablefordraggedappointment.Length; loopi++)
                        //    {
                        //        string[] splitinner = splitsessionvariablefordraggedappointment[loopi].ToString().Split('-');
                        //        if (splitinner[0].ToString() == appointmentid.ToString())
                        //        {
                        //            //int index11 = sessionvariablefordraggedappointment.IndexOf(emp_id.ToString());
                        //            int index11 = sessionvariablefordraggedappointment.IndexOf(appointmentid.ToString());
                        //            //int index112 = sessionvariablefordraggedappointment.IndexOf(",");
                        //            int index112 = sessionvariablefordraggedappointment.IndexOf(',', sessionvariablefordraggedappointment.IndexOf(appointmentid.ToString()) + 1);
                        //            string result2 = sessionvariablefordraggedappointment.Remove(index11, index112);
                        //            Session["appointment_save"] = null;
                        //            Session["appointment_save"] = result2;
                        //            result2 = Session["appointment_save"].ToString();
                        //        }
                        //    }
                        //}



                    }
                    catch (Exception ex)
                    {

                    }



                    //// comment start 
                    //if (get_current_emp_timeblock_withrecuring.Count > 0)
                    //{
                    //    for (int i = 0; i < get_current_emp_timeblock_withrecuring.Count; i++)
                    //    {
                    //        dayofrecuringappointment = get_current_emp_timeblock_withrecuring[i].day;
                    //        if (dayofrecuringappointment.Contains(currentday))
                    //        {
                    //            totaldurationofappointment = totaldurationofappointment + statictravelbuffer + Convert.ToInt32(get_current_emp_timeblock_withrecuring[i].time);
                    //        }
                    //    }
                    //}
                    //if (get_current_emp_timeblock_withoutrecuring.Count > 0)
                    //{
                    //    for (int i = 0; i < get_current_emp_timeblock_withoutrecuring.Count; i++)
                    //    {
                    //        totaldurationofappointment = totaldurationofappointment + statictravelbuffer + Convert.ToInt32(get_current_emp_timeblock_withoutrecuring[i].time);
                    //    }
                    //}

                    string sesionvarfor120blocktest = Session["appointment_save"].ToString();
                    if (get_current_emp_timeblock_withoutrecuring.Count > 0)
                    {
                        for (int i = 0; i < get_current_emp_timeblock_withoutrecuring.Count; i++)
                        {
                            string testing = get_current_emp_timeblock_withoutrecuring[i].appointment_id + "-" + get_current_emp_timeblock_withoutrecuring[i].emp_id + "-" + get_current_emp_timeblock_withoutrecuring[i].timeinterval_id;
                            sesionvarfor120blocktest = sesionvarfor120blocktest + "," + testing;
                        }
                    }

                    string[] sesionvarfor120blocksplittest = sesionvarfor120blocktest.Split(',');

                    List<string> sessionlist = new List<string>(sesionvarfor120blocksplittest);

                    if (get_current_emp_timeblock_withoutrecuring.Count > 0)
                    {
                        for (int i = 0; i < get_current_emp_timeblock_withoutrecuring.Count; i++)
                        {
                            int sessioncount = 0;
                            for (int j = 0; j < sessionlist.Count; j++)
                            {
                                if (sessionlist[j].ToString().IndexOf(get_current_emp_timeblock_withoutrecuring[i].appointment_id.ToString() + "-") == 0)
                                //if (sesionvarfor120blocksplittest[j].ToString().IndexOf(get_current_emp_timeblock_withoutrecuring[i].appointment_id.ToString() + "-") == 0)
                                {
                                    //Response.Write("found <br />");
                                    sessioncount++;
                                }

                                //if (sesionvarfor120blocksplittest[j].ToString().IndexOf(get_current_emp_timeblock_withoutrecuring[i].appointment_id.ToString() + "-") == 0 && sessioncount >= 2)
                                if (sessionlist[j].ToString().IndexOf(get_current_emp_timeblock_withoutrecuring[i].appointment_id.ToString() + "-") == 0 && sessioncount >= 2)
                                {
                                    sessionlist.RemoveAt(j);
                                }
                            }
                        }
                    }



                    sesionvarfor120blocksplittest = sessionlist.ToArray();

                    for (int ses = 0; ses < sesionvarfor120blocksplittest.Length; ses++)
                    {
                        string[] innnersplittest = sesionvarfor120blocksplittest[ses].ToString().Split('-');

                        if (!string.IsNullOrEmpty(innnersplittest[0].ToString()))
                        {
                            if (innnersplittest[1].ToString() == emp_id.ToString() && innnersplittest[2].ToString() == timeintervalidth.ToString())
                            {
                                int tempappid = Convert.ToInt32(innnersplittest[0]);
                                var getapptime = (from a in dbcon.tbl_appointment
                                                  where a.appointment_id == tempappid
                                                  select a).ToList();
                                totaldurationofappointment = totaldurationofappointment + Convert.ToInt32(getapptime[0].time);
                            }
                        }
                    }






                    //// comment end

                    // get current appointment detail
                    var get_current_appointment = (from a in dbcon.tbl_appointment
                                                   where a.appointment_id == appid
                                                   select a).ToList();


                    // testing comment start
                    //var get_current_appointment = (from a in dbcon.tbl_appointmentEveryday_hopper
                    //                               join b in dbcon.tbl_appointment on a.appointment_id equals b.appointment_id

                    //                               where a.appointment_id == appid && a.appointment_date == dating
                    //                               select new
                    //                               {
                    //                                   a.appointment_id,
                    //                                   b.time
                    //                               }
                    //                               ).ToList();

                    // testing comment end

                    int appoinemtnworktypecat = 0;
                    string empworktypecat = string.Empty;
                    if (get_current_appointment.Count > 0)
                    {
                        int currentappointmetduration = Convert.ToInt32(get_current_appointment[0].time) + statictravelbuffer;
                        empworktypecat = emp_val[0].emp_qualifiedservicetypes.ToString();
                        appoinemtnworktypecat = Convert.ToInt32(appointment_val[0].work_id);

                        if ((totaldurationofappointment + currentappointmetduration) > fixtotalblocktime)
                        {
                            validationtocheckinbetweenappointment = true;
                        }
                    }

                    // get service type data of the appointment
                    var appoinemtnworktypecate = (from a in dbcon.tbl_worktype
                                                  where a.work_id == appoinemtnworktypecat
                                                  select a).ToList();

                    string emp_skill = emp_val[0].emp_qualifiedservicetypes.ToString();
                    int appointment_worktype = Convert.ToInt32(appointment_val[0].work_id);
                    TimeSpan timediffrence = TimeSpan.Zero;
                    string timeintervalidarray = null;

                    //check time slot
                    var command = new SqlCommand("Check_timeslot", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@timestart", appointment_val[0].time_range_start);
                    command.Parameters.AddWithValue("@timeend", appointment_val[0].time_range_end);
                    command.Parameters.AddWithValue("@type", "assign");
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        timeintervalid = Convert.ToInt32(rdr["time_interval_id"]);
                        timeintervalidarray = timeintervalidarray + "," + timeintervalid;
                    }

                    DateTime currenttime;
                    string timezone = Session["timezone"].ToString();
                    TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                    DateTime newDT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, targetZone);
                    currenttime = newDT;

                    // check whether the emplyee has same time slot appointment or in between appointment
                    DateTime starttimeofappointment = Convert.ToDateTime(date + " " + appointment_val[0].time_range_start.ToString());
                    starttimeofcurrentappointment = TimeSpan.Parse(appointment_val[0].time_range_start);
                    endtimeofcurrentappointment = TimeSpan.Parse(appointment_val[0].time_range_end);

                    // loop variable for get appointment start and end time
                    TimeSpan time1 = TimeSpan.Zero;
                    TimeSpan time2 = TimeSpan.Zero;

                    // get all the timeinterval id and check that timeinterval id 
                    string[] splittimeintervalid = timeintervalidarray.Split(',');
                    Boolean checkincorrecttimeblock = false;
                    for (int timeid = 0; timeid < splittimeintervalid.Length; timeid++)
                    {
                        if (splittimeintervalid[timeid] == timeintervalidth)
                        {
                            checkincorrecttimeblock = true;
                        }
                    }

                    if (checkincorrecttimeblock == false)
                    //if (!timeintervalidarray.Contains(timeintervalidth))
                    {
                        list.Add("Incorrect time block.");
                        list.Add("false");
                        return Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    }

                    // check whether the appointment time is already passed
                    if (validationtocheckinbetweenappointment == true)
                    {
                        list.Add("Employee not available at this time.");
                        list.Add("false");
                        return Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    }
                    if (starttimeofappointment <= currenttime)
                    {
                        list.Add("Appointment time has already passed.");
                        list.Add("false");
                        return Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    }
                    // check whether the service type is same or not and check its time 
                    if (appoinemtnworktypecate.Count > 0)
                    {
                        if (!empworktypecat.Contains(appoinemtnworktypecate[0].work_id.ToString()))
                        {
                            list.Add("Service type does not match.");
                            list.Add("false");
                            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    // check whether the attribute is same or not 
                    if (checksameattribute == false)
                    {
                        list.Add("Attribute does not match.");
                        list.Add("false");

                        return Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    }
                    // when appointment assign succesfully
                    else
                    {
                        list.Add(appointmnet_desc + " Appointment moved successfully.");
                        list.Add("true");


                        ////////// when any appoinment already drag to any employee and again drag and drop that appointment to another emp or same employee
                        ////// then we need toi remove that appointment first 

                        string sessionappointment = Session["appointment_save"].ToString();
                        string[] sessionappointmentarray = sessionappointment.Split(',');
                        List<string> sessionlisting = new List<string>(sessionappointmentarray);

                        for (int i = 0; i < sessionappointmentarray.Length; i++)
                        {
                            string[] sessionappointmentarraysplit = sessionappointmentarray[i].ToString().Split('-');
                            if (sessionappointmentarraysplit[0].ToString().IndexOf(appointmentid) == 0)
                            {
                                sessionlisting.RemoveAt(i);
                            }
                        }
                        sessionappointmentarray = sessionlisting.ToArray();

                        Session["appointment_save"] = sessionappointmentarray;

                        ///////////  end

                        string topassintosession = appointmentid + "-" + empid + "-" + timeintervalidth;
                        Session["appointment_save"] = string.Join(",", sessionappointmentarray) + "," + topassintosession;
                        //Session["appointment_save"] = Session["appointment_save"].ToString() + "," + topassintosession;
                        string testingsession = Session["appointment_save"].ToString();
                        return Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    }

                }
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "validation");
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get appointment of employee
        /// </summary>
        /// <param name="emp_id">The emp identifier.</param>
        /// <param name="time">The time.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult getappointmentofemp(string emp_id, string time, string date)
        {
            try
            {
                DateTime formatdate = Convert.ToDateTime(date);
                string dating = formatdate.ToString("yyyy-MM-dd");
                jugglecontext dbcon = new jugglecontext();
                int timeinint = Convert.ToInt32(time);
                int empid = Convert.ToInt32(emp_id);
                string day = null;
                int timeofappointmet = 0;

                if (formatdate.DayOfWeek == DayOfWeek.Sunday)
                {
                    day = "1";
                }
                else if (formatdate.DayOfWeek == DayOfWeek.Monday)
                {
                    day = "2";
                }
                else if (formatdate.DayOfWeek == DayOfWeek.Tuesday)
                {
                    day = "3";
                }
                else if (formatdate.DayOfWeek == DayOfWeek.Wednesday)
                {
                    day = "4";
                }
                else if (formatdate.DayOfWeek == DayOfWeek.Thursday)
                {
                    day = "5";
                }
                else if (formatdate.DayOfWeek == DayOfWeek.Friday)
                {
                    day = "6";
                }
                else if (formatdate.DayOfWeek == DayOfWeek.Saturday)
                {
                    day = "7";
                }

                List<tbl_appointment> appointmentlist = new List<tbl_appointment>();
                var emp_avaibility = (from a in dbcon.tbl_employee_info
                                      where a.emp_id == empid
                                      select a).ToList();

                var timeintervaldata = (from a in dbcon.tbl_time_interval
                                        where a.time_interval_id == timeinint
                                        select a).ToList();

                string timeinterval = timeintervaldata[0].time_interval;
                string[] splittimeinterval = timeinterval.Split('-');

                SqlConnection connection1 = new SqlConnection(dbcon.connectionString());
                var command1 = new SqlCommand("compare_client_Availibility", connection1);
                int userid = Convert.ToInt32(Session["User_Id"]);
                command1.CommandType = CommandType.StoredProcedure;
                command1.Parameters.AddWithValue("@startdate", dating);
                command1.Parameters.AddWithValue("@enddate", dating);
                command1.Parameters.AddWithValue("@StartTime", splittimeinterval[0].ToString());
                command1.Parameters.AddWithValue("@endTime", splittimeinterval[1].ToString());
                command1.Parameters.AddWithValue("@emp_id", empid);
                command1.Parameters.AddWithValue("@empavailability_id", Convert.ToInt32(0));
                command1.Parameters.AddWithValue("@userid", userid);

                connection1.Open();
                SqlDataReader rdr1 = command1.ExecuteReader();
                Boolean reader = rdr1.Read();
                //if (reader)
                if (!reader) // change Swap Employee UnAvailability to Employee availability
                {
                    SqlConnection connection = new SqlConnection(dbcon.connectionString());
                    var command = new SqlCommand("Appointmenttimeslot", connection);
                    userid = Convert.ToInt32(Session["User_Id"]);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@userId", userid);
                    command.Parameters.AddWithValue("@empId", emp_id);
                    command.Parameters.AddWithValue("@timeintervalid", Convert.ToInt32(time));
                    command.Parameters.AddWithValue("@date", formatdate);
                    command.Parameters.AddWithValue("@StartTime", splittimeinterval[0].ToString());
                    command.Parameters.AddWithValue("@endTime", splittimeinterval[1].ToString());
                    command.Parameters.AddWithValue("@type", "notrecuring");
                    connection.Open();

                    SqlDataReader rdr = command.ExecuteReader();
                    string desc = null;
                    int appointmentidforcheck = 0;

                    bool flag = false;
                    while (rdr.Read())
                    {
                        flag = true;
                        desc = rdr["description"].ToString();
                        //everyday hopper
                        appointmentidforcheck = Convert.ToInt32(rdr["appointment_id"]);
                        if (desc.Length > 17)
                        {
                            // get only 18 characters of the desc string 
                            desc = desc.Substring(0, 17) + "...";
                        }
                        //hopper every day
                        var checkineveryappointabhopper = (from a in dbcon.tbl_appointmentEveryday_hopper
                                                           where a.appointment_id == appointmentidforcheck && a.appointment_date == formatdate && a.appointment_date == formatdate && a.emp_id == empid && a.timeinterval_id == timeinint
                                                           select a).ToList();
                        if (checkineveryappointabhopper.Count > 0)
                        {
                            timeofappointmet = Convert.ToInt32(rdr["time"]);
                            appointmentlist.Add(new tbl_appointment() { description = desc, notes = rdr["color"].ToString(), appointment_id = Convert.ToInt32(rdr["appointment_id"]), time = timeofappointmet });
                        }
                    }
                    if (flag == false)
                    {
                        appointmentlist.Add(new tbl_appointment() { description = null });
                    }
                    var commandre = new SqlCommand("Appointmenttimeslot", connection);
                    userid = Convert.ToInt32(Session["User_Id"]);
                    commandre.CommandType = CommandType.StoredProcedure;
                    commandre.Parameters.AddWithValue("@userId", userid);
                    commandre.Parameters.AddWithValue("@empId", emp_id);
                    commandre.Parameters.AddWithValue("@timeintervalid", Convert.ToInt32(time));
                    commandre.Parameters.AddWithValue("@date", formatdate);
                    commandre.Parameters.AddWithValue("@StartTime", splittimeinterval[0].ToString());
                    commandre.Parameters.AddWithValue("@endTime", splittimeinterval[1].ToString());
                    commandre.Parameters.AddWithValue("@type", "recuring");
                    SqlDataReader rdrre = commandre.ExecuteReader();
                    string descre = null;
                    int appointmentidforcheckrecuring = 0;
                    while (rdrre.Read())
                    {
                        descre = rdrre["description"].ToString();
                        string[] appointmentdays = rdrre["day"].ToString().Split(',');
                        if (appointmentdays.Contains(day))
                        {
                            desc = rdrre["description"].ToString();
                            //hopper every day
                            appointmentidforcheckrecuring = Convert.ToInt32(rdrre["appointment_id"]);
                            if (desc.Length > 17)
                            {
                                // get only 18 characters of the desc string 
                                desc = desc.Substring(0, 17) + "...";
                            }
                            //hopper every day
                            var checkineveryappointabhopperrecuring = (from a in dbcon.tbl_appointmentEveryday_hopper
                                                                       where a.appointment_id == appointmentidforcheckrecuring && a.appointment_date == formatdate && a.emp_id == empid && a.timeinterval_id == timeinint
                                                                       select a).ToList();
                            if (checkineveryappointabhopperrecuring.Count > 0)
                            {
                                timeofappointmet = Convert.ToInt32(rdrre["time"]);
                                appointmentlist.Add(new tbl_appointment() { description = desc, notes = rdrre["color"].ToString(), appointment_id = Convert.ToInt32(rdrre["appointment_id"]), time = timeofappointmet });
                            }

                        }
                    }
                    connection1.Close();
                    connection.Close();
                    return Json(new { appointmentlist = appointmentlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    connection1.Close();
                    appointmentlist.Add(new tbl_appointment() { description = "Unavailable", notes = "#000000", appointment_id = Convert.ToInt32("0") });
                    return Json(new { appointmentlist = appointmentlist }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "getappointmentofemp");
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }
            finally
            {

            }

        }

        /// <summary>
        /// get employee work type name 
        /// </summary>
        /// <param name="empid">The empid.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult displayemployee_worktype(string empid)
        {
            try
            {
                int userId = Convert.ToInt32(Session["User_Id"]);
                int emp_id = Convert.ToInt32(empid);

                using (jugglecontext dbcon1 = new jugglecontext())
                {
                    var foundDepartments = (from a in dbcon1.tbl_employee_info
                                            where a.emp_id == emp_id
                                            select new
                                            {
                                                a.emp_qualifiedservicetypes
                                            }).ToList();

                    string workid = foundDepartments[0].emp_qualifiedservicetypes.ToString();
                    string[] splitworkid = workid.Split(',');

                    var getworktypename = (from a in dbcon1.tbl_worktype
                                           where splitworkid.Contains(a.work_id.ToString())
                                           select new
                                           {
                                               a.name
                                           }).ToList();

                    if (getworktypename.Count > 0)
                    {
                        return Json(new { data = getworktypename.ToList() }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "displayemployee_worktype");
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Attributes the color of the change.
        /// </summary>
        /// <param name="appointmentid">The appointmentid.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult attribute_change_color(string appointmentid, string date)
        {
            //try
            //{
            int userId = Convert.ToInt32(Session["User_Id"]);
            DateTime formatdate = Convert.ToDateTime(date);
            string dating = formatdate.ToString("yyyy-MM-dd");
            using (jugglecontext dbcon1 = new jugglecontext())
            {

                SqlConnection connection = new SqlConnection(dbcon1.connectionString());
                string timeintervalidarray = null;
                int timeintervalid = 0;
                int appid = Convert.ToInt32(appointmentid);
                string currentappotinemntempid = null;
                string currentappointmenttimeintervalid = null;

                // get all the emoloyee detail
                var employeedetail = (from a in dbcon1.tbl_employee_info
                                      where a.user_id == userId
                                      select a).ToList();

                /// get current appointment detail
                var appointmentdetail = (from a in dbcon1.tbl_appointment
                                         where a.appointment_id == appid
                                         select a).ToList();

                int clientid = Convert.ToInt32(appointmentdetail[0].client_id);
                string currentappointmentservicetype = appointmentdetail[0].work_id.ToString();
                int currentappointmetduration = Convert.ToInt32(appointmentdetail[0].time);
                currentappotinemntempid = appointmentdetail[0].emp_id.ToString();
                currentappointmenttimeintervalid = appointmentdetail[0].time_interval_id.ToString();

                /// get current appointment client
                var clientdetail = (from a in dbcon1.tbl_client
                                    where a.client_id == clientid
                                    select a).ToList();

                // get all the timeslot of the appointment
                var command = new SqlCommand("Check_timeslot", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@timestart", appointmentdetail[0].time_range_start);
                command.Parameters.AddWithValue("@timeend", appointmentdetail[0].time_range_end);
                command.Parameters.AddWithValue("@type", "assign");
                connection.Open();
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    timeintervalid = Convert.ToInt32(rdr["time_interval_id"]);
                    timeintervalidarray = timeintervalidarray + "," + timeintervalid;
                }

                string appoimet_attribute = appointmentdetail[0].attribute_id.ToString(); // current appointment attribute 
                string client_attribute = clientdetail[0].attribute_id.ToString(); // client attribute 

                ArrayList array1 = new ArrayList();
                SqlDataReader rdr1;
                Boolean reader = false;

                // attribute validation start 
                for (int i = 0; i < employeedetail.Count; i++)
                {
                    string emp_attribute = employeedetail[i].attribute_id.ToString(); // employee attribute
                    var stringCollectionemp = emp_attribute.Split(',');
                    var stringCollectionclient = client_attribute.Split(',');
                    var stringCollectionapp = appoimet_attribute.Split(',');

                    if (string.IsNullOrEmpty(appoimet_attribute))
                    {
                        Array.Resize(ref stringCollectionapp, stringCollectionapp.Length - 1);
                    }

                    string[] union = stringCollectionclient.Union(stringCollectionapp).ToArray();
                    string[] intercept = stringCollectionemp.Intersect(union).ToArray();

                    int count = 0;
                    foreach (string value in union)
                    {
                        foreach (string un in intercept)
                        {
                            if (value == un)
                            {
                                count++;
                            }
                        }
                    }
                    string[] splittimeintervaldata = timeintervalidarray.Split(',');
                    splittimeintervaldata = splittimeintervaldata.Skip(1).ToArray();
                    if (count == union.Length)
                    {
                        for (int mj = 0; mj < splittimeintervaldata.Length; mj++)
                        {
                            int emp_idforcolor = Convert.ToInt32(employeedetail[i].emp_id);
                            int totaldurationofappointment = 0;
                            string empservicetype = employeedetail[i].emp_qualifiedservicetypes.ToString();
                            int timeintervalidforcolor = Convert.ToInt32(splittimeintervaldata[mj]);
                            string currentday = "0";
                            int statictravelbuffer = 0;
                            int fixtotalblocktime = 120;
                            Boolean validationtocheckinbetweenappointment = false;

                            string dayofrecuringappointment = null;

                            if (formatdate.DayOfWeek == DayOfWeek.Sunday)
                            {
                                currentday = "1";
                            }
                            if (formatdate.DayOfWeek == DayOfWeek.Monday)
                            {
                                currentday = "2";
                            }
                            if (formatdate.DayOfWeek == DayOfWeek.Tuesday)
                            {
                                currentday = "3";
                            }
                            if (formatdate.DayOfWeek == DayOfWeek.Wednesday)
                            {
                                currentday = "4";
                            }
                            if (formatdate.DayOfWeek == DayOfWeek.Thursday)
                            {
                                currentday = "5";
                            }
                            if (formatdate.DayOfWeek == DayOfWeek.Friday)
                            {
                                currentday = "6";
                            }
                            if (formatdate.DayOfWeek == DayOfWeek.Saturday)
                            {
                                currentday = "7";
                            }

                            var timeintervaldataforcolor = (from a in dbcon1.tbl_time_interval
                                                            where a.time_interval_id == timeintervalidforcolor
                                                            select a).ToList();
                            string[] splittimeoftimeinteval = timeintervaldataforcolor[0].time_interval.ToString().Split('-');

                            // check employee is available or not
                            var command1 = new SqlCommand("compare_client_Availibility", connection);
                            int userid = Convert.ToInt32(Session["User_Id"]);
                            command1.CommandType = CommandType.StoredProcedure;
                            command1.Parameters.AddWithValue("@startdate", dating);
                            command1.Parameters.AddWithValue("@enddate", dating);
                            command1.Parameters.AddWithValue("@StartTime", splittimeoftimeinteval[0].ToString());
                            command1.Parameters.AddWithValue("@endTime", splittimeoftimeinteval[1].ToString());
                            command1.Parameters.AddWithValue("@emp_id", emp_idforcolor);
                            command1.Parameters.AddWithValue("@empavailability_id", Convert.ToInt32(0));
                            command1.Parameters.AddWithValue("@userid", userid);
                            rdr1 = command1.ExecuteReader();
                            reader = rdr1.Read();

                            // check all the validation to assign the appointment to an emp start
                            Boolean checkservicetype = false;
                            if (!empservicetype.Contains(currentappointmentservicetype))
                            {
                                checkservicetype = true;
                            }
                            //var getcurrentempappointwithrecuring = (from a in dbcon1.tbl_appointment
                            //                                        where a.emp_id == emp_idforcolor && a.time_interval_id == timeintervalidforcolor && a.recurring == true
                            //                                        select a).ToList();

                            //var getcurrentempappointwithoutrecuring = (from a in dbcon1.tbl_appointment
                            //                                           where a.emp_id == emp_idforcolor && a.time_interval_id == timeintervalidforcolor && a.recurring == false && (a.start_date <= formatdate && a.end_date >= formatdate)
                            //                                           select a).ToList();


                            var get_current_emp_timeblock_withoutrecuring = (from a in dbcon1.tbl_appointmentEveryday_hopper
                                                                             join b in dbcon1.tbl_appointment on a.appointment_id equals b.appointment_id
                                                                             where a.emp_id == emp_idforcolor && a.appointment_date == formatdate && a.timeinterval_id == timeintervalidforcolor
                                                                             select new
                                                                             {
                                                                                 a.appointment_id,
                                                                                 b.time,
                                                                                 a.emp_id,
                                                                                 a.timeinterval_id
                                                                             }).ToList();


                            // comment start

                            //if (getcurrentempappointwithrecuring.Count > 0)
                            //{
                            //    for (int recuring = 0; recuring < getcurrentempappointwithrecuring.Count; recuring++)
                            //    {
                            //        dayofrecuringappointment = getcurrentempappointwithrecuring[recuring].day;
                            //        if (dayofrecuringappointment.Contains(currentday))
                            //        {
                            //            totaldurationofappointment = totaldurationofappointment + statictravelbuffer + Convert.ToInt32(getcurrentempappointwithrecuring[recuring].time);
                            //        }
                            //    }
                            //}
                            // comment end


                            // comment start
                            string sesionvarfor120blocktest = Session["appointment_save"].ToString();
                            if (get_current_emp_timeblock_withoutrecuring.Count > 0)
                            {
                                for (int data = 0; data < get_current_emp_timeblock_withoutrecuring.Count; data++)
                                {
                                    string testing = get_current_emp_timeblock_withoutrecuring[data].appointment_id + "-" + get_current_emp_timeblock_withoutrecuring[data].emp_id + "-" + get_current_emp_timeblock_withoutrecuring[data].timeinterval_id;
                                    sesionvarfor120blocktest = sesionvarfor120blocktest + "," + testing;
                                }
                            }

                            string[] sesionvarfor120blocksplittest = sesionvarfor120blocktest.Split(',');

                            List<string> sessionlist = new List<string>(sesionvarfor120blocksplittest);

                            if (get_current_emp_timeblock_withoutrecuring.Count > 0)
                            {
                                for (int data = 0; data < get_current_emp_timeblock_withoutrecuring.Count; data++)
                                {
                                    int sessioncount = 0;
                                    for (int j = 0; j < sessionlist.Count; j++)
                                    {
                                        //if (sesionvarfor120blocksplittest[j].ToString().IndexOf(get_current_emp_timeblock_withoutrecuring[data].appointment_id.ToString() + "-") == 0)
                                        if (sessionlist[j].ToString().IndexOf(get_current_emp_timeblock_withoutrecuring[data].appointment_id.ToString() + "-") == 0)
                                        {
                                            //Response.Write("found <br />");
                                            sessioncount++;
                                        }
                                        if (sessionlist[j].ToString().IndexOf(get_current_emp_timeblock_withoutrecuring[data].appointment_id.ToString() + "-") == 0 && sessioncount >= 2)
                                        //if (sesionvarfor120blocksplittest[j].ToString().IndexOf(get_current_emp_timeblock_withoutrecuring[data].appointment_id.ToString() + "-") == 0 && sessioncount >= 2)
                                        {
                                            sessionlist.RemoveAt(j);
                                        }
                                    }
                                }
                            }



                            sesionvarfor120blocksplittest = sessionlist.ToArray();

                            for (int ses = 0; ses < sesionvarfor120blocksplittest.Length; ses++)
                            {
                                string[] innnersplittest = sesionvarfor120blocksplittest[ses].ToString().Split('-');

                                if (!string.IsNullOrEmpty(innnersplittest[0].ToString()))
                                {
                                    if (innnersplittest[1].ToString() == emp_idforcolor.ToString() && innnersplittest[2].ToString() == timeintervalidforcolor.ToString())
                                    {
                                        int tempappid = Convert.ToInt32(innnersplittest[0]);
                                        var getapptime = (from a in dbcon1.tbl_appointment
                                                          where a.appointment_id == tempappid
                                                          select a).ToList();
                                        totaldurationofappointment = totaldurationofappointment + Convert.ToInt32(getapptime[0].time);
                                    }
                                }
                            }



                            //// comment end

                            if ((totaldurationofappointment + currentappointmetduration) > fixtotalblocktime)
                            {
                                validationtocheckinbetweenappointment = true;
                            }

                            // check all the validation to assign the appointment to an emp end
                            //  if (reader && checkservicetype != true && validationtocheckinbetweenappointment != true) // change Swap Employee Availability to Employee Unavailability
                            if (reader == false && checkservicetype != true && validationtocheckinbetweenappointment != true)
                            {
                                array1.Add(employeedetail[i].emp_id + "-" + splittimeintervaldata[mj]);
                            }
                        }
                    }
                }
                // attribute validation end 
                if (array1.Count > 0)
                {
                    return Json(new { data = array1 }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    Logfile.WriteCDNLog(ex.ToString(), "attribute_change_color");
            //    return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            //}
        }


        /// <summary>
        /// method that get all the employee detail /// it's method only for highlight the blocks
        /// </summary>
        /// <param name="appointmentid">The appointmentid.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult timeintervaldataofemp(string appointmentid, string date)
        {
            try
            {
                int userId = Convert.ToInt32(Session["User_Id"]);
                DateTime formatdate = Convert.ToDateTime(date);
                string dating = formatdate.ToString("yyyy-MM-dd");
                using (jugglecontext dbcon1 = new jugglecontext())
                {
                    SqlConnection connection = new SqlConnection(dbcon1.connectionString());
                    int appid = Convert.ToInt32(appointmentid);
                    ArrayList array1 = new ArrayList();
                    string timeintervalidarray = null;
                    int timeintervalid = 0;

                    var employeedetail = (from a in dbcon1.tbl_employee_info
                                          where a.user_id == userId
                                          select a).ToList();

                    var appointmentdetail = (from a in dbcon1.tbl_appointment
                                             where a.appointment_id == appid
                                             select a).ToList();

                    int clientid = Convert.ToInt32(appointmentdetail[0].client_id);

                    var clientdetail = (from a in dbcon1.tbl_client
                                        where a.client_id == clientid
                                        select a).ToList();

                    // get all the timeblock related to the appointment

                    var command = new SqlCommand("Check_timeslot", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@timestart", appointmentdetail[0].time_range_start);
                    command.Parameters.AddWithValue("@timeend", appointmentdetail[0].time_range_end);
                    command.Parameters.AddWithValue("@type", "assign");
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        timeintervalid = Convert.ToInt32(rdr["time_interval_id"]);
                        timeintervalidarray = timeintervalidarray + "," + timeintervalid;
                    }

                    SqlDataReader rdr1;
                    Boolean reader = false;
                    timeintervalidarray = timeintervalidarray.TrimStart();
                    timeintervalidarray = timeintervalidarray.TrimEnd();
                    string[] splittimeintervaldata = timeintervalidarray.Split(',');
                    splittimeintervaldata = splittimeintervaldata.Skip(1).ToArray();
                    for (int i = 0; i < employeedetail.Count; i++)
                    {
                        for (int mj = 0; mj < splittimeintervaldata.Length; mj++)
                        {
                            int emp_idforcolor = Convert.ToInt32(employeedetail[i].emp_id);
                            int timeintervalidforcolor = Convert.ToInt32(splittimeintervaldata[mj]);

                            var timeintervaldataforcolor = (from a in dbcon1.tbl_time_interval
                                                            where a.time_interval_id == timeintervalidforcolor
                                                            select a).ToList();
                            string[] splittimeoftimeinteval = timeintervaldataforcolor[0].time_interval.ToString().Split('-');

                            // check employee is available or not
                            var command1 = new SqlCommand("compare_client_Availibility", connection);
                            int userid = Convert.ToInt32(Session["User_Id"]);
                            command1.CommandType = CommandType.StoredProcedure;
                            command1.Parameters.AddWithValue("@startdate", dating);
                            command1.Parameters.AddWithValue("@enddate", dating);
                            command1.Parameters.AddWithValue("@StartTime", splittimeoftimeinteval[0].ToString());
                            command1.Parameters.AddWithValue("@endTime", splittimeoftimeinteval[1].ToString());
                            command1.Parameters.AddWithValue("@emp_id", emp_idforcolor);
                            command1.Parameters.AddWithValue("@empavailability_id", Convert.ToInt32(0));
                            command1.Parameters.AddWithValue("@userid", userid);
                            rdr1 = command1.ExecuteReader();
                            reader = rdr1.Read();
                            // if (reader)
                            if (!reader) // change Swap Employee Availability to Employee Unavailability
                            {
                                array1.Add(employeedetail[i].emp_id + "-" + splittimeintervaldata[mj]);
                            }
                        }
                    }

                    if (array1.Count > 0)
                    {
                        return Json(new { data = array1 }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "timeintervaldataofemp");
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// this method is use to assign the appointment and this method call when save button click 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult saveappointment(string date)
        {
            try
            {

                ArrayList appointmentid = new ArrayList();
                string gettotalappoinmentid = Session["appointment_save"].ToString();
                gettotalappoinmentid = gettotalappoinmentid.TrimStart(',');
                string[] splitgettotalappoinmentid = gettotalappoinmentid.Split(',');
                for (int i = 0; i < splitgettotalappoinmentid.Length; i++)
                {
                    try
                    {
                        string[] getspliteddata = splitgettotalappoinmentid[i].ToString().Split('-');
                        updateappointmentonsavebutton(getspliteddata[0], getspliteddata[1], getspliteddata[2], date);
                        appointmentid.Add(getspliteddata[0]);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                Session["appointment_save"] = null;
                Session["appointment_save"] = ",";
                return Json(new { data = appointmentid }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "saveappointment");
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// This method is use to reset the appointment means unassign the appointmet
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult resetappointment()
        {
            try
            {
                ArrayList appointmentid = new ArrayList();
                string gettotalappoinmentid = Session["appointment_save"].ToString();
                gettotalappoinmentid = gettotalappoinmentid.TrimStart(',');
                string[] splitgettotalappoinmentid = gettotalappoinmentid.Split(',');
                for (int i = 0; i < splitgettotalappoinmentid.Length; i++)
                {
                    string[] getspliteddata = splitgettotalappoinmentid[i].ToString().Split('-');
                    appointmentid.Add(getspliteddata[0]);
                }
                Session["appointment_save"] = null;
                return Json(new { data = appointmentid }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "resetappointment");
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// update appointment means assign appointment to employee 
        /// </summary>
        /// <param name="appointmentid">The appointmentid.</param>
        /// <param name="empid">The empid.</param>
        /// <param name="timeintervalidth">The timeintervalidth.</param>
        [HttpPost]
        public void updateappointmentonsavebutton(string appointmentid, string empid, string timeintervalidth, string date)
        {
            try
            {
                int appid = Convert.ToInt32(appointmentid);
                int timeintervalid = Convert.ToInt32(timeintervalidth);
                DateTime formatdate = Convert.ToDateTime(date);
                string dating = formatdate.ToString("yyyy-MM-dd");

                jugglecontext dbcon = new jugglecontext();
                var getappointempid = (from a in dbcon.tbl_appointment
                                       where a.appointment_id == appid
                                       select a).ToList();
                int beforeupdateempid = Convert.ToInt32(getappointempid[0].emp_id);
                if ((Convert.ToInt32(empid) != beforeupdateempid && beforeupdateempid != 0) || empid == "0")
                {
                    //  sendmailwhenunassign(appointmentid);
                    //  sendsmswhenunassign(appointmentid);
                }



                SqlConnection connection = new SqlConnection(dbcon.connectionString());
                var command = new SqlCommand("UpdateEmpID", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@emp_id", Convert.ToInt32(empid));
                command.Parameters.AddWithValue("@timeintervalid", Convert.ToInt32(timeintervalidth));
                command.Parameters.AddWithValue("@appointment_id", Convert.ToInt32(appointmentid));
                connection.Open();
                SqlDataReader rdr = command.ExecuteReader();






                int userid = Convert.ToInt32(Session["User_Id"]);
                //Send email to client and employee at drag time
                var command1 = new SqlCommand("Emailsend_clientEmp", connection);
                command1.CommandType = CommandType.StoredProcedure;
                command1.Parameters.AddWithValue("@appointment_id", Convert.ToInt32(appointmentid));
                command1.Parameters.AddWithValue("@emp_id", Convert.ToInt32(empid));
                command1.Parameters.AddWithValue("@userid", Convert.ToInt32(Session["User_Id"]));

                SqlDataReader rdr1 = command1.ExecuteReader();
                string ClientEmail = "";
                string EmpEmial = "";
                string clientname, empname = null;
                string clientphoneno = "";
                string empphoneno = "";
                string clientaddress = "";
                string starttime = "";
                string endtime = "";
                string startdate = "";
                string enddate = "";
                while (rdr1.Read())
                {
                    ClientEmail = rdr1["ClientEmail"].ToString();
                    EmpEmial = rdr1["empEmail"].ToString();
                    empname = rdr1["empname"].ToString();
                    clientname = rdr1["clientname"].ToString();
                    clientphoneno = "+1" + rdr1["clientphoneno"].ToString();
                    empphoneno = "+1" + rdr1["empphoneno"].ToString();
                    clientaddress = rdr1["clientaddress"].ToString();
                    starttime = rdr1["starttime"].ToString();
                    endtime = rdr1["endtime"].ToString();
                    startdate = rdr1["startdate"].ToString();
                    enddate = rdr1["enddate"].ToString();
                }



                if ((!string.IsNullOrEmpty(ClientEmail) && !string.IsNullOrEmpty(EmpEmial)) && !string.IsNullOrEmpty(empname))
                {
                    try
                    {
                        sendmail(appointmentid, empid, timeintervalidth, date);
                    }
                    catch (Exception ex)
                    {
                        Logfile.WriteCDNLog(ex.ToString(), "updateappointmentonsavebutton");
                    }

                }
                if ((!string.IsNullOrEmpty(clientphoneno) && !string.IsNullOrEmpty(empphoneno)) && !string.IsNullOrEmpty(empname))
                {
                    sendsms(appointmentid);
                }
                connection.Close();


                /// when unassign the appointment from employee then remove that appointment from the tbl_appointmentEveryday_hopper table 
                if (empid == "0")
                {
                    using (var ctx = new jugglecontext())
                    {
                        var x = (from y in ctx.tbl_appointmentEveryday_hopper
                                 where y.appointment_id == appid && y.appointment_date == formatdate
                                 orderby y.appointment_id descending
                                 select y).FirstOrDefault();
                        ctx.tbl_appointmentEveryday_hopper.Remove(x);
                        ctx.SaveChanges();
                    }
                }

                else
                {
                    //appointment every day in hopper code
                    var datacheckupdate = (from a in dbcon.tbl_appointmentEveryday_hopper
                                           where a.appointment_id == appid && a.appointment_date == formatdate
                                           select a).ToList();


                    SqlConnection connection1 = new SqlConnection(dbcon.connectionString());
                    var commandupdate = new SqlCommand("AppointmenteverydayHopper", connection1);
                    commandupdate.CommandType = CommandType.StoredProcedure;
                    if (datacheckupdate.Count > 0)
                    {
                        commandupdate.Parameters.AddWithValue("@type", "updatedata");
                    }
                    else
                    {
                        commandupdate.Parameters.AddWithValue("@type", "insertdate");
                    }

                    commandupdate.Parameters.AddWithValue("@emp_id", Convert.ToInt32(empid));
                    commandupdate.Parameters.AddWithValue("@appointmentdate", formatdate);
                    commandupdate.Parameters.AddWithValue("@appointment_id", Convert.ToInt32(appointmentid));
                    commandupdate.Parameters.AddWithValue("@starttime", starttime);
                    commandupdate.Parameters.AddWithValue("@endtime", endtime);
                    commandupdate.Parameters.AddWithValue("@createdate", DateTime.Now);
                    commandupdate.Parameters.AddWithValue("@timeinterval_id", timeintervalid);

                    connection1.Open();
                    SqlDataReader rdrupdate = commandupdate.ExecuteReader();
                    connection1.Close();
                }

            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "updateappointmentonsavebutton");
            }
        }




        
        }

    }
}