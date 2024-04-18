    using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebBanDienThoai.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebBanDienThoai.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }
        public IActionResult ChangeUserRole()
        {
            // Lấy danh sách tất cả người dùng từ UserManager
            var users = _userManager.Users;
            return View(users);
        }

        // POST: Admin/ChangeUserRole
        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string userId, string roleName)
        {
            // Tìm người dùng với userId
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Xác định vai trò mới dựa trên roleName
            var role = await _userManager.GetRolesAsync(user);
            if (role.Contains(roleName))
            {
                // Người dùng đã có vai trò đó, không cần phải thay đổi
                TempData["Message"] = "User already has the selected role.";
                return RedirectToAction(nameof(ChangeUserRole));
            }

            // Xóa tất cả các vai trò hiện tại của người dùng
            await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));

            // Thêm vai trò mới cho người dùng
            await _userManager.AddToRoleAsync(user, roleName);

            TempData["Message"] = "User role has been updated successfully.";
            return RedirectToAction(nameof(ChangeUserRole));
        }
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                // Xử lý lỗi nếu xảy ra khi xóa người dùng
                return RedirectToAction(nameof(Error));
            }
            return RedirectToAction(nameof(Users));
        }
    }
}
