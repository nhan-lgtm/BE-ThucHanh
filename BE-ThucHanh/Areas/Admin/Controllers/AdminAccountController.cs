using BE_ThucHanh.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace BE_ThucHanh.Areas.Admin.Controllers
{
    public class AdminAccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Mã vai trò của admin trong cột UserRole (cột chỉ dài 1 ký tự)
        private const string AdminRole = "A";

        // GET: Admin/AdminAccount/Login
        public ActionResult Login(string returnUrl)
        {
            // Đã đăng nhập rồi thì vào thẳng trang quản trị
            if (Session["AdminUsername"] != null)
            {
                return RedirectToAction("Index", "AdminHome");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Admin/AdminAccount/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập tên đăng nhập và mật khẩu.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            username = username.Trim();
            var user = db.Users.SingleOrDefault(u => u.Username == username
                                                  && u.Password == password
                                                  && u.UserRole == AdminRole);
            if (user == null)
            {
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu, hoặc tài khoản không có quyền admin.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Lưu trạng thái đăng nhập admin (tách riêng với đăng nhập của khách)
            Session["AdminUsername"] = user.Username;
            Session["AdminRole"] = user.UserRole;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "AdminHome");
        }

        // GET: Admin/AdminAccount/Logout
        public ActionResult Logout()
        {
            // Chỉ xóa thông tin admin, không xóa giỏ hàng hay phiên đăng nhập của khách
            Session.Remove("AdminUsername");
            Session.Remove("AdminRole");
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}