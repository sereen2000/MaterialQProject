using MaterialQ.Models.DataModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MaterialQ.Controllers;

public class UnitController : Controller
{
    private readonly IUnitService  _unitService;

    public UnitController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    // GET: /Unit
    public async Task<IActionResult> Index()
    {
        var units = await _unitService.GetAllAsync();
        return View(units);
    }

    // GET: /Unit/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Unit/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UnitModel unit)
    {
        if (ModelState.IsValid)
        {
            await _unitService.AddAsync(unit);
            return RedirectToAction(nameof(Index));
        }
        return View(unit);
    }

    // GET: /Unit/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var unit = await _unitService.GetByIdAsync(id);
        if (unit == null) return NotFound();
        return View(unit);
    }

    // POST: /Unit/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UnitModel unit)
    {
        if (id != unit.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            await _unitService.UpdateAsync(unit);
            return RedirectToAction(nameof(Index));
        }
        return View(unit);
    }

    // POST: /Unit/DeleteUnit
    [HttpPost]
    public async Task<IActionResult> DeleteUnit(int id)
    {
        var unit = await _unitService.GetByIdAsync(id);
        if (unit == null) return Json(new { success = false, message = "Unit not found" });

        await _unitService.DeleteAsync(id);
        return Json(new { success = true, message = "Unit deleted successfully" });
    }
}