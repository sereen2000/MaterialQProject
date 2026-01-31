using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaterialQ.Models.DataModels;

public class QuotationItemsModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int ItemId { get; set; }

    [Required]
    [StringLength(200)]
    public string ItemDescription { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Vat { get; set; }
    public int ColorId { get; set; }


    public decimal Discount { get; set; }
    public decimal Total => (Quantity * UnitPrice) + Vat;

    public int QuotationId { get; set; }

    [ForeignKey("QuotationId")]

    public QuotationModel Quotation { get; set; }

}
