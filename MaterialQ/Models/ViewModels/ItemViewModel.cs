namespace MaterialQ.Models.ViewModels;

public class ItemColor
{
    public int? ColorId { get; set; }
    public string? ColorName { get; set; }
    public float Quantity { get; set; }
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

public class UpdateItemViewModel
{
    public int Id { get; set; } // existing item id
    public string Code { get; set; }
    public string Description { get; set; }
    public float Price { get; set; }
    public float Vat { get; set; }
    public int UnitId { get; set; }
    public IFormFile ImageFile { get; set; }
    public string ExistingImage { get; set; } // to keep old image if no new file uploaded
    public List<ItemColor> Colors { get; set; } = new List<ItemColor>();
}

public class RetrieveItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public float Price { get; set; }
    public float Vat { get; set; }
    public int UnitId { get; set; }
    public string UnitName { get; set; }
    public IFormFile ImageFile { get; set; }
    public float TotalQty { get; set; }  
    public List<ItemColor> Colors { get; set; } = new List<ItemColor>();
}


public class DeleteItemViewModel
{
    public string Code { get; set; }
    public string Description { get; set; }
    public List<ItemColor> Colors { get; set; } = new List<ItemColor>();
}