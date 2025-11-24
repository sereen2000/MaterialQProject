using MaterialQ.Models.DataModels;
using Microsoft.AspNetCore.Identity;
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
    public DbSet<ColorItemModel> ColorItem { get; set; }
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

        modelBuilder.Entity<ColorItemModel>()
          .HasKey(ci => new { ci.ItemId, ci.ColorId });

        modelBuilder.Entity<ColorItemModel>()
            .HasOne(ci => ci.Item)
            .WithMany()
            .HasForeignKey(ci => ci.ItemId);

        modelBuilder.Entity<ColorItemModel>()
            .HasOne(ci => ci.Color)
            .WithMany()
            .HasForeignKey(ci => ci.ColorId);

        modelBuilder.Entity<ItemsModel>().ToTable("Items");
        modelBuilder.Entity<QuotationModel>().ToTable("Quotations");
        modelBuilder.Entity<QuotationItemsModel>().ToTable("QuotationItems");

        modelBuilder.Entity<IdentityUser>().ToTable("AspNetUsers");
        modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles");

    }
}
