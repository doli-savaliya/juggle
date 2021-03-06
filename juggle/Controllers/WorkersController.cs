﻿using juggle.Models;
using System;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace juggle.Controllers
{
    public class WorkersController : Controller
    {

        /// <summary>
        /// on load Manages workers.
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageWorkers()
        {
            try
            {
                if (Session["User_Id"] != null && Session["User_Roll_Id"].ToString() == "2")
                {
                    System.IO.DirectoryInfo myDirInfo = new DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("/App_Data/MyGoogleStorage"));
                    foreach (FileInfo file in myDirInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    using (jugglecontext dbcon = new jugglecontext())
                    {
                        var userId = Convert.ToInt32(Session["User_Id"]);
                        var user = (from a in dbcon.tbl_user
                                    join b in dbcon.tbl_status on a.status equals b.status_id
                                    where a.role_id == 3 && a.supervisor_id == userId
                                    orderby a.user_id descending
                                    select new
                                    {
                                        username = a.username,
                                        address = a.address,
                                        phoneno = a.phoneno,
                                        Status = b.status,
                                        user_id = a.user_id
                                    }
                                  ).ToList();
                        return View(user);
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "ManageWorkers");
                return View();
            }

        }

        /// <summary>
        /// Bind role drop down from db using json 
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
        /// Get create Workers
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public PartialViewResult Create_Workers()
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
        /// Creates the workers with post method.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>returns with user view</returns>
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create_Workers(tbl_user user)
        {
            var autopassword = "";
            if (Session["User_Id"] != null)
            {
                int getstatusID = Convert.ToInt32(Request.Form["Status_hidden"]);
                if (getstatusID == 0)
                {

                    getstatusID = 1;
                }
                using (jugglecontext dbcon = new jugglecontext())
                {
                    try
                    {

                        var userId = Convert.ToInt32(Session["User_Id"]);
                        tbl_user tblusr = new tbl_user();
                        tblusr.firstName = user.firstName.Trim();
                        tblusr.lastName = user.lastName.Trim();
                        tblusr.address = user.address.Trim();
                        tblusr.phoneno = user.phoneno.Trim();
                        tblusr.email = user.email.Trim();
                        tblusr.username = user.username.Trim();
                        autopassword = dbcon.GeneratePassword(6).Trim();
                        tblusr.password = dbcon.Encrypt(autopassword.Trim());
                        tblusr.created_date = DateTime.Now;
                        tblusr.status = getstatusID;
                        tblusr.supervisor_id = userId;
                        tblusr.role_id = 3;
                        tblusr.user_id = userId;
                        dbcon.tbl_user.Add(tblusr);
                        dbcon.SaveChanges();

                        string Password = autopassword;
                        string Body = "Your password '" + Password + "' ";
                        dbcon.SendMail(tblusr.email, "Password", Body, Password);

                        return RedirectToAction("ManageWorkers", "Workers");

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
                return View(user);
            }
        }

        /// <summary>
        /// Get Edits the workers.
        /// </summary>
        /// <param name="user_id">The user identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit_Workers(Int32 user_id)
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
                return View(users);
            }

        }
        /// <summary>
        /// post edit Worker
        /// </summary>
        /// <param name="tbluser">The tbluser.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit_Workers(juggle.Models.tbl_user tbluser)
        {
            if (Session["User_Id"] != null)
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    var getstatusId = 0;
                    try
                    {
                        getstatusId = Convert.ToInt32(Request.Form["status_hidden"]);
                    }
                    catch { }
                    tbl_user users = new tbl_user();
                    var userId = Convert.ToInt32(Session["User_Id"]);
                    users.user_id = tbluser.user_id;
                    users.firstName = tbluser.firstName;
                    users.lastName = tbluser.lastName;
                    users.address = tbluser.address;
                    users.phoneno = tbluser.phoneno;
                    users.email = tbluser.email;
                    users.supervisor_id = userId;
                    users.username = tbluser.username;
                    users.password = tbluser.password;
                    if (getstatusId == 0)
                    {
                        users.status = tbluser.status;
                    }
                    else
                    {
                        users.status = Convert.ToInt32(getstatusId);
                    }
                    users.created_date = tbluser.created_date;
                    users.updated_date = DateTime.Now;
                    users.role_id = 3;
                    dbcon.Entry(users).State = System.Data.Entity.EntityState.Modified;
                    dbcon.SaveChanges();
                    return RedirectToAction("ManageWorkers", "Workers");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }
        /// <summary>
        /// Deletes the workers.
        /// </summary>
        /// <param name="user_id">The user identifier.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete_Workers(Int32 user_id)
        {
            using (jugglecontext dbcon = new jugglecontext())
            {
                if (Session["User_Id"] != null)
                {
                    tbl_user objworker = dbcon.tbl_user.Find(user_id);
                    dbcon.tbl_user.Remove(objworker);
                    dbcon.SaveChanges();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }

                return RedirectToAction("ManageWorkers", "Workers");
            }

        }
        /// <summary>
        /// Check user name exist or not.
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

                return dbcon.tbl_user.Any(x => x.username == UserName.Trim() && x.user_id != user_id)
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
        /// check the email is already exist or not.
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
                return dbcon.tbl_user.Any(x => x.email == email.Trim() && x.user_id != user_id)
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
    }
}