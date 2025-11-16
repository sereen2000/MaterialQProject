using MaterialQ.Data;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MaterialQ.Controllers;

public class ItemController : Controller
{
    private readonly IItemService _itemService;
    private readonly IColorService _colorService;
    private readonly IUnitService _unitService;
    private readonly ApplicationDbContext _context;


    public ItemController(IItemService itemService, IColorService colorService,IUnitService unitService, ApplicationDbContext context)
    {
        _itemService = itemService;
        _colorService = colorService;
        _unitService = unitService;
        _context = context;
    }

    // GET: Item
    public async Task<IActionResult> Index()
    {
        var items = await _itemService.GetAllItemsIndexAsync();
       
        return View(items);
    }

    // GET: Item/Create
    public async Task<IActionResult> Create()
    {

        ViewBag.Colors = await _colorService.GetAllAsync();
        ViewBag.Units = await _unitService.GetAllAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IFormCollection form)
    {
            // Save item
        await _itemService.AddItemWithColorsAsync(form);

        return RedirectToAction(nameof(Index));
    }

    // GET: Item/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _itemService.GetItemforUpdateByIdAsync(id);
        if (item == null) return NotFound();

        // Get all color variants using item code
        var colorItems = await _context.Items
            .Where(i => i.Code == item.Code)
            .Include(i => i.Color)
            .ToListAsync();

        var model = new UpdateItemViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Description = item.Description,
            Price = item.Price,
            Vat = item.Vat,
            UnitId = item.UnitId,
            ExistingImage = item.Image,
            Colors = colorItems.Select(c => new ItemColor
            {
                ColorId = c.ColorId,
                ColorName = c.Color?.Name,
                Quantity = c.Qty
            }).ToList()
        };

        ViewBag.Colors = await _colorService.GetAllAsync();
        ViewBag.Units = await _unitService.GetAllAsync();
        ViewBag.ExistingColors = model.Colors;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, IFormCollection form)
    {
        // Deserialize colors from hidden JSON
        var colorsList = JsonSerializer.Deserialize<List<ItemColor>>(form["ColorsJson"]);

        var model = new UpdateItemViewModel
        {
            Id = id,
            Code = form["Code"],
            Description = form["Description"],
            Price = float.Parse(form["Price"]),
            Vat = float.Parse(form["Vat"]),
            UnitId = int.Parse(form["UnitId"]),
            ImageFile = form.Files["ImageFile"],
            Colors = colorsList
        };

        await _itemService.UpdateItemWithColorsAsync(model);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> DeleteItemModal(string code)
    {
        if (string.IsNullOrEmpty(code)) return NotFound();

        var items = await _context.Items
            .Where(i => i.Code == code)
            .Include(i => i.Color)
            .ToListAsync();

        if (!items.Any()) return NotFound();

        var model = new DeleteItemViewModel
        {
            Code = code,
            Description = items.First().Description,
            Colors = items.Select(i => new ItemColor
            {
                ColorId = i.ColorId,
                ColorName = i.Color?.Name,
                Quantity = i.Qty
            }).ToList()
        };

        return PartialView("_DeleteItemModal", model); // ✅ make sure this name matches the file
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItemConfirmed(string code)
    {
        if (string.IsNullOrEmpty(code)) return NotFound();

        var items = _context.Items.Where(i => i.Code == code);
        _context.Items.RemoveRange(items);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

}
