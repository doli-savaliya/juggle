using juggle.Models;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace juggle.Controllers
{
    public class ClientController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get Clients data.
        /// </summary>
        /// <returns></returns>
        public ActionResult Client()
        {
            if (Session["User_Id"] != null && Session["User_Roll_Id"].ToString() == "2")
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    var userId = Convert.ToInt32(Session["User_Id"]);
                    var client_list = (from a in dbcon.tbl_client
                                       where a.user_id == userId
                                       orderby a.client_id descending
                                       select new
                                       {
                                           clientname = a.client_firstname + " " + a.client_lastname,
                                           a.client_contact_info,
                                           a.client_address,
                                           a.client_id

                                       }).ToList();
                    return View(client_list);
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }

        /// <summary>
        /// Requests data from a specified resource
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public PartialViewResult Create_Client()
        {
            jugglecontext dbcon = new jugglecontext();
            int userid = Convert.ToInt32(Session["User_Id"]);
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

        /// <summary>
        ///  Submits client data to be processed to a specified resource
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create_Client(tbl_client Client)
        {
            if (Session["User_Id"] != null)
            {
                try
                {
                    using (jugglecontext dbcon = new jugglecontext())
                    {
                        var getButton = HttpContext.Request.Form["Save"];
                        if (getButton == "Save")
                        {
                            string multipleattribute = Request.Form["attribute_id_multiple"].ToString();
                            {
                                var getScheduleId = Request.Form["schedule_hidden"];
                                var x_lat = 0.0;
                                var y_long = 0.0;
                                var clientnote = string.Empty;

                                try
                                {
                                    x_lat = Convert.ToDouble(Request.Form["x_lat"]);
                                }
                                catch
                                {
                                    x_lat = 0.0;
                                }

                                try
                                {
                                    y_long = Convert.ToDouble(Request.Form["y_long"]);
                                }
                                catch
                                {
                                    y_long = 0.0;
                                }

                                tbl_client tblClient = new tbl_client();
                                tblClient.client_firstname = Client.client_firstname;
                                tblClient.client_lastname = Client.client_lastname;
                                tblClient.client_companyname = Client.client_companyname;
                                tblClient.client_secondaryname = Client.client_secondaryname;
                                tblClient.client_email = Client.client_email;
                                tblClient.client_code = autogenerateid();
                                tblClient.client_contact_info = Client.client_contact_info;
                                tblClient.client_address = Client.client_address;
                                if (!string.IsNullOrEmpty(Client.client_note))
                                {
                                    tblClient.client_note = Client.client_note;
                                }
                                else
                                {
                                    tblClient.client_note = "N/A";
                                }
                                tblClient.x_lat = x_lat;
                                tblClient.y_long = y_long;
                                tblClient.attribute_id = multipleattribute.ToString();
                                tblClient.created_date = DateTime.Now;
                                tblClient.user_id = Convert.ToInt32(Session["User_Id"]);
                                dbcon.tbl_client.Add(tblClient);
                                dbcon.SaveChanges();
                                return RedirectToAction("Client", "Client");
                            }
                        }
                        else
                        {

                        }
                        return View();
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

                    return View(Client);
                }
            }

            else
            {
                return RedirectToAction("Login", "Account");
            }

        }

        /// <summary>
        /// Requests client data from a specified resource
        /// </summary>
        /// <param name="client_id">The client identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Client_Edit(Int32 client_id)
        {
            using (jugglecontext dbcon = new jugglecontext())
            {
                jugglecontext dbcon1 = new jugglecontext();
                int userid = Convert.ToInt32(Session["User_Id"]);
                var attribute = from p in dbcon.tbl_attribute_data
                                where p.user_id == userid
                                select new { p.attribute_id, p.attribute_name };

                var getmultipledata = (from a in dbcon.tbl_client
                                       where a.client_id == client_id
                                       select a).ToList();

                string multipleattribute = getmultipledata[0].attribute_id.ToString();
                tbl_client client = dbcon.tbl_client.Where(x => x.client_id == client_id).FirstOrDefault();
                tbl_client client_info = new tbl_client();
                client_info.client_id = client.client_id;
                client_info.client_firstname = client.client_firstname;
                client_info.client_lastname = client.client_lastname;
                client_info.client_companyname = client.client_companyname;
                client_info.client_secondaryname = client.client_secondaryname;
                client_info.client_email = client.client_email;
                client_info.client_code = client.client_code;
                client_info.client_contact_info = client.client_contact_info;
                client_info.client_address = client.client_address;
                client_info.client_note = client.client_note;
                client_info.x_lat = client.x_lat;
                client_info.y_long = client.y_long;
                client_info.created_date = client.created_date;
                client_info.updated_date = DateTime.Now;
                client_info.x_lat = client.x_lat;
                client_info.y_long = client.y_long;
                client_info.attribute_id = client.attribute_id;
                client.attribute_id = client.attribute_id;
                var attri = attribute.ToList().Select(c => new SelectListItem
                {
                    Text = c.attribute_name.ToString(),
                    Value = c.attribute_id.ToString(),
                }).ToList();

                ViewBag.attributelist = attri;

                return View(client);
            }

        }
        /// <summary>
        /// Submits client data to be processed to a specified resource
        /// </summary>
        /// <param name="clientmodel">The clientmodel.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Client_Edit(tbl_client clientmodel)
        {
            if (Session["User_Id"] != null)
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    string multipleattribute = Request.Form["attribute_id_multiple"].ToString();
                    var x_lat = 0.0;
                    var y_long = 0.0;
                    var getScheduleId = 0;
                    try
                    {
                        x_lat = Convert.ToDouble(Request.Form["x_lat"]);
                    }
                    catch
                    {
                        x_lat = 0.0;
                    }
                    try
                    {
                        y_long = Convert.ToDouble(Request.Form["y_long"]);
                    }
                    catch
                    {
                        y_long = 0.0;
                    }
                    try
                    {
                        getScheduleId = Convert.ToInt32(Request.Form["schedule_hidden"]);
                    }
                    catch { }

                    tbl_client client_info = new tbl_client();
                    client_info.client_id = clientmodel.client_id;
                    client_info.client_firstname = clientmodel.client_firstname;
                    client_info.client_lastname = clientmodel.client_lastname;
                    client_info.client_companyname = clientmodel.client_companyname;
                    client_info.client_secondaryname = clientmodel.client_secondaryname;
                    client_info.client_email = clientmodel.client_email;
                    client_info.client_contact_info = clientmodel.client_contact_info;
                    client_info.client_address = clientmodel.client_address;
                    if (!string.IsNullOrEmpty(clientmodel.client_note))
                    {
                        client_info.client_note = clientmodel.client_note;
                    }
                    else
                    {
                        client_info.client_note = "N/A";
                    }

                    client_info.user_id = clientmodel.user_id;
                    client_info.updated_date = DateTime.Now;
                    client_info.created_date = clientmodel.created_date;
                    client_info.attribute_id = multipleattribute;
                    client_info.client_code = clientmodel.client_code;

                    if (x_lat == clientmodel.x_lat)
                    {
                        client_info.x_lat = clientmodel.x_lat;
                    }
                    else
                    {
                        client_info.x_lat = x_lat;
                    }
                    if (y_long == clientmodel.y_long)
                    {
                        client_info.y_long = clientmodel.y_long;
                    }
                    else
                    {
                        client_info.y_long = y_long;
                    }
                    dbcon.Entry(client_info).State = System.Data.Entity.EntityState.Modified;
                    dbcon.SaveChanges();
                    return RedirectToAction("Client", "Client");
                }

            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }
        [HttpPost]
        public ActionResult Client_delete(Int32 client_id)
        {
            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    if (Session["User_Id"] != null)
                    {
                        tbl_client objclient = dbcon.tbl_client.Find(client_id);
                        dbcon.tbl_client.Remove(objclient);
                        dbcon.SaveChanges();
                    }
                    return RedirectToAction("Client", "Client");
                }
            }
            catch
            {
                return View();
            }
        }
        //auto genreated client ID
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
        //remote validation to check client exist or not 
        [AllowAnonymous]
        public JsonResult doesclientsExist(string client_email, int client_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                int userid = Convert.ToInt32(Session["User_Id"]);
                var does_emp_email = (from a in dbcon.tbl_employee_info
                                      where a.emp_googlecalendarID == client_email && a.user_id == userid
                                      select a).ToList();
                if (does_emp_email.Count > 0)
                {
                    return Json(string.Format("{0} already exists.", client_email));
                }
                return dbcon.tbl_client.Any(x => x.client_id != client_id && x.client_email == client_email && x.user_id == userid)
                         ? Json(string.Format("{0} already exists.", client_email),
                                                JsonRequestBehavior.AllowGet)
                      : Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
        }

        //remote validation to check client exist or not 
        [AllowAnonymous]
        public JsonResult doesphoneExist(string client_contact_info, int client_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                int userid = Convert.ToInt32(Session["User_Id"]);

                var does_emp_email = (from a in dbcon.tbl_employee_info
                                      where a.emp_contactinfo == client_contact_info && a.user_id == userid
                                      select a).ToList();
                if (does_emp_email.Count > 0)
                {
                    return Json(string.Format("{0} already exists.", client_contact_info));
                }

                return dbcon.tbl_client.Any(x => x.client_id != client_id && x.client_contact_info == client_contact_info && x.user_id == userid)
                         ? Json(string.Format("{0} already exists.", client_contact_info),
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