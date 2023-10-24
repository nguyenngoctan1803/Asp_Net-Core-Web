using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAn2VADT.Database;
using DoAn2VADT.Database.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagedList.Core;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace DoAn2VADT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BrandController : GlobalController
    {
        public BrandController(AppDbContext context, IHostingEnvironment environment, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<DashboardController> logger) : base(context, environment, notyfService, httpContextAccessor, logger)
        {

        }

        // GET: Brand/Index
        public IActionResult Index(int? page, string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 8;
            var lsbrand = _context.Brands
                .AsNoTracking()
                .OrderByDescending(x => x.Id).ToList();
            if (searchkey != "")
            {
                ViewBag.SearchKey = searchkey;
                lsbrand = lsbrand.Where(b => b.Name.ToLower().Contains(searchkey.ToLower())).ToList();
            }
            PagedList<Brand> models = new PagedList<Brand>(lsbrand.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Brand/Detail/Id
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Brands == null)
            {
                return NotFound();
            }

            var Brand = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Brand == null)
            {
                return NotFound();
            }

            return View(Brand);
        }

        // GET: Brand/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Brand/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,Id,Name,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Brand Brand)
        {
            if (ModelState.IsValid)
            {
                Brand.Id = Guid.NewGuid().ToString();
                Brand.CreateUserId = HttpContext.Session.GetString("Id");
                Brand.CreatedAt = DateTime.Now;
                Brand.UpdatedAt = DateTime.Now;
                _context.Add(Brand);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm thượng hiệu thành công");
                return RedirectToAction(nameof(Index));
            }
            return View(Brand);
        }

        // GET: Brand/Edit/Id
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Brands == null)
            {
                return NotFound();
            }

            var Brand = await _context.Brands.FindAsync(id);
            if (Brand == null)
            {
                return NotFound();
            }
            return View(Brand);
        }

        // POST: Brand/Edit/Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Description,Id,Name,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Brand Brand)
        {
            if (id != Brand.Id.ToString())
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Brand.UpdatedAt = DateTime.Now;
                    Brand.UpdateUserId = HttpContext.Session.GetString("Id");
                    _context.Update(Brand);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Cập nhật thương hiệu thành công!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(Brand.Id.ToString()))
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
            return View(Brand);
        }

        // GET: Brand/Delete/Id
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Brands == null)
            {
                return NotFound();
            }

            var publisher = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null)
            {
                return NotFound();
            }

            return View(publisher);
        }

        // POST: Brand/Delete/Id
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Brands == null)
            {
                return Problem("Entity set 'AppDbContext.Brands'  is null.");
            }
            var brand = _context.Brands.Find(id);
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            _notyfService.Success("Xóa thương hiệu thành công");
            return RedirectToAction(nameof(Index));
        }

        private bool BrandExists(string id)
        {
            return _context.Brands.Any(e => e.Id == id);
        }
    }
}
