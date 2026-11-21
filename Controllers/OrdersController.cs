using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using proyectoSistemas1.Models;

namespace proyectoSistemas1.Controllers;

public class OrdersController : Controller
{
    private readonly NorthwindContext _context;
    public OrdersController(NorthwindContext context) => _context = context;

    // Lista de pedidos (maestro)
    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Take(100)
            .ToListAsync();
        return View(orders);
    }

    // Pedido + sus líneas (maestro-detalle)
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return NotFound();

      ViewBag.Products = new SelectList(
    _context.Products.Where(p => p.Discontinued == "0").OrderBy(p => p.ProductName),
    "ProductId", "ProductName");

        return View(order);
    }

    // Agregar línea al pedido
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDetail(int orderId, int productId, short quantity, float discount)
    {
        if (quantity <= 0 || discount < 0 || discount > 1)
        {
            TempData["Error"] = "Cantidad debe ser mayor a 0 y el descuento entre 0 y 1 (ej. 0.10).";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        if (await _context.OrderDetails.AnyAsync(d => d.OrderId == orderId && d.ProductId == productId))
        {
            TempData["Error"] = "Ese producto ya está en el pedido.";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        var product = await _context.Products.FindAsync(productId);
        if (product == null) return NotFound();

        _context.OrderDetails.Add(new OrderDetail
{
    OrderId = orderId,
    ProductId = productId,
    UnitPrice = product.UnitPrice ?? 0.0,
    Quantity = quantity,
    Discount = discount
});
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    // Eliminar línea del pedido
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDetail(int orderId, int productId)
    {
        var detail = await _context.OrderDetails.FindAsync(orderId, productId);
        if (detail != null)
        {
            _context.OrderDetails.Remove(detail);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Details), new { id = orderId });
    }
}