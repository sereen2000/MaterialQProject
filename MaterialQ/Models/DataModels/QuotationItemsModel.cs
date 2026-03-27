using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaterialQ.Models.DataModels;

public class QuotationItemsModel
{
    public int Id { get; set; }
    public int QuotationId { get; set; }
    public int ItemId { get; set; }
    public string ItemDescription { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }      // سعر البيع
    public decimal ActualPrice { get; set; }    // السعر الأصلي (التكلفة)
    public decimal Vat { get; set; }

    public QuotationModel Quotation { get; set; }
}
