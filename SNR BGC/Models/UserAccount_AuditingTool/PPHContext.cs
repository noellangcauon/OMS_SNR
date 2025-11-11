using Microsoft.EntityFrameworkCore;

namespace SNR_BGC.Models.UserAccount_AuditingTool
{
    public class PPHContext: DbContext
    {
        public PPHContext(DbContextOptions<PPHContext> options) : base(options)
        {

        }

        public DbSet<PPH_Employee_Details> PPH_Employee_Details { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PPH_Employee_Details>(entity =>
            {
                entity.HasNoKey();
            });
        }
    }
}
