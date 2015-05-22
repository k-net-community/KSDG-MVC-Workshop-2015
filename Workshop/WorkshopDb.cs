namespace Workshop
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Workshop.Models;

    public partial class WorkshopDb : DbContext
    {
        public WorkshopDb()
            : base("name=WorkshopDb")
        {
        }

        public virtual DbSet<GuestBook> GuestBooks { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuestBook>()
                .Property(e => e.Email)
                .IsUnicode(false);
        }
    }
}
