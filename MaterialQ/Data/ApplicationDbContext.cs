using MaterialQ.Models.DataModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

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
    public DbSet<QuotationModel> Quotations { get; set; }
    public DbSet<QuotationItemsModel> QuotationItems { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

       
        modelBuilder.Entity<QuotationItemsModel>()
            .HasOne(qi => qi.Quotation)
            .WithMany(q => q.Items)
            .HasForeignKey(qi => qi.QuotationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuotationItemsModel>()
            .HasOne<ItemsModel>()
            .WithMany()
            .HasForeignKey(p => p.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ItemsModel>().ToTable("Items");
        modelBuilder.Entity<QuotationModel>().ToTable("Quotations");
        modelBuilder.Entity<QuotationItemsModel>().ToTable("QuotationItems");
    }
}
