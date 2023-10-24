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

    public class AccountController : GlobalController
    {
        public AccountController(AppDbContext context, IHostingEnvironment environment, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<DashboardController> logger) : base(context, environment, notyfService, httpContextAccessor, logger)
        {

        }

        // GET: Account/Login
        [AllowAnonymous]
        public IActionResult Login()
        {

            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");

            }
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel usr)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            string pass = usr.Password.ToMD5();
            var u = _context.Accounts.SingleOrDefault(m => m.UserName == usr.UserName && m.Password == pass);
            if (u != null)
            {
                string admin = JsonConvert.SerializeObject(u); //json object->string
                //Identity
                var claims = new List<Claim>
                {
                      new Claim(ClaimTypes.Name, u.UserName),
                      new Claim(Const.ADMINIDSESSION, u.Id.ToString()),
                      new Claim(Const.ADMINSESSION, admin),
                      new Claim(ClaimTypes.Role, u.Role.ToString())
                 };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                };
                /*    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);*/
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    properties);

                _notyfService.Success("Đăng nhập thành công");
                return RedirectToAction("Index", "Dashboard");
            }


            _notyfService.Error("Tên tài khoản hoặc mật khẩu không chính xác!");
            return View();

        }


        // GET: Account/Logout
        [AllowAnonymous]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _notyfService.Success("Đăng xuất thành công");
            return RedirectToAction("Login", "Account");
        }


        // GET: Account/Index
        [Authorize(Roles = "Admin")]
        public IActionResult Index(int? page, string role = "", string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 8;
            /* string id = HttpContext.User.Claims.FirstOrDefault(c => c.ValueType == "Id").Value;*/
            string id = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
            List<Account> lsAccount = _context.Accounts.AsNoTracking().Where(x => x.Id != id).OrderBy(x => x.CreatedAt).ToList();
            if (role != "")
            {
                lsAccount = lsAccount.Where(a => a.Role == role).ToList();
            }
            if (searchkey != "")
            {
                ViewBag.SearchKey = searchkey;
                lsAccount = lsAccount.Where(a => a.Name.ToLower().Contains(searchkey.ToLower()) || a.UserName.ToLower().Contains(searchkey.ToLower())).ToList();
            }
            PagedList<Account> models = new PagedList<Account>(lsAccount.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Profile
        [Authorize]
        [Route("profile")]
        public async Task<IActionResult> Profile()
        {
            if (_context.Accounts == null)
            {
                return NotFound();
            }
            var admin = JsonConvert.DeserializeObject<Account>(HttpContext.Session.GetString(Const.ADMINSESSION));
            if (admin == null)
            {
                return NotFound();
            }
            return View(admin);
        }
        // GET: Account/ChangePassword
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult ChangePassword(ChangePassword model)
        {
            try
            {
                var id = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
                if (id == null)
                {
                    return RedirectToAction("Logout", "Account");
                }
                if (ModelState.IsValid)
                {
                    var taikhoan = _context.Accounts.Find(id);
                    if (taikhoan == null) return RedirectToAction("Login", "Account");
                    var pass = model.PasswordNow.Trim().ToMD5();
                    if (taikhoan.Password == pass)
                    {
                        string passnew = model.Password.Trim().ToMD5();
                        taikhoan.Password = passnew;
                        _context.Update(taikhoan);
                        _context.SaveChanges();
                        _notyfService.Success("Đổi mật khẩu thành công");
                    }
                    else
                    {
                        _notyfService.Error("Mật khẩu hiện tại không đúng");
                    }
                }
                else
                {
                    _notyfService.Error("Thông tin nhập sai");
                }
            }
            catch
            {
                _notyfService.Error("Thử lại");

            }
            return RedirectToAction("Profile", "Account");
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
        public async Task<IActionResult> Create([Bind("Name,UserName,Password,Role,Id,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Account account)
        {
            if (ModelState.IsValid)
            {
                account.Id = Guid.NewGuid().ToString();
                account.Password = account.Password.ToMD5();
                account.CreatedAt = DateTime.Now;
                account.UpdatedAt = DateTime.Now;
                _context.Add(account);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm tài khoản thành công!");
                return RedirectToAction("Index", "Account");
            }
            return View(account);
        }

        // GET: Account/Detail/Id
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(m => m.Id == id);
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
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound();
            }
            if (HttpContext.User.IsInRole("Admin"))
            {
                if (account.Id == User.FindFirstValue(Const.ADMINIDSESSION).ToString())
                {
                    ViewData["EditAccount"] = "EditProfileAdmin";
                }
                else
                {
                    ViewData["EditAccount"] = "EditAccountAdmin";
                }
            }
            else
            {
                ViewData["EditAccount"] = "EditProfileUser";
            }

            return View(account);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("UserName,Password,Name,Role,Id,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Account account)
        {
            var AccountId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
            if (_context.Accounts == null)
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
                return RedirectToAction("Index", "Account");
            }
            return View(account);
        }
        // GET: Account/Delete/Id
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
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
            if (_context.Accounts == null)
            {
                return Problem("Entity set 'AppDbContext.Accounts'  is null.");
            }
            var account = _context.Accounts.Find(id);
            _context.Accounts.Remove(account);
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
            return _context.Accounts.Any(e => e.Id.ToString() == id);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
