using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _25dh110017_Mystore.Models
{
    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category
    {
    }

    public class CategoryMetadata
    {
        [Display(Name = "Mã danh mục")]
        public int CategoryID { get; set; }

        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "Tên danh mục không được để trống.")]
        [StringLength(100, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        public string CategoryName { get; set; }
    }
}

