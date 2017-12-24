namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    public partial class tbl_attribute_data
    {
        [Key]
        public int attribute_id { get; set; }

        public int? user_id { get; set; }

        [StringLength(60)]
        [Required(ErrorMessage = "Attribute Name is required ")]
        // [RegularExpression(@"/^[a-zA-Z ]*$/", ErrorMessage = "Invalid Input")]

        [RegularExpression(@"^[a-zA-Z ]*$", ErrorMessage = "Invalid Input")]
        [Remote("doesattributeExist", "Attribute", HttpMethod = "POST", ErrorMessage = "AttributeName already exist.", AdditionalFields = "attribute_id")]
        public string attribute_name { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? updated_date { get; set; }
    }
}
