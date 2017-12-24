namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_time_interval
    {
        [Key]
        public int time_interval_id { get; set; }

        [StringLength(100)]
        public string time_interval { get; set; }

        [StringLength(50)]
        public string time_start { get; set; }

        [StringLength(50)]
        public string time_end { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? updated_date { get; set; }
    }
}
