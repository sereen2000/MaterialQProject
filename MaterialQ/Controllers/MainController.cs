using MaterialQ.Data;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

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
    var allItems = await _context.Items
        .Include(i => i.ColorItems)
            .ThenInclude(ci => ci.Color)
        .ToListAsync();

    var lowStockItems = allItems
        .SelectMany(i => i.ColorItems.Select(ci => new
        {
            ItemCode = i.Code,
            ColorName = ci.Color.Name,
            Qty = ci.Quantity
        }))
        .Where(x => x.Qty <= 5)
        .ToList();

    var model = new DashboardViewModel
    {
        LowStockItems = lowStockItems.Select(x => new LowStockItemViewModel
        {
            Code = x.ItemCode,
            ColorName = x.ColorName,
            Qty = x.Qty
        }).ToList(),
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

}

