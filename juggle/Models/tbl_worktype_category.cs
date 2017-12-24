namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_worktype_category
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_worktype_category()
        {
            tbl_worktype = new HashSet<tbl_worktype>();
        }

        [Key]
        public int worktypecat_id { get; set; }

        public string worktypecategory_name { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? updated_date { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_worktype> tbl_worktype { get; set; }
    }
}
