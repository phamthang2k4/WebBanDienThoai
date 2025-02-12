using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanDienThoai.Models;

namespace WebBanDienThoai.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly BtlLtwQlbdtContext _context;

        public CheckoutController(BtlLtwQlbdtContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            string userId = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Access");
            }

            var gioHang = _context.GioHangs.FirstOrDefault(g => g.TenDangNhap == userId);
            if (gioHang == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            var chiTietGioHang = _context.ChiTietGioHangs
                .Include(c => c.MaGioHangNavigation)
                .Include(c => c.MaSanPhamNavigation)
                .Where(c => c.MaGioHang == gioHang.MaGioHang)
                .ToList();
            return View(chiTietGioHang);
        }
    }
}
