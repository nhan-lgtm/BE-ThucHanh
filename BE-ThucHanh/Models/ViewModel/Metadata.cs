using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BE_ThucHanh.Models
{
    // Metadata cho bảng Product
    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
    }

    public class ProductMetadata
    {
        [HiddenInput(DisplayValue = false)]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm!")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự.")]
        public string ProductName { get; set; }

        [DataType(DataType.Currency)]
        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm!")]
        public decimal ProductPrice { get; set; }

        [DataType(DataType.MultilineText)]
        public string ProductDescription { get; set; }

        public string ProductImage { get; set; }

        public int CategoryID { get; set; }
    }

    // Metadata cho bảng Category
    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category
    {
    }

    public class CategoryMetadata
    {
        [HiddenInput]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục!")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Tên danh mục từ 2 đến 50 ký tự.")]
        public string CategoryName { get; set; }
    }

    // Metadata cho bảng User
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
    }

    public class UserMetadata
    {
        [Required(ErrorMessage = "Username is required!")]
        [StringLength(30, MinimumLength = 5)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required!")]
        public string UserRole { get; set; }
    }
}