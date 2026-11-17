using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using _25dh110017_Mystore.Models;
using _25dh110017_Mystore.Models.Cart;
using _25dh110017_Mystore.Models.ViewModel;
// Sử dụng Alias để định nghĩa rõ ràng Class Cart, tránh đụng độ Namespace
using CartClass = _25dh110017_Mystore.Models.Cart.Cart;

namespace _25dh110017_Mystore.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // 1. TRANG ĐẶT HÀNG (GET: Order/Checkout)
        public ActionResult Checkout()
        {
            var cart = GetCartService().GetCart();
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutVM
            {
                Cart = cart
            };

            // Nếu người dùng đã đăng nhập, tự động lấy thông tin điền sẵn
            if (Session["Username"] != null)
            {
                string username = Session["Username"].ToString();
                var customer = db.Customers.FirstOrDefault(c => c.Username == username);
                if (customer != null)
                {
                    model.CustomerName = customer.CustomerName;
                    model.Phone = customer.CustomerPhone;
                    model.AddressDelivery = customer.CustomerAddress;
                }
            }

            return View("Checkout");
        }

        // 2. XỬ LÝ ĐẶT HÀNG (POST: Order/Checkout)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = GetCartService().GetCart();
            model.Cart = cart;

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        int customerId = 0;

                        // TRƯỜNG HỢP 1: Đã đăng nhập hệ thống
                        if (Session["Username"] != null)
                        {
                            string username = Session["Username"].ToString();
                            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
                            if (customer == null)
                            {
                                // Tìm theo tên hoặc SĐT nếu record cũ chưa có Username
                                customer = db.Customers.FirstOrDefault(c => c.CustomerPhone == model.Phone || c.CustomerName == username);
                                if (customer != null)
                                {
                                    customer.Username = username;
                                }
                                else
                                {
                                    customer = new Customer
                                    {
                                        Username = username,
                                        CustomerName = model.CustomerName,
                                        CustomerPhone = model.Phone,
                                        CustomerAddress = model.AddressDelivery
                                    };
                                    db.Customers.Add(customer);
                                    db.SaveChanges();
                                }
                            }

                            customerId = customer.CustomerID;
                            customer.CustomerName = model.CustomerName;
                            customer.CustomerPhone = model.Phone;
                            customer.CustomerAddress = model.AddressDelivery;
                        }

                        // TRƯỜNG HỢP 2: Khách vãng lai -> Tự tạo hoặc tìm Customer dựa theo số điện thoại
                        if (customerId == 0)
                        {
                            var existingCustomer = db.Customers.FirstOrDefault(c => c.CustomerPhone == model.Phone);
                            if (existingCustomer != null)
                            {
                                customerId = existingCustomer.CustomerID;
                            }
                            else
                            {
                                Customer newCustomer = new Customer
                                {
                                    CustomerName = model.CustomerName,
                                    CustomerPhone = model.Phone,
                                    CustomerAddress = model.AddressDelivery
                                };
                                db.Customers.Add(newCustomer);
                                db.SaveChanges(); // Lưu để lấy CustomerID vừa sinh tự động
                                customerId = newCustomer.CustomerID;
                            }
                        }

                        // Tạo đối tượng Đơn hàng
                        Order order = new Order
                        {
                            CustomerID = customerId,
                            OrderDate = DateTime.Now,
                            TotalAmount = cart.TotalValue(),
                            AddressDelivery = model.AddressDelivery,
                            PaymentStatus = "Đang xử lý"
                        };

                        db.Orders.Add(order);
                        db.SaveChanges(); // Lưu để sinh tự động OrderID

                        // Lưu chi tiết từng sản phẩm trong đơn hàng
                        foreach (var item in cart.Items)
                        {
                            OrderDetail detail = new OrderDetail
                            {
                                OrderID = order.OrderID,
                                ProductID = item.product.ProductID,
                                ProductName = item.product.ProductName,
                                Quantity = item.quantity,
                                UnitPrice = item.product.ProductPrice
                            };
                            db.OrderDetails.Add(detail);
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        // Đặt hàng thành công -> Xóa giỏ hàng
                        GetCartService().ClearCart();

                        return RedirectToAction("OrderSuccess", new { id = order.OrderID });
                    }
                    catch (DbEntityValidationException ex)
                    {
                        transaction.Rollback();
                        foreach (var entityErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in entityErrors.ValidationErrors)
                            {
                                ModelState.AddModelError("", $"Lỗi CSDL [{validationError.PropertyName}]: {validationError.ErrorMessage}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        string errorMsg = ex.Message;
                        if (ex.InnerException != null)
                        {
                            errorMsg += " -> Chi tiết: " + ex.InnerException.Message;
                            if (ex.InnerException.InnerException != null)
                            {
                                errorMsg += " (" + ex.InnerException.InnerException.Message + ")";
                            }
                        }
                        ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo đơn hàng: " + errorMsg);
                    }
                }
            }

            return View(model);
        }

        // 3. TRANG CHI TIẾT / XÁC NHẬN ĐƠN HÀNG (GET: Order/OrderSuccess/5)
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderDetails.Product").FirstOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // 4. LỊCH SỬ ĐƠN HÀNG CỦA TÔI (GET: Order/History)
        public ActionResult History(string status = "All")
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                customer = db.Customers.FirstOrDefault(c => c.CustomerName == username);
                if (customer != null && string.IsNullOrEmpty(customer.Username))
                {
                    customer.Username = username;
                    db.SaveChanges();
                }
            }

            if (customer == null)
            {
                ViewBag.CurrentStatus = "All";
                ViewBag.Counts = new Dictionary<string, int>();
                return View(new List<Order>());
            }

            // Lấy tất cả đơn hàng của khách hàng này
            var allOrders = db.Orders.Where(o => o.CustomerID == customer.CustomerID).ToList();

            // Đếm số lượng theo từng trạng thái giống TikTok Shop
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

            // Lọc theo trạng thái đã chọn
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

        // 5. KHÁCH HÀNG HỦY ĐƠN HÀNG (POST: Order/CancelOrder/5)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int id)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer != null)
            {
                var order = db.Orders.FirstOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
                if (order != null && (order.PaymentStatus == "Đang xử lý" || order.PaymentStatus == "Pending"))
                {
                    order.PaymentStatus = "Đã hủy";
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Hủy đơn hàng thành công!";
                }
            }
            return RedirectToAction("History");
        }

        // 6. MUA LẠI SẢN PHẨM TỪ ĐƠN HÀNG (GET: Order/ReOrder)
        public ActionResult ReOrder(int? productId)
        {
            if (productId.HasValue && productId.Value > 0)
            {
                var product = db.Products.Find(productId.Value);
                if (product != null)
                {
                    GetCartService().AddItem(product, 1);
                    TempData["SuccessMessage"] = "Đã thêm '" + product.ProductName + "' vào giỏ hàng!";
                    return RedirectToAction("Index", "Cart");
                }
            }

            // Nếu sản phẩm đã bị xóa hoặc hết hàng -> Trả lại thông báo không tìm thấy hoặc hết hàng
            TempData["ErrorMessage"] = "Không tìm thấy sản phẩm này hoặc sản phẩm đã hết hàng!";
            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return RedirectToAction("History");
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