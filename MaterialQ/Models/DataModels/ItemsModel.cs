namespace MaterialQ.Models.DataModels;

public class ItemsModel
{
    public int Id { get; set; }
    public string? Image { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public Double Qty { get; set; }   // <-- الكمية الآن مباشرة هنا
    public float Price { get; set; }
    public float ActualPrice { get; set; }
    public float Vat { get; set; }


    public int UnitId { get; set; }
    public UnitModel Unit { get; set; }
}
