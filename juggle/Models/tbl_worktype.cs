namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    public partial class tbl_worktype
    {
        [Key]
        public int work_id { get; set; }

        public int? user_id { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Service Type name is required")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Invalid input.")]
        [Remote("doesNameExist", "Work_Type", HttpMethod = "POST", ErrorMessage = "Service Type name already exist.", AdditionalFields = "work_id")]
        public string name { get; set; }


        [StringLength(100)]
        [Required(ErrorMessage = "Service Type Color is required")]
        [Remote("doescolorExist", "Work_Type", HttpMethod = "POST", ErrorMessage = "Service Type Color already exist.", AdditionalFields = "work_id")]
        public string color { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Service Type time is required")]
        public string time { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? updated_date { get; set; }

        public virtual tbl_user tbl_user { get; set; }
    }
}
