using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAn2VADT.Database;
using DoAn2VADT.Database.Entities;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagedList.Core;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace DoAn2VADT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : GlobalController
    {
        public CategoryController(AppDbContext context, IHostingEnvironment environment, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<DashboardController> logger) : base(context, environment, notyfService, httpContextAccessor, logger)
        {

        }

        // GET: Category/Index
        public IActionResult Index(int? page, string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 8;
            var lscategory = _context.Categories
                .AsNoTracking()
                .OrderByDescending(x => x.Id).ToList();
            if (searchkey != "")
            {
                ViewBag.SearchKey = searchkey;
                lscategory = lscategory.Where(b => b.Name.ToLower().Contains(searchkey.ToLower())).ToList();
            }
            PagedList<Category> models = new PagedList<Category>(lscategory.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Category/Detail/Id
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var Category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Category == null)
            {
                return NotFound();
            }

            return View(Category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,Id,Name,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Category Category)
        {
            if (ModelState.IsValid)
            {
                Category.Id = Guid.NewGuid().ToString();
                Category.CreateUserId = HttpContext.Session.GetString("Id");
                Category.CreatedAt = DateTime.Now;
                Category.UpdatedAt = DateTime.Now;
                _context.Add(Category);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm thượng hiệu thành công");
                return RedirectToAction(nameof(Index));
            }
            return View(Category);
        }

        // GET: Category/Edit/Id
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var Category = await _context.Categories.FindAsync(id);
            if (Category == null)
            {
                return NotFound();
            }
            return View(Category);
        }

        // POST: Category/Edit/Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Description,Id,Name,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Category Category)
        {
            if (id != Category.Id.ToString())
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Category.UpdatedAt = DateTime.Now;
                    Category.UpdateUserId = HttpContext.Session.GetString("Id");
                    _context.Update(Category);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Cập nhật danh mục thành công!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(Category.Id.ToString()))
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
            return View(Category);
        }

        // GET: Category/Delete/Id
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var publisher = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null)
            {
                return NotFound();
            }

            return View(publisher);
        }

        // POST: Category/Delete/Id
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'AppDbContext.Categories'  is null.");
            }
            var brand = _context.Categories.Find(id);
            _context.Categories.Remove(brand);
            await _context.SaveChangesAsync();
            _notyfService.Success("Xóa danh mục thành công");
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(string id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
