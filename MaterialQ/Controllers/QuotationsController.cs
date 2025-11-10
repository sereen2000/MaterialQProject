using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MaterialQ.Data;
using MaterialQ.Models.DataModels;

namespace MaterialQ.Controllers
{
    public class QuotationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuotationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Quotations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Quotations.ToListAsync());
        }

        // GET: Quotations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quotationModel = await _context.Quotations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quotationModel == null)
            {
                return NotFound();
            }

            return View(quotationModel);
        }

        // GET: Quotations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Quotations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuotationNumber,CustomerName,DateCreated,CreatedBy,TotalAmount,Discount,NetAmount,Status")] QuotationModel quotationModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(quotationModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(quotationModel);
        }

        // GET: Quotations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quotationModel = await _context.Quotations.FindAsync(id);
            if (quotationModel == null)
            {
                return NotFound();
            }
            return View(quotationModel);
        }

        // POST: Quotations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuotationNumber,CustomerName,DateCreated,CreatedBy,TotalAmount,Discount,NetAmount,Status")] QuotationModel quotationModel)
        {
            if (id != quotationModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quotationModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuotationModelExists(quotationModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(quotationModel);
        }

        // GET: Quotations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quotationModel = await _context.Quotations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quotationModel == null)
            {
                return NotFound();
            }

            return View(quotationModel);
        }

        // POST: Quotations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quotationModel = await _context.Quotations.FindAsync(id);
            if (quotationModel != null)
            {
                _context.Quotations.Remove(quotationModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuotationModelExists(int id)
        {
            return _context.Quotations.Any(e => e.Id == id);
        }
    }
}
