namespace MaterialQ.Models.DataModels;

public class ColorItemModel
{
    public int Id { get; set; }
    public int ColorId { get; set; }
    public ColorsModel Color { get; set; }

    public int ItemId { get; set; }
    public ItemsModel Item { get; set; }
    public float Quantity { get; set; }
}
