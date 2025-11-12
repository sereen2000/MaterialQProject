using MaterialQ.Data;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaterialQ.Controllers;

[Authorize]
public class MainController : Controller
{
    private readonly ApplicationDbContext _context;

    public MainController(ApplicationDbContext context)
    {
        _context = context;
    }

        public IActionResult Index()
        {
            return View();
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

        return View();
    }

}

