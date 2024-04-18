namespace WebBanDienThoai.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DiscountAmount { get; set; } // Số tiền giảm giá hoặc phần trăm giảm giá
        public bool IsPercentage { get; set; } // True nếu giảm giá theo phần trăm, False nếu giảm giá theo số tiền cố định
    }
}
