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

        public IActionResult AddToCart(string id, decimal? currentPrice = null, string? maMau = null, string? maRom = null, int quantity = 1)
        {
            string userId = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Access");
            }

            var sanPham = _context.SanPhams.SingleOrDefault(x => x.MaSanPham == id);
            if (sanPham == null) return RedirectToAction("Index", "Home");



            // Lấy danh sách màu sắc của sản phẩm
            var danhSachMau = _context.MauSacs.Where(m => m.MaSanPham == id).ToList();

            // Nếu không có mã màu (maMau) hoặc mã màu không tìm thấy, chọn màu đầu tiên
            MauSac selectedMau;
            if (string.IsNullOrEmpty(maMau))
            {
                selectedMau = danhSachMau.FirstOrDefault(); // Chọn màu đầu tiên nếu không truyền maMau
            }
            else
            {
                // Tìm màu với maMau, nếu không có thì chọn màu đầu tiên
                selectedMau = danhSachMau.FirstOrDefault(m => m.MaMau == maMau) ?? danhSachMau.FirstOrDefault();
            }



            // Lấy danh sách ROM của sản phẩm
            var danhSachRom = _context.Roms.Where(r => r.MaSanPham == id).ToList();

            // Xác định ROM được chọn
            Rom selectedRom;
            if (string.IsNullOrEmpty(maRom))
            {
                // Nếu không có maRom, chọn ROM đầu tiên
                selectedRom = danhSachRom.FirstOrDefault();
            }
            else
            {
                // Tìm ROM khớp với maRom, nếu không có thì chọn ROM đầu tiên
                selectedRom = danhSachRom.FirstOrDefault(r => r.MaRom == maRom) ?? danhSachRom.FirstOrDefault();
            }



            if (selectedMau == null || selectedRom == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin màu sắc hoặc ROM cho sản phẩm này";
                return RedirectToAction("Index", "Home");
            }

            // Khai báo gioHang ở ngoài để có thể dùng trong cả try và catch
            GioHang gioHang = null;

            try
            {
                // Kiểm tra và tạo giỏ hàng nếu chưa có
                gioHang = _context.GioHangs.FirstOrDefault(g => g.TenDangNhap == userId);
                if (gioHang == null)
                {
                    gioHang = new GioHang
                    {
                        MaGioHang = Guid.NewGuid().ToString(),
                        TenDangNhap = userId,
                        TongTien = 0
                    };
                    _context.GioHangs.Add(gioHang);
                    _context.SaveChanges();
                }

                // Kiểm tra sản phẩm trong giỏ hàng
                var chiTietGioHang = _context.ChiTietGioHangs.Where(c =>
                    c.MaGioHang == gioHang.MaGioHang &&
                    c.MaSanPham == id &&
                    c.ThongSoMau == selectedMau.MaMau &&
                    c.ThongSoRom == selectedRom.MaRom
                ).FirstOrDefault();

                if (chiTietGioHang != null)
                {
                    // Nếu đã có sản phẩm với cùng màu và ROM, tăng số lượng
                    chiTietGioHang.SoLuong += quantity;
                }
                else
                {
                    // Nếu chưa có, tạo mới chi tiết giỏ hàng
                    chiTietGioHang = new ChiTietGioHang
                    {
                        MaChiTietGioHang = Guid.NewGuid().ToString(),
                        MaGioHang = gioHang.MaGioHang,
                        MaSanPham = id,
                        ThongSoMau = selectedMau.MaMau,
                        ThongSoRom = selectedRom.MaRom,
                        SoLuong = quantity
                    };
                    _context.ChiTietGioHangs.Add(chiTietGioHang);            
                }

                // Lưu thay đổi
                _context.SaveChanges();
                //  ViewBag.CartCount = GetCartCount(userId);

                TempData["SuccessMessage"] = "Thêm sản phẩm vào giỏ hàng thành công";
                //return RedirectToAction("DetailCart");
                return RedirectToAction("index");
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi chi tiết hơn cho người dùng
                TempData["ErrorMessage"] = $"Thêm sản phẩm vào giỏ hàng thất bại"; 
                return RedirectToAction("DetailCart");
            }
        }

        [HttpPost]
        public IActionResult RemoveFromCart(string maSanPham, string? maMau, string? maRom)
        {
            string userId = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Access");

            var gioHang = _context.GioHangs.SingleOrDefault(g => g.TenDangNhap == userId);
            if (gioHang == null) return RedirectToAction("index");

            var item = _context.ChiTietGioHangs.SingleOrDefault(c =>
                c.MaGioHang == gioHang.MaGioHang &&
                c.MaSanPham == maSanPham &&
                c.ThongSoMau == maMau &&
                c.ThongSoRom == maRom);

            if (item != null)
            {
                _context.ChiTietGioHangs.Remove(item);
                //gioHang.TongTien = CalculateTotal(userId);   
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
