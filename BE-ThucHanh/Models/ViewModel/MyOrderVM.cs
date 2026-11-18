using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BE_ThucHanh.Models.ViewModel
{
    /// <summary>
    /// Danh sách trạng thái đơn hàng hiển thị trên các tab của trang "Đơn hàng của tôi".
    /// Trạng thái được lấy từ cột Order.PaymentStatus. Nếu CSDL của bạn dùng giá trị khác
    /// thì chỉ cần sửa/ bổ sung ở mảng Values bên dưới.
    /// </summary>
    public static class OrderStatuses
    {
        public class Info
        {
            public string Key { get; set; }        // dùng trên URL (tab=...)
            public string Name { get; set; }       // tên hiển thị trên tab
            public string CardTitle { get; set; }  // tiêu đề hiển thị trên đầu mỗi đơn hàng
            public string Icon { get; set; }       // biểu tượng nhỏ trước tiêu đề
            public string[] Values { get; set; }   // các giá trị PaymentStatus tương ứng (viết thường)
        }

        public static readonly List<Info> List = new List<Info>
        {
            new Info { Key = "pending",    Name = "Chờ thanh toán",  CardTitle = "Chờ thanh toán",       Icon = "\u23F3",
                       Values = new[] { "pending", "chờ thanh toán", "chưa thanh toán", "unpaid" } },
            new Info { Key = "processing", Name = "Đang xử lý",      CardTitle = "Đang xử lý",           Icon = "\u2699",
                       Values = new[] { "processing", "paid", "đang xử lý", "chờ xử lý", "đã thanh toán" } },
            new Info { Key = "shipping",   Name = "Đang vận chuyển", CardTitle = "Đang vận chuyển",      Icon = "\U0001F69A",
                       Values = new[] { "shipping", "shipped", "đang vận chuyển", "đang giao", "đang giao hàng" } },
            new Info { Key = "delivered",  Name = "Đã giao",         CardTitle = "Giao hàng thành công", Icon = "\u2714",
                       Values = new[] { "delivered", "completed", "đã giao", "hoàn thành", "đã hoàn thành", "giao hàng thành công" } },
            new Info { Key = "cancelled",  Name = "Đã hủy",          CardTitle = "Đã hủy",               Icon = "\u2298",
                       Values = new[] { "cancelled", "canceled", "đã hủy", "đã huỷ", "hủy", "huỷ" } }
        };

        /// <summary>Đổi giá trị PaymentStatus trong CSDL thành key của tab. Giá trị lạ được xếp vào "Đang xử lý".</summary>
        public static string KeyOf(string paymentStatus)
        {
            string s = (paymentStatus ?? "").Trim().ToLowerInvariant();
            foreach (var st in List)
            {
                if (st.Values.Contains(s)) return st.Key;
            }
            return "processing";
        }

        public static string NameOf(string key)
        {
            var st = List.FirstOrDefault(x => x.Key == key);
            return st != null ? st.Name : "Đang xử lý";
        }

        public static Info InfoOf(string key)
        {
            return List.FirstOrDefault(x => x.Key == key) ?? List.First(x => x.Key == "processing");
        }

        public static int IndexOf(string key)
        {
            int i = List.FindIndex(x => x.Key == key);
            return i < 0 ? List.Count : i;
        }

        /// <summary>Tab hợp lệ: "all" hoặc một trong các key ở trên.</summary>
        public static string NormalizeTab(string tab)
        {
            tab = (tab ?? "all").Trim().ToLowerInvariant();
            return tab == "all" || List.Any(x => x.Key == tab) ? tab : "all";
        }
    }

    /// <summary>Dữ liệu cho trang Đơn hàng của tôi (MyOrder.cshtml).</summary>
    public class MyOrderVM
    {
        public string SearchTerm { get; set; }   // từ khóa: mã đơn hàng hoặc tên sản phẩm
        public string Tab { get; set; }          // all / pending / processing / shipping / delivered / cancelled
        public bool ShowAll { get; set; }        // true khi bấm "Xem thêm" (tối đa 50 đơn)
        public bool HasAnyOrder { get; set; }    // khách đã từng đặt đơn nào chưa
        public int TotalInTab { get; set; }      // tổng số đơn thỏa điều kiện (trước khi cắt 2 đơn / 50 đơn)
        public List<Order> Orders { get; set; }  // các đơn sẽ hiển thị

        public MyOrderVM()
        {
            Orders = new List<Order>();
            Tab = "all";
        }
    }
}
