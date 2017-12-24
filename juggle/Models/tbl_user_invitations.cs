namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_user_invitations
    {
        [Key]
        public int user_invitations_id { get; set; }

        public int? role_id { get; set; }

        [StringLength(50)]
        public string email { get; set; }

        public DateTime? expires_on { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? updated_date { get; set; }

        public virtual tbl_role tbl_role { get; set; }
    }
}
