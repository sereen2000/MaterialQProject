using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Implementations;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Imaging;
using System.Text.Json;

namespace MaterialQ.Controllers;

public class ItemController : Controller
{
    private readonly IItemService _itemService;
    private readonly IColorService _colorService;
    private readonly IUnitService _unitService;

    public ItemController(IItemService itemService, IColorService colorService,IUnitService unitService)
    {
        _itemService = itemService;
        _colorService = colorService;
        _unitService = unitService;
    }

    // GET: Item
    public async Task<IActionResult> Index()
    {
        var items = await _itemService.GetAllAsync();
       
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
        var item = await _itemService.GetByIdAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }


    // POST: Item/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ItemsModel item)
    {
        if (id != item.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            await _itemService.UpdateAsync(item);
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    // POST: Item/Delete/5
    [HttpPost]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var item = await _itemService.GetByIdAsync(id);
        if (item == null) return Json(new { success = false, message = "Item not found" });

        await _itemService.DeleteAsync(id);
        return Json(new { success = true, message = "Item deleted successfully" });
    }


}
