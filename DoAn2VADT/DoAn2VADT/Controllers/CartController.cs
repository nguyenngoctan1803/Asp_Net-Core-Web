using AspNetCoreHero.ToastNotification.Abstractions;
using DoAn2VADT.Database;
using DoAn2VADT.Database.Entities;
using DoAn2VADT.Shared;
using DoAn2VADT.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Net;
using DoAn2VADT.OnlinePayment;
using Newtonsoft.Json.Linq;

namespace DoAn2VADT.Controllers
{
    public class CartController : GlobalController
    {
        public CartController(AppDbContext _context, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<CartController> logger) : base(_context, notyfService, httpContextAccessor, logger)
        {

        }
        public IActionResult Index()
        {
            string sessionId = HttpContext.Session.GetString(Const.CARTSESSION).ToString();
            var carts = _context.Carts.Where(x => x.SessionId == sessionId).Include(x => x.Product.Brand).OrderByDescending(x => x.CreatedAt).ToList();
            foreach(var item in carts)
            {
                var product = _context.Products.Find(item.ProductId);
                if(item.Quantity > product.Quantity)
                {
                    item.Quantity = product.Quantity;
                    _context.Carts.Update(item);
                    _context.SaveChanges();
                    _notyfService.Warning("Giỏ hàng của bạn chứa một số sản phẩm không đủ số lượng kho để cung cấp!");
                    _notyfService.Warning("Số lượng đã được cập nhật về mức tối đa!");
                }
            }
            return View(GetCart());
        }
        List<Cart> GetCart()
        {
            string sessionId = HttpContext.Session.GetString(Const.CARTSESSION).ToString();
            var carts = _context.Carts.Where(x => x.SessionId == sessionId).Include(x => x.Product).OrderByDescending(x => x.CreatedAt).ToList();
            return carts;
        }
        // Xóa cart khỏi session
        void ClearCart()
        {
            string sessionId = HttpContext.Session.GetString(Const.CARTSESSION).ToString();
            var carts = _context.Carts.Where(x => x.SessionId == sessionId).ToList();
            foreach (var item in carts)
            {
                _context.Carts.Remove(item);
            }
            _context.SaveChanges();
        }

        public bool CheckQuantity(string productid, int quantity)
        {
            var product = _context.Products.Find(productid);
            return product.Quantity >= quantity ? true : false;
        }
        public bool CheckCart()
        {
            var cart = GetCart();
            foreach(var i in cart)
            {
                if(i.Quantity > i.Product.Quantity)
                {
                    return false;
                }
            }
            return true;
        }

