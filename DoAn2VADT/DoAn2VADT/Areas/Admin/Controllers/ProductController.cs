using AspNetCoreHero.ToastNotification.Abstractions;
using ClosedXML.Excel;
using DoAn2VADT.Database;
using DoAn2VADT.Database.Entities;
using DoAn2VADT.Helpper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using DoAn2VADT.Shared;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using System.Security.Claims;
namespace DoAn2VADT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : GlobalController
    {
        public ProductController(AppDbContext context, IHostingEnvironment environment, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<DashboardController> logger) : base(context, environment, notyfService, httpContextAccessor, logger)
        {

        }
        // GET: Product/Index
        public IActionResult Index(int page = 1, string catid = "0", string brandid = "0", string searchkey = "")
        {
            var pageNumber = page;
            var pageSize = 8;

            List<Product> lsProducts = new List<Product>();
            lsProducts = _context.Products
               .AsNoTracking()
               .Include(p => p.Category)
               .Include(p => p.Brand)
               .OrderByDescending(x => x.CreatedAt).ToList();

            if (catid != "0")
            {
                ViewBag.CurrentCateId = catid;
                lsProducts = lsProducts.Where(x => x.CategoryId == catid).OrderByDescending(x => x.CreatedAt).ToList();
            }
            if (brandid != "0")
            {
                ViewBag.CurrentBrandId = brandid;
                lsProducts = lsProducts.Where(x => x.BrandId == brandid).OrderByDescending(x => x.CreatedAt).ToList();
            }
            if (searchkey != "")
            {
                ViewBag.SearchKey = searchkey;
                lsProducts = lsProducts
                        .Where(x => x.Name.Contains(searchkey) || x.Brand.Name.Contains(searchkey) || x.Category.Name.Contains(searchkey))
                        .OrderByDescending(x => x.CreatedAt)
                        .ToList();
            }
            PagedList<Product> models = new PagedList<Product>(lsProducts.AsQueryable(), pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.BrandId = new SelectList(_context.Brands, "Id", "Name");
            return View(models);
        }
        public IActionResult Filter(string catid = "0", string brandid = "0")
        {
            var url = "/Product";
            if (catid != "0")
            {
                url += $"?catid={catid}";
            }
            if (brandid != "0")
            {
                url += $"&brandid={brandid}";
            }

            return Redirect(url);
        }
        // GET: Product/Detail/Id
        public IActionResult Details(string id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = _context.Products
                .Include(b => b.Category)
                .Include(b => b.Brand)
                .FirstOrDefault(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.BrandId = new SelectList(_context.Brands, "Id", "Name");
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,Image,Price,Discount,BrandId,CategoryId,Id,Name,Code,Quantity,Main,Color,Ram,Memory,Screen")] Product product, IFormFile fThumb)
        {
            if (ModelState.IsValid)
            {
                product.Id = Guid.NewGuid().ToString();
                product.Name = Utilities.ToTitleCase(product.Name);
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    product.Image = Utilities.SEOUrl(product.Name) + $"-{product.Id}" + extension;
                    await Utilities.UploadFile(fThumb, @"product", product.Image);
                }
                else product.Image = "default.jpg";

                product.CreateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
                product.CreatedAt = DateTime.Now;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm sản phẩm thành công");
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.BrandId = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            return View(product);
        }


        // POST: Product/ChangeEffective/Id
        public IActionResult ChangeEffective(string id)
        {
            if (id == null)
            {
                _notyfService.Error("Vui lòng thử lại!");
            }
            else
            {
                var product = _context.Products.Find(id);
                product.Effective = !product.Effective;
                _context.Products.Update(product);
                _context.SaveChanges();
                _notyfService.Success("Đã cập nhật hiệu lực");
            }
            
            return RedirectToAction("Index");
        }
        // GET: Product/Edit/Id
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.BrandId = new SelectList(_context.Brands, "Id", "Name", product.BrandId);

            return View(product);
        }

        // POST: Product/Edit/Id
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Description,Image,Price,Discount,BrandId,CategoryId,Id,Name,Code,Quantity,Main,Color,Ram,Memory,Screen")] Product product, IFormFile fThumb)
        {
            if (id != product.Id.ToString())
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var productEdit = _context.Products.Find(id);
                productEdit.Name = Utilities.ToTitleCase(product.Name);
                productEdit.Code = product.Code;
                productEdit.Price = product.Price;
                productEdit.Discount = product.Discount;
                productEdit.Quantity = product.Quantity;
                productEdit.Main = Utilities.ToTitleCase(product.Main);
                productEdit.Color = Utilities.ToTitleCase(product.Color);
                productEdit.Ram = product.Ram;
                productEdit.Memory = Utilities.ToTitleCase(product.Memory);
                productEdit.Screen = Utilities.ToTitleCase(product.Screen);
                productEdit.BrandId = product.BrandId;
                productEdit.CategoryId = product.CategoryId;
                productEdit.Description = product.Description;
                if (fThumb != null)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product", product.Image);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    string extension = Path.GetExtension(fThumb.FileName);
                    productEdit.Image = Utilities.SEOUrl(productEdit.Name) + $"-{productEdit.Id}" + extension;
                    await Utilities.UploadFile(fThumb, @"product", productEdit.Image);

                }
                productEdit.UpdateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
                productEdit.UpdatedAt = DateTime.Now;
                _context.Products.Update(productEdit);
                await _context.SaveChangesAsync();
                _notyfService.Success("Cập nhật sản phẩm thành công");
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.BrandId = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            return View(product);
        }

        // GET: Product/Delete/Id
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(b => b.Category)
                .Include(b => b.Brand)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/Id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!ProductExists(id))
            {
                return Problem("Product is null!");
            }
            var product = _context.Products.Find(id);
           
            if (!String.IsNullOrEmpty(product.Image))
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product", product.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _notyfService.Success("Xóa sản phẩm thành công");
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
        ///////////////////////////////////////////
        [HttpPost]
        public FileResult ExportToExcel(string catid = "0", string brandid = "0", string limit = "all")
        {
            DataTable dt = new DataTable("product");
            dt.Columns.AddRange(new DataColumn[] {
                new DataColumn("Ma"),
                new DataColumn("Ten"),
                new DataColumn("So luong"),
                new DataColumn("Gia"),
                new DataColumn("Gia giam"),
                new DataColumn("Danh muc"),
                new DataColumn("Thuong hieu"),
                new DataColumn("Mo ta"),
                new DataColumn("Ngay nhap"),
            });

            var insuranceCertificate = _context.Products
                                        .Include(p => p.Category)
                                        .Include(p => p.Brand)
                                        .ToList();

            insuranceCertificate = catid != "0" ? insuranceCertificate.Where(p => p.CategoryId == catid).ToList() : insuranceCertificate;
            insuranceCertificate = brandid != "0" ? insuranceCertificate.Where(p => p.BrandId == brandid).ToList() : insuranceCertificate;
            if (limit != "all")
            {
                insuranceCertificate = limit == StatusConst.COMINGEND ? insuranceCertificate.Where(p => p.Quantity <= 5).ToList() : insuranceCertificate.Where(p => p.Quantity > 5).ToList();
            }

            foreach (var insurance in insuranceCertificate)
            {
                dt.Rows.Add(insurance.Code, insurance.Name, insurance.Quantity, insurance.Price, insurance.Discount, insurance.Category.Name, insurance.Brand.Name, insurance.Description, insurance.CreatedAt);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExcelFile.xlsx");
                }
            }
        }
    }
    /////////////////////////////////////////////
}

