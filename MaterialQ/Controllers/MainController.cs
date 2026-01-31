using MaterialQ.Data;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaterialQ.Controllers;

[Authorize]
public class MainController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IItemService _itemRepo;
    private readonly IItemColorService _itemColorRepo;


    public MainController(ApplicationDbContext context, IItemService itemRepo , IItemColorService itemColorRepo)
    {
        _context = context;
        _itemRepo = itemRepo;
        _itemColorRepo = itemColorRepo;
    }

   public async Task<IActionResult> Index()
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
        ViewBag.TotalCount = _context.Quotations.Count();
        ViewBag.ItemsCount = _context.Items.Count();
        ViewBag.UsersCount = _context.Users.Count();

        ViewBag.Recent = recent;
        var allColorItems = await _context.ColorItem
            .Include(ci => ci.Color)
            .Include(ci => ci.Item)
            .AsNoTracking()
            .ToListAsync();

        var lowStockItems = allColorItems
            .Where(ci => ci.Quantity <= 5)
            .Select(ci => new LowStockItemViewModel
            {
                Code = ci.Item.Code,
                ColorName = ci.Color.Name,
                Qty = ci.Quantity
            })
            .ToList();

        var model = new DashboardViewModel
        {
            LowStockItems = lowStockItems,
            TodaySales = 145,        
            ThisMonthRevenue = 3264,
            ThisYearCustomers = 1244
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
        // Bring quotations including items
        var quotations = await _context.Quotations
            .Include(q => q.Items)
            .ToListAsync();

        ViewBag.TotalCount = quotations.Count;

        ViewBag.TotalAmount = quotations.Sum(q => q.TotalAmount);

        ViewBag.DraftCount = quotations.Count(q => q.Status == "Draft");
        ViewBag.ApprovedCount = quotations.Count(q => q.Status == "Approved");
        ViewBag.RejectedCount = quotations.Count(q => q.Status == "Rejected");
        ViewBag.TotalCount = _context.Quotations.Count();
        ViewBag.ItemsCount = _context.Items.Count();
        ViewBag.UsersCount = _context.Users.Count();

        // Last 10 quotations
        ViewBag.Recent = quotations
            .OrderByDescending(q => q.DateCreated)
            .Take(10)
            .ToList();

        // Low stock items
        var lowStockItems = await _context.ColorItem
            .Include(ci => ci.Item)
            .Include(ci => ci.Color)
            .Where(ci => ci.Quantity <= 5)
            .Select(ci => new LowStockItemViewModel
            {
                Code = ci.Item.Code,
                ColorName = ci.Color.Name,
                Qty = ci.Quantity
            })
            .ToListAsync();

        // Today Sales = sum of today’s quotation totalAmount
        var todaySales = quotations
            .Where(q => q.DateCreated.Date == DateTime.Now.Date)
            .Sum(q => q.TotalAmount);

        // Monthly revenue
        var monthRevenue = quotations
            .Where(q => q.DateCreated.Month == DateTime.Now.Month
                        && q.DateCreated.Year == DateTime.Now.Year)
            .Sum(q => q.TotalAmount);

        // Year customers = unique customer names
        var customerCount = quotations
            .Select(q => q.CustomerName)
            .Distinct()
            .Count();

        var model = new DashboardViewModel
        {
            LowStockItems = lowStockItems,
            TodaySales = todaySales,
            ThisMonthRevenue = monthRevenue,
            ThisYearCustomers = customerCount
        };

        return View(model);
    }

}

