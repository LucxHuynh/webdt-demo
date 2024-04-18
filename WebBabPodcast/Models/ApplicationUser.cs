﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebBanDienThoai.Models
{
	public class ApplicationUser : IdentityUser
	{
		[Required]
		public string FullName { get; set; }
		public string? Address { get; set; }
		public int Age { get; set; }
	}
}
