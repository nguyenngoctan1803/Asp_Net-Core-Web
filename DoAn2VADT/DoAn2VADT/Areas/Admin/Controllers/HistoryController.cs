using AspNetCoreHero.ToastNotification.Abstractions;
using DoAn2VADT.Database;
using DoAn2VADT.Database.Entities;
using DoAn2VADT.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PagedList.Core;
using System.Globalization;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using DoAn2VADT.Helpper;
using Aspose.Cells;
using DoAn2VADT.ViewModel;
using System.Net;
using DoAn2VADT.OnlinePayment;
using Newtonsoft.Json.Linq;

namespace DoAn2VADT.Areas.Admin.Controllers
{
    [Authorize(Roles ="Admin")]
    public class HistoryController : GlobalController
    {
        public HistoryController(AppDbContext context, IHostingEnvironment environment, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<HistoryController> logger) : base(context, environment, notyfService, httpContextAccessor, logger)
        {

        }
        // History
        public IActionResult Index(int page = 1, string accid = "0", string searchkey = "")
        {
            var pageNumber = page;
            var pageSize = 8;

            List<Order> lsOrders = new List<Order>();
            lsOrders = _context.Orders
               .Where(i => i.Status == StatusConst.DONE)
               .AsNoTracking()
               .Where(i=>i.UpdateUserId != null)
               .Include(i => i.Account)
               .Include(i => i.OrderDetails)
               .Include(i => i.Customer)
               .OrderByDescending(x => x.CreatedAt).ToList();
            if (accid != "0")
            {
                ViewBag.CurrentAccId = accid;
                lsOrders = lsOrders.Where(x => x.Account.Id == accid).OrderByDescending(x => x.CreatedAt).ToList();
            }

            if (searchkey != "")
            {
                ViewBag.SearchKey = searchkey;
                lsOrders = lsOrders
                            .Where(x => x.Name.ToLower().Contains(searchkey.ToLower()) ||
                                    x.Phone.ToLower().Contains(searchkey.ToLower()) ||
                                    x.Account.Name.ToLower().Contains(searchkey.ToLower()))
                            .ToList();
            }

            PagedList<Order> models = new PagedList<Order>(lsOrders.AsQueryable(), pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.AccountId = new SelectList(_context.Accounts, "Id", "Name");
            ViewBag.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Chờ xác nhận", Value = StatusConst.WAITCONFIRM},
                new SelectListItem { Text = "Đã xác nhận", Value = StatusConst.CONFIRMED},
                new SelectListItem { Text = "Đóng gói", Value = StatusConst.EXPORT},
                new SelectListItem { Text = "Đã xuất đơn", Value = StatusConst.EXPORTED},
                new SelectListItem { Text = "Đang giao", Value = StatusConst.SHIPPING},
                new SelectListItem { Text = "Đã nhận", Value = StatusConst.RECEIVE},
                new SelectListItem { Text = "Đã thanh toán", Value = StatusConst.PAID},
                new SelectListItem { Text = "Hoàn thành", Value = StatusConst.DONE},
                new SelectListItem { Text = "Đã hủy", Value = StatusConst.CANCEL},
            };
            return View(models);
        }
    }
}
