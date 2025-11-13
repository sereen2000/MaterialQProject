using MaterialQ.Data;
using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

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

       
        //public async Task<IActionResult> Index()
        //{
        //    var quotations = await _context.Quotations
        //        .OrderByDescending(q => q.DateCreated)
        //        .ToListAsync();

        //    return View(quotations);
        //}
        public async Task<IActionResult> Index(string? from, string? to, string? status)
        {
            var query = _context.Quotations.AsQueryable();

            // 🔹 فلتر التاريخ من
            if (!string.IsNullOrEmpty(from))
            {
                DateTime fromDate = DateTime.Parse(from);
                query = query.Where(q => q.DateCreated.Date >= fromDate.Date);
            }

            // 🔹 فلتر التاريخ إلى
            if (!string.IsNullOrEmpty(to))
            {
                DateTime toDate = DateTime.Parse(to);
                query = query.Where(q => q.DateCreated.Date <= toDate.Date);
            }

            // 🔹 فلتر الستاتس
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(q => q.Status == status);
            }

            var result = await query
                .OrderByDescending(q => q.DateCreated)
                .ToListAsync();

            return View(result);
        }

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
                quotation.TotalAmount = quotation.Items.Sum(i => (i.Quantity * i.UnitPrice) + i.Vat);
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
                        Vat = item.Vat,
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

        [HttpGet]
        public async Task<IActionResult> ExportToPdf(int id)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();

            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // 🔹 العنوان
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                document.Add(new Paragraph($"Quotation #{quotation.QuotationNumber}", titleFont));
                document.Add(new Paragraph($"Customer: {quotation.CustomerName}"));
                document.Add(new Paragraph($"Date: {quotation.DateCreated:dd/MM/yyyy}"));
                document.Add(new Paragraph(" "));

                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 40, 15, 15, 15, 15 });

                table.AddCell("Item");
                table.AddCell("Qty");
                table.AddCell("Unit Price");
                table.AddCell("VAT");
                table.AddCell("Total");

                foreach (var item in quotation.Items)
                {
                    table.AddCell(item.ItemDescription);
                    table.AddCell(item.Quantity.ToString());
                    table.AddCell(item.UnitPrice.ToString("F2"));
                    table.AddCell(item.Vat.ToString("F2"));
                    table.AddCell(((item.UnitPrice * item.Quantity) + item.Vat).ToString("F2"));
                }


                foreach (var item in quotation.Items)
                {
                    table.AddCell(item.ItemDescription);
                    table.AddCell(item.Quantity.ToString());
                    table.AddCell(item.UnitPrice.ToString("F2"));
                    table.AddCell((item.Quantity * item.UnitPrice).ToString("F2"));
                }

                document.Add(table);
                document.Add(new Paragraph(" "));

                // 🔹 الإجماليات
                document.Add(new Paragraph($"Discount: {quotation.Discount:C2}"));
                document.Add(new Paragraph($"Net Amount: {quotation.NetAmount:C2}"));

                document.Close();

                return File(ms.ToArray(), "application/pdf", $"Quotation_{quotation.QuotationNumber}.pdf");
            }
        }
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
