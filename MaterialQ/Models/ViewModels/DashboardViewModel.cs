using MaterialQ.Models.DataModels;

namespace MaterialQ.Models.ViewModels;

public class DashboardViewModel
{
    public List<ItemsModel> LowStockItems { get; set; } = new List<ItemsModel>();

    // You can add other dashboard metrics here if needed
    public int TodaySales { get; set; }
    public int ThisMonthRevenue { get; set; }
    public int ThisYearCustomers { get; set; }
}
