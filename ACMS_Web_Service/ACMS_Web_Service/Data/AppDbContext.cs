using Microsoft.EntityFrameworkCore;
using ACMS_Web_Service.Models;

namespace ACMS_Web_Service.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ServicePlanItem> ServicePlanItems { get; set; }
        public DbSet<ProgramItem> ProgramItems { get; set; }
        public DbSet<StepItem> StepItems { get; set; }
        public DbSet<Cycle> Cycles { get; set; }
        public DbSet<UnitProgram> UnitPrograms { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Unit> Units { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}