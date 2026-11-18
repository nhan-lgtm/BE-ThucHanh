using BE_ThucHanh.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BE_ThucHanh.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Các trạng thái đơn hàng (giữ ngắn để vừa cột PaymentStatus)
        // Lưu ý: giá trị đầu tiên phải trùng với trạng thái mặc định khi khách đặt hàng (OrderController)
        private static readonly string[] Statuses =
            { "Chờ xử lý", "Đã thanh toán", "Đang giao", "Hoàn thành", "Đã hủy" };

        // GET: Admin/Orders
        // Danh sách đơn hàng, lọc theo trạng thái và tìm theo khách hàng
        public ActionResult Index(string status, string q)
        {
            var orders = db.Orders.Include("Customer").AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(o => o.PaymentStatus == status);
            }
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                orders = orders.Where(o => o.Customer.CustomerName.Contains(q)
                                        || o.Customer.Username.Contains(q));
            }

            ViewBag.Status = status;
            ViewBag.Q = q;
            ViewBag.Statuses = Statuses;
            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }

        // GET: Admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(400);
            var order = db.Orders.Include("Customer").Include("OrderDetails.Product")
                          .FirstOrDefault(o => o.OrderID == id);
            if (order == null) return HttpNotFound();

            ViewBag.Statuses = Statuses;
            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var order = db.Orders.Find(id);
            if (order == null) return HttpNotFound();

            if (!Statuses.Contains(status))
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
                return RedirectToAction("Details", new { id = id });
            }

            order.PaymentStatus = status;
            order.StatusUpdatedAt = DateTime.Now;
            db.SaveChanges();

            TempData["Message"] = "Đã cập nhật trạng thái đơn hàng.";
            return RedirectToAction("Details", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}