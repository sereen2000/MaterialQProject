using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaterialQ.Controllers;

[Authorize]
public class MainController : Controller
{
    private readonly IItemService _itemRepo;
    public MainController( IItemService  service)
    {
        
        _itemRepo = service;
    }

    public async Task<IActionResult> Index()
    {
        var allItems = await _itemRepo.GetAllAsync();
        var lowStockItems = allItems.Where(i => i.Qty <= 5).ToList();

        var model = new DashboardViewModel
        {
            LowStockItems = lowStockItems,
            TodaySales = 145,        // Example: replace with real logic
            ThisMonthRevenue = 3264, // Example: replace with real logic
            ThisYearCustomers = 1244 // Example: replace with real logic
        };

        return View(model);
    }
    public IActionResult Login()
    {
        return View();
    }
    public IActionResult Profile()
    {
        return View();
    }

    public IActionResult Quotations()
    {

        return View();
    }

    public IActionResult AddQuotation()
    {

        return View();
    }

    public IActionResult Products()
    {
        return View();
    }
}
