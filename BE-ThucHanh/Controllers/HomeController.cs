using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BE_ThucHanh.Models;
using BE_ThucHanh.Models.ViewModel;
using PagedList;

namespace BE_ThucHanh.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();

            //Lọc danh sách product theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.ProductDescription.Contains(searchTerm) ||
                    p.Category.CategoryName.Contains(searchTerm));

            }

            //tha, số để paging
            int pageNumber = page ?? 1; //gắn bảng trang hiện tại hoặc trang đầu tiên (nếu nsd không chọn)
            int pageSize = 6; //só sản phẩm/trang

            //lấy top 10 sản phẩm bán chạy nhất
            model.FeaturedProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(10).ToList();

            //lấy 20 sản phẩm ế nhất và phân trang
            model.NewProduct = products.OrderBy(p => p.OrderDetails.Count()).Take(20).ToPagedList(pageNumber,pageSize);
            return View(model);
        }
    }
}