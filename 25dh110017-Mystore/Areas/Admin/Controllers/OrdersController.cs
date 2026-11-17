using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25dh110017_Mystore.Models;

namespace _25dh110017_Mystore.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // 1. GET: Admin/Orders (Danh sách đơn hàng phân theo tab trạng thái)
        public ActionResult Index(string status = "All")
        {
            var allOrders = db.Orders.Include(o => o.Customer).ToList();

            // Tính số lượng cho từng tab kiểu TikTok Shop
            var counts = new Dictionary<string, int>
            {
                { "All", allOrders.Count },
                { "Đang xử lý", allOrders.Count(o => o.PaymentStatus == "Đang xử lý" || o.PaymentStatus == "Pending") },
                { "Đã xác nhận", allOrders.Count(o => o.PaymentStatus == "Đã xác nhận") },
                { "Đang giao", allOrders.Count(o => o.PaymentStatus == "Đang giao") },
                { "Đã giao", allOrders.Count(o => o.PaymentStatus == "Đã giao") },
                { "Đã hủy", allOrders.Count(o => o.PaymentStatus == "Đã hủy" || o.PaymentStatus == "Cancelled") }
            };
            ViewBag.Counts = counts;
            ViewBag.CurrentStatus = status ?? "All";

            // Lọc theo tab được chọn
            IEnumerable<Order> filtered = allOrders;
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "Đang xử lý")
                {
                    filtered = filtered.Where(o => o.PaymentStatus == "Đang xử lý" || o.PaymentStatus == "Pending");
                }
                else if (status == "Đã hủy")
                {
                    filtered = filtered.Where(o => o.PaymentStatus == "Đã hủy" || o.PaymentStatus == "Cancelled");
                }
                else
                {
                    filtered = filtered.Where(o => o.PaymentStatus == status);
                }
            }

            var orders = filtered.OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        // 2. GET: Admin/Orders/Details/5 (Chi tiết đơn hàng)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders.Include(o => o.Customer).FirstOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var orderDetails = db.OrderDetails.Include(od => od.Product).Where(od => od.OrderID == id).ToList();
            ViewBag.OrderDetails = orderDetails;

            return View(order);
        }

        // 3. GET: Admin/Orders/Edit/5 (Giao diện chỉnh sửa đơn hàng)
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders.Include(o => o.Customer).FirstOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            ViewBag.StatusList = new SelectList(new[]
            {
                "Đang xử lý",
                "Đã xác nhận",
                "Đang giao",
                "Đã giao",
                "Đã hủy"
            }, order.PaymentStatus);

            return View(order);
        }

        // 4. POST: Admin/Orders/Edit/5 (Xử lý cập nhật đơn hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderID,CustomerID,OrderDate,TotalAmount,AddressDelivery,PaymentStatus")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StatusList = new SelectList(new[]
            {
                "Đang xử lý",
                "Đã xác nhận",
                "Đang giao",
                "Đã giao",
                "Đã hủy"
            }, order.PaymentStatus);

            return View(order);
        }

        // 5. CẬP NHẬT NHANH TRẠNG THÁI (POST: Admin/Orders/UpdateStatus)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string newStatus, string currentTab = "All")
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                order.PaymentStatus = newStatus;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { status = currentTab });
        }

        // 6. XÁC NHẬN ĐƠN HÀNG NHANH
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmOrder(int id)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                order.PaymentStatus = "Đã xác nhận";
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // 7. GET: Admin/Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders.Include(o => o.Customer).FirstOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // 8. POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            if (order != null)
            {
                var orderDetails = db.OrderDetails.Where(od => od.OrderID == id).ToList();
                db.OrderDetails.RemoveRange(orderDetails);

                db.Orders.Remove(order);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
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
