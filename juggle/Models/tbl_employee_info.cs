namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    public partial class tbl_employee_info
    {
        [Key]
        public int emp_id { get; set; }

        public int? user_id { get; set; }

        [Required(ErrorMessage = "Please select Attribute.")]
        public string attribute_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? emp_code { get; set; }

        [Required(ErrorMessage = "Please enter FirstName.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Invalid Input")]
        public string emp_firstname { get; set; }

        [Required(ErrorMessage = "Please enter Employee Transportation.")]
        //[RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Invalid Input")]
        [RegularExpression(@"[A-Za-z ]*", ErrorMessage = "Invalid Input")]


        public string emp_transportion { get; set; }

        [Required(ErrorMessage = "Please enter LastName.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Invalid Input")]
        public string emp_lastname { get; set; }


        [StringLength(50)]
        [Required(ErrorMessage = "Google Calender is required.")]
        [RegularExpression("[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z](?:[a-z-]*[a-z])?\\.)+[a-z](?:[a-z-]*[a-z])?", ErrorMessage = "Invalid Email Address")]
        //[Remote("doesemailexist", "Employee", HttpMethod = "POST", ErrorMessage = "Email already exist.", AdditionalFields = "emp_id")]
        [Remote("doesemailexist", "Employee", HttpMethod = "POST", ErrorMessage = "Email already exist.", AdditionalFields = "emp_id")]
        public string emp_googlecalendarID { get; set; }

        [Required(ErrorMessage = "Please select Qualified Service Types")]
        public string emp_qualifiedservicetypes { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"(?=.*\d)(?=.*[A-Za-z]).{6,}", ErrorMessage = "Password must be at least 6 characters long and contain at least 1 letter and 1 number")]
        public string emp_password { get; set; }

        public string emp_name { get; set; }

        public string emp_email { get; set; }

        //public int? emp_transpotation { get; set; }

     

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Please enter Phone No.")]
        //[RegularExpression(@"^(?!0+$)\d{8,}$", ErrorMessage = "Not a valid Phone number")]
        [Remote("doesphonenoexist", "Employee", HttpMethod = "POST", ErrorMessage = "Phone Number already exist.", AdditionalFields = "emp_id")]
        public string emp_contactinfo { get; set; }

        public int? emp_status { get; set; }

        public string emp_note { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? updated_date { get; set; }
    }
}
