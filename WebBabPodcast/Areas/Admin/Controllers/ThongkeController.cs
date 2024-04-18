using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanDienThoai.Models;

namespace WebBanDienThoai.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Role_Admin)]
    public class ThongkeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongkeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? month)
        {
            if (month.HasValue && month >= 1 && month <= 12)
            {
                var firstDayOfMonth = new DateTime(DateTime.Now.Year, month.Value, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var orders = await _context.Orders
                    .Where(o => o.OrderDate >= firstDayOfMonth && o.OrderDate <= lastDayOfMonth)
                    .ToListAsync();



                var totalAmount = orders.Sum(o => o.TotalPrice);
                var countOrders = orders.Count();

                var statistics = new Dictionary<string, (decimal TotalAmount, int CountOrders)>
                {
                    { $"Tháng {month}", (totalAmount, countOrders) }
                };

                ViewData["TotalAmount"] = totalAmount;
                ViewData["CountOrders"] = countOrders;

                return View(statistics);
            }
            else
            {
                // Nếu không có tháng được chọn hoặc tháng không hợp lệ, trả về thông tin thống kê cho tháng hiện tại
                return RedirectToAction(nameof(Index), new { month = DateTime.Now.Month });
            }
        }
    }
}