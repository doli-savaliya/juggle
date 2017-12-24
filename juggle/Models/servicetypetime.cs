namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("servicetypetime")]
    public partial class servicetypetime
    {
        public int id { get; set; }

        public int? value { get; set; }

        public int? timeinminute { get; set; }
    }
}
