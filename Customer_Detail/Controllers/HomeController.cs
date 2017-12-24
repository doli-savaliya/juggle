using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Customer_Detail.Models;

namespace Customer_Detail.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        Customer_Context dbcon = new Customer_Context();

        public ActionResult Index()
        {
            var cust_list = (from a in dbcon.Customer_Details
                             select a).ToList();
            return View(cust_list);
        }
        [HttpGet]
        public ActionResult AddCustomre()
        {
            // get client data
            var countrylist = from p in dbcon.CountryMasters
                              select new { p.CountryID, p.Country_Name, };
            var countrylistx = countrylist.ToList().Select(c => new SelectListItem
            {
                Text = c.Country_Name.ToString(),
                Value = c.CountryID.ToString(),
            }).ToList();

            ViewBag.viewcountrylist = countrylistx;

            var Statelist = from p in dbcon.StateMasters
                            select new { p.StateID, p.StateName, };
            var Statelistx = Statelist.ToList().Select(c => new SelectListItem
            {
                Text = c.StateName.ToString(),
                Value = c.StateID.ToString(),
            }).ToList();

            ViewBag.viewstatelist = Statelistx;

            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="custdetail"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddCustomre(Customer_Details custdetail, HttpPostedFileBase Image)
        {
            Customer_Details custlist = new Customer_Details();
            custlist.FirstName = custdetail.FirstName;
            custlist.LastName = custdetail.LastName;
            custlist.Country = custdetail.Country;
            custlist.State = custdetail.State;



            // file1 to store image in binary formate and file2 to store path and url  
            // we are checking file1 and file2 are null or not according to that different case are there  
            if (Image != null && Image.ContentLength > 0)
            {
                string ImageName = System.IO.Path.GetFileName(Image.FileName);
                string physicalPath = Server.MapPath("~/img/" + ImageName);
                // save image in folder  
                Image.SaveAs(physicalPath);
                custlist.Image = "img/" + ImageName;
            }
            //custlist.Image = custdetail.Image;
            dbcon.Customer_Details.Add(custlist);
            dbcon.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult editcustomer(int id)
        {
            Customer_Details custlist = new Customer_Details();
            Customer_Details client = dbcon.Customer_Details.Where(x => x.ID == id).FirstOrDefault();

            custlist.FirstName = client.FirstName;
            custlist.LastName = client.LastName;
            custlist.Country = client.Country;
            custlist.State = client.State;
            custlist.Image = client.Image;
            int co_id = Convert.ToInt32(client.Country);

            var countrylist = from p in dbcon.CountryMasters
                              select new { p.CountryID, p.Country_Name, };
            var countrylistx = countrylist.ToList().Select(c => new SelectListItem
            {
                Text = c.Country_Name.ToString(),
                Value = c.CountryID.ToString(),
                Selected = (c.CountryID == co_id)
            }).ToList();

            ViewBag.viewcountrylist = countrylistx;

            var Statelist = from p in dbcon.StateMasters
                            select new { p.StateID, p.StateName, };
            var Statelistx = Statelist.ToList().Select(c => new SelectListItem
            {
                Text = c.StateName.ToString(),
                Value = c.StateID.ToString(),
            }).ToList();

            ViewBag.viewstatelist = Statelistx;

            return View(custlist);
        }

        [HttpPost]
        public ActionResult editcustomer(Customer_Details custlist, HttpPostedFileBase Image)
        {
            Customer_Details custlisting = new Customer_Details();

            custlisting.FirstName = custlist.FirstName;
            custlisting.LastName = custlist.LastName;
            custlisting.Country = custlist.Country;
            custlisting.State = custlist.State;

            if (Image != null && Image.ContentLength > 0)
            {
                string ImageName = System.IO.Path.GetFileName(Image.FileName);
                string physicalPath = Server.MapPath("~/img/" + ImageName);
                // save image in folder  
                Image.SaveAs(physicalPath);
                custlisting.Image = "img/" + ImageName;
            }

            //custlisting.Image = custlist.Image;
            custlisting.ID = custlist.ID;
            dbcon.Entry(custlisting).State = System.Data.Entity.EntityState.Modified;
            dbcon.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public void Deletecustomer(int id)
        {
            Customer_Details objclient = dbcon.Customer_Details.Find(id);
            dbcon.Customer_Details.Remove(objclient);
            dbcon.SaveChanges();
        }

        public ActionResult changestate(int c_id)
        {

            var statelist = (from a in dbcon.StateMasters
                             where a.CountryID == c_id
                             select a).ToList();
            if (statelist.Count > 0)
            {
                return Json(new { data = statelist.ToList() }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "No" }, JsonRequestBehavior.AllowGet);
            }

        }

    }
}