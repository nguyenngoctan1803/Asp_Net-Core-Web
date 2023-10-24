using AspNetCoreHero.ToastNotification.Abstractions;
using DoAn2VADT.Database;
using DoAn2VADT.Database.Entities;
using DoAn2VADT.OnlinePayment;
using DoAn2VADT.Shared;
using DoAn2VADT.ViewModel;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
namespace DoAn2VADT.Areas.Admin.Controllers
{
    [Authorize]
    public class SellController : GlobalController
    {
        public SellController(AppDbContext context, IHostingEnvironment environment, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<SellController> logger) : base(context, environment, notyfService, httpContextAccessor, logger)
        {

        }
        public IActionResult Index()
        {
            ViewBag.ProductList = _context.Products.Include(p => p.Brand).Include(p => p.Category).Where(p => p.Effective == true).ToList();
            return View(GetProductSession());
        }

        /// <summary>
        /// Quầy bán hàng 
        /// </summary>
        /// 
        // lưu order vào session
        void SaveProductToSession(List<OrderDetail> orders)
        {
            var session = HttpContext.Session;
            string jsonOrder = JsonConvert.SerializeObject(orders);
            session.SetString(Const.ORDERSESSION, jsonOrder);
        }

        // Lấy Order từ Session (danh sách OrderItem)
        List<OrderDetail> GetProductSession()
        {
            var session = HttpContext.Session;
            string jsonOrder = session.GetString(Const.ORDERSESSION);
            if (jsonOrder != null)
            {
                return JsonConvert.DeserializeObject<List<OrderDetail>>(jsonOrder).Where(x=>x.Product.Quantity > 0 && x.Product.Effective == true).ToList();
            }
            return new List<OrderDetail>();
        }

        // Xóa Order khỏi session
        void ClearProductSession()
        {
            var session = HttpContext.Session;
            session.Remove(Const.ORDERSESSION);
        }

        // Lưu product vào session
        public IActionResult AddProduct(string id)
        {
            var product = _context.Products.Find(id);
            

            // Xử lý đưa vào Session ...
            var orders = GetProductSession();

            var checkExist = orders.Any(p => p.ProductId == id);
            if (checkExist)
            {
                int quantitytCheck = (int)orders.FirstOrDefault(p => p.ProductId == id).Quantity;
                if (CheckQuantity(id, quantitytCheck + 1))
                {
                    // Đã tồn tại, tăng thêm số lượng
                    orders.FirstOrDefault(p => p.ProductId == id).Quantity += 1;
                    UpdateTotal(id);
                    _notyfService.Success("Đã thêm sản phẩm!");
                }
                else
                {
                    _notyfService.Error("Số lượng sản phẩm không đủ!");
                }
            }
            else
            {
                var orderDetail = new OrderDetail()
                {
                    Quantity = 1,
                    ProductId = id,
                    Total = (product.Price - product.Discount),
                    Product = product
                };
                //  Thêm mới
                orders.Add(orderDetail);
                _notyfService.Success("Đã thêm sản phẩm!");
            }
            // Lưu Order vào Session
            SaveProductToSession(orders);
            // Chuyển đến trang hiện thị Order đã lưu vào Session
            return RedirectToAction(nameof(Index));
        }
        public void UpdateTotal(string id)
        {
            var orders = GetProductSession();
            var order = orders.FirstOrDefault(p => p.ProductId == id);
            orders.FirstOrDefault(p => p.ProductId == id).Total = order.Total * order.Quantity;
            SaveProductToSession(orders);
        }
        public bool CheckQuantity(string productid, int quantity)
        {
            var product = _context.Products.Find(productid);
            return product.Quantity >= quantity ? true : false;
        }
        public bool CheckOrder()
        {
            var orders = GetProductSession();
            foreach (var i in orders)
            {
                if (i.Quantity > i.Product.Quantity || i.Product.Effective == false)
                {
                    return false;
                }
            }
            return true;
        }
        [HttpPost]
        public IActionResult UpdateQuantity(string productid, int quantity)
        {
            if (CheckQuantity(productid, quantity))
            {
                // Cập nhật Cart thay đổi số lượng quantity ...
                var orders = GetProductSession();
                var checkExist = orders.Any(p => p.ProductId == productid);
                if (checkExist)
                {
                    var orderDetail = orders.FirstOrDefault(p => p.ProductId == productid);
                    if (orderDetail.Quantity == 1 && quantity == 1)
                    {
                        TempData["ErrorQuantity"] = "Số lượng phải lớn hơn hoặc bằng 1";
                    }
                    else
                    {
                        // Đã tồn tại
                        orders.FirstOrDefault(p => p.ProductId == productid).Quantity = quantity;
                        UpdateTotal(productid);
                        SaveProductToSession(orders);
                        TempData["UpdateQuantity"] = "Đã cập nhật số lượng";
                    }
                }
                else
                {
                    TempData["ErrorQuantity"] = "Vui lòng thử lại";
                }
              
            }
            else
            {
                TempData["ErrorQuantity"] = "Số lượng sản phẩm không đủ!";
            }
            // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            return Ok();
        }

