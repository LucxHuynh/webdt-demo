using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebBanDienThoai.Models;
using Vonage;
using Vonage.Request;

namespace WebBanPodcast.Areas.Identity.Pages.Account
{
    public class ConfirmSMSModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ConfirmSMSModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string Code { get; set; }

        }
        public string PhoneNumber { get; set; }

        public string GeneratedCode { get; set; }



        public async Task<IActionResult> OnGetAsync()
        {
            //GeneratedCode = TempData["GeneratedCode"].ToString();
            //PhoneNumber = TempData["PhoneNumber"]?.ToString();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            PhoneNumber = TempData["PhoneNumber"].ToString();
            GeneratedCode = TempData["GeneratedCode"].ToString();
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Kiểm tra mã xác nhận
                    //var result = await _userManager.VerifyChangePhoneNumberTokenAsync(user, Input.Code, user.PhoneNumber);
                    if (Input.Code.Equals(GeneratedCode))
                    {
                        // Cập nhật trạng thái xác nhận
                        await _userManager.SetPhoneNumberAsync(user, PhoneNumber);
                        return Redirect("/Identity/Account/Manage"); // Chuyển hướng về trang chủ hoặc trang nào bạn muốn
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid code.");
                    }
                }
                else
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }
            }

            return Page();
        }
    }
}