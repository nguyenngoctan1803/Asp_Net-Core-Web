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
using System.Security.Claims;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DoAn2VADT.Areas.Admin.Controllers
{
    [Authorize]
    public class OrderController : GlobalController
    {
        public OrderController(AppDbContext context, IHostingEnvironment environment, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<DashboardController> logger) : base(context,environment, notyfService, httpContextAccessor, logger)
        {

        }
        // GET: Order/Index?CatId0=&BrandId=0
        public IActionResult Index(int page = 1, string accid = "0", string sttcode = "0", string searchkey = "")
        {
            var pageNumber = page;
            var pageSize = 8;

            List<Order> lsOrders = new List<Order>();
            lsOrders = _context.Orders
               //.Where(i=>i.Status != StatusConst.DONE)
               .AsNoTracking()
               .Include(i => i.Account)
               .Include(i => i.OrderDetails)
               .Include(i => i.Customer)
               .OrderByDescending(x => x.CreatedAt).ToList();
            if (accid != "0")
            {
                ViewBag.CurrentAccId = accid;
                lsOrders = lsOrders.Where(x => x.Account.Id == accid).OrderByDescending(x => x.CreatedAt).ToList();
            }
            if (sttcode != "0")
            {
                ViewBag.CurrentStatusCode = sttcode;
                lsOrders = lsOrders.Where(x => x.Status == sttcode).OrderByDescending(x => x.CreatedAt).ToList();
            }
            if (searchkey != "")
            {
                ViewBag.SearchKey = searchkey;
                lsOrders = lsOrders
                            .Where(x => x.Name.ToLower().Contains(searchkey.ToLower()) || 
                                    x.Phone.ToLower().Contains(searchkey.ToLower()))
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
            foreach(var item in lsOrders)
            {
                if(item.Status == StatusConst.CANCEL)
                {
                    ViewData[item.Id] = true;
                }
                else
                {
                    ViewData[item.Id] = false;
                }
            }
            return View(models);
        }
        public IActionResult Filter(string accid = "0", string sttcode = "0")
        {
            var url = "/Order";
            if (accid != "0")
            {
                url += $"?accid={accid}";
            }
            if (sttcode != "0")
            {
                url += $"&sttcode={sttcode}";
            }

            return Redirect(url);
        }


        ///////////////////////////////////////////////
        // GET: Order/Detail/Id
        public IActionResult Details(string id)
        {
            if (id == null || _context.OrderDetails == null)
            {
                return NotFound();
            }
            var pageNumber = 1;
            var pageSize = 8;
            var orderDetail = _context.OrderDetails
                                .Include(i => i.Order)
                                .Include(i => i.Product)
                                .Include(i => i.Product.Brand)
                                .Include(i => i.Product.Category)
                                .Where(x => x.OrderId == id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            ViewBag.ProductList = new SelectList(_context.Products, "Id", "Name");
            PagedList<OrderDetail> models = new PagedList<OrderDetail>(orderDetail.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.Id = id;
            ViewBag.Status = _context.Orders.Find(id).Status;
            return View(models);
        }
       

        /// <summary>
        /// ////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        // GET: Order/Create
/*        public IActionResult Create()
        {
            return View();
        }*/
        /*[HttpPost]
        public async Task<IActionResult> CreateConfirm()
        {
            var orderSessions = GetOrderSession();
            string orderID = Guid.NewGuid().ToString();
            decimal? total = 0;
            foreach (var ord in orderSessions)
            {
                ord.OrderId = orderID;
                total += ord.Total;
                _context.OrderDetails.Add(ord);
            }
            string userID = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
            Order order = new Order
            {
                Id = orderID,
                Total = total,
                CreatedAt = DateTime.Now,
                CreateUserId = userID,
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _notyfService.Success("Đặt hàng thành công!");
            return RedirectToAction(nameof(Index));
        }*/
        // POST: Order/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Phone,Address,Discount,ShipFee,Status")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            var orderedit = _context.Orders.Find(id);
            if (ModelState.IsValid)
            {
                orderedit.Name = order.Name;
                orderedit.Phone = order.Phone;
                orderedit.Address = order.Address;
                orderedit.Discount = order.Discount;
                orderedit.ShipFee = order.ShipFee;
                orderedit.Total = orderedit.Total + orderedit.Discount - order.Discount + order.ShipFee - orderedit.ShipFee;
                orderedit.Status = order.Status;
                orderedit.UpdatedAt = DateTime.Now;
                orderedit.UpdateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
                _context.Orders.Update(orderedit);
                await _context.SaveChangesAsync();
                _notyfService.Success("Cập nhật thành công");
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // POST: Order/EditDetail/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDetail(string id, [Bind("Id,Quantity,ProductId")] OrderDetail orderDetail)
        {
            if (id != orderDetail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var orderDetailEdit = _context.OrderDetails.Find(id);
                orderDetailEdit.UpdatedAt = DateTime.Now;
                orderDetailEdit.UpdateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
                orderDetailEdit.Quantity = orderDetail.Quantity;
                orderDetailEdit.ProductId = orderDetail.ProductId;
                var product = _context.Products.Find(orderDetail.ProductId);
                orderDetailEdit.Total = product.Discount != null ? (product.Price - product.Discount) * orderDetail.Quantity : product.Price * orderDetail.Quantity;

                Order order = await _context.Orders.FindAsync(orderDetailEdit.OrderId);
                order.Total = await _context.OrderDetails.Where(x=> x.OrderId == order.Id && x.Id != id).SumAsync(x => x.Total) + orderDetailEdit.Total- order.Discount + order.ShipFee;
                order.UpdatedAt = DateTime.Now;
                order.UpdateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
                _context.OrderDetails.Update(orderDetailEdit);
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                _notyfService.Success("Cập nhật thành công");
                return RedirectToAction("Details", new {id = order.Id});
            }
            return View(orderDetail);
        }

        // Delete Order
        // GET: Order/Cancel/Id
        public async Task<IActionResult> Cancel(string id)
        {
            if (id == null || _context.OrderDetails == null)
            {
                return NotFound();
            }
            var pageNumber = 1;
            var pageSize = 8;
            var orderDetail = _context.OrderDetails
                                .Include(i => i.Order)
                                .Include(i => i.Product)
                                .Include(i => i.Product.Brand)
                                .Include(i => i.Product.Category)
                                .Where(x => x.OrderId == id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            PagedList<OrderDetail> models = new PagedList<OrderDetail>(orderDetail.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.Id = id;
            return View(models);
        }


        // POST: Order/Cancel/Id
        [HttpPost, ActionName("Cancel")]
        public async Task<IActionResult> CancelConfirm(ReasonView rs)
        {
            if (rs.Id == null ||_context.Orders == null)
            {
                return Problem("Entity set 'AppDbContext.Orders'  is null.");
            }
            var order = _context.Orders.Find(rs.Id);
            order.Status = StatusConst.CANCEL;
            if (rs.Reason == Const.REFUSEREASON)
            {
                order.Reason = rs.RefuseReason;
            }
            else
            {
                order.Reason = rs.Reason;
            }
            order.UpdatedAt = DateTime.Now;
            order.UpdateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
            var orderDetails = _context.OrderDetails.Where(o => o.OrderId == rs.Id).ToList();
            foreach (var item in orderDetails)
            {
                var product = _context.Products.Find(item.ProductId);
                product.Quantity = product.Quantity + item.Quantity;
                _context.Products.Update(product);
            }
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            _notyfService.Success("Hủy đơn hàng thành công");
            return RedirectToAction(nameof(Index));
        }

        // POST: Order/Delete/Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'AppDbContext.Orders'  is null.");
            }
            var order = _context.Orders.Find(id);
            await _context.OrderDetails.Where(x => x.OrderId == id).ForEachAsync(i =>
            {
                _context.OrderDetails.Remove(i);
            });
            await _context.SaveChangesAsync();
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            _notyfService.Success("Xóa đơn hàng thành công");
            return RedirectToAction(nameof(Index));
        }

        // POST: Order/DeleteOrderDetail/Id
        [HttpPost, ActionName("DeleteDetail")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDetail(string deleteid)
        {
            if (_context.OrderDetails == null)
            {
                return Problem("Entity set 'AppDbContext.OrderDetails'  is null.");
            }
            var orderdetail = await _context.OrderDetails.FindAsync(deleteid);
            _context.OrderDetails.Remove(orderdetail);
            Order order = await _context.Orders.FindAsync(orderdetail.OrderId);
            order.Total = await _context.OrderDetails.Where(x => x.OrderId == order.Id && x.Id != deleteid).SumAsync(x => x.Total) - order.Discount + order.ShipFee;
            order.UpdatedAt = DateTime.Now;
            order.UpdateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
            string notify = "";
            if(order.Total == 0)
            {
                await _context.SaveChangesAsync();
                order.Status = Shared.StatusConst.CANCEL;
                _context.Orders.Update(order);
                notify = "Đơn hàng trống! Đã hủy đơn hàng";
            }
            else
            {
                _context.Orders.Update(order);
                notify = "Xóa thành công! Đơn hàng đã được cập nhật";
            }
          
            await _context.SaveChangesAsync();

            _notyfService.Success(notify);
            return RedirectToAction("Details", new {id= order.Id});
        }

        // Xử lí đơn hàng
        public async Task<IActionResult> Skip(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var order = await _context.Orders.FindAsync(id);
            string notify = "";
            switch (order.Status)
            {
                case Shared.StatusConst.WAITCONFIRM:
                    order.Status = Shared.StatusConst.CONFIRMED;
                    notify = "Đã xác nhận đơn hàng!";
                    break;
                case (Shared.StatusConst.CONFIRMED):
                    order.Status = Shared.StatusConst.EXPORT;
                    notify = "Đã đóng gói! Hãy in hóa đơn và xuất đơn";
                    break;
                case (Shared.StatusConst.EXPORT):
                    break;
                case Shared.StatusConst.EXPORTED:
                    order.Status = Shared.StatusConst.SHIPPING;
                    order.ShipDate = DateTime.Now;
                    notify = "Đã chuyển cho đơn vị giao hàng!";
                    break;
                case Shared.StatusConst.SHIPPING:
                    if(order.PayWay == Shared.PayConst.ONLINE)
                    {
                        order.Status = Shared.StatusConst.PAID;
                        order.ReceiveDate = DateTime.Now;
                        notify = "Đã nhận hàng thành công! Đơn hàng đã được thanh toán MoMo trước đó";
                        break;
                    }
                    else
                    {
                        order.Status = Shared.StatusConst.RECEIVE;
                        order.ReceiveDate = DateTime.Now;
                        notify = "Đã nhận hàng thành công! Hãy xác nhận thanh toán";
                        break;
                    }
                case Shared.StatusConst.RECEIVE:
                    order.Status = Shared.StatusConst.PAID;
                    order.PayStatus = PayStatusConst.DONE;
                    notify = "Xác nhận thanh toán thành công!";
                    break;
                case Shared.StatusConst.PAID:
                    order.Status = Shared.StatusConst.DONE;
                    notify = "Đơn hàng thành công!";
                    break;
                default:
                    break;

            }
            order.UpdateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            _notyfService.Success(notify);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> ConfirmPay(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var order = await _context.Orders.FindAsync(id);
            string notify = "";
            switch (order.Status)
            {
                case Shared.StatusConst.WAITCONFIRM:
                    notify = "Đơn hàng chưa được xác nhận!";
                    _notyfService.Warning(notify);
                    break;
                case (Shared.StatusConst.CONFIRMED):
                case Shared.StatusConst.EXPORT:
                case Shared.StatusConst.EXPORTED:
                case Shared.StatusConst.SHIPPING:
                    notify = "Đơn hàng chưa được giao thành công!";
                    _notyfService.Warning(notify);
                    break;
                case Shared.StatusConst.RECEIVE:
                    order.Status = Shared.StatusConst.PAID;
                    order.UpdatedAt = DateTime.Now;
                    order.UpdateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                    notify = "Xác nhận thanh toán thành công!";
                    _notyfService.Success(notify);
                    break;
                default:
                    break;
            }
            return RedirectToAction("Details", new {id = id});
        }
        // Admin/Order/GetBill -> export bill
        [HttpPost]
        public IActionResult GetBill(string id)
        {
            var order = _context.Orders.Find(id);
            order.Status = StatusConst.EXPORTED;
            order.UpdatedAt = DateTime.Now;
            order.UpdateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
            _context.Orders.Update(order);
            _context.SaveChanges();
            order.OrderDetails = _context.OrderDetails.Where(o => o.OrderId == id).Include(o => o.Product).ToList();
            return PartialView("BillPartial", order);
        }
        // Xuất hóa đơn
        public IActionResult ExportBill(string id, string option = "excel")
        {
            var order = _context.Orders.Find(id);
            string timestamp = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToUpper().Replace(':', '_').Replace('.', '_').Replace(' ', '_').Trim();
            var templateFileInfo = new FileInfo(Path.Combine(_environment.ContentRootPath, "Template", "BillTemplate.xlsx"));
            var orderDetails = _context.OrderDetails.Include(x=>x.Product).Where(x => x.OrderId == order.Id).ToList();
            var stream = ExportToExcelHelper.UpdateDataIntoExcelTemplate(orderDetails,order, templateFileInfo);
            
            if(option == "pdf")
            {
                Workbook workbook = new Workbook(stream);

                Aspose.Cells.PdfSaveOptions opts = new Aspose.Cells.PdfSaveOptions();
                opts.AllColumnsInOnePagePerSheet = true;
                opts.OptimizationType = Aspose.Cells.Rendering.PdfOptimizationType.MinimumSize;

                MemoryStream msPdf = new MemoryStream();
                workbook.Save(msPdf, opts);
                msPdf.Seek(0, SeekOrigin.Begin);

                byte[] buffer = new byte[msPdf.Length];
                buffer = msPdf.ToArray();
                return File(buffer, "application/pdf", "Bill-" + timestamp + ".pdf");
            }
            //excel
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Bill-" + timestamp + ".xlsx");
        }
        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.Id.ToString() == id);
        }
    }
}
