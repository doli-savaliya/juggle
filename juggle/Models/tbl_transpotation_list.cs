namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_transpotation_list
    {
        [Key]
        public int transpotation_id { get; set; }

        [StringLength(50)]
        public string transpotation_name { get; set; }
    }
}
