using AspNetCoreHero.ToastNotification.Abstractions;
using DoAn2VADT.Database;
using DoAn2VADT.Database.Entities;
using DoAn2VADT.Shared;
using DoAn2VADT.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Net;
using PagedList.Core;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoAn2VADT.Controllers
{
    public class OrderController : GlobalController
    {
        public OrderController(AppDbContext _context, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<OrderController> logger) : base(_context, notyfService, httpContextAccessor, logger)
        {

        }
        public IActionResult Index(int page = 1)
        {
            var pageNumber = page;
            var pageSize = 8;

            var cusId = HttpContext.Session.GetString(Const.CARTSESSION).ToString();
            var orders = _context.Orders.Where(x => x.CustomerId == cusId).Include(x => x.Customer).Include(x=>x.OrderDetails).ToList();
            ViewData["StatusUpdate"] = new List<string>()
            {
                StatusConst.WAITCONFIRM,
                StatusConst.CONFIRMED,
                StatusConst.EXPORT
            };
            ViewData["StatusCancel"] = new List<string>()
            {
                StatusConst.WAITCONFIRM,
                StatusConst.CONFIRMED
            };
            PagedList<Order> models = new PagedList<Order>(orders.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
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
            ViewData["SubStatus"] = returnListStatus(orders);
            return View(models);
        }

        // Render Status
        public List<SubStatus> returnListStatus(List<Order> listorder)
        {
            List <SubStatus>  subStatus = new List<SubStatus>();
            foreach(var item in listorder){
                switch (item.Status)
                {
                    case StatusConst.WAITCONFIRM:
                        subStatus.Add(new SubStatus()
                        {
                            Code = item.Status,
                            SubString = "Chờ xác nhận",
                            Color = "dark",
                        });
                        break;
                    case StatusConst.CONFIRMED:
                    case StatusConst.EXPORT:
                    case StatusConst.EXPORTED:
                        subStatus.Add(new SubStatus()
                        {
                            Code = item.Status,
                            SubString = "Đã xác nhận",
                            Color = "warning",
                        });
                        break;
                    case StatusConst.SHIPPING:
                        subStatus.Add(new SubStatus()
                        {
                            Code = item.Status,
                            SubString = "Đang giao",
                            Color = "info",
                        });
                        break;
                    case StatusConst.RECEIVE:
                    case StatusConst.PAID:
                    case StatusConst.DONE:
                        subStatus.Add(new SubStatus()
                        {
                            Code = item.Status,
                            SubString = "Đã nhận",
                            Color = "success",
                        });
                        break;
                    case StatusConst.CANCEL:
                        subStatus.Add(new SubStatus()
                        {
                            Code = item.Status,
                            SubString = "Đã hủy",
                            Color = "danger",
                        });
                        break;
                    default: break;

                }
            }
            return subStatus;
        }
        // GET: Order/Details/5
        public IActionResult Details(string id,int page = 1)
        {
            var pageNumber = page;
            var pageSize = 8;

            if (id == null)
            {
                return NotFound();
            }
            var order = _context.Orders.Find(id);
            var orderDetails = _context.OrderDetails
                .Where(x => x.OrderId == id)
                .Include(x => x.Order)
                .Include(x => x.Product.Brand)
                .Include(x => x.Product.Category)
                .ToList();
            if (orderDetails == null)
            {
                return NotFound();
            }
            PagedList<OrderDetail> models = new PagedList<OrderDetail>(orderDetails.AsQueryable(), pageNumber, pageSize);
            
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Order/Edit/5
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Order order = _context.Orders.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, [Bind("Id,Name,Phone, Address")] Order order)
        {
            if (id != order.Id.ToString())
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var orderEdit = _context.Orders.Find(id);
                var listUpdate = new List<string>()
                {
                    StatusConst.WAITCONFIRM,
                    StatusConst.CONFIRMED,
                    StatusConst.EXPORT
                };
                if (listUpdate.Contains(orderEdit.Status))
                {
                    orderEdit.UpdatedAt = DateTime.Now;
                    orderEdit.Name = order.Name;
                    orderEdit.Phone = order.Phone;
                    orderEdit.Address = order.Address;

                    _context.Orders.Update(orderEdit);
                    _context.SaveChanges();
                    _notyfService.Success("Đã cập nhật đơn hàng");
                    return RedirectToAction("Index");
                }
                else
                {
                    _notyfService.Error("Đơn hàng của bạn đã được xuất vừa song, vui lòng kiểm tra lại!");
                    return RedirectToAction("Index");
                }

            }
            return BadRequest();
        }
        // POST: Order/Cancel
        [HttpPost]
        public async Task<IActionResult> Cancel(ReasonView rs)
        {
            if (rs.Id == null || _context.Orders == null)
            {
                return Problem("Entity set 'AppDbContext.Orders'  is null.");
            }
            var order = _context.Orders.Find(rs.Id);
            var listCancel = new List<string>()
                {
                    StatusConst.WAITCONFIRM,
                    StatusConst.CONFIRMED
                };
            if (listCancel.Contains(order.Status))
            {
                order.Status = StatusConst.CANCEL;
                order.UpdatedAt= DateTime.Now;
                if(rs.Reason == Const.REFUSEREASON)
                {
                    order.Reason = rs.RefuseReason;
                }
                else
                {
                    order.Reason = rs.Reason;
                }
                _context.Orders.Update(order);
                // Cập nhật lại số lượng sản phẩm
                var orderDetails = _context.OrderDetails.Where(o => o.OrderId == rs.Id).ToList();
                foreach (var item in orderDetails)
                {
                    var product = _context.Products.Find(item.ProductId);
                    product.Quantity = product.Quantity + item.Quantity;
                    _context.Products.Update(product);
                }
               
                await _context.SaveChangesAsync();

                _notyfService.Success("Hủy đơn hàng thành công");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _notyfService.Error("Đơn hàng của bạn đã được đóng gói vừa song, vui lòng kiểm tra lại!");
                return RedirectToAction("Index");
            }
            
        }
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string orderid)
        {
            if (orderid == null)
            {
                return BadRequest();
            }
            Order order = _context.Orders.Find(orderid);
            if (order == null)
            {
                return NotFound();
            }
            if (order.Status == StatusConst.WAITCONFIRM || order.Status == StatusConst.CONFIRMED)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
                _notyfService.Success("Đã xóa đơn hàng!");
                return RedirectToAction("Index");
            }
            else
            {
                _notyfService.Success("Đơn hàng của bạn đã được xử lí vừa song, vui lòng kiểm tra lại");
                return RedirectToAction("Index");
            }

        }
*/
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
