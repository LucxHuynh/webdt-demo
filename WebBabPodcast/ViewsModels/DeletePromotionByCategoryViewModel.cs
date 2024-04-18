using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebBanDienThoai.ViewsModels
{
    public class DeletePromotionByCategoryViewModel
    {
        public SelectList CategoryList { get; set; }
        public int SelectedCategoryId { get; set; }
    }

}
