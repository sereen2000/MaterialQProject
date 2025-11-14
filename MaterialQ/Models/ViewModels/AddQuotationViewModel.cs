using System.Collections.Generic;
using MaterialQ.Models.DataModels;

namespace MaterialQ.Models.ViewModels
{
    public class AddQuotationViewModel
    {
        public List<ItemsModel> Items { get; set; } = new();
        public QuotationModel Quotation { get; set; } = new();
    }
}
