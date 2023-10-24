using DoAn2VADT.Database.Entities;
using DoAn2VADT.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using System.Linq;

namespace DoAn2VADT.Areas.Admin.Controllers
{
    public class ThongKeUser : Controller
    {
        private readonly AppDbContext _context;

        public ThongKeUser(AppDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public IActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 8;
            var lsOrder = _context.Orders
                .AsNoTracking()
                .OrderByDescending(x => x.Id);
            PagedList<Order> models = new PagedList<Order>(lsOrder, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }
        [HttpPost]
        public IActionResult Index(DateTime from_date, DateTime to_date)
        {
            using (_context)
            {
                ViewBag.GetBills = (from b in _context.Orders where b.CreatedAt >= from_date && b.CreatedAt <= to_date == true select b).ToList();
                ViewBag.GetQuantityOrder = (from b in _context.Orders where b.CreatedAt >= from_date && b.CreatedAt <= to_date == true select b.Id).Count();
                ViewBag.SumToTal = (from b in _context.Orders where b.CreatedAt >= from_date && b.CreatedAt <= to_date == true select b.Total).Sum();
                return View();
            }
        }
        public IActionResult Filtter(string CatID = "0")
        {
            var url = $"/Order?CatID={CatID}";
            if (CatID == "0")
            {
                url = $"/Order";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        // GET: Orders/Details/5
        public IActionResult Details(string id)
        {
            var pageNumber = 1;
            var pageSize = 8;
            var orderDetail = _context.OrderDetails
                .Where(x => x.OrderId.ToString() == id)
                .Include(x => x.Product);
            PagedList<OrderDetail> models = new PagedList<OrderDetail>(orderDetail.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }
        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id.ToString() == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'AppDbContext.Orders'  is null.");
            }
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.Id.ToString() == id);
        }
    }
}
