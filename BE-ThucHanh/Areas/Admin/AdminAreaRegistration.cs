using System.Web.Mvc;

namespace BE_ThucHanh.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            // Đường dẫn khu quản trị: /quan-tri-website/... (thay cho /Admin/...)
            context.MapRoute(
                "Admin_default",
                "quan-tri-website/{controller}/{action}/{id}",
                new { controller = "AdminHome", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}