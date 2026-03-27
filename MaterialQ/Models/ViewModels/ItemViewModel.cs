using MaterialQ.Models.DataModels;

namespace MaterialQ.Models.ViewModels;
public class DeleteItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
}
public class ItemViewModel
{
    public string Code { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal ActualPrice { get; set; }
    public float Vat { get; set; }
    public Double Qty { get; set; }   // <-- أضفناها هنا
    public int UnitId { get; set; }
    public IFormFile ImageFile { get; set; }
}

public class UpdateItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public float Price { get; set; }
    public float ActualPrice { get; set; }
    public float Vat { get; set; }
    public Double Qty { get; set; }   // <-- الكمية هنا
    public int UnitId { get; set; }
    public IFormFile ImageFile { get; set; }
    public string ExistingImage { get; set; }
}

public class RetrieveItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public float Price { get; set; }
    public float ActualPrice { get; set; }
    public float Vat { get; set; }
    public Double Qty { get; set; }   // <-- الكمية من Items
    public int UnitId { get; set; }
    public string UnitName { get; set; }
    public string ImageFile { get; set; }
}
