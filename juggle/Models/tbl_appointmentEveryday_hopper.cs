namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_appointmentEveryday_hopper
    {
        [Key]
        public int appointmentEveryday_id { get; set; }

        public int? appointment_id { get; set; }

        public DateTime? appointment_date { get; set; }

        public int? emp_id { get; set; }

        public int? timeinterval_id { get; set; }

        [StringLength(50)]
        public string appointment_starttime { get; set; }

        [StringLength(50)]
        public string appointment_endtime { get; set; }

        public DateTime? created_date { get; set; }
    }
}
