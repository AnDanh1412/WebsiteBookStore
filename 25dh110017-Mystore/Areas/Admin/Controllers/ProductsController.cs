using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25dh110017_Mystore.Models;
using _25dh110017_Mystore.Models.ViewModel;

namespace _25dh110017_Mystore.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // -------------------------------------------------
        // GET: Admin/Products (Tích hợp Tìm kiếm & Sắp xếp)
        // -------------------------------------------------
        public ActionResult Index(string searchTerm, decimal? minPrice, decimal? maxPrice, string sortOrder)
        {
            var model = new ProductSearchVM
            {
                SearchTerm = searchTerm,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder
            };

            var products = db.Products.Include(p => p.Category).AsQueryable();

            // 1. Tìm kiếm theo từ khóa (Tên sản phẩm hoặc Mô tả)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                               p.ProductDescription.Contains(searchTerm));
            }

            // 2. Lọc theo khoảng giá
            if (minPrice.HasValue)
                products = products.Where(p => p.ProductPrice >= minPrice.Value);
            if (maxPrice.HasValue)
                products = products.Where(p => p.ProductPrice <= maxPrice.Value);

            // 3. Sắp xếp kết quả
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductName);
                    break;
            }

            model.Products = products.ToList();
            return View(model);
        }

        // -------------------------------------------------
        // GET: Admin/Products/Details/5
        // -------------------------------------------------
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);
            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        // -------------------------------------------------
        // GET: Admin/Products/Create
        // -------------------------------------------------
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // -------------------------------------------------
        // POST: Admin/Products/Create (Upload hình ảnh)
        // -------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "ProductID,CategoryID,ProductName,ProductPrice,ProductDescription")] Product product,
            HttpPostedFileBase uploadImage)
        {
            // ---- Kiểm tra người dùng đã chọn danh mục chưa ----
            if (product.CategoryID == 0)
                ModelState.AddModelError("CategoryID", "Vui lòng chọn danh mục cho sản phẩm.");

            if (ModelState.IsValid)
            {
                // ---- Xử lý upload ảnh (nếu có) ----
                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    string fileName = Path.GetFileNameWithoutExtension(uploadImage.FileName);
                    string extension = Path.GetExtension(uploadImage.FileName);
                    fileName = $"{fileName}_{DateTime.Now:yyMMddssfff}{extension}";

                    string path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                    product.ProductImage = "~/Images/" + fileName;
                    uploadImage.SaveAs(path);
                }
                else if (string.IsNullOrEmpty(product.ProductImage))
                {
                    product.ProductImage = "~/Images/default.png";
                }

                // Đảm bảo không bị null ProductDescription tránh lỗi CSDL
                if (string.IsNullOrEmpty(product.ProductDescription))
                {
                    product.ProductDescription = product.ProductName ?? "";
                }

                try
                {
                    db.Products.Add(product);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm sản phẩm mới thành công!";
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var entityErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityErrors.ValidationErrors)
                        {
                            ModelState.AddModelError("", $"{validationError.PropertyName}: {validationError.ErrorMessage}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu sản phẩm: " + ex.Message);
                }
            }

            // ---- Khi có lỗi, giữ lại giá trị danh mục đã chọn ----
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // -------------------------------------------------
        // GET: Admin/Products/Edit/5
        // -------------------------------------------------
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);
            if (product == null)
                return HttpNotFound();

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // -------------------------------------------------
        // POST: Admin/Products/Edit/5 (Cập nhật hình ảnh)
        // -------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "ProductID,CategoryID,ProductName,ProductPrice,ProductImage,ProductDescription")] Product product,
            HttpPostedFileBase uploadImage)
        {
            // ---- Kiểm tra người dùng đã chọn danh mục chưa ----
            if (product.CategoryID == 0)
                ModelState.AddModelError("CategoryID", "Vui lòng chọn danh mục cho sản phẩm.");

            if (ModelState.IsValid)
            {
                // ---- Nếu người dùng chọn ảnh mới, thay thế ảnh cũ ----
                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    string fileName = Path.GetFileNameWithoutExtension(uploadImage.FileName);
                    string extension = Path.GetExtension(uploadImage.FileName);
                    fileName = $"{fileName}_{DateTime.Now:yyMMddssfff}{extension}";

                    string path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                    product.ProductImage = "~/Images/" + fileName;
                    uploadImage.SaveAs(path);
                }
                else if (string.IsNullOrEmpty(product.ProductImage))
                {
                    product.ProductImage = "~/Images/default.png";
                }

                // Đảm bảo không bị null ProductDescription
                if (string.IsNullOrEmpty(product.ProductDescription))
                {
                    product.ProductDescription = product.ProductName ?? "";
                }

                try
                {
                    db.Entry(product).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var entityErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityErrors.ValidationErrors)
                        {
                            ModelState.AddModelError("", $"{validationError.PropertyName}: {validationError.ErrorMessage}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật sản phẩm: " + ex.Message);
                }
            }

            // ---- Khi có lỗi, giữ lại danh mục đã chọn ----
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // -------------------------------------------------
        // GET: Admin/Products/Delete/5
        // -------------------------------------------------
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);
            if (product == null)
                return HttpNotFound();

            // Kiểm tra các đơn hàng chứa sản phẩm này
            var orderDetails = db.OrderDetails.Include(od => od.Order).Where(od => od.ProductID == id).ToList();

            // Kiểm tra xem có đơn hàng nào đang xử lý hoặc đã xác nhận hay không
            bool hasActiveOrder = orderDetails.Any(od => od.Order != null &&
                (od.Order.PaymentStatus == "Đang xử lý" ||
                 od.Order.PaymentStatus == "Pending" ||
                 od.Order.PaymentStatus == "Đã xác nhận"));

            if (hasActiveOrder)
            {
                ViewBag.CanDelete = false;
                ViewBag.DeleteWarning = "Không thể xóa vì có đơn hàng đang chờ xử lý hoặc đã xác nhận. Vui lòng thử lại sau khi đơn hàng được giao hoặc hủy!";
            }
            else
            {
                ViewBag.CanDelete = true;
                if (orderDetails.Any())
                {
                    ViewBag.DeleteInfo = "Sản phẩm này đã có lịch sử đặt hàng. Khi xác nhận xóa, sản phẩm sẽ được xóa khỏi hệ thống, đồng thời toàn bộ thông tin đơn hàng của khách vẫn được lưu giữ nguyên vẹn.";
                }
            }

            return View(product);
        }

        // -------------------------------------------------
        // POST: Admin/Products/Delete/5
        // -------------------------------------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            if (product != null)
            {
                // Kiểm tra các đơn hàng chứa sản phẩm này
                var orderDetails = db.OrderDetails.Include(od => od.Order).Where(od => od.ProductID == id).ToList();

                // 1. Nếu có đơn hàng đang xử lý hoặc đã xác nhận -> CHẶN XÓA
                bool hasActiveOrder = orderDetails.Any(od => od.Order != null &&
                    (od.Order.PaymentStatus == "Đang xử lý" ||
                     od.Order.PaymentStatus == "Pending" ||
                     od.Order.PaymentStatus == "Đã xác nhận"));

                if (hasActiveOrder)
                {
                    TempData["ErrorMessage"] = "Không thể xóa vì có đơn hàng đang chờ xử lý hoặc đã xác nhận. Vui lòng thử lại sau khi đơn hàng được giao hoặc hủy!";
                    return RedirectToAction("Index");
                }

                try
                {
                    // 2. Lưu lại tên sản phẩm vào OrderDetail trước khi xóa
                    foreach (var detail in orderDetails)
                    {
                        if (string.IsNullOrEmpty(detail.ProductName))
                        {
                            detail.ProductName = product.ProductName;
                        }
                        detail.ProductID = null; // Gỡ khóa ngoại an toàn để xóa được sản phẩm
                    }
                    db.SaveChanges();

                    // 3. XÓA HOÀN TOÀN SẢN PHẨM KHỎI HỆ THỐNG
                    db.Products.Remove(product);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Đã xóa sản phẩm '" + product.ProductName + "' thành công khỏi hệ thống!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi xóa sản phẩm: " + ex.Message;
                }
            }

            return RedirectToAction("Index");
        }

        // -------------------------------------------------
        // Dispose
        // -------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}