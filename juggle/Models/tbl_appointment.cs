namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    public partial class tbl_appointment
    {
        [Key]
        public int appointment_id { get; set; }

        //[Remote("doesclientExist", "AppointMent", HttpMethod = "POST", ErrorMessage = " This Customer has already booked appointment.", AdditionalFields = "appointment_id,time_range_start,time_range_end,start_date,end_date,day,recurring")]
        [Required(ErrorMessage = "Select customer name")]
        public int? client_id { get; set; }

        public int? emp_id { get; set; }

        [Required(ErrorMessage = "Select service type name")]
        public int? work_id { get; set; }

        public int? user_id { get; set; }

      //  [Required(ErrorMessage = "Select Attribute")]
        public string attribute_id { get; set; }

        public int? time_interval_id { get; set; }

        [Required(ErrorMessage = "Please Enter Description")]
        public string description { get; set; }

        public Boolean recurring { get; set; }

        [Required(ErrorMessage = "Please Select Day")]
        public string day { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? start_date { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? end_date { get; set; }

        [Remote("checktimerangestart", "AppointMent", HttpMethod = "POST", ErrorMessage = "Error in time start range.", AdditionalFields = "start_date,end_date")]
        [Required(ErrorMessage = "Please Select Start Time")]
        public string time_range_start { get; set; }


        [Remote("checktimerangeend", "AppointMent", HttpMethod = "POST", ErrorMessage = "Error in time end range.", AdditionalFields = "start_date,time_range_start,time")]
        [Required(ErrorMessage = "Please Select End Time")]
        public string time_range_end { get; set; }

        public int? length { get; set; }

    //    [Required(ErrorMessage = "Please Enter Notes")]
        public string notes { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? Calendar_Start_datetime { get; set; }

        public DateTime? Calendar_End_datetime { get; set; }

        public DateTime? updated_date { get; set; }

        public int? time { get; set; }
    }
}
