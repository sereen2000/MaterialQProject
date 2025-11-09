using MaterialQ.Models.DataModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MaterialQ.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<ItemsModel> Items { get; set; }
    public DbSet<ColorsModel> Colors { get; set; }
    public DbSet<UnitModel> Units { get; set; }
}
