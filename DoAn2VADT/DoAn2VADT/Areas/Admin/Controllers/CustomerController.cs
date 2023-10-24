using AspNetCoreHero.ToastNotification.Abstractions;
using DoAn2VADT.Database;
using DoAn2VADT.Database.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoAn2VADT.ViewModel;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LoginViewModel = DoAn2VADT.ViewModel.LoginViewModel;
using DoAn2VADT.Extension;
using System.ServiceProcess;
using Microsoft.AspNetCore.Authentication.Cookies;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.AspNetCore.Identity;
using PagedList.Core;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using AspNetCoreHero.ToastNotification.Notyf;
using DoAn2VADT.Shared;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace DoAn2VADT.Areas.Admin.Controllers
{

    public class CustomerController : GlobalController
    {
        public CustomerController(AppDbContext context, IHostingEnvironment environment, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<DashboardController> logger) : base(context, environment, notyfService, httpContextAccessor, logger)
        {

        }

        // GET: Account/Index
        [Authorize(Roles = "Admin")]
        public IActionResult Index(int? page, string role = "", string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 8;
            /* string id = HttpContext.User.Claims.FirstOrDefault(c => c.ValueType == "Id").Value;*/
            List<Customer> lsAccount = _context.Customers.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToList();
            
            PagedList<Customer> models = new PagedList<Customer>(lsAccount.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

       
        // GET: Account/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Account/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,Phone,Address,Password,Id,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Customer account)
        {
            if (ModelState.IsValid)
            {
                account.Id = Guid.NewGuid().ToString();
                account.Password = account.Password.ToMD5();
                account.CreatedAt = DateTime.Now;
                _context.Add(account);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm tài khoản thành công!");
                return RedirectToAction("Index", "Customer");
            }
            return View(account);
        }

        // GET: Account/Detail/Id
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var account = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Account/Edit/id
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var account = await _context.Customers.FindAsync(id);

            if (account == null)
            {
                return NotFound();
            }
           
            return View(account);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Name,Email,Phone,Address,Password,Id,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Customer account)
        {
            var AccountId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
            if (_context.Customers == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    account.Password = account.Password.ToMD5();
                    account.UpdatedAt = DateTime.Now;
                    account.UpdateUserId = AccountId;
                    _context.Update(account);
                    _notyfService.Success("Đã cập nhật tài khoản thành công!");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.Id.ToString()))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Customer");
            }
            return View(account);
        }
        // GET: Account/Delete/Id
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var account = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Account/Delete/Id
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Customers == null)
            {
                return Problem("Entity set 'AppDbContext.Accounts'  is null.");
            }
            var account = _context.Customers.Find(id);
            var check = _context.Orders.Any(o=>o.CustomerId == account.Id);
            if (check)
            {
                _notyfService.Success("Tài khoản này đã tồn tại đơn hàng. Xóa thành công!");
                return RedirectToAction(nameof(Index));
            }
            _context.Customers.Remove(account);
            await _context.SaveChangesAsync();
            _notyfService.Success("Đã xóa tài khoản thành công!");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public IActionResult ValidatePhone(Account acc)
        {
            try
            {
                var khachhang = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.UserName.ToLower() == acc.UserName.ToLower());
                if (khachhang != null)
                    return Json(data: "Tên ĐN : " + acc.UserName + "đã được sử dụng");

                return Json(data: true);

            }
            catch
            {
                return Json(data: true);
            }
        }
        private bool AccountExists(string id)
        {
            return _context.Customers.Any(e => e.Id.ToString() == id);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
