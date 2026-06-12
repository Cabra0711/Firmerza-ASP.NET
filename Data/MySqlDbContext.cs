using Microsoft.EntityFrameworkCore;

namespace Firmeza.Data;

public class MySqlDbContext : DbContext
{
    public MySqlDbContext(DbContextOptions<MySqlDbContext> options) : base(options)
    {
        
    }
}