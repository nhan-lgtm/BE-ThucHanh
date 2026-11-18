using BE_ThucHanh.Models;
using BE_ThucHanh.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BE_ThucHanh.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Lấy dịch vụ giỏ hàng (giống CartController)
        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // GET: Order/Checkout
        // Hiển thị trang thanh toán
        public ActionResult Checkout()
        {
            var customer = db.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCartService().GetCart();
            if (cart.Items == null || !cart.Items.Any())
            {
                // Giỏ hàng rỗng thì quay lại trang giỏ hàng
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.Cart = cart;
            ViewBag.Total = cart.Items.Sum(i => i.TotalPrice);
            return View(customer);
        }

        // POST: Order/Checkout
        // Tạo đơn hàng từ giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string addressDelivery)
        {
            var customer = db.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartService = GetCartService();
            var cart = cartService.GetCart();
            if (cart.Items == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // Tạo đơn hàng
            var order = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Items.Sum(i => i.TotalPrice),
                PaymentStatus = "Chờ xử lý",
                AddressDelivery = string.IsNullOrWhiteSpace(addressDelivery)
                                    ? customer.CustomerAddress
                                    : addressDelivery.Trim(),
                StatusUpdatedAt = DateTime.Now
            };
            db.Orders.Add(order);

            // Mỗi mặt hàng trong giỏ thành một dòng chi tiết đơn hàng
            foreach (var item in cart.Items)
            {
                db.OrderDetails.Add(new OrderDetail
                {
                    Order = order,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            try
            {
                // Một lần SaveChanges: hoặc lưu cả đơn và chi tiết, hoặc không lưu gì
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        string msg = eve.Entry.Entity.GetType().Name + " - "
                                     + ve.PropertyName + ": " + ve.ErrorMessage;
                        Debug.WriteLine(msg);
                        ModelState.AddModelError("", msg);
                    }
                }
                ViewBag.Cart = cart;
                ViewBag.Total = cart.Items.Sum(i => i.TotalPrice);
                return View(customer);
            }
            catch (DbUpdateException ex)
            {
                Exception inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                Debug.WriteLine(inner.Message);
                ModelState.AddModelError("", "Lỗi CSDL: " + inner.Message);
                ViewBag.Cart = cart;
                ViewBag.Total = cart.Items.Sum(i => i.TotalPrice);
                return View(customer);
            }

            // Đặt hàng xong thì làm trống giỏ
            cartService.ClearCart();

            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }

        // GET: Order/OrderSuccess/5
        // Xác nhận đơn hàng sau khi đặt
        public ActionResult OrderSuccess(int id)
        {
            // Chỉ cho xem đơn của chính mình
            var order = db.Orders.FirstOrDefault(o => o.OrderID == id
                                                  && o.Customer.Username == User.Identity.Name);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Order/MyOrder
        // Danh sách đơn hàng đã đặt của khách đang đăng nhập
        public ActionResult MyOrder()
        {
            var orders = db.Orders
                .Where(o => o.Customer.Username == User.Identity.Name)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}