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
        public async Task<IActionResult> Index(string? from, string? to, string? search)
        {
            var query = _context.Quotations.AsQueryable();

            if (!string.IsNullOrEmpty(from))
            {
                var fromDate = DateTime.Parse(from);
                query = query.Where(q => q.DateCreated.Date >= fromDate.Date);
            }

            if (!string.IsNullOrEmpty(to))
            {
                var toDate = DateTime.Parse(to);
                query = query.Where(q => q.DateCreated.Date <= toDate.Date);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(q =>
                    q.CustomerName.Contains(search) ||
                    q.QuotationNumber.Contains(search));
            }

            var result = await query
                .OrderByDescending(q => q.DateCreated)
                .ToListAsync();

            return View(result);
        }


        [HttpGet]
        public async Task<IActionResult> Create(string search)
        {
            var items = await _itemService.GetAllAsync();
            if (!string.IsNullOrEmpty(search))
            {
                items = items
                    .Where(i =>
                        i.Description.Contains(search) ||
                        i.Code.Contains(search))
                    .ToList();
            }

          
            var viewModel = new AddQuotationViewModel
            {
                Items = items.ToList()
            };


            ViewBag.Search = search;
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult ReviewQuotation()
        {
            return View();
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
                //quotation.TotalAmount = quotation.Items.Sum(i => (i.Quantity * i.UnitPrice) + i.Vat);
                // 1️⃣ حساب المجموع مع VAT * الكمية
                quotation.TotalAmount = quotation.Items.Sum(i =>
                {
                    var baseAmount = i.Quantity * i.UnitPrice;
                    var discountAmount = baseAmount * (i.Discount / 100m);
                    return baseAmount - discountAmount + i.Vat;
                });

                // 2️⃣ الخصم كنسبة مئوية (0 – 100)
                var discountAmount = quotation.TotalAmount * (quotation.Discount / 100m);

                // 3️⃣ الصافي بعد الخصم
                quotation.NetAmount = quotation.TotalAmount - discountAmount;
                quotation.CreatedBy = User?.Identity?.Name ?? "System";

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
                foreach (var item in quotation.Items)
                {
                    var colorStock = await _context.ColorItem
                        .Include(c => c.Color)   // ✅ مهم
                        .FirstOrDefaultAsync(c =>
                            c.ItemId == item.ItemId &&
                            c.ColorId == item.ColorId);

                    if (colorStock == null)
                        return Json(new { success = false, message = "Color not found" });

                    if (item.Quantity > colorStock.Quantity)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Requested quantity is not available. Color: {colorStock.Color.Name}, Available stock: {colorStock.Quantity}."
                        });
                    }

                }

                _context.Quotations.Add(newQuotation);
                await _context.SaveChangesAsync();

                foreach (var item in quotation.Items)
                {
                    var colorStock = await _context.ColorItem
                        .FirstOrDefaultAsync(c =>
                            c.ItemId == item.ItemId &&
                            c.ColorId == item.ColorId);

                    colorStock.Quantity -= item.Quantity;

                    _context.QuotationItems.Add(new QuotationItemsModel
                    {
                        ItemId = item.ItemId,
                        ColorId = item.ColorId,
                        ItemDescription = item.ItemDescription,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Vat = item.Vat,
                        Discount = item.Discount,
                        QuotationId = newQuotation.Id
                    });
                }

                await _context.SaveChangesAsync();


                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Quotation saved successfully!" });
            }
            catch (Exception ex)
            {
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

                // ===== Fonts =====
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                // ===== Title =====
                var title = new Paragraph("QUOTATION", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 15
                };
                document.Add(title);

                // ===== Quotation Info =====
                PdfPTable infoTable = new PdfPTable(2);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 50, 50 });
                infoTable.SpacingAfter = 15;

                infoTable.AddCell(GetCell($"Quotation No: {quotation.QuotationNumber}", boldFont, Element.ALIGN_LEFT));
                infoTable.AddCell(GetCell($"Date: {quotation.DateCreated:dd/MM/yyyy}", boldFont, Element.ALIGN_RIGHT));

                infoTable.AddCell(GetCell($"Customer: {quotation.CustomerName}", normalFont, Element.ALIGN_LEFT));
                //infoTable.AddCell(GetCell($"Status: {quotation.Status}", normalFont, Element.ALIGN_RIGHT));

                document.Add(infoTable);

                // ===== Items Table =====
                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 40, 10, 15, 15,15, 20 });
                table.SpacingBefore = 10;

                // Header
                AddHeaderCell(table, "Item");
                AddHeaderCell(table, "Qty");
                AddHeaderCell(table, "Unit Price");
                AddHeaderCell(table, "VAT");
                AddHeaderCell(table, "Discount");
                AddHeaderCell(table, "Total");

                foreach (var item in quotation.Items)
                {
                    table.AddCell(GetCell(item.ItemDescription, normalFont, Element.ALIGN_LEFT));
                    table.AddCell(GetCell(item.Quantity.ToString(), normalFont, Element.ALIGN_CENTER));
                    table.AddCell(GetCell(item.UnitPrice.ToString("F2"), normalFont, Element.ALIGN_RIGHT));
                    table.AddCell(GetCell(item.Vat.ToString("F2"), normalFont, Element.ALIGN_RIGHT));

                    table.AddCell(GetCell(item.Discount.ToString("F2"), normalFont, Element.ALIGN_RIGHT));
                    var baseAmount = item.UnitPrice * item.Quantity;
                    var discountAmount = baseAmount * (item.Discount / 100m);
                    var total = baseAmount - discountAmount + item.Vat;

                    table.AddCell(GetCell(total.ToString("F2"), normalFont, Element.ALIGN_RIGHT));

                   
                }

                document.Add(table);

                // ===== Summary =====
                PdfPTable summaryTable = new PdfPTable(2);
                summaryTable.WidthPercentage = 40;
                summaryTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                summaryTable.SpacingBefore = 15;

                summaryTable.AddCell(GetCell("Net Amount", boldFont, Element.ALIGN_LEFT));
                summaryTable.AddCell(GetCell(quotation.NetAmount.ToString("F2") + " AED", boldFont, Element.ALIGN_RIGHT));

                document.Add(summaryTable);

                document.Close();

                return File(ms.ToArray(), "application/pdf",
                    $"Quotation_{quotation.QuotationNumber}.pdf");
            }
        }
        private PdfPCell GetCell(string text, Font font, int alignment)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = alignment,
                Padding = 6
            };
        }

        private void AddHeaderCell(PdfPTable table, string text)
        {
            var cell = new PdfPCell(new Phrase(text, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)))
            {
                BackgroundColor = new BaseColor(230, 230, 230),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 6
            };
            table.AddCell(cell);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var quotation = await _context.Quotations.FindAsync(id);
            if (quotation == null) return NotFound();

            return View(quotation);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateQuotation([FromBody] QuotationModel model)
        {
            if (model == null || model.Items == null)
                return Json(new { success = false, message = "Invalid data" });

            var quotation = await _context.Quotations
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == model.Id);

            if (quotation == null)
                return Json(new { success = false, message = "Quotation not found" });

            quotation.CustomerName = model.CustomerName;
            quotation.Discount = model.Discount;
            quotation.TotalAmount = model.TotalAmount;
            quotation.NetAmount = model.NetAmount;

            quotation.Items.Clear();
            foreach (var item in model.Items)
            {
                quotation.Items.Add(new QuotationItemsModel
                {
                    ItemId = item.ItemId,
                    ItemDescription = item.ItemDescription,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Vat = item.Vat,
                    Discount = item.Discount
                });
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();

            return View(quotation);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, QuotationModel updated)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();

            quotation.CustomerName = updated.CustomerName;
            quotation.Discount = updated.Discount;
            quotation.NetAmount = updated.NetAmount;
            quotation.TotalAmount = updated.TotalAmount;

            _context.QuotationItems.RemoveRange(quotation.Items);

            foreach (var item in updated.Items)
            {
                quotation.Items.Add(new QuotationItemsModel
                {
                    ItemId = item.ItemId,
                    ItemDescription = item.ItemDescription,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Vat = item.Vat
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


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
        [HttpGet]
        public async Task<IActionResult> GetItemColors(int itemId)
        {
            var colors = await _context.ColorItem
                .Where(ci => ci.ItemId == itemId)
                .Include(ci => ci.Color)
                .Select(ci => new
                {
                    colorId = ci.ColorId,
                    colorName = ci.Color.Name,
                    quantity = ci.Quantity
                })
                .ToListAsync();

            return Json(colors);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var quotation = await _context.Quotations
                .Include(q => q.Items) 
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();

            return View(quotation);
        }

    }
}
