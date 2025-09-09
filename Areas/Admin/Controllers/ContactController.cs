using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public class ContactController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ContactController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var contact = await _dataContext.Contact.AsNoTracking().FirstOrDefaultAsync();
            if (contact == null)
            {
                // Create default contact if none exists
                contact = new ContactModel
                {
                    Name = "Aurum Watches",
                    Phone = "Đang cập nhật",
                    Email = "info@aurumwatches.vn",
                    Map = "<div style=\"width: 100%; height: 400px; background: #f0f0f0; display: flex; align-items: center; justify-content: center; border: 2px solid #000000;\"><p style=\"color: #666; font-size: 18px;\">Bản đồ sẽ được cập nhật</p></div>",
                    Description = "Chào mừng bạn đến với Aurum Watches - Nơi Thời Gian Kể Lại Câu Chuyện. Đây không chỉ là một cửa hàng, mà là một giấc mơ đã thành hiện thực. Giấc mơ về một không gian nơi những chiếc đồng hồ được trân trọng đúng với giá trị của chúng."
                };
                _dataContext.Contact.Add(contact);
                await _dataContext.SaveChangesAsync();
            }
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactModel contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm contact hiện có trong database
                    var existingContact = await _dataContext.Contact.FirstOrDefaultAsync();
                    
                    if (existingContact == null)
                    {
                        // Nếu không có contact nào, tạo mới
                        existingContact = new ContactModel
                        {
                            Name = contact.Name,
                            Phone = contact.Phone,
                            Email = contact.Email,
                            Map = contact.Map,
                            Description = contact.Description
                        };
                        _dataContext.Contact.Add(existingContact);
                    }
                    else
                    {
                        // Cập nhật thông tin từ form (KHÔNG thay đổi Name vì nó là primary key)
                        existingContact.Phone = contact.Phone;
                        existingContact.Email = contact.Email;
                        existingContact.Map = contact.Map;
                        existingContact.Description = contact.Description;
                        
                        // Chỉ cập nhật Name nếu nó khác với Name hiện tại
                        if (existingContact.Name != contact.Name)
                        {
                            // Xóa contact cũ và tạo mới với Name mới
                            _dataContext.Contact.Remove(existingContact);
                            await _dataContext.SaveChangesAsync();
                            
                            existingContact = new ContactModel
                            {
                                Name = contact.Name,
                                Phone = contact.Phone,
                                Email = contact.Email,
                                Map = contact.Map,
                                Description = contact.Description,
                                LogoImage = existingContact.LogoImage // Giữ lại logo cũ
                            };
                            _dataContext.Contact.Add(existingContact);
                        }
                    }

                    // Handle logo image upload
                    if (contact.ImageUpload != null)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "contact");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + contact.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await contact.ImageUpload.CopyToAsync(fileStream);
                        }

                        existingContact.LogoImage = "/images/contact/" + uniqueFileName;
                    }

                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Cập nhật thông tin liên hệ thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Có lỗi xảy ra khi cập nhật: " + ex.Message;
                }
            }
            else
            {
                TempData["error"] = "Dữ liệu không hợp lệ";
            }

            return View("Index", contact);
        }
    }
}