        [HttpPost]
        public IActionResult UpdateCart(string productid, int quantity)
        {
            if(CheckQuantity(productid, quantity))
            {
                // Cập nhật Cart thay đổi số lượng quantity ...
                string sessionId = HttpContext.Session.GetString(Const.CARTSESSION).ToString();
                var carts = GetCart();
                var cartitem = carts.FirstOrDefault(p => p.Product.Id == productid && p.SessionId == sessionId);
                if (cartitem != null)
                {
                    if(cartitem.Quantity == 1 && quantity == 1)
                    {
                        TempData["ErrorQuantity"] = "Số lượng phải lớn hơn hoặc bằng 1";
                    }
                    else
                    {
                        cartitem.Quantity = quantity;
                        cartitem.UpdatedAt = DateTime.Now;
                        _context.Carts.Update(cartitem);
                        _context.SaveChanges();
                        TempData["UpdateQuantity"] = "Đã cập nhật số lượng";
                    }
                }
                else
                {
                    TempData["ErrorQuantity"] = "Vui lòng thử lại";
                }
                // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            }
            else
            {
                TempData["ErrorQuantity"] = "Số lượng sản phẩm không đủ!";
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult AddToCart(string productid, string quantity)
        {
            if (CheckQuantity(productid, Int32.Parse(quantity)))
            {
                var proId = productid.ToString();
                var prod = _context.Products.FirstOrDefault(p => p.Id == proId);
                if (prod == null)
                    return NotFound();

                // Xử lý đưa vào Cart ...
                var carts = GetCart();
                var cartitem = carts.Find(p => p.Product.Id == proId);
                if (cartitem != null)
                {
                    // Đã tồn tại, tăng thêm 1
                    cartitem.Quantity += Int32.Parse(quantity);
                    cartitem.UpdatedAt = DateTime.Now;
                    _notyfService.Information("Sản phầm đã được thêm vào giỏ hàng trước đó, số lượng cập nhật đã được cập nhật");
                }
                else
                {
                    //  Thêm mới
                    _context.Carts.Add(new Cart()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = proId,
                        Quantity = Int32.Parse(quantity),
                        SessionId = HttpContext.Session.GetString(Const.CARTSESSION).ToString(),
                        CreatedAt = DateTime.Now,
                    });
                    _notyfService.Success("Đã thêm sản phẩm vào giỏ hàng");
                }
                // Lưu cart
                _context.SaveChanges();

                // Chuyển đến trang hiện thị Cart
                return RedirectToAction("Index");
            }
            else
            {
                _notyfService.Error("Số lượng sản phẩm không đủ!");
                return RedirectToAction("Details", "Product", new { id=productid });
            }
        }

        public IActionResult RemoveCart(string productid)
        {
            var carts = GetCart();
            var cartitem = carts.Find(p => p.Product.Id == productid);
            if (cartitem != null)
            {
                _context.Carts.Remove(cartitem);
                _context.SaveChanges();
                _notyfService.Success("Đã xóa sản phẩm khỏi giỏ hàng");
            }
            else
            {
                _notyfService.Error("Vui lòng thử lại");
            }
            return RedirectToAction("Index");
        }
        public IActionResult CheckOut()
        {
            ViewBag.Name = "";
            ViewBag.Phone = "";
            ViewBag.Address = "";
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString(Const.USERIDSESSION)))
            {
                Customer cus = JsonConvert.DeserializeObject<Customer>(HttpContext.Session.GetString(Const.USERSESSION).ToString());
                ViewBag.Name = cus.Name;
                ViewBag.Phone = cus.Phone;
                ViewBag.Address = cus.Address;
            }

            return View(GetCart());
        }
        public async Task<IActionResult> AddOrder()
        {
            if(!CheckCart())
            {
                _notyfService.Error("Đặt hàng không thành công! Số lượng trong kho không đủ\nVui lòng kiểm tra lại", 10);
                return Redirect(nameof(CheckOut));
            }
            Checkout checkout = JsonConvert.DeserializeObject<Checkout>(HttpContext.Session.GetString(Const.CHECKOUTSESSION).ToString());
            var name = checkout.Name.Trim();
            var phone = checkout.Phone.Trim();
            var address = checkout.Address.Trim();
            if (String.IsNullOrEmpty(HttpContext.Session.GetString(Const.USERIDSESSION)))
            {
                string cus_id = HttpContext.Session.GetString(Const.CARTSESSION).ToString();
                Customer cus = new Customer()
                {
                    Id = cus_id,
                    Name = name,
                    Phone = phone,
                    Address = address
                };
                _context.Customers.Add(cus);
                await _context.SaveChangesAsync();
            }
            var cart = GetCart();
            decimal? totalOrder = 0;
            string orderId = Guid.NewGuid().ToString();
            Order order = new Order()
            {
                Id = orderId,
                Name = name,
                Phone = phone,
                Address = address,
                PayWay = HttpContext.Session.GetString(Const.PAYWAY).ToString(),
                PayStatus = HttpContext.Session.GetString(Const.PAYSTATUS).ToString(),
                Total = totalOrder,
                CustomerId = HttpContext.Session.GetString(Const.CARTSESSION).ToString(),
                CreatedAt = DateTime.Now,
                CreateUserId = HttpContext.Session.GetString(Const.CARTSESSION).ToString(),
                Status = StatusConst.WAITCONFIRM
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // thêm sản phẩm vào đơn hàng
            foreach (var item in cart)
            {
                try
                {         
                    decimal? discount = item.Product.Discount != null ? item.Product.Discount * item.Quantity : 0;
                    var totaldetail = (item.Product.Price * item.Quantity) - discount;
                    totalOrder += totaldetail;
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Quantity = item.Quantity,
                        ProductId = item.ProductId,
                        Total = totaldetail,
                        OrderId = orderId,
                        CreatedAt = DateTime.Now,

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
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            _notyfService.Success("Đã đặt hàng thành công! Cảm ơn quý khách hàng đã ủng hộ", 10);
            ClearCart();
            return Redirect("Index");
        }

        [HttpPost]
        public IActionResult CheckOut(Checkout checkout)
        {
            string checkoutString = JsonConvert.SerializeObject(checkout);
            HttpContext.Session.SetString(Const.CHECKOUTSESSION, checkoutString);
            if (checkout.PayOption == "ship")
            {
                HttpContext.Session.SetString(Const.PAYWAY, PayConst.OFFLINE);
                HttpContext.Session.SetString(Const.PAYSTATUS, PayStatusConst.NODONE);
                return RedirectToAction("AddOrder");
            }
            else
            {
                HttpContext.Session.SetString(Const.PAYWAY, PayConst.ONLINE);
                var cart = GetCart();
                decimal? totalOrder = 0;
                foreach (var item in cart)
                {
                    decimal? discount = !String.IsNullOrEmpty(item.Product.Discount.ToString()) ? item.Product.Discount * item.Quantity : 0;
                    var totaldetail = (item.Product.Price * item.Quantity) - discount;
                    totalOrder += totaldetail;
                }
                if(totalOrder > 50000000) 
                {
                    _notyfService.Error("Số tiền thanh toán quá lớn! Vui lòng chọn thanh toán khi nhận hàng");
                    return Redirect(nameof(CheckOut));
                }
                else
                {
                    var amount = ((int?)totalOrder + 30000).ToString();
                    return Redirect($"Payment?total={amount}");
                }
               
            }
        }
        public IActionResult Payment(string total = "0")
        {
            //request params need to request to MoMo system
            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
            string partnerCode = "MOMOOJOI20210710";
            string accessKey = "iPXneGmrJH0G8FOP";
            string serectkey = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
            string orderInfo = "Quét mã QR để thanh toán";
            string returnUrl = "https://localhost:44333/Cart/ConfirmPaymentClient";
            string notifyurl = "https://4c8d-2001-ee0-5045-50-58c1-b2ec-3123-740d.ap.ngrok.io/Cart/SavePayment"; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test

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
            if(errorCode == "0")
            {
                HttpContext.Session.SetString(Const.PAYSTATUS, PayStatusConst.DONE);
                return RedirectToAction(nameof(AddOrder));
            }
            _notyfService.Error("Thanh toán không thành công! Vui lòng thử lại");
            return RedirectToAction("Checkout", "Cart");
        }

        [HttpPost]
        public void SavePayment()
        {
            //cập nhật dữ liệu vào db
            
        }
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
