using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebBanDienThoai.Models;
using X.PagedList;

namespace WebBanDienThoai.Controllers
{
    public class HomeController : Controller
    {
        BtlLtwQlbdtContext db = new BtlLtwQlbdtContext();
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var lstSanPham = db.SanPhams.AsNoTracking().OrderBy(x => x.MaHang);
            PagedList<SanPham> lst = new PagedList<SanPham>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }

        [HttpGet]
        public int GetCartQuantity()
        {
            var userId = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(userId))
                return 0;

            var gioHang = db.GioHangs.FirstOrDefault(g => g.TenDangNhap == userId);
            if (gioHang == null)
                return 0;

            // Tính tổng số lượng sản phẩm trong giỏ hàng
            return db.ChiTietGioHangs
                .Where(c => c.MaGioHang == gioHang.MaGioHang)
                .Sum(c => c.SoLuong) ?? 0;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
