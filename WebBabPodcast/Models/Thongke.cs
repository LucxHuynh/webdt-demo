namespace WebBanPodcast.Models
{
    public class Thongke
    {
        public decimal TotalAmount { get; set; } // Tổng số tiền
        public int CountOrders { get; set; } // Số lượng đơn hàng
        //public decimal TotalAmount
        //{
        //    get
        //    {
        //        return OrderDetails.Sum(od => od.Quantity * od.Product.Price);
        //    }
        //}
    }
}
