using BE_ThucHanh.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BE_ThucHanh.Models
{
    public partial class Product
    {
        [NotMapped]
        public HttpPostedFileBase UploadImage { get; set; }

        [NotMapped]
        public List<ProductDetailsVM> RemainProducts { get; set; }
    }
}