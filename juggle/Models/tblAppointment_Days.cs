namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tblAppointment_Days
    {
        public int Id { get; set; }

        public int? Appoint_Id { get; set; }

        public int? MultipleValues { get; set; }
    }
}
