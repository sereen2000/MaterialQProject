using MaterialQ.Data;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MaterialQ.Controllers
{
    public class ItemController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IColorService _colorService;
        private readonly IUnitService _unitService;
        private readonly ApplicationDbContext _context;

        public ItemController(
            IItemService itemService,
            IColorService colorService,
            IUnitService unitService,
            ApplicationDbContext context)
        {
            _itemService = itemService;
            _colorService = colorService;
            _unitService = unitService;
            _context = context;
        }

        // ----------------- Index -----------------
        public async Task<IActionResult> Index()
        {
            var items = await _itemService.GetAllItemsIndexAsync();
            return View(items);
        }

        // ----------------- Create -----------------
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
            await _itemService.AddItemWithColorsAsync(form);
            return RedirectToAction(nameof(Index));
        }

        // ----------------- Edit -----------------
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _itemService.GetItemforUpdateByIdAsync(id);
            if (item == null) return NotFound();

            // جلب كل الألوان المرتبطة بهذا Item
            var colorItems = await _context.ColorItem
                .Where(ci => ci.ItemId == item.Id)
                .Include(ci => ci.Color)
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
                Colors = colorItems.Select(ci => new ItemColor
                {
                    ColorId = ci.ColorId,
                    ColorName = ci.Color?.Name,
                    Quantity = ci.Quantity
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

        // ----------------- Delete Modal -----------------
        [HttpGet]
        public async Task<IActionResult> _DeleteItemModal(int id)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == id);
            if (item == null) return NotFound();

            var colorItems = await _context.ColorItem
                .Where(ci => ci.ItemId == id)
                .Include(ci => ci.Color)
                .ToListAsync();

            var model = new DeleteItemViewModel
            {
                Id= id,
                Code = item.Code,
                Description = item.Description,
                Colors = colorItems.Select(ci => new ItemColor
                {
                    ColorId = ci.ColorId,
                    ColorName = ci.Color?.Name,
                    Quantity = ci.Quantity
                }).ToList()
            };

            return PartialView("_DeleteItemModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItemConfirmed(int id)
        {
            var item = await _context.Items
                .Include(i => i.ColorItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();

            if (item.ColorItems.Any())
            {
                _context.ColorItem.RemoveRange(item.ColorItems);
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