        // Xóa OrderSession
        public IActionResult RemoveProduct(string id)
        {
            var orders = GetProductSession();
            var checkExist = orders.Any(p => p.ProductId == id);
            if (checkExist)
            {
                orders.Remove(orders.FirstOrDefault(x => x.ProductId == id));
                SaveProductToSession(orders);
                _notyfService.Success("Đã xóa sản phẩm khỏi đơn hàng!");
            }
            else
            {
                _notyfService.Error("Xin vui lòng thử lại!");
            }
            return RedirectToAction(nameof(Index));
        }


        // GET: Sell/Detail/Id
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


        public async Task<IActionResult> AddOrder()
        {
            if (!CheckOrder())
            {
                _notyfService.Error("Đặt hàng không thành công! Số lượng trong kho không đủ hoặc sản phẩm đã hết hiệu lực\nVui lòng kiểm tra lại", 10);
                return Redirect(nameof(Index));
            }
            Checkout checkout = JsonConvert.DeserializeObject<Checkout>(HttpContext.Session.GetString(Const.SELL).ToString());
            var orderId = checkout.Id;
            var name = checkout.Name.Trim();
            var phone = checkout.Phone.Trim();
            var address = checkout.Address.Trim();
            var discount = checkout.Discount == null ? 0 : checkout.Discount;
            Customer cus = new Customer()
            {
                Id = Guid.NewGuid().ToString(),
                Name = checkout.Name,
                Phone = checkout.Phone,
                Address = checkout.Address
            };
            _context.Customers.Add(cus);
            await _context.SaveChangesAsync();

            //// thêm đơn hàng
            decimal? totalOrder = 0;
            Order order = new Order()
            {
                Id = orderId,
                Name = name,
                Phone = phone,
                Address = address,
                Discount = discount,
                ShipFee = 0,
                PayWay = HttpContext.Session.GetString(Const.PAYWAY).ToString(),
                PayStatus = PayStatusConst.DONE,
                CustomerId = cus.Id,
                CreatedAt = DateTime.Now,
                CreateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString(),
                Status = StatusConst.DONE
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            // thêm sản phẩm vào đơn hàng
            var orderDetails = GetProductSession();
            foreach (var item in orderDetails)
            {
                try
                {
                    decimal? discountt = item.Product.Discount != null ? item.Product.Discount * item.Quantity : 0;
                    var totaldetail = (item.Product.Price * item.Quantity) - discountt;
                    totalOrder += totaldetail;
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Quantity = item.Quantity,
                        ProductId = item.ProductId,
                        Total = totaldetail,
                        OrderId = orderId,
                        CreatedAt = DateTime.Now,
                        CreateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString(),
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // cập nhật số lượng
                    var product = _context.Products.Find(item.ProductId);
                    product.Quantity = product.Quantity - item.Quantity;
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw;
                }

            }
            order.Total = totalOrder - order.Discount + order.ShipFee;
            order.UpdatedAt = DateTime.Now;
            order.UpdateUserId = User.FindFirstValue(Const.ADMINIDSESSION).ToString();
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            // Lưu session 
            HttpContext.Session.SetString(Const.SELLID, orderId);
            //
            _notyfService.Success("Đã xác nhận thanh toán! Hãy in hóa đơn để hoàn thành đơn hàng", 10);
            ClearProductSession();
            return Redirect("Index");
        }

        // POST: Sell/ConfirmPay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmPay(Checkout checkout)
        {
            checkout.Id = Guid.NewGuid().ToString();
            string checkoutString = JsonConvert.SerializeObject(checkout);
            HttpContext.Session.SetString(Const.SELL, checkoutString);
            if (checkout.PayOption == "ship")
            {
                HttpContext.Session.SetString(Const.PAYWAY, PayConst.OFFLINE);
                return RedirectToAction("AddOrder");
            }
            else
            {
                HttpContext.Session.SetString(Const.PAYWAY, PayConst.ONLINE);
                var orders = GetProductSession();
                decimal? totalOrder = 0;
                foreach (var item in orders)
                {
                    decimal? discount = item.Product.Discount * item.Quantity;
                    var totaldetail = (item.Product.Price * item.Quantity) - discount;
                    totalOrder += totaldetail;
                }
                if (totalOrder > 50000000)
                {
                    _notyfService.Error("Số tiền thanh toán quá lớn! Vui lòng chọn thanh toán khi nhận hàng");
                    return Redirect(nameof(Index));
                }
                else
                {
                    var amount = ((int?)totalOrder - (int?)checkout.Discount).ToString();
                    return Redirect($"Payment?total={amount}");
                }

            }
            
        }
        public IActionResult Done()
        {
            HttpContext.Session.Remove(Const.SELLID);
            return Redirect("Index");
        }
        // Sell/GetBill -> export bill
        [HttpPost]
        public IActionResult GetBill()
        {   
            if(HttpContext.Session.GetString(Const.SELLID) == null)
            {
                _notyfService.Error("Đơn hàng đã hoàn thành! Vui lòng kiểm tra lại");
                return Redirect(nameof(Index));
            }
            else
            {
                var id = HttpContext.Session.GetString(Const.SELLID).ToString();
                var order = _context.Orders.Find(id);
                order.OrderDetails = _context.OrderDetails.Where(o => o.OrderId == order.Id).Include(o => o.Product).ToList();
                HttpContext.Session.Remove(Const.SELLID);
                return PartialView("BillPartial", order);
            }
            
        }

        // MoMo
        public IActionResult Payment(string total = "0")
        {
            //request params need to request to MoMo system
            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
            string partnerCode = "MOMOOJOI20210710";
            string accessKey = "iPXneGmrJH0G8FOP";
            string serectkey = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
            string orderInfo = "Quét mã QR để thanh toán";
            string returnUrl = "https://localhost:44333/Admin/Sell/ConfirmPaymentClient";
            string notifyurl = "https://4c8d-2001-ee0-5045-50-58c1-b2ec-3123-740d.ap.ngrok.io/AdminSell/SavePayment"; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test

            string amount = total;
            string orderid = DateTime.Now.Ticks.ToString(); //mã đơn hàng
            string requestId = DateTime.Now.Ticks.ToString();
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyurl + "&extraData=" +
                extraData;

            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }

            };

            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);

            return Redirect(jmessage.GetValue("payUrl").ToString());
        }

        //Khi thanh toán xong ở cổng thanh toán Momo, Momo sẽ trả về một số thông tin, trong đó có errorCode để check thông tin thanh toán
        //errorCode = 0 : thanh toán thành công (Request.QueryString["errorCode"])
        //Tham khảo bảng mã lỗi tại: https://developers.momo.vn/#/docs/aio/?id=b%e1%ba%a3ng-m%c3%a3-l%e1%bb%97i
        public IActionResult ConfirmPaymentClient(string errorCode)
        {
            //lấy kết quả Momo trả về và hiển thị thông báo cho người dùng (có thể lấy dữ liệu ở đây cập nhật xuống db)
            if (errorCode == "0")
            {
                return RedirectToAction(nameof(AddOrder));
            }
            _notyfService.Error("Thanh toán không thành công! Vui lòng thử lại");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public void SavePayment()
        {
            //cập nhật dữ liệu vào db

        }
    }
}
