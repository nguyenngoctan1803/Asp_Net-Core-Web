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
using AspNetCoreHero.ToastNotification.Notyf;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagedList.Core;
using DoAn2VADT.ViewModel;
using DoAn2VADT.Extension;
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using System.Diagnostics.Metrics;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace DoAn2VADT.Areas.Admin.Controllers
{
    [Authorize]
    public class DashboardController : GlobalController

    {
        public DashboardController(AppDbContext context, IHostingEnvironment environment, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<DashboardController> logger) : base(context, environment, notyfService, httpContextAccessor, logger)
        {
            
        }

        // GET: Dashboard/Index
        public IActionResult Index(int? page)
        {
            using (_context)
            {
                ViewBag.Orders = _context.Orders.OrderBy(o => o.CreatedAt).ToList();//đơn hàng
                ViewBag.Categories = _context.Categories.OrderBy(o => o.CreatedAt).ToList();//danh mục
                ViewBag.Brands = _context.Brands.OrderBy(o => o.CreatedAt).ToList();//thương hiệu
                ViewBag.Customers = _context.Customers.OrderBy(o => o.CreatedAt).ToList();//khách hàng
                ViewBag.Products = _context.Products.OrderBy(o => o.CreatedAt).ToList();//sản phẩm

                ViewBag.SumOrToTal = (from b in _context.Orders select b.Total).Sum(); //tổng tiền bán
                ViewBag.SumInTotal = (from b in _context.Imports select b.Total).Sum(); // tổng tiền nhập
                ViewBag.Revenue = ViewBag.SumOrToTal - ViewBag.SumInTotal; // lợi nhuận


                var TopOrder = _context.OrderDetails.OrderByDescending(o => o.ProductId).ToList().Take(5);
                var model = new List<Product>(); // sản phẩm bán chạy
                foreach (var order in TopOrder)
                {
                    model.Add(_context.Products.Where(p => p.Id == order.ProductId).FirstOrDefault());
                }
                return View(model);
            }

        }

    }
}
