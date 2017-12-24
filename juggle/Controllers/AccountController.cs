using juggle.Models;
using System;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace juggle.Controllers
{
    public class AccountController : Controller
    {
        HttpCookie AccountCookies = new HttpCookie("AccountCookies");

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

        // GET: Account
        /// <summary>
        /// Logins this instance.
        /// </summary>
        /// <returns></returns>
        public ActionResult Login()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now);
            Response.Cache.SetNoServerCaching();
            Response.Cache.SetNoStore();

            Session["User_Id"] = null; //it's my session variable
            Session.Remove("appointment_save");
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();
            AccountCookies.Value = null;
            this.ControllerContext.HttpContext.Response.Cookies.Add(AccountCookies);
            FormsAuthentication.SignOut();
            return View();
        }

        /// <summary>
        /// Logins the specified use acc.
        /// </summary>
        /// <param name="use_acc">The use acc.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(tbl_user use_acc)
        {
            try
            {

                if (!string.IsNullOrEmpty(use_acc.username) && !string.IsNullOrEmpty(use_acc.password))
                {

                    var btnType = Request.Form["Login"];

                    if (btnType == "Login")
                    {
                        var u_Id = "";
                        var u_Name = "";
                        var u_Roll_Id = "";

                        using (jugglecontext dbcon = new jugglecontext())
                        {
                            if (use_acc.username == null || string.IsNullOrEmpty(use_acc.username))
                            {
                                ModelState.AddModelError("", "User Name is Required.");
                            }
                            else if (use_acc.password == null || string.IsNullOrEmpty(use_acc.password))
                            {
                                ModelState.AddModelError("", "Password is Required.");
                            }
                            else
                            {
                                var pass = dbcon.Encrypt(use_acc.password);
                                var role1 = RoleType.Administrator.GetHashCode();

                                var getadministrator = dbcon.tbl_user.Where(administrator => administrator.role_id == role1).SingleOrDefault(u => u.username == use_acc.username && (u.password) == pass);
                               // var getadministrator = dbcon.tbl_user.Where(administrator => administrator.role_id == 4).SingleOrDefault(u => u.username == user.Username && (u.password) == user.Password);

                                if (getadministrator != null)
                                {
                                    Session["User_Id"] = getadministrator.user_id.ToString();
                                    Session["profile_pict"] = getadministrator.profile_pict.ToString();
                                    Session["User_Name"] = getadministrator.username.ToString();
                                    Session["firstname"] = getadministrator.firstName.ToString();
                                    Session["User_Roll_Id"] = getadministrator.role_id.ToString();

                                    u_Id = getadministrator.user_id.ToString();
                                    u_Name = getadministrator.username.ToString();
                                    u_Roll_Id = getadministrator.role_id.ToString();

                                    return RedirectToAction("ManageSupervisors", "Supervisors");

                                }
                                else
                                {

                                    var role2 = RoleType.Supervisors.GetHashCode();
                                    var getSupervisor = dbcon.tbl_user.Where(Supervisor => Supervisor.role_id == role2 && Supervisor.status == 1).SingleOrDefault(u => u.username == use_acc.username && (u.password) == pass);
                                    if (getSupervisor != null)
                                    {
                                        Session["User_Id"] = getSupervisor.user_id.ToString();
                                        Session["User_Name"] = getSupervisor.username.ToString();
                                        Session["firstname"] = getSupervisor.firstName.ToString();
                                        Session["profile_pict"] = getSupervisor.profile_pict.ToString();
                                        Session["User_Roll_Id"] = getSupervisor.role_id.ToString();
                                        try
                                        {
                                            Session["timezone"] = getSupervisor.timezoneid.ToString();
                                        }
                                        catch
                                        {
                                            Session["timezone"] = "Central Standard Time";
                                        }

                                        HttpCookie cookie = new HttpCookie("Timezone");
                                        cookie.Value = getSupervisor.timezoneid.ToString();
                                        this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);

                                        AccountCookies.Value = "Testing";
                                        this.ControllerContext.HttpContext.Response.Cookies.Add(AccountCookies);
                                        AccountCookies.Expires = DateTime.Now.AddHours(1);

                                        u_Id = getSupervisor.user_id.ToString();
                                        u_Name = getSupervisor.username.ToString();
                                        u_Roll_Id = getSupervisor.role_id.ToString();

                                        return RedirectToAction("Employee", "Employee");

                                    }
                                    else
                                    {
                                        var role3 = RoleType.Workers.GetHashCode();
                                        var getuser = dbcon.tbl_user.Where(userlogin => userlogin.role_id == role3 && userlogin.status == 1).SingleOrDefault(u => u.username == use_acc.username && (u.password) == pass);
                                        if (getuser != null)
                                        {
                                            Session["User_Id"] = getuser.user_id.ToString();
                                            Session["User_Name"] = getuser.username.ToString();
                                            Session["firstname"] = getuser.firstName.ToString();
                                            Session["profile_pict"] = getuser.profile_pict.ToString();
                                            Session["User_Roll_Id"] = getuser.role_id.ToString();
                                            u_Id = getuser.user_id.ToString();
                                            u_Name = getuser.username.ToString();
                                            u_Roll_Id = getuser.role_id.ToString();
                                            return RedirectToAction("Client", "Client");
                                        }
                                        else
                                        { ModelState.AddModelError("", "Invalid username or password !"); }

                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Usename and Password Required !");
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
            }
            //ModelState.Clear();
            return View();
        }


        //Getchange password screen
        public ActionResult changePassword()
        {
            return View();
        }
        //post password to DB
        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="changepass">The changepass.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult changePassword(changePassword changepass)
        {

            if (Session["User_Id"] != null)
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    var old_pass = changepass.oldPassword.ToString().Trim();
                    var new_pass = dbcon.Encrypt(changepass.NewPassword.ToString());
                    tbl_user user = null;

                    var user_id = Convert.ToInt32(Session["User_Id"].ToString());
                    string password_fromdb;
                    var getuser_pass = dbcon.tbl_user.SingleOrDefault(user_id_db => user_id_db.user_id == user_id);
                    user = dbcon.tbl_user.Where(s => s.user_id == user_id).FirstOrDefault<tbl_user>();
                    if (getuser_pass != null)
                    {
                        password_fromdb = dbcon.Decrypt(getuser_pass.password.ToString());
                        if (password_fromdb == old_pass)
                        {
                            user.password = Convert.ToString(new_pass);

                            dbcon.tbl_user.Attach(user);
                            dbcon.Entry(user).State = System.Data.Entity.EntityState.Modified;
                            dbcon.SaveChanges();
                            ViewBag.message = "Your password has been changed.";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(old_pass))
                            {
                                ModelState.AddModelError("oldPassword", "Please enter old password.");
                            }
                            else
                            {
                                ModelState.AddModelError("", "Invalid old password!");
                            }

                        }
                    }
                    else
                    {
                        ViewBag.InvalidUser = "Invalid Email";
                    }
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        //Reset password screen
        public ActionResult ResetPassword()
        {
            return View();
        }

        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <param name="changepass">The changepass.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ResetPassword(changePassword changepass)
        {
            if (Session["User_Id"] != null)
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    var old_pass = changepass.oldPassword.ToString();
                    var new_pass = dbcon.Encrypt(changepass.NewPassword.ToString());
                    tbl_user user = null;
                    var user_id = Convert.ToInt32(Session["User_Id"].ToString());
                    string password_fromdb;
                    var getuser_pass = dbcon.tbl_user.SingleOrDefault(user_id_db => user_id_db.user_id == user_id);
                    user = dbcon.tbl_user.Where(s => s.user_id == user_id).FirstOrDefault<tbl_user>();
                    if (getuser_pass != null)
                    {
                        password_fromdb = dbcon.Decrypt(getuser_pass.password.ToString());
                        if (password_fromdb == old_pass)
                        {
                            user.password = Convert.ToString(new_pass);
                            dbcon.tbl_user.Attach(user);
                            dbcon.Entry(user).State = System.Data.Entity.EntityState.Modified;
                            dbcon.SaveChanges();
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid old password!");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid user!");
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
        /// Logouts this instance.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Logout()
        {
            Session["User_Id"] = null;
            Session["appointment_save"] = null; // sesssion that call when save button press to assign appointment to employee
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();
            AccountCookies.Value = null;
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Forgots the password.
        /// </summary>
        /// <returns></returns>
        public ActionResult ForgotPassword()
        {
            return View();
        }
        /// <summary>
        /// Forgots the password.
        /// </summary>
        /// <param name="Fpss">The FPSS.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ForgotPassword(juggle.Models.Forgotpassword_Model Fpss)
        {
            var female = Fpss.Email.ToString();
            using (jugglecontext dbcon = new jugglecontext())
            {
                var getPass = dbcon.tbl_user.Where(useremail => useremail.email == female).SingleOrDefault();
                if (getPass != null)
                {
                    var Password = dbcon.Decrypt(getPass.password.ToString());
                    var UseraName = getPass.username.ToString();
                    try
                    {
                        string Body = "Your  Username : '" + UseraName + "' and password :'" + Password + "'";
                        dbcon.SendMail(Fpss.Email, "Forgot Password - Juggle", Body, Password, UseraName);
                        ViewBag.Isokay = "auth";
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Invalid user!");
                    }
                }
                else
                {

                }
            }
            return View();
        }
        /// <summary>
        /// Doesemails the exist.
        /// </summary>
        /// <param name="Email">The email.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult doesemailExist(string Email)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                var role = RoleType.Supervisors.GetHashCode();
                var id = (from a in dbcon.tbl_user where a.email == Email && a.role_id == role select a).ToList();
                if (id.Count > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(string.Format("Email Address is not exist"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteCDNLog(ex.ToString(), "doesemailExist");
                return Json("error", JsonRequestBehavior.AllowGet);
            }
        }
    }
}
