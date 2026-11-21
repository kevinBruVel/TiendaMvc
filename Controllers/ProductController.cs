using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using proyectoSistemas1.Models;

namespace proyectoSistemas1.Controllers;

public class ProductController : Controller
{
    private readonly NorthwindContext _context;
    public ProductController(NorthwindContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsNoTracking()
            .OrderByDescending(p => p.ProductId)
            .ToListAsync();
        return View(products);
    }

    public IActionResult Create()
    {
        ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName");
        ViewBag.SupplierId = new SelectList(_context.Suppliers, "SupplierId", "CompanyName");
        return View(new Product { Discontinued = "0" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
        ViewBag.SupplierId = new SelectList(_context.Suppliers, "SupplierId", "CompanyName", product.SupplierId);
        return View(product);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
        ViewBag.SupplierId = new SelectList(_context.Suppliers, "SupplierId", "CompanyName", product.SupplierId);
        return View(product);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.ProductId) return NotFound();
        if (ModelState.IsValid)
        {
            _context.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
        ViewBag.SupplierId = new SelectList(_context.Suppliers, "SupplierId", "CompanyName", product.SupplierId);
        return View(product);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(m => m.ProductId == id);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
