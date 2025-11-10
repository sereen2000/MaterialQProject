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
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productModel = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productModel == null)
            {
                return NotFound();
            }

            return View(productModel);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,UnitId,UnitName,ColorId,ColorName,IsAvailable,DiscountPrice")] ProductsModel productModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productModel);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productModel = await _context.Products.FindAsync(id);
            if (productModel == null)
            {
                return NotFound();
            }
            return View(productModel);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,UnitId,UnitName,ColorId,ColorName,IsAvailable,DiscountPrice")] ProductsModel productModel)
        {
            if (id != productModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductsModelExists(productModel.Id))
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
            return View(productModel);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productModel = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productModel == null)
            {
                return NotFound();
            }

            return View(productModel);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productModel = await _context.Products.FindAsync(id);
            if (productModel != null)
            {
                _context.Products.Remove(productModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductsModelExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
