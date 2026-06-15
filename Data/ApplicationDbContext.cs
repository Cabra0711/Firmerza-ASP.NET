using Firmeza.Models;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
}