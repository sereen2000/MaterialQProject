using iTextSharp.text;
using iTextSharp.text.pdf;
using MaterialQ.Models;
using MaterialQ.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
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
            Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            doc.Add(new Paragraph("Request for Quotation (RFQ)", titleFont));
            doc.Add(new Paragraph(" "));

            doc.Add(new Paragraph($"From Company: {model.FromCompany}"));
            doc.Add(new Paragraph($"To Company: {model.ToCompany}"));
            doc.Add(new Paragraph(" "));

            PdfPTable table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 40, 20, 40 });

            table.AddCell("Product Name");
            table.AddCell("Quantity");
            table.AddCell("Notes / Specifications");

            foreach (var item in model.Items)
            {
                table.AddCell(item.Name ?? "");
                table.AddCell(item.Qty ?? "");
                table.AddCell(item.Notes ?? "");
            }

            doc.Add(table);
            doc.Close();

            return File(ms.ToArray(), "application/pdf", "RFQ.pdf");
        }
    }
}
