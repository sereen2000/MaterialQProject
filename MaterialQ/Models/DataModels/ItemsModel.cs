namespace MaterialQ.Models.DataModels;

public class ItemsModel
{
    public int Id { get; set; }
    public string ? Image { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public float Qty { get; set; }
    public float Price { get; set; }
    public float ActualPrice { get; set; }
    public float Vat { get; set; }

    // Foreign key for Unit
    public int UnitId { get; set; }
    public UnitModel Unit { get; set; }

    public List<ColorItemModel> ColorItems { get; set; } = new();

}
