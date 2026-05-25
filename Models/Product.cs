using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OMS.Models
{
    public class Product
    {
        [Key]
        public string Sku { get; set; }

        [DisplayName("Tên sản phẩm")]
        public string Name { get; set; }

        [DisplayName("Danh mục")]
        public string Category { get; set; }

        [DisplayName("Giá nhập")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ImportPrice { get; set; }

        [DisplayName("Giá bán")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal SellingPrice { get; set; }

        [DisplayName("Nguồn hàng")]
        public string Source { get; set; }

        [DisplayName("Kho")]
        public string Warehouse { get; set; }
    }
}
