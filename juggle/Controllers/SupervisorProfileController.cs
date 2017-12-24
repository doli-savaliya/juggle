using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using juggle.Models;
using System.IO;
using System.Data.Entity.Validation;

namespace juggle.Controllers
{
    public class SupervisorProfileController : Controller
    {
        // GET: SupervisorProfile
        public ActionResult Index()
        {
            try
            {
                //if (Session["User_Id"] != null && Session["User_Roll_Id"].ToString() == "2")
                if (Session["User_Id"] != null)
                {

                    using (jugglecontext dbcon = new jugglecontext())
                    {
                        var userId = Convert.ToInt32(Session["User_Id"]);
                       
                        var supervisoruser = (from a in dbcon.tbl_user
                                              join b in dbcon.tbl_status on a.status equals b.status_id
                                              where a.user_id == userId
                                              orderby a.user_id descending
                                              select new
                                              {
                                                  username = a.username,
                                                  address = a.address,
                                                  phoneno = a.phoneno,
                                                  Status = b.status,
                                                  user_id = a.user_id,
                                                  a.firstName,
                                                  a.profile_pict
                                                  
                                              }
                                  ).ToList();

                        //Session["firstname"] = supervisoruser[0].firstName;
                        //Session["profile_pict"] = supervisoruser[0].profile_pict;
                        return View(supervisoruser);
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "SupervisorIndex");
                return RedirectToAction("Login", "Account");
            }
        }
        /// <summary>
        /// bind role drop down from db using json 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult getStatus()
        {
            using (jugglecontext dbcon = new jugglecontext())
            {
                var query = new SelectList(dbcon.tbl_status.OrderByDescending(x => x.status_id).ToList(), "status_id", "status");
                return Json(query, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Edits the supervisor with get event.
        /// </summary>
        /// <param name="user_id">The user identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit_Supervisorprofile(Int32 user_id)
        {
            jugglecontext dbcon1 = new jugglecontext();
            var editstatus = (from a in dbcon1.tbl_user
                              where a.user_id == user_id
                              select a).ToList();

            int selected_statusid = Convert.ToInt32(editstatus[0].status);

            var content = from p in dbcon1.tbl_status
                          select new { p.status, p.status_id };


            var contentx = content.ToList().Select(c => new SelectListItem
            {
                Text = c.status.ToString(),
                Value = c.status_id.ToString(),
                Selected = (c.status_id == selected_statusid)
            }).ToList();

            ViewBag.status1 = contentx;


            using (jugglecontext dbcon = new jugglecontext())
            {
                //var id = (from a in dbcon.tbl_user
                //          where a.user_id==user_id
                //          select a).ToList();
                tbl_user user = dbcon.tbl_user.Where(x => x.user_id == user_id).FirstOrDefault();
                tbl_user users = new tbl_user();
                users.supervisor_id = user.supervisor_id;
                //users.supervisor_id = id[0].supervisor_id;
                users.firstName = user.firstName;
                users.lastName = user.lastName;
                users.address = user.address;
                users.phoneno = user.phoneno;
                users.email = user.email;
                users.username = user.username;
                users.status = user.status;
                users.created_date = user.created_date;
                users.password = user.password;
                users.profile_pict = user.profile_pict;
                return View(users);
            }

        }

        /// <summary>
        /// Edits the supervisor with post event.
        /// </summary>
        /// <param name="tbluser">The tbluser.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit_Supervisorprofile(tbl_user tbluser)
        {
            if (Session["User_Id"] != null)
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    tbl_user users = new tbl_user();
                    var userId = Convert.ToInt32(Session["User_Id"]);
                    users.supervisor_id = tbluser.supervisor_id;
                    users.user_id = tbluser.user_id;
                    users.firstName = tbluser.firstName;
                    users.lastName = tbluser.lastName;
                    users.address = tbluser.address;
                    users.phoneno = tbluser.phoneno;
                    users.email = tbluser.email;
                    users.status = tbluser.status;
                    users.username = tbluser.username;
                    users.password = tbluser.password;
                    users.created_date = tbluser.created_date;
                    users.updated_date = DateTime.Now;
                    users.timezoneid = "Central Standard Time";
                    Session["firstname"] = tbluser.firstName;
                    Session["profile_pict"] = tbluser.profile_pict;
                    if (Request.Files.Count > 0)
                    {
                        var file = Request.Files[0];
                        if (file != null && file.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);

                            users.profile_pict = fileName;

                            var path = Path.Combine(Server.MapPath("~/Profile_Picture/"), fileName);
                            file.SaveAs(path);

                        }
                        else
                        {
                            users.profile_pict = tbluser.profile_pict;
                        }
                    }
                    users.role_id = 2;
                    dbcon.Entry(users).State = System.Data.Entity.EntityState.Modified;
                    dbcon.SaveChanges();
                    return RedirectToAction("Index", "SupervisorProfile");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }
    }
}