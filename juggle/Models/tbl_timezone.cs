namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    public partial class tbl_timezone
    {
        [Key]
        public int time_id { get; set; }
       
        public string time_zone_Id { get; set; }

        public string time_zone_displayname { get; set; }
    }
}
