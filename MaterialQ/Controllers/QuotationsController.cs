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
        private readonly IWebHostEnvironment _env;

        public QuotationsController(ApplicationDbContext context, IItemService itemService, IWebHostEnvironment env)
        {
            _context = context;
            _itemService = itemService;
            _env = env;
        }
      
        public async Task<IActionResult> Index(string? from, string? to, string? search)
        {
            var query = _context.Quotations.Include(q => q.Company).AsQueryable();


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

                    q.QuotationNumber.Contains(search));
                   
               
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(q =>

                    q.Company.Name.Contains(search));

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
        public async Task<IActionResult> ReviewQuotation()
        {

            ViewBag.Companies = await _context.Companies
        .OrderBy(c => c.Name)
        .ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveQuotation([FromBody] QuotationModel quotation)
        {
            try
            {
                if (quotation == null || quotation.Items == null || !quotation.Items.Any())
                    return Json(new { success = false, message = "Quotation is empty." });

                var lastQuotation = await _context.Quotations
                    .OrderByDescending(q => q.Id)
                    .FirstOrDefaultAsync();

                int nextNumber = (lastQuotation?.Id ?? 0) + 1;

                quotation.QuotationNumber = $"Q-{DateTime.Now:yyyy}-{nextNumber:D4}";
                quotation.DateCreated = DateTime.Now;
                quotation.Status = "Draft";
                quotation.CreatedBy = User?.Identity?.Name ?? "System";

                
                quotation.TotalAmount = quotation.Items.Sum(i =>
                    (i.UnitPrice * i.Quantity) + i.Vat
                );

                var discountAmount = quotation.TotalAmount * (quotation.Discount / 100m);

                quotation.NetAmount = quotation.TotalAmount - discountAmount;


                var newQuotation = new QuotationModel
                {
                    QuotationNumber = quotation.QuotationNumber,
                    CompanyId = quotation.CompanyId,
                    DateCreated = quotation.DateCreated,
                    CreatedBy = quotation.CreatedBy,
                    TotalAmount = quotation.TotalAmount,
                    Discount = quotation.Discount,
                    NetAmount = quotation.NetAmount,
                    Status = quotation.Status
                };

                _context.Quotations.Add(newQuotation);
                await _context.SaveChangesAsync();

                
                foreach (var item in quotation.Items)
                {
                    _context.QuotationItems.Add(new QuotationItemsModel
                    {
                        ItemId = item.ItemId,
                        ItemDescription = item.ItemDescription,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,    
                        ActualPrice = item.ActualPrice,  
                        Vat = item.Vat,
                        QuotationId = newQuotation.Id
                    });
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Quotation saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while saving: " + (ex.InnerException?.Message ?? ex.Message)
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> ExportToPdf(int id)
        {
            var lpo = await _context.Quotations
                .Include(q => q.Items)
                .Include(q => q.Company)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (lpo == null)
                return NotFound();

            // This data should be fetched from your database
            var lpoData = new
            {
                Date = new DateTime(2025, 11, 10),
                FromCompany = "Building Art Com. Bro. L.L.C",
                FromTel = "02-5864000",
                FromFax = "02-5864004",
                FromContact = "Ahmed Al Abdullat",
                ToCompany = lpo.Company.Name,
                ToTel = "050-8401185",
                ToFax = "0",
                ToAttention = "Mr. SHAHID",
                PlaceDeliver = "Abu Dhabi - Mafraq Industrial Area",
                Contact = "050-9059922"
            };

            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 30, 30, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // --- FONTS ---
                var arabicFontPath = Path.Combine(_env.WebRootPath, "font", "Amiri-Regular.ttf");
                if (!System.IO.File.Exists(arabicFontPath))
                    throw new FileNotFoundException("Arabic font not found at: " + arabicFontPath);

                BaseFont arabicBase = BaseFont.CreateFont(arabicFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font fontArabicBold = new Font(arabicBase, 12, Font.BOLD);
                Font fontArabicNormal = new Font(arabicBase, 10, Font.NORMAL);
                Font fontEnglishBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                Font fontEnglishNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);

                // --- HEADER ---
                var headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 60, 40 }); // Right side larger
                headerTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                // -- Company Info Cell (Right)
                var companyInfoCell = new PdfPCell { Border = Rectangle.NO_BORDER, PaddingRight = 10, HorizontalAlignment = Element.ALIGN_LEFT };
                companyInfoCell.AddElement(new Paragraph("شركة فن البناء للوساطة التجارية ذ.م.م", fontArabicBold) { Alignment = Element.ALIGN_LEFT });
                companyInfoCell.AddElement(new Paragraph("تلفون: 02/5864000", fontArabicNormal) { Alignment = Element.ALIGN_LEFT });
                companyInfoCell.AddElement(new Paragraph("فاكس: 02/5864004", fontArabicNormal) { Alignment = Element.ALIGN_LEFT });
                companyInfoCell.AddElement(new Paragraph("صندوق بريد: 92477", fontArabicNormal) { Alignment = Element.ALIGN_LEFT });
                companyInfoCell.AddElement(new Paragraph("موبايل: 050-3196919 / 050-6115988", fontArabicNormal) { Alignment = Element.ALIGN_LEFT });
                companyInfoCell.AddElement(new Paragraph("بريد إلكتروني: buildingart_cb@yahoo.com", fontArabicNormal) { Alignment = Element.ALIGN_LEFT });
                companyInfoCell.AddElement(new Paragraph("المكتب الرئيسي: المفرق الصناعية", fontArabicNormal) { Alignment = Element.ALIGN_LEFT });
                headerTable.AddCell(companyInfoCell);

                // -- Logo Cell (Left)
                var logoCell = new PdfPCell { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
                try
                {
                    var logoPath = Path.Combine(_env.WebRootPath, "img", "LOGO_page.png");
                    var logo = Image.GetInstance(logoPath);
                    logo.ScaleToFit(120f, 120f);
                    logoCell.AddElement(logo);
                }
                catch { logoCell.AddElement(new Paragraph("Building Art", fontEnglishBold)); }
                headerTable.AddCell(logoCell);
                document.Add(headerTable);

                // --- L.P.O TITLE & DATE ---
                var titleTable = new PdfPTable(2);
                titleTable.WidthPercentage = 100;
                titleTable.SetWidths(new float[] { 80, 20 });
                titleTable.SpacingBefore = 15;

                var lpoTitleCell = new PdfPCell(new Phrase("L.P.O.", fontEnglishBold))
                {
                    Border = Rectangle.BOX,
                    BackgroundColor = BaseColor.LightGray,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 5
                };
                titleTable.AddCell(lpoTitleCell);

                var dateCell = new PdfPCell(new Phrase($"Date: {lpoData.Date:dd.MM.yyyy}", fontEnglishBold))
                {
                    Border = Rectangle.BOX,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 5
                };
                titleTable.AddCell(dateCell);
                document.Add(titleTable);

                // --- TO/FROM INFO ---
                var toFromTable = new PdfPTable(2);
                toFromTable.WidthPercentage = 100;
                toFromTable.SpacingBefore = 5;

                var toCell = new PdfPCell { Padding = 5 };
                toCell.AddElement(new Phrase($"To: {lpoData.ToCompany}", fontEnglishBold));
                toCell.AddElement(new Phrase($"Tel: {lpoData.ToTel}", fontEnglishNormal));
                toCell.AddElement(new Phrase($"Fax: {lpoData.ToFax}", fontEnglishNormal));
                toCell.AddElement(new Phrase($"Attention: {lpoData.ToAttention}", fontEnglishBold));
                toFromTable.AddCell(toCell);

                var fromCell = new PdfPCell { Padding = 5 };
                fromCell.AddElement(new Phrase($"From: {lpoData.FromCompany}", fontEnglishBold));
                fromCell.AddElement(new Phrase($"Tel: {lpoData.FromTel}", fontEnglishNormal));
                fromCell.AddElement(new Phrase($"Fax: {lpoData.FromFax}", fontEnglishNormal));
                fromCell.AddElement(new Phrase($"From: {lpoData.FromContact}", fontEnglishBold));
                toFromTable.AddCell(fromCell);
                document.Add(toFromTable);

                // --- DELIVERY INFO ---
                var deliveryCell = new PdfPCell { Padding = 5 };
                deliveryCell.AddElement(new Phrase($"Company Name - {lpoData.FromCompany}", fontEnglishNormal));
                deliveryCell.AddElement(new Phrase($"Place Deliver: {lpoData.PlaceDeliver}", fontEnglishNormal));
                deliveryCell.AddElement(new Phrase($"Contact: {lpoData.Contact}", fontEnglishNormal));
                var deliveryTable = new PdfPTable(1) { WidthPercentage = 100 };
                deliveryTable.AddCell(deliveryCell);
                document.Add(deliveryTable);

                // --- ITEMS TABLE ---
                var itemsTable = new PdfPTable(5);
                itemsTable.WidthPercentage = 100;
                itemsTable.SetWidths(new float[] { 8, 52, 10, 15, 15 });
                itemsTable.SpacingBefore = 10;
                itemsTable.HeaderRows = 1; // Repeat header on new pages

                itemsTable.AddCell(CreateHeaderCell("No."));
                itemsTable.AddCell(CreateHeaderCell("Description"));
                itemsTable.AddCell(CreateHeaderCell("L.M"));
                itemsTable.AddCell(CreateHeaderCell("Price"));
                itemsTable.AddCell(CreateHeaderCell("Total Amount"));

                int counter = 1;
                foreach (var item in lpo.Items)
                {
                    itemsTable.AddCell(CreateItemCell(counter.ToString(), fontEnglishNormal, Element.ALIGN_CENTER));
                    itemsTable.AddCell(CreateItemCell(item.ItemDescription, fontEnglishNormal, Element.ALIGN_LEFT));
                    itemsTable.AddCell(CreateItemCell(item.Quantity.ToString(), fontEnglishNormal, Element.ALIGN_CENTER));
                    itemsTable.AddCell(CreateItemCell(item.UnitPrice.ToString("F2"), fontEnglishNormal, Element.ALIGN_RIGHT));
                    itemsTable.AddCell(CreateItemCell((item.Quantity * item.UnitPrice).ToString("F2"), fontEnglishNormal, Element.ALIGN_RIGHT));
                    counter++;
                }
                document.Add(itemsTable);

                // --- FOOTER SECTION (TOTALS, NOTES, SIGNATURES) ---
                // This is the main fix: put everything in a single table to prevent page breaks.
                var footerTable = new PdfPTable(1)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10
                };
                footerTable.DefaultCell.Border = Rectangle.NO_BORDER;

                // -- Totals
                decimal subTotal = lpo.Items.Sum(i => i.Quantity * i.UnitPrice);
                decimal vatAmount = subTotal * 0.05m;
                decimal grandTotal = subTotal + vatAmount;

                var totalsTable = new PdfPTable(2);
                totalsTable.WidthPercentage = 45;
                totalsTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                totalsTable.AddCell(CreateTotalCell("TOTAL", fontEnglishBold, BaseColor.White));
                totalsTable.AddCell(CreateTotalCell(subTotal.ToString("F2"), fontEnglishNormal, BaseColor.White));
                totalsTable.AddCell(CreateTotalCell("VAT 5%", fontEnglishBold, BaseColor.White));
                totalsTable.AddCell(CreateTotalCell(vatAmount.ToString("F2"), fontEnglishNormal, BaseColor.White));
                totalsTable.AddCell(CreateTotalCell("GRAND TOTAL", fontEnglishBold, BaseColor.LightGray));
                totalsTable.AddCell(CreateTotalCell(grandTotal.ToString("F2"), fontEnglishBold, BaseColor.LightGray));
                footerTable.AddCell(totalsTable);

                // -- Notes
                var notesCell = new PdfPCell { Border = Rectangle.NO_BORDER, PaddingTop = 20 };
                notesCell.AddElement(new Paragraph("{Notes}:", fontEnglishBold));
                notesCell.AddElement(new Paragraph("Payment Terms :AS AGREED", fontEnglishNormal));
                notesCell.AddElement(new Paragraph("Kindly attach one copy of our L.P.O. with your invoice.", fontEnglishNormal));
                footerTable.AddCell(notesCell);

                // -- Signatures
                var signatureTable = new PdfPTable(3);
                signatureTable.WidthPercentage = 100;
                signatureTable.SpacingBefore = 40;
                signatureTable.AddCell(CreateSignatureCell("General manager"));
                signatureTable.AddCell(CreateSignatureCell("Accountant"));
                signatureTable.AddCell(CreateSignatureCell("Purchase Department"));
                footerTable.AddCell(signatureTable);

                // Add the entire footer block to the document
                document.Add(footerTable);

                document.Close();
                return File(ms.ToArray(), "application/pdf", $"LPO_{lpo.Id}.pdf");
            }
        }

        // --- HELPER METHODS (Updated CreateTotalCell) ---

        private PdfPCell CreateHeaderCell(string text)
        {
            return new PdfPCell(new Phrase(text, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9)))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5,
                BackgroundColor = new BaseColor(217, 217, 217)
            };
        }

        private PdfPCell CreateItemCell(string text, Font font, int alignment)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = alignment,
                Padding = 5,
                MinimumHeight = 20
            };
        }

        // Updated to handle background color
        private PdfPCell CreateTotalCell(string text, Font font, BaseColor bgColor)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 5,
                Border = Rectangle.BOX,
                BackgroundColor = bgColor
            };
        }

        private PdfPCell CreateSignatureCell(string title)
        {
            var cell = new PdfPCell { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 20 };
            cell.AddElement(new Phrase(title, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)));
            cell.AddElement(new Phrase("____________________", FontFactory.GetFont(FontFactory.HELVETICA, 10)));
            return cell;
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

        private PdfPCell GetSignatureCell(string title)
        {
            var cell = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                PaddingTop = 30,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };

            cell.AddElement(new Phrase(title, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)));
            cell.AddElement(new Phrase("__________________________", FontFactory.GetFont(FontFactory.HELVETICA, 10)));

            return cell;
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

        private PdfPCell GetBoxCell(string text, Font font)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                Border = Rectangle.NO_BORDER,
                Padding = 5
            };
        }

       
    
        private string NumberToWords(decimal amount)
        {
            long whole = (long)Math.Floor(amount);
            return NumberToWords(whole);
        }

        private string NumberToWords(long number)
        {
            if (number == 0) return "Zero";

            string[] units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
            string[] teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (number < 10) return units[number];
            if (number < 20) return teens[number - 10];
            if (number < 100) return tens[number / 10] + (number % 10 != 0 ? " " + units[number % 10] : "");
            if (number < 1000) return units[number / 100] + " Hundred" + (number % 100 != 0 ? " " + NumberToWords(number % 100) : "");

            return number.ToString();
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
                .Include(q => q.Company)
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == model.Id);

            if (quotation == null)
                return Json(new { success = false, message = "Quotation not found" });

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
                .Include(q => q.Company)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();
    
            ViewBag.Companies = await _context.Companies
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(quotation);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, QuotationModel updated)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Items)
                .Include(q => q.Company)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();

            quotation.CompanyId = updated.CompanyId;
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
        

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var quotation = await _context.Quotations
                .Include(q => q.Items)
                .Include(q => q.Company)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();

            return View(quotation);
        }

    }
}
