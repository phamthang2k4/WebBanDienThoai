using Microsoft.AspNetCore.Mvc;
using WebBanDienThoai.Models;

namespace WebBanDienThoai.Controllers
{
    public class CartController : Controller
    {
        private readonly BtlLtwQlbdtContext _context;

        public CartController(BtlLtwQlbdtContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult RemoveFromCart(string maSanPham, string? maMau, string? maRom)
        {
            string userId = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Access");

            var gioHang = _context.GioHangs.SingleOrDefault(g => g.TenDangNhap == userId);
            if (gioHang == null) return RedirectToAction("DetailCart");

            var item = _context.ChiTietGioHangs.SingleOrDefault(c =>
                c.MaGioHang == gioHang.MaGioHang &&
                c.MaSanPham == maSanPham &&
                c.ThongSoMau == maMau &&
                c.ThongSoRom == maRom);

            if (item != null)
            {

                gioHang.TongTien = CalculateTotal(userId);
                _context.ChiTietGioHangs.Remove(item);
                _context.SaveChanges();
            }
            else
            {
                return RedirectToAction("");

            }

            return RedirectToAction("index");
        }

        // Tạo method tính tổng tiền
        private decimal CalculateTotal(string userId)
        {
            var gioHang = _context.GioHangs.FirstOrDefault(g => g.TenDangNhap == userId);
            if (gioHang == null) return 0;

            var chiTietGioHang = _context.ChiTietGioHangs
                .Where(c => c.MaGioHang == gioHang.MaGioHang)
                .ToList();

            decimal totalAmount = (decimal)chiTietGioHang.Sum(item =>
            {
                var sp = _context.SanPhams.FirstOrDefault(s => s.MaSanPham == item.MaSanPham);
                var danhSachRom = _context.Roms.Where(r => r.MaSanPham == item.MaSanPham).ToList();
                var baseRom = danhSachRom.OrderBy(r => r.Gia).FirstOrDefault();
                var selectedRom = _context.Roms.FirstOrDefault(r => r.MaRom == item.ThongSoRom);

                decimal baseRomGia = baseRom?.Gia.GetValueOrDefault() ?? 0;
                decimal selectedRomGia = selectedRom?.Gia.GetValueOrDefault() ?? 0;
                decimal donGia = (selectedRomGia - baseRomGia) + sp.DonGiaBanRa.GetValueOrDefault();

                return donGia * item.SoLuong;
            });

            return totalAmount;
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
                return View(new List<ChiTietGioHang>());
            }

            var chiTietGioHang = _context.ChiTietGioHangs
                .Where(c => c.MaGioHang == gioHang.MaGioHang)
                .ToList();

            // Tính tổng tiền và truyền vào ViewBag
            ViewBag.TotalAmount = CalculateTotal(userId);
            ViewBag.ShippingFee = 25000;
            ViewBag.FinalTotal = ViewBag.TotalAmount + ViewBag.ShippingFee;

            return View(chiTietGioHang);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(string maSanPham, string maMau, string maRom, string action)
        {
            string userId = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false });

            var gioHang = _context.GioHangs.FirstOrDefault(g => g.TenDangNhap == userId);
            if (gioHang == null)
                return Json(new { success = false });

            var item = _context.ChiTietGioHangs.SingleOrDefault(c =>
                c.MaGioHang == gioHang.MaGioHang &&
                c.MaSanPham == maSanPham &&
                c.ThongSoMau == maMau &&
                c.ThongSoRom == maRom);

            if (item != null)
            {
                if (action == "increase")
                {
                    item.SoLuong++;
                }
                else if (action == "decrease" && item.SoLuong > 1)
                {
                    item.SoLuong--;
                }

                _context.SaveChanges();

                // Tính lại tổng tiền sau khi cập nhật
                decimal newTotal = CalculateTotal(userId);
                decimal shippingFee = 25000;
                decimal finalTotal = newTotal + shippingFee;

                return Json(new
                {
                    success = true,
                    newQuantity = item.SoLuong,
                    totalAmount = newTotal,
                    shippingFee = shippingFee,
                    finalTotal = finalTotal
                });
            }

            return Json(new { success = false });
        }
    }
}
