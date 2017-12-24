using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace juggle.Models
{
    public class Forgotpassword_Model
    {

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression("[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z](?:[a-z-]*[a-z])?\\.)+[a-z](?:[a-z-]*[a-z])?", ErrorMessage = "Invalid Email Address")]
        [Remote("doesemailExist", "Account", HttpMethod = "POST", ErrorMessage = "Email Address is not exist")]
        public string Email { get; set; }


    }
}