using AspNetCoreHero.ToastNotification.Abstractions;
using DoAn2VADT.Database;
using DoAn2VADT.Database.Entities;
using DoAn2VADT.Shared;
using DoAn2VADT.ViewModel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Vml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using PagedList.Core;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace DoAn2VADT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ImportController : GlobalController
    {
        public ImportController(AppDbContext context, IHostingEnvironment environment, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<DashboardController> logger) : base(context, environment, notyfService, httpContextAccessor, logger)
        {

        }

        // GET Import/Index
        public IActionResult Index(int page = 1, string accid = "0", string searchkey = "")
        {
            var pageNumber = page;
            var pageSize = 8;

            var lsImports = _context.Imports
               .AsNoTracking()
               .Include(i => i.Account)
               .OrderByDescending(x => x.CreatedAt).ToList();
            if (accid != "0")
            {
                ViewBag.CurrentAccId = accid;
                lsImports = lsImports.Where(x => x.Account.Id == accid).OrderByDescending(x => x.CreatedAt).ToList();
            }
            if (searchkey != "")
            {
                ViewBag.SearchKey = searchkey;
                lsImports = lsImports.Where(x => x.Account.Name.ToLower().Contains(searchkey.ToLower())).ToList();
            }
            PagedList<Import> models = new PagedList<Import>(lsImports.AsQueryable(), pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.AccountId = new SelectList(_context.Accounts, "Id", "Name");
            return View(models);
        }

        ///////////////////////////////////////////////
        // GET: Import/Detail/Id
        public IActionResult Details(string id)
        {
            if (id == null || _context.ImportDetails == null)
            {
                return NotFound();
            }
            var pageNumber = 1;
            var pageSize = 8;
            var importDetail = _context.ImportDetails
                                .Include(i => i.Import)
                                .Include(i => i.Product)
                                .Include(i => i.Product.Brand)
                                .Include(i => i.Product.Category)
                                .Where(x => x.ImportId == id);
            if (importDetail == null)
            {
                return NotFound();
            }
            PagedList<ImportDetail> models = new PagedList<ImportDetail>(importDetail.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // Nhập kho
        void SaveImportToSession(List<ImportDetail> ls)
        {
            var session = HttpContext.Session;
            string jsonImport = JsonConvert.SerializeObject(ls);
            session.SetString(Const.IMPORTSESSION, jsonImport);
        }

        // Lấy Order từ Session
        List<ImportDetail> GetImportSession()
        {
            var session = HttpContext.Session;
            string jsonImport = session.GetString(Const.IMPORTSESSION);
            if (jsonImport != null)
            {
                return JsonConvert.DeserializeObject<List<ImportDetail>>(jsonImport).ToList();
            }
            return new List<ImportDetail>();
        }

        // Xóa Order khỏi session
        void ClearImportSession()
        {
            var session = HttpContext.Session;
            session.Remove(Const.IMPORTSESSION);
        }

        public IActionResult AddImport()
        {
            return View();
        }
        // Lưu ImportDetail vào session
        public IActionResult AddImportSession(ImportDetail imp)
        {
            imp.Id = Guid.NewGuid().ToString();
            imp.Total = imp.PriceIn * imp.Quantity;

            // Xử lý đưa vào Session ...
            var imports = GetImportSession();
            var checkExist = imports.Any(p => p.ProductId == imp.ProductId);
            if (checkExist)
            {
                // Đã tồn tại, tăng thêm số lượng
                imports.FirstOrDefault(p => p.ProductId == imp.ProductId).Quantity += imp.Quantity;
            }
            else
            {
                //  Thêm mới
                imports.Add(imp);
            }
            // Lưu Import vào Session
            SaveImportToSession(imports);
            // Chuyển đến trang hiện thị Order đã lưu vào Session
            return RedirectToAction(nameof(Create));
        }
        // Xóa ImportSession
        public IActionResult RemoveImport(string id)
        {
            var imports = GetImportSession();
            var checkExist = imports.Any(p => p.Id == id);
            if (checkExist)
            {
                imports.Remove(imports.FirstOrDefault(x => x.Id == id));
            }

            SaveImportToSession(imports);
            return RedirectToAction(nameof(Create));
        }
        // GET: Import/Create
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateConfirm()
        {
            var importSessions = GetImportSession();
            string importID = Guid.NewGuid().ToString();
            decimal? total = 0;
            foreach (var imp in importSessions)
            {
                imp.ImportId = importID;
                total += imp.Total;
                _context.ImportDetails.Add(imp);
            }
            string userID = HttpContext.Session.GetString(Const.USERIDSESSION);
            Import import = new Import
            {
                Id = importID,
                Total = total,
                CreatedAt = DateTime.Now,
                CreateUserId = userID,
            };
            _context.Imports.Add(import);
            await _context.SaveChangesAsync();

            _notyfService.Success("Nhập hàng thành công!");
            return RedirectToAction(nameof(Index));
        }


        // Chỉnh sửa ImportDetail
        // GET: Import/Edit/Id
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ImportDetails == null)
            {
                return NotFound();
            }
            var importDetail = await _context.ImportDetails.FindAsync(id);
            if (importDetail == null)
            {
                return NotFound();
            }
            return View(importDetail);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,PriceIn,Quantity,ProductId")] ImportDetail importDetail)
        {
            if (id != importDetail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                importDetail.UpdatedAt = DateTime.Now;
                importDetail.UpdateUserId = HttpContext.Session.GetString(Const.USERIDSESSION);
                importDetail.Total = importDetail.PriceIn * importDetail.Quantity;
                Import import = await _context.Imports.FindAsync(importDetail.ImportId);
                import.Total = await _context.ImportDetails.SumAsync(x => x.Total);
                import.UpdatedAt = DateTime.Now;
                import.UpdateUserId = HttpContext.Session.GetString(Const.USERIDSESSION);
                _context.Update(importDetail);
                _context.Update(import);
                await _context.SaveChangesAsync();
                _notyfService.Success("Cập nhật thành công");
                return RedirectToAction(nameof(Details));
            }
            return View(importDetail);
        }
        // Delete Import
        // GET: Import/DeleteImport/Id
        public async Task<IActionResult> DeleteImport(string id)
        {
            if (id == null || _context.Imports == null)
            {
                return NotFound();
            }

            var import = await _context.Imports.FirstOrDefaultAsync(m => m.Id == id);
            if (import == null)
            {
                return NotFound();
            }

            return View(import);
        }


        // POST: Import/DeleteImport/Id
        [HttpPost, ActionName("DeleteImport")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImportConfirmed(string id)
        {
            if (_context.Imports == null)
            {
                return Problem("Entity set 'AppDbContext.Orders'  is null.");
            }
            var import = _context.Imports.Find(id);
            _context.Imports.Remove(import);
            await _context.SaveChangesAsync();

            _notyfService.Success("Xóa thành công");
            return RedirectToAction(nameof(Index));
        }

        // Delete ImportDetail
        // GET: Import/DeleteImportDetail/Id
        public async Task<IActionResult> DeleteImportDetail(string id)
        {
            if (id == null || _context.ImportDetails == null)
            {
                return NotFound();
            }

            var importDetail = await _context.ImportDetails.FirstOrDefaultAsync(m => m.Id == id);
            if (importDetail == null)
            {
                return NotFound();
            }

            return View(importDetail);
        }


        // POST: Import/DeleteImportDetail/Id
        [HttpPost, ActionName("DeleteImportDetail")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImportDetailConfirmed(string id)
        {
            if (_context.Imports == null)
            {
                return Problem("Entity set 'AppDbContext.Orders'  is null.");
            }
            var import = _context.Imports.Find(id);
            _context.Imports.Remove(import);
            await _context.SaveChangesAsync();

            _notyfService.Success("Xóa thành công");
            return RedirectToAction(nameof(Details));
        }

        private bool ImportExists(string id)
        {
            return _context.ImportDetails.Any(e => e.Id == id);
        }


        /*[HttpPost]
        public async Task<IActionResult> ImportfromExcel([FromForm] IFormFile file)
        {
            *//*if (file == null)
            {
                _notyfService.Error("Vui lòng chọn file trước khi nhập");
                return View();
            }
            var import = new Import();
            var getId = HttpContext.Session.GetString("Id");
            var nameUser = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.Id == getId);
            import.Name = nameUser.Name;
            import.CreatedAt = DateTime.Now;
            _context.Add(import);
            await _context.SaveChangesAsync();
            try
            {
                using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets[0];
                    var rowcount = ws.Dimension.Rows;
                        int rowccheck = 2;
                    for (int row = 2; row <= rowcount; row++)
                    {
                            if (ws.Cells[row, 1].Value != null && ws.Cells[row, 2].Value != null && ws.Cells[row, 3].Value != null && ws.Cells[row, 4].Value != null)
                            {
                                if (_context.Products.Where(n => n.Name == ws.Cells[row, 1].Value.ToString().Trim()).FirstOrDefault() == null)
                                {
                                    ImportDetail imd = new ImportDetail()
                                    {
                                        Name= "",
                                        ImportId = import.Id,
                                        Quantity = int.Parse(ws.Cells[row, 2].Value.ToString().Trim()),
                                        PriceIn = int.Parse(ws.Cells[row, 3].Value.ToString().Trim()),                             
                                    };
                                    _context.Add(imd);
                                    await _context.SaveChangesAsync();
                                    var b = new Product()
                                    {
                                        Name = ws.Cells[row, 1].Value.ToString().Trim(),
                                        Quantity = int.Parse(ws.Cells[row, 2].Value.ToString().Trim()),
                                        Price = int.Parse(ws.Cells[row, 3].Value.ToString().Trim()),
                                        Discount = int.Parse(ws.Cells[row, 4].Value.ToString().Trim())
                                    };
                                    _context.Add(b);
                                    await _context.SaveChangesAsync();
                                    rowccheck++;
                                }
                                else
                                {
                                    ImportDetail imd = new ImportDetail()
                                    {
                                        Name = "",
                                        ImportId = import.Id,
                                      
                                        Quantity = int.Parse(ws.Cells[row, 2].Value.ToString().Trim()),
                                        PriceIn = int.Parse(ws.Cells[row, 3].Value.ToString().Trim()),
                  

                                    };
                                    _context.Add(imd);
                                    await _context.SaveChangesAsync();
                                    var b = _context.Products.Where(n => n.Name == ws.Cells[row, 1].Value.ToString().Trim()).FirstOrDefault();
                                    b.Quantity += int.Parse(ws.Cells[row, 2].Value.ToString().Trim());
                                    b.Price = int.Parse(ws.Cells[row, 3].Value.ToString().Trim());
                                    b.Discount = int.Parse(ws.Cells[row, 4].Value.ToString().Trim());
                                    _context.Update(b);
                                    await _context.SaveChangesAsync();
                                    rowccheck++;
                                }
                            }
                            else
                            {
                                var tt = _context.ImportDetails.Where(x => x.ImportId == import.Id);
               *//*                 var totalt = tt.Sum((x => x.Price*x.Quanlity));*/
        /*  import.Total = (int)totalt;*//*
          _context.Update(import);
          await _context.SaveChangesAsync();
          _notyfService.Error("lỗi dữ liệu ở dòng thứ "+rowccheck+", những dòng trước đó đã được thêm vào!");
          return RedirectToAction("Index", "Book");
      }
}
}
var t = _context.ImportDetails.Where(x => x.ImportId == import.Id);
*//* var total = t.Sum((x => x.Price*x.Quanlity));
 import.Total = (int)total;*//*
 _context.Update(import);
 await _context.SaveChangesAsync();
 _notyfService.Success("Nhập dữ liệu thành công");
return RedirectToAction("Index", "Book");
}
}
catch (Exception ex)
{
_notyfService.Error("Nhập không thành công, vui lòng xem lại: " + ex.Message);
return RedirectToAction(nameof(Index));
}*//*
}*/


        ///////////////////////////////////////////

    }
}
