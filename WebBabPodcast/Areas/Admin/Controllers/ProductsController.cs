using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanDienThoai.Models;
using WebBanDienThoai.ViewsModels;

namespace WebBanDienThoai.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Role_Admin)]
    public class ProductsController : Controller
    {

        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Include(p => p.Category);
            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<IActionResult> Users()
{
    var users = await _context.Users.ToListAsync();
    return View(users);
}
        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Description,ImageUrl,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Description,ImageUrl,CategoryId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
          return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // them ma khuyen mai
        [HttpGet]
        public IActionResult UpdatePromotion()
        {
            var viewModel = new UpdatePromotionViewModel
            {
                CategoryList = new SelectList(_context.Categories, "Id", "Name"),
                PromotionList = new SelectList(_context.Promotions, "Id", "Name")
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> UpdatePromotion(UpdatePromotionViewModel model)
        {
            try
            {
                // Tìm tất cả các sản phẩm thuộc CategoryId
                var products = await _context.Products
                    .Where(p => p.CategoryId == model.CategoryId)
                    .ToListAsync();

                // Kiểm tra xem mã giảm giá có tồn tại không
                var promotion = await _context.Promotions.FindAsync(model.PromotionId);
                if (promotion == null)
                {
                    return NotFound("Không tìm thấy mã giảm giá");
                }

                // Cập nhật mã giảm giá cho từng sản phẩm và lưu vào cơ sở dữ liệu
                foreach (var product in products)
                {
                    product.PromotionId = promotion.Id;
                    if (promotion.IsPercentage)
                    {
                        product.Price_Promotion = product.Price - (product.Price * promotion.DiscountAmount / 100);
                    }
                    else
                    {
                        product.Price_Promotion = product.Price - promotion.DiscountAmount;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok("Đã cập nhật mã giảm giá cho sản phẩm trong danh mục");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
        [HttpGet]
        public IActionResult DeletePromotionByCategory()
        {
            // Lấy danh sách loại sản phẩm từ cơ sở dữ liệu
            var categories = _context.Categories.ToList();

            // Tạo SelectList từ danh sách loại sản phẩm
            var categoryList = new SelectList(categories, "Id", "Name");

            // Gán SelectList vào ViewModel
            var viewModel = new DeletePromotionByCategoryViewModel
            {
                CategoryList = categoryList
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePromotionByCategory(int categoryId)
        {
            try
            {
                // Tìm tất cả các sản phẩm thuộc CategoryId
                var products = await _context.Products
                    .Where(p => p.CategoryId == categoryId)
                    .ToListAsync();

                // Xóa giảm giá và đặt Price_Promotion về null cho từng sản phẩm
                foreach (var product in products)
                {
                    product.PromotionId = null;
                    product.Price_Promotion = 0;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Product"); // Chuyển hướng về trang danh sách sản phẩm sau khi xóa giảm giá
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}
