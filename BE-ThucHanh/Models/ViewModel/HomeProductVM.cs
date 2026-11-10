using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BE_ThucHanh.Models.ViewModel
{
    public class HomeProductVM
    {
        //tiêu chí để search
        public string SearchTerm {  get; set; }

        //tiêu chí để paging
        public int PageNumber { get; set; } //đang đứng ở trang nào
        public int PageSize { get; set; } = 10; // mỗi trang có bao nhiêu bản ghi

        //danh sách sản phẩm nôi bật
        public List<Product> FeaturedProducts { get; set;}

        //danh sách sản phẩm mới đã phân trang
        public PagedList.IPagedList<Product> NewProduct { get; set; }
    }
}