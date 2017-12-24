
namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;
    public partial class tbl_employee_availability
    {
        [Key]
        public int empavailability_id { get; set; }

        [Column(TypeName = "date")]
        [Required(ErrorMessage = "Select Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? start_date { get; set; }

        [Column(TypeName = "date")]
        [Required(ErrorMessage = "Select End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? end_date { get; set; }

        [StringLength(50)]
        //[Remote("checktimerangestart", "EmployeeAvailability", HttpMethod = "POST", ErrorMessage = "Error in time start range.", AdditionalFields = "start_date,end_date")]
        [Required(ErrorMessage = "Please Select Start Time")]
        public string time_range_start { get; set; }

        [StringLength(50)]
        [Remote("checktimerangeend", "EmployeeAvailability", HttpMethod = "POST", ErrorMessage = "Error in time end range.", AdditionalFields = "start_date,time_range_start")]
        [Required(ErrorMessage = "Please Select End Time")]
        public string time_range_end { get; set; }

        public int? userid { get; set; }

        [Required(ErrorMessage = "Select Employee")]
        [Remote("doesemployeeavailability", "EmployeeAvailability", HttpMethod = "POST", ErrorMessage = "This Employee has already availability.", AdditionalFields = "empavailability_id,time_range_start,time_range_end,start_date,end_date")]
        public int? emp_id { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? updated_date { get; set; }
    }
}

