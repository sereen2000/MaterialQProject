using MaterialQ.Models.DataModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public decimal UnitPrice { get; set; }

    public decimal Total => Quantity * UnitPrice;

    [ForeignKey("Quotation")]
    public int QuotationId { get; set; }

    public QuotationModel Quotation { get; set; }
}
