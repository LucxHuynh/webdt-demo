using WebBanDienThoai.Models;

namespace WebBanDienThoai.ViewsModels
{
    public class ProductViewModel
    {
        public List<Product> Prods { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
