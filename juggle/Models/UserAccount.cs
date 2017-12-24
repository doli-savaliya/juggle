using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace Juggle.Models
{
    public class UserAccount
    {

        [Key]
        public int User_Id { get; set; }
        [Required(ErrorMessage ="Please enter username.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Please enter password.")]
        public string Password { get; set; }
        public Nullable<int> Role_Id { get; set; }

    }
}