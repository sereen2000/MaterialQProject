using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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


    // Create Unit Modal
    [HttpGet]
    public IActionResult CreateUnit()
    {
        return PartialView("_CreateUnitModal");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUnit(UnitModel model)
    {
        if (!ModelState.IsValid) return BadRequest();
        await _unitService.AddAsync(model);
        return RedirectToAction(nameof(Index));
    }

    // Edit Unit Modal
    [HttpGet]
    public async Task<IActionResult> EditUnit(int id)
    {
        var unit = await _unitService.GetByIdAsync(id);
        if (unit == null) return NotFound();
        return PartialView("_EditUnitModal", unit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUnitConfirmed(UnitModel model)
    {
        if (!ModelState.IsValid) return BadRequest();
        await _unitService.UpdateAsync(model);
        return RedirectToAction(nameof(Index));
    }

    // Delete Unit Modal
    [HttpGet]
    public async Task<IActionResult> DeleteUnit(int id)
    {
        var unit = await _unitService.GetByIdAsync(id);
        if (unit == null) return NotFound();
        return PartialView("_DeleteUnitModal", unit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUnitConfirmed(int id)
    {
        await _unitService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

}