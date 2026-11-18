using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace BE_ThucHanh.Models.ViewModel
{
    public class ProductDetailsVM
    {
        public Product product { get; set; }
        public int quantity { get; set; } = 1;

        // Tính giá trị tạm thời tự động (Số lượng x Giá sản phẩm)
        public decimal estimatedValue
        {
            get { return quantity * (product != null ? product.ProductPrice : 0); }
        }

        // Các thuộc tính hỗ trợ phân trang 
        public int PageNumber { get; set; } // Trang hiện tại
        public int PageSize { get; set; } = 3; // Số sản phẩm mỗi trang

        // Danh sách sản phẩm cùng danh mục 
        public IPagedList<Product> RelatedProducts { get; set; }

        // Danh sách sản phẩm bán chạy nhất cùng danh mục
        public IPagedList<Product> TopProducts { get; set; }
    }
}