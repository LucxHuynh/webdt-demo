using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebBanDienThoai.ViewsModels
{
    public class UpdatePromotionViewModel
    {
        public int CategoryId { get; set; }
        public int PromotionId { get; set; }
        public SelectList CategoryList { get; set; }
        public SelectList PromotionList { get; set; }
    }


}
