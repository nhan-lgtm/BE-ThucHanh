using BE_ThucHanh.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BE_ThucHanh.Areas.Admin.Controllers
{
    public class CustomersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Customers
        // Danh sách khách hàng, có tìm kiếm theo tên / tài khoản / SĐT / email
        public ActionResult Index(string q)
        {
            var customers = db.Customers.Include("Orders").AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                customers = customers.Where(c => c.CustomerName.Contains(q)
                                              || c.Username.Contains(q)
                                              || c.CustomerPhone.Contains(q)
                                              || c.CustomerEmail.Contains(q));
            }

            ViewBag.Q = q;
            return View(customers.OrderBy(c => c.CustomerID).ToList());
        }

        // GET: Admin/Customers/Details/5
        // Thông tin khách hàng và các đơn hàng của họ
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(400);
            var customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();
            return View(customer);
        }

        // GET: Admin/Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(400);
            var customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();
            return View(customer);
        }

        // POST: Admin/Customers/Edit/5
        // Chỉ cho sửa họ tên, SĐT, email, địa chỉ (không sửa tên đăng nhập)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string customerName, string customerPhone,
                                 string customerEmail, string customerAddress)
        {
            var customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();

            customer.CustomerName = customerName;
            customer.CustomerPhone = customerPhone;
            customer.CustomerEmail = customerEmail;
            customer.CustomerAddress = customerAddress;

            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        ModelState.AddModelError("", ve.PropertyName + ": " + ve.ErrorMessage);
                return View(customer);
            }

            TempData["Message"] = "Đã cập nhật thông tin khách hàng.";
            return RedirectToAction("Details", new { id = id });
        }

        // POST: Admin/Customers/Delete/5
        // Xóa khách hàng và tài khoản đăng nhập (chỉ khi chưa có đơn hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();

            if (customer.Orders.Any())
            {
                TempData["Error"] = "Khách hàng này đã có đơn hàng nên không thể xóa.";
                return RedirectToAction("Details", new { id = id });
            }

            var user = db.Users.Find(customer.Username);
            db.Customers.Remove(customer);
            if (user != null)
            {
                db.Users.Remove(user);
            }
            db.SaveChanges();

            TempData["Message"] = "Đã xóa khách hàng.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}