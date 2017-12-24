namespace Customer_Detail.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Customer_Context : DbContext
    {
        public Customer_Context()
            : base("name=Customer_Context1")
        {
        }

        public virtual DbSet<CountryMaster> CountryMasters { get; set; }
        public virtual DbSet<Customer_Details> Customer_Details { get; set; }
        public virtual DbSet<StateMaster> StateMasters { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
