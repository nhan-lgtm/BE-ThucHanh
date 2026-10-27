using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BE_ThucHanh.Models.ViewModel
{
    public class ProductSearchVM
    {
        //tiêu chí tìm kiếm
        public string SearchTerm {  get; set; }

        //sắp xếp

        //phân trang

        //kết quả trả về: Danh sách sản phẩm
        public List<Product> Products { get; set; }
    }
}