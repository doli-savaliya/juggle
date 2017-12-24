namespace Customer_Detail.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("StateMaster")]
    public partial class StateMaster
    {
        [Key]
        public int StateID { get; set; }

        [StringLength(50)]
        public string StateName { get; set; }

        public int? CountryID { get; set; }
    }
}
