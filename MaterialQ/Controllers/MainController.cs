using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using MaterialQ.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaterialQ.Controllers;

[Authorize]
public class MainController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IItemService _itemRepo;


    public MainController(ApplicationDbContext context, IItemService itemRepo)
    {
        _context = context;
        _itemRepo = itemRepo;
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

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var quotations = await _context.Quotations.ToListAsync();

        var totalCount = quotations.Count;
        var totalAmount = quotations.Any() ? quotations.Sum(q => q.TotalAmount) : 0;
        var draftCount = quotations.Count(q => q.Status == "Draft");
        var approvedCount = quotations.Count(q => q.Status == "Approved");
        var rejectedCount = quotations.Count(q => q.Status == "Rejected");

        var recent = quotations
            .OrderByDescending(q => q.DateCreated)
            .Take(5)
            .ToList();

        ViewBag.TotalCount = totalCount;
        ViewBag.TotalAmount = totalAmount;
        ViewBag.DraftCount = draftCount;
        ViewBag.ApprovedCount = approvedCount;
        ViewBag.RejectedCount = rejectedCount;
        ViewBag.Recent = recent;


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

}

