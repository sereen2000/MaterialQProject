namespace MaterialQ.Models.ViewModels;

public class ItemColor
{
    public int? ColorId { get; set; }
    public string? ColorName { get; set; }
    public int Quantity { get; set; }
}

public class ItemViewModel
{
    public string Code { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int UnitId { get; set; }
    public float Vat { get; set; }

    public IFormFile ImageFile { get; set; }
    public List<ItemColor> Colors { get; set; } = new List<ItemColor>();
}
