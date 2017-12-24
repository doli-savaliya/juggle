namespace Customer_Detail.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Customer_Details
    {
        [Key]
        public int ID { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "FirstName is required")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Invalid FirstName")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "LastName is required")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Invalid LastName")]
        public string LastName { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }


        public string Image { get; set; }
    }
}
