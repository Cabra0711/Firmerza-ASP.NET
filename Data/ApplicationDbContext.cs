using Firmeza.Models;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Customer>().Property(c => c.Id).ValueGeneratedNever();
        modelBuilder.Entity<Product>().Property(p => p.Id).ValueGeneratedNever();
        modelBuilder.Entity<Sale>().Property(s => s.Id).ValueGeneratedNever();
        modelBuilder.Entity<SaleDetail>().Property(sd => sd.Id).ValueGeneratedNever();

        
        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Customer)
            .WithMany() 
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict); 

       
        modelBuilder.Entity<SaleDetail>()
            .HasOne(sd => sd.Sale)
            .WithMany(s => s.SaleDetails)
            .HasForeignKey(sd => sd.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        
        modelBuilder.Entity<SaleDetail>()
            .HasOne(sd => sd.Product)
            .WithMany(p => p.SaleDetails)
            .HasForeignKey(sd => sd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Sale> Sales { get; set; }        
    public DbSet<SaleDetail> SaleDetails { get; set; }
}