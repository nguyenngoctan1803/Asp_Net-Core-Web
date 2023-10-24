using AspNetCoreHero.ToastNotification.Abstractions;
using DoAn2VADT.Areas.Admin.Controllers;
using DoAn2VADT.Controllers;
using DoAn2VADT.Database;
using DoAn2VADT.Database.Entities;
using DoAn2VADT.Shared;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DoAn2VADT.Extension;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using LoginViewModel = DoAn2VADT.ViewModel.LoginViewModel;

namespace DoAn2VADT.Controllers
{
    public class HomeController : GlobalController
    {
        public HomeController(AppDbContext _context, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<HomeController> logger) : base(_context, notyfService, httpContextAccessor, logger)
        {
            
        }
        public IActionResult Index()
        {
            var products = _context.Products.Where(p=>p.Effective == true).Take(4).ToList();
            return View(products);
        }
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(Contact contact)
        {
            contact.Id = Guid.NewGuid().ToString();
            contact.CreatedAt = DateTime.Now;
            
            _context.Contacts.Add(contact);
            _context.SaveChanges();
            _notyfService.Success("Cảm ơn bạn đã gửi thông tin liên hệ đến chúng tôi");
            return View();
        }
        public IActionResult Login()
        {
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString(Const.USERIDSESSION)))
            {
                return Redirect("~/");
            }
            ViewBag.Error = "";
            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            string phone = login.UserName;
            string pass = login.Password.ToString().ToMD5();
            Customer acc = _context.Customers.FirstOrDefault(x => x.Phone == phone && x.Password != null);
            if (acc == null)
            {
                _notyfService.Error("Tài khoản không tồn tại");
            }
            else
            {
                if (acc.Password.Equals(pass))
                {
                    // đăng nhập đúng
                    string cus = JsonConvert.SerializeObject(acc); //json object->string

                    HttpContext.Session.SetString(Const.USERIDSESSION, acc.Id.ToString());
                    HttpContext.Session.SetString(Const.USERSESSION, cus);
                    HttpContext.Session.SetString(Const.CARTSESSION, acc.Id.ToString());
                    
                    _notyfService.Success("Đăng nhập thành công!");
                    return Redirect("~/");
                }
                else
                {
                    _notyfService.Error("Mật khẩu không chính xác");
                }
            }
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(Const.USERIDSESSION);
            HttpContext.Session.Remove(Const.USERSESSION);
            HttpContext.Session.SetString(Const.CARTSESSION, Guid.NewGuid().ToString());
            _notyfService.Success("Đăng xuất thành công");
            return Redirect("~/");
        }
        public IActionResult Register()
        {
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString(Const.USERIDSESSION)))
            {
                return Redirect("~/");
            }
            ViewBag.Error = "";
            return View();
        }
        [HttpPost]
        public IActionResult Register([Bind("Name, Phone,Email,Address,Password")] Customer customer)
        {
            customer.Password = customer.Password.ToMD5();
            HttpContext.Session.SetString(Const.CARTSESSION, String.IsNullOrEmpty(HttpContext.Session.GetString(Const.CARTSESSION).ToString()) ? Guid.NewGuid().ToString() : HttpContext.Session.GetString(Const.CARTSESSION).ToString());
            customer.Id = HttpContext.Session.GetString(Const.CARTSESSION).ToString();
            var check_phone = _context.Customers.FirstOrDefault(x => x.Phone == customer.Phone && x.Password != null);
            if (check_phone != null)
            {
                _notyfService.Error("Số điện thoại đã được đăng ký!");
                return View();
            }
            else
            {
                _context.Customers.Add(customer);
                _context.SaveChanges();
                _notyfService.Success("Đăng ký thành công!");
                return RedirectToAction("Login", "Home");
            }

        }
        public IActionResult Profile()
        {
            if (String.IsNullOrEmpty(HttpContext.Session.GetString(Const.USERIDSESSION)))
            {
                return Redirect("~/");
            }
            Customer cus = JsonConvert.DeserializeObject<Customer>(HttpContext.Session.GetString(Const.USERSESSION).ToString());
            return View(cus);
        }

        /// <summary>
        /// Menu render ở đây
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public IActionResult GetMenu()
        {
            var categories = _context.Categories.ToList();
            var brands = _context.Brands.ToList();
            var menu = new Menu()
            {
                Categories = categories,
                Brands = brands,
            };
            return PartialView("_MenuBar", menu);
        }
        public IActionResult Error()
        {
            return View();
        }
    }

}

