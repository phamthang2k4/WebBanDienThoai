using BTLW_BDT.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public IActionResult Profile()
        {
            var model = new KhachHang
            {
                MaKhachHang = HttpContext.Session.GetString("MaKhachHang"),
                TenKhachHang = HttpContext.Session.GetString("HoTen"),
                NgaySinh = DateOnly.Parse(HttpContext.Session.GetString("NgaySinh") ?? DateOnly.MinValue.ToString()),
                SoDienThoai = HttpContext.Session.GetString("SoDienThoai"),
                DiaChi = HttpContext.Session.GetString("DiaChi"),
                Email = HttpContext.Session.GetString("Email"),
                GhiChu = HttpContext.Session.GetString("GhiChu"),
                AnhDaiDien = HttpContext.Session.GetString("Avatar")
            };
            return View(model);
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
    }
}
