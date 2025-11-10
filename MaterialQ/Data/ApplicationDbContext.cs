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
    public DbSet<ProductsModel> Products { get; set; }
    public DbSet<QuotationModel> Quotations { get; set; }
    public DbSet<QuotationItemModel> QuotationItems { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // العلاقة بين Quotation ↔ QuotationItems
        modelBuilder.Entity<QuotationItemModel>()
            .HasOne<QuotationModel>()                       // كل Item ينتمي إلى Quotation واحد
            .WithMany(q => q.Items)                         // والـ Quotation عنده أكثر من Item
            .HasForeignKey("QuotationId")                   // المفتاح الخارجي
            .OnDelete(DeleteBehavior.Cascade);              // لو انمسح العرض، تنمسح تفاصيله

        // العلاقة بين Product ↔ QuotationItem
        modelBuilder.Entity<QuotationItemModel>()
            .HasOne<ProductsModel>()                         // كل Item مرتبط بمنتج
            .WithMany()                                     // والمنتج ممكن يكون في أكثر من عرض
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Restrict);             // ما نحذف المنتج لو استخدم بعرض

        // اسم الجدول في قاعدة البيانات لو بدك تنسقه
        modelBuilder.Entity<ProductsModel>().ToTable("Products");
        modelBuilder.Entity<QuotationModel>().ToTable("Quotations");
        modelBuilder.Entity<QuotationItemModel>().ToTable("QuotationItems");
    }
}
