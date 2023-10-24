/*using DoAn2VADT.Database.Entities;
using DoAn2VADT.Database;
using Microsoft.AspNetCore.Mvc;
using PagedList.Core;
using Microsoft.EntityFrameworkCore;
using DoAn2VADT.Helpper;
using Microsoft.AspNetCore.Mvc.Rendering;
using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;

namespace DoAn2VADT.Controllers
{
    public class ThongKeTonKhoController : Controller
    {
        private readonly AppDbContext _context;
        public INotyfService _notyfService { get; }

        public ThongKeTonKhoController(AppDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Orders
        public IActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 8;
            var lsBook = _context.Products
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt);
            PagedList<Product> models = new PagedList<Product>(lsBook, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }
        [HttpPost]
        public IActionResult Index(DateTime from_date, DateTime to_date)
        {
            using (_context)
            {
                ViewBag.GetBills = (from b in _context.Products where b.CreatedAt >= from_date && b.CreatedAt <= to_date == true select b).ToList();
                ViewBag.GetQuantityOrder = (from b in _context.Products where b.CreatedAt >= from_date && b.CreatedAt <= to_date == true select b.Id).Count();
                ViewBag.Import = (from b in _context.Imports where b.CreatedAt >= from_date && b.CreatedAt <= to_date == true select b.Id).Count();
                ViewBag.GetQuantityImport = (from b in _context.Imports where b.CreatedAt >= from_date && b.CreatedAt <= to_date == true select b.Total).Sum();
                return View();
            }
        }
        public IActionResult Filtter(int CatID = 0)
        {
            var url = $"/Book?CatID={CatID}";
            if (CatID == 0)
            {
                url = $"/Book";
            }
            return Json(new { status = "success", redirectUrl = url });
        }


        public async Task<IActionResult> Details(string? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var book = await _context.Products
                .Include(b => b.Category)
                .Include(b => b.Branch)
                .Include(b => b.Color)
                .FirstOrDefaultAsync(m => m.Id.ToString() == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var book = await _context.Products.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            ViewData["BranchId"] = new SelectList(_context.Branchs, "Id", "Name", book.BranchId);
            ViewData["ColorId"] = new SelectList(_context.Colors, "Id", "Name", book.ColorId);
            return View(book);
        }

        // POST: Admin/Book/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Description,Image,Price,Discount,DistributorId,PublisherId,AuthorId,TitleId,CategoryId,Id,Name,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId,Amont")] Product book, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (id != book.Id.ToString())
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                book.Name = Utilities.ToTitleCase(book.Name);
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl(book.Name) + extension;
                    book.Image = await Utilities.UploadFile(fThumb, @"book", image.ToLower());
                }
                if (string.IsNullOrEmpty(book.Name)) book.Image = "default.jpg";
                if (System.IO.File.Exists(book.Name))
                {
                    System.IO.File.Delete(book.Name);
                }
                book.UpdatedAt = DateTime.Now;
                _context.Update(book);
                await _context.SaveChangesAsync();
                _notyfService.Success("Update mới thành công");
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            ViewData["BranchId"] = new SelectList(_context.Branchs, "Id", "Name", book.BranchId);
            ViewData["ColorId"] = new SelectList(_context.Colors, "Id", "Name", book.ColorId);
            return View(book);
        }
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var book = await _context.Products
                .Include(b => b.Category)
                .Include(b => b.Branch)
                .Include(b => b.Color)
                .FirstOrDefaultAsync(m => m.Id.ToString() == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'AppDbContext.Books'  is null.");
            }
            var book = await _context.Products.FindAsync(id);
            if (book != null)
            {
                book.Name = Utilities.ToTitleCase(book.Name);
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl(book.Name) + extension;
                    book.Image = await Utilities.UploadFile(fThumb, @"book", image.ToLower());
                }
                if (string.IsNullOrEmpty(book.Name)) book.Image = "default.jpg";
                if (System.IO.File.Exists(book.Name))
                {
                    System.IO.File.Delete(book.Name);
                }
                _context.Products.Remove(book);
            }

            await _context.SaveChangesAsync();
            _notyfService.Success("Xóa thành công");
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(string id)
        {
            return _context.Products.Any(e => e.Id.ToString() == id);
        }
    }
}
*/