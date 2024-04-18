// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebBanDienThoai.Models;
using Vonage;
using Vonage.Request;

namespace WebBanPodcast.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }
        public async Task SendConfirmationSmsAsync(string phoneNumber)
        {
            var credentials = Vonage.Request.Credentials.FromApiKeyAndSecret(
                "8823188d",
                "iAYtkwp8uMWQidJz"
    );

            var VonageClient = new VonageClient(credentials);

            var code = GenerateRandomCode(); // Hàm này để tạo mã xác nhận ngẫu nhiên
            TempData["GeneratedCode"] = code;
            var message = $"Your verification code is: {code}";
            var numberToSend = "";
            if (Input.PhoneNumber.StartsWith("0"))
            {
                numberToSend = "84" + Input.PhoneNumber.Substring(1);
            }
            var response = VonageClient.SmsClient.SendAnSmsAsync(new Vonage.Messaging.SendSmsRequest()
            {
                To = numberToSend,
                From = "Vonage",
                Text = message
            });
        }
        private string GenerateRandomCode()
        {
            // Hàm này để tạo mã xác nhận ngẫu nhiên
            var random = new Random();
            return random.Next(1000, 9999).ToString();
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var code = GenerateRandomCode(); // Hàm này để tạo mã xác nhận ngẫu nhiên
            TempData["GeneratedCode"] = code;
            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }
            var phoneNumber = "";
            TempData["PhoneNumber"] = Input.PhoneNumber;
            if (Input.PhoneNumber.StartsWith("0"))
            {
                phoneNumber = "84" + Input.PhoneNumber.Substring(1);
            }
            SendConfirmationSmsAsync(phoneNumber);
            return Redirect("/Identity/Account/ConfirmSMS");
        }
    }
}