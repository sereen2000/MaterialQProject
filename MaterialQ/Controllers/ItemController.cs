using MaterialQ.Data;
using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaterialQ.Controllers
{
    public class ItemController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IUnitService _unitService;
        private readonly ApplicationDbContext _context;

        public ItemController(
            IItemService itemService,
            IUnitService unitService,
            ApplicationDbContext context)
        {
            _itemService = itemService;
            _unitService = unitService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _itemService.GetAllItemsIndexAsync();
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Units = await _unitService.GetAllAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form, IFormFile ImageFile)
        {
            string imagePath = null;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/items");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                imagePath = "uploads/items/" + fileName; // هذا اللي بنخزّنه بالداتا
            }

            var item = new ItemsModel
            {
                Code = form["Code"],
                Description = form["Description"],
                Price = float.Parse(form["Price"]),
                ActualPrice = float.Parse(form["ActualPrice"]),
                Vat =  0.05f,
                Qty = float.Parse(form["Qty"]),
                UnitId = int.Parse(form["UnitId"]),
                Image = imagePath   // ✅ صار يخزن المسار صح
            };

            await _itemService.AddAsync(item);
            return RedirectToAction(nameof(Index));
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(IFormCollection form)
        //{
        //    var item = new ItemsModel
        //    {
        //        Code = form["Code"],
        //        Description = form["Description"],
        //        Price = float.Parse(form["Price"]),
        //        ActualPrice = float.Parse(form["ActualPrice"]),
        //        Vat = float.Parse(form["Vat"]),
        //        Qty = float.Parse(form["Qty"]),
        //        UnitId = int.Parse(form["UnitId"]),
        //        Image = (form["Image"])
        //    };

        //    await _itemService.AddAsync(item);
        //    return RedirectToAction(nameof(Index));
        //}

        // --------- Edit ---------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return NotFound();

            // 🔹 مهم جدًا — هذا هو سبب الخطأ
            ViewBag.Units = await _context.Units.ToListAsync();

            var model = new UpdateItemViewModel
            {
                Id = item.Id,
                Code = item.Code,
                Description = item.Description,
                Price = item.Price,
                ActualPrice = item.ActualPrice,
                Vat = item.Vat,
                Qty = item.Qty,
                UnitId = item.UnitId,
                ExistingImage = item.Image
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateItemViewModel model, IFormFile ImageFile)
        {
            ViewBag.Units = await _unitService.GetAllAsync();

            var item = await _itemService.GetByIdAsync(id);
            if (item == null) return NotFound();

            item.Code = model.Code;
            item.Description = model.Description;
            item.Price = model.Price;
            item.ActualPrice = model.ActualPrice;
            item.Vat = model.Vat;
            item.Qty = model.Qty;
            item.UnitId = model.UnitId;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/items");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                item.Image = "uploads/items/" + fileName;
            }

            await _itemService.UpdateAsync(item);

            return RedirectToAction(nameof(Index));
        }
        // --------- Delete ---------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItemConfirmed(int id)
        {
            var isUsed = await _context.QuotationItems
                .AnyAsync(q => q.ItemId == id);

            if (isUsed)
            {
                TempData["Error"] = "Cannot delete this item because it is used in quotations.";
                return RedirectToAction(nameof(Index));
            }

            await _itemService.DeleteAsync(id);

            TempData["Success"] = "Item deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> _DeleteItemModal(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return NotFound();

            return PartialView("_DeleteItemModal", item);
        }


    }
}
