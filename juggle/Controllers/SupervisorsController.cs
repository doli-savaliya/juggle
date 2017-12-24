using juggle.Models;
using System;
using System.Configuration;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace juggle.Controllers
{
    public class SupervisorsController : Controller
    {

        /// <summary>
        /// Manages the supervisors.
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageSupervisors()
        {
            try
            {
                //if (Session["User_Id"] != null && Session["User_Roll_Id"].ToString() == "2")
                if (Session["User_Id"] != null )
                {
                    
                    using (jugglecontext dbcon = new jugglecontext())
                    {
                        var userId = Convert.ToInt32(Session["User_Id"]);
                        //var supervisoruser = (from a in dbcon.tbl_user
                        //                      join b in dbcon.tbl_status on a.status equals b.status_id
                        //                      where a.role_id == 2 && a.user_id == userId
                        //                      orderby a.user_id descending
                        //                      select new
                        //                      {
                        //                          username = a.username,
                        //                          address = a.address,
                        //                          phoneno = a.phoneno,
                        //                          Status = b.status,
                        //                          user_id = a.user_id
                        //                      }
                        //          ).ToList();
                        var supervisoruser = (from a in dbcon.tbl_user
                                              join b in dbcon.tbl_status on a.status equals b.status_id
                                              where a.supervisor_id == userId //&& a.user_id == userId
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
                Logfile.WriteCDNLog(ex.ToString(), "ManageSupervisors");
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
        /// Creates the supervisor get events.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public PartialViewResult Create_Supervisor()
        {
            jugglecontext dbcon = new jugglecontext();
            var status = from p in dbcon.tbl_status
                         select new { p.status_id, p.status };

            var statusname = status.ToList().Select(c => new SelectListItem
            {
                Text = c.status.ToString(),
                Value = c.status_id.ToString(),
            }).ToList();

            ViewBag.status = statusname;
            return PartialView();
        }

        /// <summary>
        /// Creates the supervisor with post event.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create_Supervisor(tbl_user user)
        {
            var autopassword = "";
            if (Session["User_Id"] != null)
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    try
                    {
                        var getButton = HttpContext.Request.Form["Save"];
                        if (getButton == "Save")
                        {
                            {
                                int getstatusID = Convert.ToInt32(Request.Form["Status_hidden"]);
                                if (getstatusID == 0)
                                {

                                    getstatusID = 1;
                                }

                                var status = from p in dbcon.tbl_status
                                             select new { p.status_id, p.status };

                                var statusname = status.ToList().Select(c => new SelectListItem
                                {
                                    Text = c.status.ToString(),
                                    Value = c.status_id.ToString(),
                                }).ToList();

                                ViewBag.status = statusname;
                                var userId = Convert.ToInt32(Session["User_Id"]);
                                tbl_user tblusr = new tbl_user();
                                tblusr.firstName = user.firstName.Trim();
                                tblusr.lastName = user.lastName.Trim();
                                tblusr.address = user.address;
                                tblusr.phoneno = user.phoneno.Trim();
                                tblusr.email = user.email.Trim();
                                tblusr.username = user.username.Trim();
                                autopassword = dbcon.GeneratePassword(6);
                                tblusr.password = dbcon.Encrypt(autopassword.Trim());
                                tblusr.supervisor_id = userId;
                                tblusr.created_date = DateTime.Now;

                                tblusr.timezoneid = "Central Standard Time";
                                tblusr.status = user.status;
                                tblusr.role_id = 2;
                                tblusr.user_id = 0;
                                Session["firstname"] = user.firstName;
                                Session["profile_pict"] = user.profile_pict;
                                if (Request.Files.Count > 0)
                                {
                                    var file = Request.Files[0];

                                    if (file != null && file.ContentLength > 0)
                                    {
                                        var fileName = Path.GetFileName(file.FileName);
                                        var path = Path.Combine(Server.MapPath("~/Profile_Picture/"), fileName);
                                        file.SaveAs(path);
                                        tblusr.profile_pict = fileName;
                                    }
                                    else
                                    {
                                        tblusr.profile_pict = "Noimage.png";
                                    }
                                }

                                dbcon.tbl_user.Add(tblusr);
                                dbcon.SaveChanges();

                                string Body = "Hello, \nYour Username: " + tblusr.username + " \nPassword: " + autopassword + " \n\n for login in Juggle Click this Link to login  \n " + dbcon.redirectUrl() + "";
                                dbcon.SendMail(tblusr.email, "Login Credntials for Juggle", Body, tblusr.username, autopassword);

                                return RedirectToAction("ManageSupervisors", "Supervisors");

                            }
                        }
                        else
                        {

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
                        return View(user);
                    }
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            return View(user);
        }

        /// <summary>
        /// Edits the supervisor with get event.
        /// </summary>
        /// <param name="user_id">The user identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit_Supervisor(Int32 user_id)
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
                tbl_user user = dbcon.tbl_user.Where(x => x.user_id == user_id).FirstOrDefault();
                tbl_user users = new tbl_user();
                users.supervisor_id = user.supervisor_id;
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
        public ActionResult Edit_Supervisor(tbl_user tbluser)
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
                    users.username = tbluser.username;
                    users.password = tbluser.password;
                    users.status = tbluser.status;
                    users.created_date = tbluser.created_date;
                    users.updated_date = DateTime.Now;
                    users.timezoneid = "Central Standard Time";

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
                    return RedirectToAction("ManageSupervisors", "Supervisors");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }

        /// <summary>
        /// Deletes the supervisor.
        /// </summary>
        /// <param name="user_id">The user identifier.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete_Supervisor(Int32 user_id)
        {
            using (jugglecontext dbcon = new jugglecontext())
            {
                try
                {
                    if (Session["User_Id"] != null)
                    {
                        tbl_user objuser = dbcon.tbl_user.Find(user_id);
                        dbcon.tbl_user.Remove(objuser);
                        dbcon.SaveChanges();
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }
                catch (Exception ex)
                {
                    Logfile.WriteCDNLog(ex.ToString(), "Delete supervisor");
                }
                return RedirectToAction("ManageSupervisors", "Supervisors");
            }
        }

        /// <summary>
        /// check the username is already exist or not 
        /// </summary>
        /// <param name="UserName">Name of the user.</param>
        /// <param name="user_id">The user identifier.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult doesUserNameExist(string UserName, int user_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                return dbcon.tbl_user.Any(x => x.username == UserName.Trim() && x.user_id != user_id && x.role_id == 2)
                      ? Json(string.Format("{0} already exists.", UserName),
                                                JsonRequestBehavior.AllowGet)
                      : Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "doesUserNameExist");
                return Json("error", JsonRequestBehavior.AllowGet);
            }

        }
      

        /// <summary>
        ///  check the email is already exist or not
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="user_id">The user identifier.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult doesemailexist(string email, int user_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                return dbcon.tbl_user.Any(x => x.email == email.Trim() && x.user_id != user_id && x.role_id == 2)
                      ? Json(string.Format("{0} already exists.", email),
                                                JsonRequestBehavior.AllowGet)
                      : Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "doesemailexist");
                return Json("error", JsonRequestBehavior.AllowGet);

            }
        }

        /// <summary>
        ///check the phone number is already exist or not
        /// </summary>
        /// <param name="phoneno">The phoneno.</param>
        /// <param name="user_id">The user identifier.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult doesphoneexist(string phoneno, int user_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                return dbcon.tbl_user.Any(x => x.phoneno == phoneno.Trim() && x.user_id != user_id && x.role_id == 2)
                      ? Json(string.Format("{0} already exists.", phoneno),
                                                JsonRequestBehavior.AllowGet)
                      : Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "doesphoneexist");
                return Json("error", JsonRequestBehavior.AllowGet);

            }

        }
    }
}
