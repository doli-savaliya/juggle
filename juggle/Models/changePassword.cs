using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace juggle.Models
{
    public class changePassword
    {
        [Required (ErrorMessage ="Please enter old password.")]
        public string oldPassword { get; set; }
        [Required(ErrorMessage = "Please enter New password.")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Please enter confirm password.")]
        [Compare("NewPassword",ErrorMessage = "Confirm password and Password do not match.")]
        public string ConfirmPassword { get; set; }
        public int user_id { get; set; }


    }
}