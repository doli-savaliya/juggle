namespace Customer_Detail.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CountryMaster")]
    public partial class CountryMaster
    {
        [Key]
        public int CountryID { get; set; }

        [StringLength(50)]
        public string Country_Name { get; set; }
    }
}
