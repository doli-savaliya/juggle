using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace juggle.Models
{
    public class Transportlist
    {
        [Key]
        public int transpotation_id { get; set; }
        public string transpotation_name { get; set; }

    }
}