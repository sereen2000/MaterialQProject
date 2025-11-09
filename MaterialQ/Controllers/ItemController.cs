using MaterialQ.Models.DataModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaterialQ.Controllers;

public class ItemController : Controller
{
    private readonly IItemService _itemService;

    public ItemController(IItemService itemService)
    {
        _itemService = itemService;
    }

    // GET: Item
    public async Task<IActionResult> Index()
    {
        var items = await _itemService.GetAllAsync();
       
        return View(items);
    }

    // GET: Item/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Item/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(List<ItemsModel> items)
    {
        if(ModelState.IsValid)
        {
            await _itemService.AddItemsAsync(items);
            return RedirectToAction(nameof(Index));
        }
        return View(items); 
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
