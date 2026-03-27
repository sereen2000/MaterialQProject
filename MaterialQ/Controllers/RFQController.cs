using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using MaterialQ.Models;
using MaterialQ.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

public class RFQController : Controller
{
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult GeneratePdf([FromBody] RFQModel model)
    {
        using (var ms = new MemoryStream())
        {
            var document = new Document(PageSize.A4, 40, 40, 40, 40);
            PdfWriter.GetInstance(document, ms);
            document.Open();

            var companyNameFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
            var companyInfoFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

            // =====================================================
            // COMPANY INFO (عدل بيانات شركتك هون)
            // =====================================================

            string companyName = "MaterialQ Trading LLC";
            string companyPhone = "+971 50 123 4567";
            string companyEmail = "info@materialq.com";
            string companyAddress = "Dubai, United Arab Emirates";

            // =====================================================
            // HEADER (Logo Left - Company Info Right)
            // =====================================================

            PdfPTable headerTable = new PdfPTable(2);
            headerTable.WidthPercentage = 100;
            headerTable.SetWidths(new float[] { 40, 60 });
            headerTable.SpacingAfter = 10;

            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/LOGO_page.png");

            PdfPCell logoCell;

            if (System.IO.File.Exists(logoPath))
            {
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(170f, 80f);
                logoCell = new PdfPCell(logo);
            }
            else
            {
                logoCell = new PdfPCell(new Phrase(""));
            }

            logoCell.Border = Rectangle.NO_BORDER;
            logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
            logoCell.VerticalAlignment = Element.ALIGN_MIDDLE;

            headerTable.AddCell(logoCell);

            Paragraph companyParagraph = new Paragraph();
            companyParagraph.Add(new Chunk(companyName + "\n\n", companyNameFont));
            companyParagraph.Add(new Chunk(companyAddress + "\n\n", companyInfoFont));
            companyParagraph.Add(new Chunk("Phone: " + companyPhone + "\n\n", companyInfoFont));
            companyParagraph.Add(new Chunk("Email: " + companyEmail, companyInfoFont));

            PdfPCell infoCell = new PdfPCell(companyParagraph);
            infoCell.Border = Rectangle.NO_BORDER;
            infoCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            infoCell.VerticalAlignment = Element.ALIGN_MIDDLE;

            headerTable.AddCell(infoCell);

            document.Add(headerTable);

            // =====================================================
            // LINE SEPARATOR
            // =====================================================

            LineSeparator line = new LineSeparator();
            line.Offset = -2;
            document.Add(new Chunk(line));
            document.Add(new Paragraph(" "));

            // =====================================================
            // DOCUMENT TITLE
            // =====================================================

            Paragraph title = new Paragraph("REQUEST FOR QUOTATION", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 15;
            document.Add(title);

            // =====================================================
            // INFO TABLE (Clean - No Borders)
            // =====================================================

            PdfPTable infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SetWidths(new float[] { 50, 50 });
            infoTable.SpacingAfter = 15;

            infoTable.AddCell(GetCell($"RFQ No: {Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}", boldFont, Element.ALIGN_LEFT));
            infoTable.AddCell(GetCell($"Date: {DateTime.Now:dd/MM/yyyy}", boldFont, Element.ALIGN_RIGHT));

            infoTable.AddCell(GetCell($"From: {model.FromCompany}", normalFont, Element.ALIGN_LEFT));
            infoTable.AddCell(GetCell($"To: {model.ToCompany}", normalFont, Element.ALIGN_RIGHT));

            document.Add(infoTable);

            // =====================================================
            // ITEMS TABLE
            // =====================================================

            PdfPTable table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 45, 15, 40 });
            table.SpacingBefore = 10;

            AddHeaderCell(table, "Item");
            AddHeaderCell(table, "Qty");
            AddHeaderCell(table, "Specifications");

            foreach (var item in model.Items)
            {
                table.AddCell(GetCell(item.Name ?? "", normalFont, Element.ALIGN_LEFT));
                table.AddCell(GetCell(item.Qty ?? "", normalFont, Element.ALIGN_CENTER));
                table.AddCell(GetCell(item.Notes ?? "", normalFont, Element.ALIGN_LEFT));
            }

            document.Add(table);

            document.Close();

            return File(ms.ToArray(), "application/pdf", "RFQ.pdf");
        }
    }

    // =====================================================
    // SAME HELPERS AS QUOTATION
    // =====================================================

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
}