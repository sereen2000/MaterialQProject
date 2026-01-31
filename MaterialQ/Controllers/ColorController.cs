using MaterialQ.Models.DataModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
 
    // CREATE
    [HttpGet]
    public IActionResult CreateColor()
    {
        return PartialView("_CreateColorModal");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateColor(ColorsModel model)
    {
        if (!ModelState.IsValid) return BadRequest();
        await _colorService.AddAsync(model);
        return RedirectToAction(nameof(Index));
    }

    // EDIT
    [HttpGet]
    public async Task<IActionResult> EditColor(int id)
    {
        var color = await _colorService.GetByIdAsync(id);
        if (color == null) return NotFound();
        return PartialView("_EditColorModal", color);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditColorConfirmed(ColorsModel model)
    {
        if (!ModelState.IsValid) return BadRequest();
        await _colorService.UpdateAsync(model);
        return RedirectToAction(nameof(Index));
    }

    // DELETE
    [HttpGet]
    public async Task<IActionResult> DeleteColor(int id)
    {
        var color = await _colorService.GetByIdAsync(id);
        if (color == null) return NotFound();
        return PartialView("_DeleteColorModal", color);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteColorConfirmed(int id)
    {
        try
        {
            await _colorService.DeleteAsync(id);

            return Json(new { success = true, message = "Color deleted successfully." });
        }
        catch (InvalidOperationException ex)
        {
           
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception)
        {
            
            return Json(new { success = false, message = "Unexpected error occurred." });
        }
    }



}
