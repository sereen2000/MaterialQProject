using MaterialQ.Models.DataModels;

namespace MaterialQ.Models.ViewModels;

public class DashboardViewModel
{
    public List<LowStockItemViewModel> LowStockItems { get; set; } = new List<LowStockItemViewModel>();

    // You can add other dashboard metrics here if needed
    public int TodaySales { get; set; }
    public int ThisMonthRevenue { get; set; }
    public int ThisYearCustomers { get; set; }
}

public class LowStockItemViewModel
{
    public string Code { get; set; }
    public string ColorName { get; set; }
    public float Qty { get; set; }
}