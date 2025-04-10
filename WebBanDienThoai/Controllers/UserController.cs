﻿using BTLW_BDT.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebBanDienThoai.Helpers;
using WebBanDienThoai.Models;

namespace WebBanDienThoai.Controllers
{
    public class UserController : Controller
    {

        private readonly BtlLtwQlbdtContext _context;

        public UserController(BtlLtwQlbdtContext context)
        {
            _context = context;
        }

        // Action hiển thị trang Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Lấy MaKhachHang từ Session
            string maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            if (string.IsNullOrEmpty(maKhachHang))
            {
                return RedirectToAction("Login", "Access");
            }

            // Lấy thông tin khách hàng từ database
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang);

            if (khachHang == null)
            {
                return NotFound();
            }

            // Cập nhật Session với thông tin mới nhất
            HttpContext.Session.SetString("HoTen", khachHang.TenKhachHang);
            HttpContext.Session.SetString("NgaySinh", khachHang.NgaySinh.ToString());
            HttpContext.Session.SetString("SoDienThoai", khachHang.SoDienThoai);
            HttpContext.Session.SetString("DiaChi", khachHang.DiaChi);
            HttpContext.Session.SetString("Email", khachHang.Email);
            HttpContext.Session.SetString("GhiChu", khachHang.GhiChu ?? "");
            HttpContext.Session.SetString("Avatar", Url.Content("~/Images/Customer/" + khachHang.AnhDaiDien ?? ""));

            return View(khachHang);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // Action xử lý đổi mật khẩu
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (ModelState.IsValid)
            {
                string userId = HttpContext.Session.GetString("MaKhachHang");

                var user = await (from tk in _context.TaiKhoans
                                  join kh in _context.KhachHangs on tk.TenDangNhap equals kh.TenDangNhap
                                  where kh.MaKhachHang == userId
                                  select tk).FirstOrDefaultAsync();


                if (user != null)
                {
                    // Hash mật khẩu hiện tại và so sánh
                    string hashedCurrentPassword = model.CurrentPassword.ToSHA256Hash("MySaltKey");
                    if (user.MatKhau == hashedCurrentPassword)
                    {
                        // Cập nhật mật khẩu mới sau khi hash
                        user.MatKhau = model.NewPassword.ToSHA256Hash("MySaltKey");
                        await _context.SaveChangesAsync();

                        // Thêm thông báo thành công vào TempData
                        TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";

                        return RedirectToAction("Profile");
                    }
                    else
                    {
                        ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                    }
                }
                else
                {
                    ModelState.AddModelError("CurrentPassword", "Không tìm thấy tài khoản người dùng.");
                }
            }
            return View(model);
        }

        // Action lưu thông tin chỉnh sửa của khách hàng
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(KhachHang khachHang, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingCustomer = await _context.KhachHangs.FindAsync(khachHang.MaKhachHang);
                if (existingCustomer != null)
                {
                    // Validate số điện thoại
                    var phoneRegex = new Regex(@"^(0)[0-9]{9}$");
                    if (!phoneRegex.IsMatch(khachHang.SoDienThoai))
                    {
                        TempData["ErrorMessage"] = "Số điện thoại không hợp lệ!";
                        return RedirectToAction("Profile");
                    }

                    // Validate email
                    var emailRegex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
                    if (!emailRegex.IsMatch(khachHang.Email))
                    {
                        TempData["ErrorMessage"] = "Email không hợp lệ!";
                        return RedirectToAction("Profile");
                    }

                    // Kiểm tra email đã tồn tại (chỉ kiểm tra với email khác của chính user này)
                    var existingEmail = await _context.KhachHangs
                        .FirstOrDefaultAsync(x => x.Email == khachHang.Email && x.MaKhachHang != khachHang.MaKhachHang);
                    if (existingEmail != null)
                    {
                        TempData["ErrorMessage"] = "Email đã được sử dụng bởi tài khoản khác!";
                        return RedirectToAction("Profile");
                    }

                    // Cập nhật thông tin
                    existingCustomer.TenKhachHang = khachHang.TenKhachHang;
                    existingCustomer.NgaySinh = khachHang.NgaySinh;
                    existingCustomer.SoDienThoai = khachHang.SoDienThoai;
                    existingCustomer.DiaChi = khachHang.DiaChi;
                    existingCustomer.Email = khachHang.Email;
                    existingCustomer.GhiChu = khachHang.GhiChu;

                    // Xử lý upload ảnh nếu có
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(extension))
                        {
                            TempData["ErrorMessage"] = "Chỉ chấp nhận file ảnh .jpg, .jpeg hoặc .png!";
                            return RedirectToAction("Profile");
                        }

                        if (imageFile.Length > 5 * 1024 * 1024)
                        {
                            TempData["ErrorMessage"] = "Kích thước ảnh không được vượt quá 5MB!";
                            return RedirectToAction("Profile");
                        }

                        string newAvatarPath = MyUtil.UploadHinh(imageFile, "Customer");
                        existingCustomer.AnhDaiDien = newAvatarPath;
                        HttpContext.Session.SetString("Avatar", Url.Content("~/Images/Customer/" + newAvatarPath));
                    }

                    await _context.SaveChangesAsync();

                    // Cập nhật session
                    HttpContext.Session.SetString("HoTen", existingCustomer.TenKhachHang);
                    HttpContext.Session.SetString("NgaySinh", existingCustomer.NgaySinh.ToString());
                    HttpContext.Session.SetString("SoDienThoai", existingCustomer.SoDienThoai);
                    HttpContext.Session.SetString("DiaChi", existingCustomer.DiaChi);
                    HttpContext.Session.SetString("Email", existingCustomer.Email);
                    HttpContext.Session.SetString("GhiChu", existingCustomer.GhiChu ?? "");

                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Profile");
                }
            }
            return View("Profile", khachHang);
        }
    }
}
