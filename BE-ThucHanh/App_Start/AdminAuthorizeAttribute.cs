using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace BE_ThucHanh
{
    // Bộ lọc toàn cục cho khu Admin:
    //  1. Chỉ cho truy cập từ chính máy chạy web (người ngoài vào thì báo 404, như không có trang này).
    //  2. Phải đăng nhập admin mới được vào các trang quản trị.
    // Không ảnh hưởng tới trang của khách hàng.
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var area = filterContext.RouteData.DataTokens["area"] as string;
            if (!string.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return; // không phải khu Admin
            }

            // Truy cập qua ngrok/proxy luôn có header X-Forwarded-For nên không tính là máy local
            var request = filterContext.HttpContext.Request;
            bool isLocal = request.IsLocal && string.IsNullOrEmpty(request.Headers["X-Forwarded-For"]);
            if (!isLocal)
            {
                filterContext.Result = new HttpNotFoundResult();
                return;
            }

            string controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            if (string.Equals(controller, "AdminAccount", StringComparison.OrdinalIgnoreCase))
            {
                return; // trang đăng nhập admin thì cho vào
            }

            var session = filterContext.HttpContext.Session;
            if (session == null || session["AdminUsername"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "area", "Admin" },
                    { "controller", "AdminAccount" },
                    { "action", "Login" },
                    { "returnUrl", request.RawUrl }
                });
            }
        }
    }
}