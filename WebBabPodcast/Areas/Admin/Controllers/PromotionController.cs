using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanDienThoai.Models;
using WebBanDienThoai.ViewsModels;

namespace WebBanPodcast.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Role_Admin)]
    public class PromotionController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public PromotionController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Promotion
        public async Task<IActionResult> Index()
        {
            var promotions = await _dbContext.Promotions.ToListAsync();
            return View(promotions);
        }

        // GET: Promotion/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Promotion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,DiscountAmount,IsPercentage")] Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Add(promotion);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(promotion);
        }

        // GET: Promotion/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _dbContext.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }
            return View(promotion);
        }

        // POST: Promotion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartDate,EndDate,DiscountAmount,IsPercentage")] Promotion promotion)
        {
            if (id != promotion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _dbContext.Update(promotion);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromotionExists(promotion.Id))
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
            return View(promotion);
        }

        // GET: Promotion/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _dbContext.Promotions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (promotion == null)
            {
                return NotFound();
            }

            return View(promotion);
        }

        // POST: Promotion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promotion = await _dbContext.Promotions.FindAsync(id);
            _dbContext.Promotions.Remove(promotion);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PromotionExists(int id)
        {
            return _dbContext.Promotions.Any(e => e.Id == id);
        }

        // them ma khuyen mai
        [HttpGet]
        public IActionResult UpdatePromotion()
        {
            var viewModel = new UpdatePromotionViewModel
            {
                CategoryList = new SelectList(_dbContext.Categories, "Id", "Name"),
                PromotionList = new SelectList(_dbContext.Promotions, "Id", "Name")
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> UpdatePromotion(UpdatePromotionViewModel model)
        {
            try
            {
                // Tìm tất cả các sản phẩm thuộc CategoryId
                var products = await _dbContext.Products
                    .Where(p => p.CategoryId == model.CategoryId)
                    .ToListAsync();

                // Kiểm tra xem mã giảm giá có tồn tại không
                var promotion = await _dbContext.Promotions.FindAsync(model.PromotionId);
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

                await _dbContext.SaveChangesAsync();

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
            var categories = _dbContext.Categories.ToList();

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
                var products = await _dbContext.Products
                    .Where(p => p.CategoryId == categoryId)
                    .ToListAsync();

                // Xóa giảm giá và đặt Price_Promotion về null cho từng sản phẩm
                foreach (var product in products)
                {
                    product.PromotionId = null;
                    product.Price_Promotion = 0;
                }

                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Index", "Product"); // Chuyển hướng về trang danh sách sản phẩm sau khi xóa giảm giá
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

    }
}
