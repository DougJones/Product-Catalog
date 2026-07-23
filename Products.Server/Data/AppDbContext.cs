using Microsoft.EntityFrameworkCore;


namespace Products.Server
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
             modelBuilder.Entity<Product>()
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}