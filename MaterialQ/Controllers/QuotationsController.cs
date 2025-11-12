using MaterialQ.Data;
using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MaterialQ.Controllers
{
    public class QuotationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IItemService _itemService;

        public QuotationsController(ApplicationDbContext context, IItemService itemService)
        {
            _context = context;
            _itemService = itemService;
        }

        // 🧾 عرض كل عروض الأسعار
        public async Task<IActionResult> Index()
        {
            var quotations = await _context.Quotations
                .OrderByDescending(q => q.DateCreated)
                .ToListAsync();

            return View(quotations);
        }

        // ➕ إنشاء عرض سعر جديد
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var items = await _itemService.GetAllAsync();
            var viewModel = new AddQuotationViewModel
            {
                Items = items.ToList()
            };

            return View(viewModel);
        }

        // 👁️ عرض محتوى السلة للمراجعة
        [HttpGet]
        public IActionResult ReviewQuotation()
        {
            return View(); // رح نعمل فيو خاص إلها بعد شوي
        }
      
        [HttpPost]
        public async Task<IActionResult> SaveQuotation([FromBody] QuotationModel quotation)
        {
            try
            {
                if (quotation == null || quotation.Items == null || !quotation.Items.Any())
                    return Json(new { success = false, message = "Quotation is empty." });

                quotation.QuotationNumber = $"Q-{DateTime.Now:yyyyMMddHHmmss}";
                quotation.DateCreated = DateTime.Now;
                quotation.Status = "Draft";
                quotation.TotalAmount = quotation.Items.Sum(i => i.Quantity * i.UnitPrice);
                quotation.NetAmount = quotation.TotalAmount - quotation.Discount;
                quotation.CreatedBy = User?.Identity?.Name ?? "System";

                // 🔹 نحفظ رأس العرض أولاً
                var newQuotation = new QuotationModel
                {
                    QuotationNumber = quotation.QuotationNumber,
                    CustomerName = quotation.CustomerName,
                    DateCreated = quotation.DateCreated,
                    CreatedBy = quotation.CreatedBy,
                    TotalAmount = quotation.TotalAmount,
                    Discount = quotation.Discount,
                    NetAmount = quotation.NetAmount,
                    Status = quotation.Status
                };

                _context.Quotations.Add(newQuotation);
                await _context.SaveChangesAsync();

                // 🔹 نحفظ العناصر المرتبطة
                foreach (var item in quotation.Items)
                {
                    var qi = new QuotationItemsModel
                    {
                        ItemId = item.ItemId,
                        ItemDescription = item.ItemDescription,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        QuotationId = newQuotation.Id
                    };
                    _context.QuotationItems.Add(qi);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Quotation saved successfully!" });
            }
            catch (Exception ex)
            {
                // 🔥 نرجّع نص بسيط بدل ما نرسل رسالة EF كاملة
                return Json(new { success = false, message = "An error occurred while saving: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ✏️ تعديل عرض سعر
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var quotation = await _context.Quotations.FindAsync(id);
            if (quotation == null) return NotFound();

            return View(quotation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuotationModel quotation)
        {
            if (id != quotation.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quotation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Quotations.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(quotation);
        }

        // 🗑️ حذف عرض سعر
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var quotation = await _context.Quotations.FindAsync(id);
            if (quotation == null)
                return Json(new { success = false, message = "Quotation not found." });

            _context.Quotations.Remove(quotation);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Quotation deleted successfully!" });
        }
        // 👁️ عرض تفاصيل عرض السعر
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var quotation = await _context.Quotations
                .Include(q => q.Items) // تضمين العناصر المرتبطة
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();

            return View(quotation);
        }

    }
}
