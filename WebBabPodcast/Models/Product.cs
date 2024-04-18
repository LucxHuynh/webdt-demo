using System.ComponentModel.DataAnnotations;
using WebBanPodcast.Models;

namespace WebBanDienThoai.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<ProductImage>? Images { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        // Khóa ngoại cho khuyến mãi
        public int? PromotionId { get; set; }
        public Promotion? Promotion { get; set; }
        public decimal Price_Promotion { get; set; } // Giá tiền sau khi khuyến mãi
    }
}
