using Ecom_Project_1145.DataAccess.Repository.IRepository;
using Ecom_Project_1145.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace Ecom_Project_1145.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ----------------------------
        // SHOW ALL PRODUCTS
        // ----------------------------
        public IActionResult ProductManagement()
        {
            var products = _unitOfWork.Product.GetAll();
            return View(products);
        }

        // ----------------------------
        // TOGGLE PRODUCT STATUS
        // ----------------------------
        [HttpPost]
        public IActionResult ToggleProductStatus(int id)
        {
            var product = _unitOfWork.Product.Get(id);

            if (product == null)
            {
                TempData["SuccessMessage"] = "Product not found!";
                return RedirectToAction("ProductManagement");
            }

            product.Status = !product.Status;
            _unitOfWork.Product.Update(product);
            _unitOfWork.save();

            string emailReport = "";

            // Send emails if discontinued
            if (!product.Status)
            {
                var orders = _unitOfWork.OrderDetails
                    .GetAll(o => o.ProductId == id, includeProperties: "OrderHeader,OrderHeader.ApplicationUser")
                    .ToList();

                foreach (var order in orders)
                {
                    string email = order.OrderHeader.ApplicationUser.Email;

                    if (string.IsNullOrEmpty(email))
                    {
                        emailReport += $"UserId {order.OrderHeader.ApplicationUserId} has no email.<br/>";
                        continue;
                    }

                    bool sent = SendEmail(email, product.Title);

                    if (sent)
                        emailReport += $"Email successfully sent to {email}.<br/>";
                    else
                        emailReport += $"Failed to send email to {email}.<br/>";
                }
            }

            TempData["EmailReport"] = emailReport;

            // TempData message for popup
            TempData["SuccessMessage"] = product.Status
                ? "Product is now active."
                : "Product successfully discontinued!";

            return RedirectToAction("ProductManagement");
        }

        // ----------------------------
        // PRODUCT DETAILS PAGE
        // ----------------------------
        public IActionResult ProductDetail(int id)
        {
            var product = _unitOfWork.Product.Get(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // ----------------------------
        // EMAIL NOTIFICATION FUNCTION
        // ----------------------------
        private bool SendEmail(string userEmail, string productName)
        {
            try
            {
                string fromEmail = "yourstore@gmail.com";            // Gmail
                string appPassword = "your_app_password";            // App Password

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, "Book Store");
                mail.To.Add(userEmail);
                mail.Subject = $"Product Discontinued: {productName}";
                mail.Body = $"Hello,\n\nThe product '{productName}' you purchased has now been discontinued.\n\nThank you!";
                mail.IsBodyHtml = false;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential(fromEmail, appPassword);

                    smtp.Send(mail);
                }

                Console.WriteLine($"Email successfully sent to {userEmail}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed to {userEmail}: {ex.Message}");
                return false;
            }
        }
    }
}
