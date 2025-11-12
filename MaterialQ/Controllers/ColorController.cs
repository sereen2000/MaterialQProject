using MaterialQ.Models.DataModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MaterialQ.Controllers;

public class ColorController : Controller
{
    private readonly IColorService _colorService;

    public ColorController(IColorService colorService)
    {
        _colorService = colorService;
    }

    public async Task<IActionResult> Index()
    {
        var colors = await _colorService.GetAllAsync();
        return View(colors);
    }

    // GET: Color/Create
    public async Task<IActionResult> Create()
    {
        return View();
    }

    // POST: Color/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ColorsModel color)
    {
        if (ModelState.IsValid)
        {
            await _colorService.AddAsync(color);
            return RedirectToAction(nameof(Index));
        }
        return View(color);
    }

    // GET: Color/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var color = await _colorService.GetByIdAsync(id);
        if (color == null) return NotFound();
        return View(color);
    }

    // POST: Color/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ColorsModel color)
    {
        if (id != color.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            await _colorService.UpdateAsync(color);
            return RedirectToAction(nameof(Index));
        }
        return View(color);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteColor(int id)
    {
        var color = await _colorService.GetByIdAsync(id);
        if (color == null) return Json(new { success = false, message = "Color not found" });

        await _colorService.DeleteAsync(id);
        return Json(new { success = true, message = "Color deleted successfully" });
    }


}
