namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_status
    {
        [Key]
        public int status_id { get; set; }

        [StringLength(50)]
        public string status { get; set; }

        public string Descreption { get; set; }
    }
}
