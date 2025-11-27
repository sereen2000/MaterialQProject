using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaterialQ.Models.DataModels;

public class QuotationModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Company name is required")]
    [StringLength(200)]
    public string CustomerName { get; set; }

    public string QuotationNumber { get; set; }

    [Required]
    public DateTime DateCreated { get; set; } = DateTime.Now; 

    [StringLength(200)]
    public string CreatedBy { get; set; }        

    [Column(TypeName = "decimal(18,2)")]
    public decimal Discount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }


    [StringLength(50)]
    public string Status { get; set; }
    public List<QuotationItemsModel> Items { get; set; } = new();

}


