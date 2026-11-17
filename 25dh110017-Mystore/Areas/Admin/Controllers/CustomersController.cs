using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25dh110017_Mystore.Models;

namespace _25dh110017_Mystore.Areas.Admin.Controllers
{
    public class CustomersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Customers
        public ActionResult Index()
        {
            // Thay "Users" bằng tên đúng của bảng khách hàng trong DbContext của bạn (VD: Customers, Accounts, v.v.)
            var customers = db.Customers.ToList();
            return View(customers);
        }
        // GET: Admin/Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }
        // GET: Admin/Customers/Delete/5 - Redirect về Index (nút xóa dùng POST form)
        public ActionResult Delete(int? id)
        {
            return RedirectToAction("Index");
        }

        // POST: Admin/Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers.Find(id);
            if (customer != null)
            {
                string username = customer.Username;

                // 1. Xóa các đơn hàng và chi tiết đơn hàng liên quan trước
                var orders = db.Orders.Where(o => o.CustomerID == id).ToList();
                foreach (var order in orders)
                {
                    var details = db.OrderDetails.Where(od => od.OrderID == order.OrderID).ToList();
                    db.OrderDetails.RemoveRange(details);
                }
                db.Orders.RemoveRange(orders);

                // 2. Xóa hồ sơ khách hàng trong bảng Customer
                db.Customers.Remove(customer);
                db.SaveChanges();

                // 3. Xóa luôn tài khoản đăng nhập trong bảng User
                if (!string.IsNullOrEmpty(username))
                {
                    var user = db.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        db.Users.Remove(user);
                        db.SaveChanges();
                    }
                }

                TempData["SuccessMessage"] = "Đã xóa khách hàng và toàn bộ tài khoản đăng nhập thành công!";
            }
            return RedirectToAction("Index");
        }

        // GET: Admin/Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Admin/Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer);
        }
    }
}