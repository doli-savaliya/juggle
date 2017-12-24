using juggle.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace juggle.Controllers
{
    public class AttributeController : Controller
    {
        // GET: Attribute
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Attribute() // banse
        {
            try
            {
                if (Session["User_Id"] != null && Session["User_Roll_Id"].ToString() == "2")
                {
                    using (jugglecontext dbcon = new jugglecontext())
                    {
                        var userId = Convert.ToInt32(Session["User_Id"]);
                        var attribute = (from a in dbcon.tbl_attribute_data
                                         where a.user_id == userId
                                         orderby a.attribute_id descending
                                         select a).ToList();

                        return View(attribute);
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                string rs = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    rs = string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    Console.WriteLine(rs);

                    foreach (var ve in eve.ValidationErrors)
                    {
                        rs += "<br />" + string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw new Exception(rs);
            }


        }

        //Get and post create client
        [HttpGet]
        public PartialViewResult Create_attribute()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Create_attribute(tbl_attribute_data attribute)
        {
            if (Session["User_Id"] != null)
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    tbl_attribute_data data_attribute = new tbl_attribute_data();
                    data_attribute.attribute_name = attribute.attribute_name;
                    data_attribute.created_date = DateTime.Now;
                    data_attribute.user_id = Convert.ToInt32(Session["User_Id"]);
                    dbcon.tbl_attribute_data.Add(data_attribute);
                    dbcon.SaveChanges();
                    return RedirectToAction("attribute", "Attribute");
                }
            }

            else
            {
                return RedirectToAction("Login", "Account");
            }

        }


        //Edit attribute
        [HttpGet]
        public ActionResult attribute_Edit(Int32 attribute_id) // banse
        {
            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    tbl_attribute_data work_type = dbcon.tbl_attribute_data.Where(x => x.attribute_id == attribute_id).FirstOrDefault();
                    tbl_attribute_data prod = new tbl_attribute_data();
                    prod.attribute_id = work_type.attribute_id;
                    prod.attribute_name = work_type.attribute_name;
                    prod.created_date = work_type.created_date;
                    prod.updated_date = DateTime.Now;
                    prod.user_id = Convert.ToInt32(Session["User_Id"]);
                    return View(prod);
                }
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult attribute_Edit(juggle.Models.tbl_attribute_data work_type_category)
        {
            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {
                    var userId = Convert.ToInt32(Session["User_Id"]);
                    tbl_attribute_data attribute = new tbl_attribute_data();
                    attribute.attribute_name = work_type_category.attribute_name.Trim();
                    attribute.attribute_id = work_type_category.attribute_id;
                    attribute.updated_date = DateTime.Now;
                    attribute.created_date = work_type_category.created_date;
                    attribute.user_id = userId;
                    dbcon.Entry(attribute).State = System.Data.Entity.EntityState.Modified;
                    dbcon.SaveChanges();
                    return RedirectToAction("attribute", "Attribute");
                }
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult attribute_delete(Int32 atrribute_id)
        {
            try
            {
                using (jugglecontext dbcon = new jugglecontext())
                {

                    if (Session["User_Id"] != null)
                    {
                        string atrributeid = atrribute_id.ToString();
                        // when delete attribute then remove that attribute from the appointment, client,employee
                        var client_detail = (from a in dbcon.tbl_client
                                             where a.attribute_id.Contains(atrributeid)
                                             select a).ToList();

                        var employee_detail = (from a in dbcon.tbl_employee_info
                                               where a.attribute_id.Contains(atrributeid)
                                               select a).ToList();

                        var appointment_detail = (from a in dbcon.tbl_appointment
                                                  where a.attribute_id.Contains(atrributeid)
                                                  select a).ToList();

                        for (int i = 0; i < client_detail.Count; i++)
                        {
                            string client_attri = client_detail[i].attribute_id.ToString();
                            client_attri = client_attri.Replace(atrributeid, "");
                            SqlConnection connection = new SqlConnection(dbcon.connectionString());
                            var command = new SqlCommand("[updatedata]", connection);
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@client_id", Convert.ToInt32(client_detail[i].client_id));
                            command.Parameters.AddWithValue("@attribute_id", client_attri);
                            command.Parameters.AddWithValue("@appointment_id", 0);
                            command.Parameters.AddWithValue("@StatementType", "attribute_data");
                            command.Parameters.AddWithValue("@emp_id", 0);
                            command.Parameters.AddWithValue("@startdate", 0);
                            command.Parameters.AddWithValue("@enddate", 0);
                            command.Parameters.AddWithValue("@StartTime", 0);
                            command.Parameters.AddWithValue("@endTime", 0);
                            command.Parameters.AddWithValue("@user_id", Convert.ToInt32(Session["User_Id"]));
                            connection.Open();
                            SqlDataReader rdr = command.ExecuteReader();
                        }

                        for (int i = 0; i < employee_detail.Count; i++)
                        {
                            string emp_attri = employee_detail[i].attribute_id.ToString();
                            emp_attri = emp_attri.Replace(atrributeid, "");
                            SqlConnection connection = new SqlConnection(dbcon.connectionString());
                            var command = new SqlCommand("[updatedata]", connection);
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@client_id", 0);
                            command.Parameters.AddWithValue("@attribute_id", emp_attri);
                            command.Parameters.AddWithValue("@StatementType", "attribute_data");
                            command.Parameters.AddWithValue("@appointment_id", 0);
                            command.Parameters.AddWithValue("@emp_id", employee_detail[i].emp_id);
                            command.Parameters.AddWithValue("@startdate", 0);
                            command.Parameters.AddWithValue("@enddate", 0);
                            command.Parameters.AddWithValue("@StartTime", 0);
                            command.Parameters.AddWithValue("@endTime", 0);
                            command.Parameters.AddWithValue("@user_id", Convert.ToInt32(Session["User_Id"]));
                            connection.Open();
                            SqlDataReader rdr = command.ExecuteReader();
                        }

                        for (int i = 0; i < appointment_detail.Count; i++)
                        {
                            string appointment_attri = appointment_detail[i].attribute_id.ToString();
                            appointment_attri = appointment_attri.Replace(atrributeid, "");
                            SqlConnection connection = new SqlConnection(dbcon.connectionString());
                            var command = new SqlCommand("[updatedata]", connection);
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@client_id", 0);
                            command.Parameters.AddWithValue("@attribute_id", appointment_attri);
                            command.Parameters.AddWithValue("@StatementType", "attribute_data");
                            command.Parameters.AddWithValue("@appointment_id", appointment_detail[i].appointment_id);
                            command.Parameters.AddWithValue("@emp_id", 0);
                            command.Parameters.AddWithValue("@startdate", 0);
                            command.Parameters.AddWithValue("@enddate", 0);
                            command.Parameters.AddWithValue("@StartTime", 0);
                            command.Parameters.AddWithValue("@endTime", 0);
                            command.Parameters.AddWithValue("@user_id", Convert.ToInt32(Session["User_Id"]));
                            connection.Open();
                            SqlDataReader rdr = command.ExecuteReader();
                        }
                        tbl_attribute_data objEmp = dbcon.tbl_attribute_data.Find(atrribute_id);
                        dbcon.tbl_attribute_data.Remove(objEmp);
                        dbcon.SaveChanges();
                    }
                    return RedirectToAction("attribute", "Attribute");
                }
            }
            catch
            {
                return View();
            }
        }

        [AllowAnonymous]
        public JsonResult doesattributeExist(string attribute_name, int attribute_id = 0)
        {
            try
            {
                jugglecontext dbcon = new jugglecontext();
                int userid = Convert.ToInt32(Session["User_Id"]);
                return dbcon.tbl_attribute_data.Any(x => x.attribute_id != attribute_id && x.attribute_name == attribute_name && x.user_id == userid)
                         ? Json(string.Format("{0} already exists.", attribute_name),
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