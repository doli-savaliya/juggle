namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    public partial class tbl_client
    {
        [Key]
        public int client_id { get; set; }

        public int? user_id { get; set; }

        public int? schedule_id { get; set; }

        [Required(ErrorMessage = "Please select Attribute.")]
        public string attribute_id { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Please enter FirstName.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Invalid Input")]
        public string client_firstname { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Please enter LastName.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Invalid Input")]
        public string client_lastname { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "CompanyName is required ")]
        //[RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Invalid Input")]
        public string client_companyname { get; set; }

        [StringLength(50)]
        public string client_secondaryname { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression("[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z](?:[a-z-]*[a-z])?\\.)+[a-z](?:[a-z-]*[a-z])?", ErrorMessage = "Invalid Email Address")]
        [Remote("doesclientsExist", "Client", HttpMethod = "POST", ErrorMessage = "Email already exist.", AdditionalFields = "client_id")]
        public string client_email { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? client_code { get; set; }

        [StringLength(15, MinimumLength = 10)]
        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone No is required ")]
        //   [RegularExpression(@"^(?!0+$)\d{8,}$", ErrorMessage = "Not a valid Phone number")]
        [RegularExpression(@"^[(]{0,1}[0-9]{3}[)]{0,1}[-\s\.]{0,1}[0-9]{3}[-\s\.]{0,1}[0-9]{4}$", ErrorMessage = "Not a valid Phone number")]
        [Remote("doesphoneExist", "Client", HttpMethod = "POST", ErrorMessage = "Phone Number already exist.", AdditionalFields = "client_id")]
        public string client_contact_info { get; set; }

        [StringLength(80)]
        [Required(ErrorMessage = "Address is required ")]
        public string client_address { get; set; }

        [StringLength(500)]
    //    [Required(ErrorMessage = "Note is required ")]
        public string client_note { get; set; }

        public double? x_lat { get; set; }

        public double? y_long { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? updated_date { get; set; }

        public virtual tbl_schedule tbl_schedule { get; set; }

        public virtual tbl_user tbl_user { get; set; }
    }
}
