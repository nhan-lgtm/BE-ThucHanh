using System.Web;
using System.Web.Mvc;

namespace BE_ThucHanh
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

            // Chặn các trang trong khu Admin nếu chưa đăng nhập admin
            filters.Add(new AdminAuthorizeAttribute());
        }
    }
}