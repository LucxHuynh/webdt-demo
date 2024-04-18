using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebBanDienThoai.Extentions;
using WebBanDienThoai.Models;
using WebBanDienThoai.Others;
using WebBanPodcast.Others;

namespace WebBanDienThoai.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        //quản lý đăng nhập người dùng
        private readonly UserManager<ApplicationUser> _userManager;
        public ShoppingCartController(IProductRepository productRepository, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {

            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }
        public InputModel Input { get; set; }
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> GetUserEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Input.Email = user.Email;
            }
            return View();
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await GetProductFromDatabase(productId);
            decimal tmp;
            if (product.Price_Promotion != 0)
            {
                tmp = product.Price_Promotion;
            }
            else
            {
                tmp = product.Price;
            }
            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = tmp,
                Quantity = quantity
            };
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index","Home");
        }
        //tìm sản phẩm trong DB dựa vào productID
        private async Task<Product> GetProductFromDatabase(int productID)
        {
            var product = await _productRepository.GetByIdAsync(productID);
            return product;
        }


        [Authorize(Roles = Roles.Role_Customer)]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (cart == null || !cart.Items.Any())
            {
                // Xử lý giỏ hàng trống...
                return RedirectToAction("Index", "Home");
            }
            return View(cart);
        }
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is not null)
            {
                cart.RemoveItem(productId);
                // Lưu lại giỏ hàng vào Session sau khi đã xóa mục
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        public IActionResult RemoveAllFromCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is not null)
            {
                cart.ClearItems(); // Xóa tất cả các mặt hàng từ giỏ hàng
                                   // Lưu lại giỏ hàng vào Session sau khi đã xóa tất cả các mục
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int newQuantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                cart.UpdateQuantity(productId, newQuantity);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        //Checkout: dùng để người dùng nhập thông tin về địa chỉ giao hàng/ ghi chú khi đặt hàng
        [HttpGet]
        public IActionResult Checkout()
        {
            //TempData["ShippingAddress"]= Input.ship
            return View(new Order());
        }
        [HttpPost]
        //khi người dùng nhấn nút đặt hàng thì lưu thông tin vào DB
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                // Xử lý giỏ hàng trống...
                return RedirectToAction("Index", "Home");
            }

            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            //// Chuẩn bị dữ liệu cho thanh toán MoMo
            decimal totalMOMO = decimal.Parse(order.TotalPrice.ToString()); // Số tiền cần thanh toán
            decimal result = Math.Truncate(totalMOMO);

            return await Payment(result);
        }

        public async Task<IActionResult> Payment(decimal totalMOMO)
        {
            try
            {
                // Các thông tin cần thiết cho yêu cầu thanh toán
                string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
                string partnerCode = "MOMOOJOI20210710";
                string accessKey = "iPXneGmrJH0G8FOP";
                string secretKey = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
                string orderInfo = "Thanh toán online";
                string returnUrl = "https://localhost:7295/ShoppingCart/ReturnUrl";
                string notifyUrl = "https://4c8d-2001-ee0-5045-50-58c1-b2ec-3123-740d.ap.ngrok.io/Home/SavePayment";
                string extraData = "";

                // Mã đơn hàng và request ID
                string orderId = DateTime.Now.Ticks.ToString();
                string requestId = DateTime.Now.Ticks.ToString();

                // Chuẩn bị chuỗi rawHash và chữ ký signature
                string amount = totalMOMO.ToString();
                string rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount}&orderId={orderId}&orderInfo={orderInfo}&returnUrl={returnUrl}&notifyUrl={notifyUrl}&extraData={extraData}";
                MoMoSecurity crypto = new MoMoSecurity();
                string signature = crypto.signSHA256(rawHash, secretKey);

                // Xây dựng body JSON request
                JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyUrl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }
            };

                // Gửi yêu cầu thanh toán đến MoMo và nhận URL thanh toán
                string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());
                JObject jmessage = JObject.Parse(responseFromMomo);

                // Chuyển hướng người dùng đến URL thanh toán từ MoMo
                string payUrl = jmessage.GetValue("payUrl").ToString();
                
                return Redirect(payUrl);

            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return BadRequest(ex.Message);
            }
        }

        public IActionResult OrderCompleted()
        {
            return View();
        }
        public async Task<IActionResult> OrderHistory()
        {
            // Lấy ID người dùng hiện tại
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy lịch sử đơn hàng của người dùng từ cơ sở dữ liệu
            var orders = await _context.Orders
                                        .Include(o => o.OrderDetails)
                                        .ThenInclude(od => od.Product)
                                        .Where(o => o.UserId == userId)
                                        .OrderByDescending(o => o.OrderDate)
                                        .ToListAsync();

            return View(orders);
        }
        public async Task<IActionResult> ReturnUrl(Order order)
        {
            string param = Request.QueryString.ToString().Substring(0, Request.QueryString.ToString().IndexOf("signature") - 1);
            param = System.Net.WebUtility.UrlDecode(param);
            if (param.Contains("message=Success"))
            {
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
                if (cart == null || !cart.Items.Any())
                {
                    // Xử lý giỏ hàng trống...
                    return RedirectToAction("Index", "Home");
                }
                var orderI = HttpContext.Session.GetObjectFromJson<ShoppingCart>("OrderItem");
                var user = await _userManager.GetUserAsync(User);//lấy thông tin người dùng đã đăng nhập
                order.UserId = user.Id;
                order.OrderDate = DateTime.UtcNow;
                order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                order.OrderDetails = cart.Items.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList();
                HttpContext.Session.Remove("Cart");
                // Lưu thông tin đơn hàng vào cơ sở dữ liệu
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await _emailSender.SendEmailAsync(user.Email, "Thanh toán thành công", "Bạn đã đặt hàng thàng công. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi !");
                return RedirectToAction("Index", "ShoppingCart");
            }
            else
            {
                return RedirectToAction("Index", "ShoppingCart");
            }
        }
    }
}
