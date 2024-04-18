
namespace WebBanDienThoai.Models
{
    public class ShoppingCart
    {
        // Danh sách các sản phẩm trong giỏ hàng
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        // Tổng giá tiền của các sản phẩm trong giỏ hàng
        public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);

        // Các hàm thêm/xóa sản phẩm trong giỏ hàng
        public void AddItem(CartItem item)
        {
            // Tìm xem sản phẩm đã có trong giỏ hàng chưa
            var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
        }

        // Xóa tất cả các mặt hàng khỏi danh sách Items
        public void ClearItems()
        {
            Items.Clear();
        }
        // Cập nhật số lượng sản phẩm trong giỏ hàng
        public void UpdateQuantity(int productId, int newQuantity)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (newQuantity > 0)
                {
                    item.Quantity = newQuantity;
                }
                else
                {
                    RemoveItem(productId);
                }
            }
        }

        internal void AddItem(object cartItem)
        {
            throw new NotImplementedException();
        }
    }
}
