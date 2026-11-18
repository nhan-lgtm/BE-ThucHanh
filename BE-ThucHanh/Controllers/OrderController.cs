using BE_ThucHanh.Models;
using BE_ThucHanh.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        private const int PreviewCount = 2;   // mỗi trạng thái chỉ hiện 2 đơn gần nhất
        private const int MaxCount = 50;      // bấm "Xem thêm" thì hiện tối đa 50 đơn gần nhất

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

        // GET: Order/MyOrder?tab=all&searchTerm=...&showAll=false
        // Trang "Đơn hàng của tôi": tìm kiếm, các tab theo trạng thái, 2 đơn gần nhất / tối đa 50 đơn
        public ActionResult MyOrder(string tab = "all", string searchTerm = null, bool showAll = false)
        {
            var customer = db.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            tab = OrderStatuses.NormalizeTab(tab);

            // Toàn bộ đơn của khách này
            var myOrders = db.Orders
                .Include("OrderDetails.Product.Category")
                .Where(o => o.CustomerID == customer.CustomerID);

            var model = new MyOrderVM
            {
                SearchTerm = searchTerm,
                Tab = tab,
                ShowAll = showAll,
                HasAnyOrder = myOrders.Any()
            };

            // Tìm theo Mã đơn hàng hoặc Tên sản phẩm
            var query = myOrders;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim();
                int orderId;
                if (int.TryParse(term, out orderId))
                {
                    query = query.Where(o => o.OrderID == orderId
                        || o.OrderDetails.Any(d => d.Product.ProductName.Contains(term)));
                }
                else
                {
                    query = query.Where(o => o.OrderDetails.Any(d => d.Product.ProductName.Contains(term)));
                }
            }

            // Đơn mới nhất lên trước
            var orders = query.OrderByDescending(o => o.OrderDate)
                              .ThenByDescending(o => o.OrderID)
                              .ToList();

            if (tab == "all")
            {
                // Tab Tất cả đơn: hiện tất cả, gom nhóm theo trạng thái
                // (OrderBy của LINQ to Objects giữ nguyên thứ tự ngày trong cùng một nhóm)
                orders = orders.OrderBy(o => OrderStatuses.IndexOf(OrderStatuses.KeyOf(o.PaymentStatus))).ToList();
                model.TotalInTab = orders.Count;
            }
            else
            {
                // Các tab còn lại: lọc theo trạng thái, mặc định 2 đơn gần nhất, "Xem thêm" thì tối đa 50 đơn
                orders = orders.Where(o => OrderStatuses.KeyOf(o.PaymentStatus) == tab).ToList();
                model.TotalInTab = orders.Count;
                orders = orders.Take(showAll ? MaxCount : PreviewCount).ToList();
            }

            model.Orders = orders;
            return View(model);
        }

        // GET: Order/Reorder/5
        // Nút MUA LẠI: thêm các sản phẩm của đơn vào giỏ hàng, số lượng = 1
        public ActionResult Reorder(int id)
        {
            var customer = db.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Chỉ cho mua lại đơn của chính mình
            var order = db.Orders
                .Include("OrderDetails.Product.Category")
                .FirstOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order == null)
            {
                return HttpNotFound();
            }

            var cart = GetCartService().GetCart();
            foreach (var detail in order.OrderDetails)
            {
                var product = detail.Product;
                if (product == null) continue;

                cart.AddItem(product.ProductID, product.ProductImage, product.ProductName,
                    product.ProductPrice, 1,
                    product.Category != null ? product.Category.CategoryName : "");
            }

            return RedirectToAction("Index", "Cart");
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
