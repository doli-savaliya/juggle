namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_role
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_role()
        {
            tbl_user = new HashSet<tbl_user>();
            tbl_user_invitations = new HashSet<tbl_user_invitations>();
        }

        [Key]
        public int role_id { get; set; }

        [StringLength(50)]
        public string role_name { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? updated_date { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_user> tbl_user { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_user_invitations> tbl_user_invitations { get; set; }
    }
}
