namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    public partial class tbl_user
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_user()
        {
            tbl_client = new HashSet<tbl_client>();
            tbl_worktype = new HashSet<tbl_worktype>();
        }

        [Key]
        public int user_id { get; set; }

        public int? role_id { get; set; }

        public int? supervisor_id { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Please enter FirstName.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Invalid Input")]
        public string firstName { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Please enter LastName.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Invalid Input")]
        public string lastName { get; set; }

        [Required(ErrorMessage = "Please enter Address.")]
        public string address { get; set; }

        [Required(ErrorMessage = "Please enter Phone No.")]
        [RegularExpression(@"^(?!0+$)\d{8,}$", ErrorMessage = "Not a valid Phone number")]
        [Remote("doesphoneexist", "Supervisors", HttpMethod = "POST", ErrorMessage = "Phone Number already exist.", AdditionalFields = "user_id")]
        public string phoneno { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Please enter Email.")]
        [RegularExpression("[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z](?:[a-z-]*[a-z])?\\.)+[a-z](?:[a-z-]*[a-z])?", ErrorMessage = "Invalid Email Address")]
        [Remote("doesemailexist", "Supervisors", HttpMethod = "POST", ErrorMessage = "Email already exist.", AdditionalFields = "user_id")]
        public string email { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Please enter Username.")]
        [RegularExpression(@"(?!^\d+$)^.+$", ErrorMessage = "Invalid UserName")]
        [Remote("doesUserNameExist", "Supervisors", HttpMethod = "POST", ErrorMessage = "Email already exist.", AdditionalFields = "user_id")]
        public string username { get; set; }

        public string password { get; set; }

        [Required(ErrorMessage = "Please enter Status.")]
        public int? status { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? updated_date { get; set; }

        public string timezoneid { get; set; }

        //[Required(ErrorMessage = "Please add Profile Picture.")]
        //[FileExtensions(Extensions = "png", ErrorMessage = "Please upload valid format")]
        
        public string profile_pict { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_client> tbl_client { get; set; }

        public virtual tbl_role tbl_role { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_worktype> tbl_worktype { get; set; }

       
     
    }

    public enum RoleType
    {
        Administrator=1 ,
        Supervisors=2,
        Workers=3,
        Client=4
    }
}
