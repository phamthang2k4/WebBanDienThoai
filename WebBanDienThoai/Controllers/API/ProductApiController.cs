using Microsoft.AspNetCore.Mvc;
using QLBanSach.Models.ProductModels;
using WebBanDienThoai.Models;

namespace WebBanDienThoai.Controllers.API
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductApiController : Controller
    {
        BtlLtwQlbdtContext db = new BtlLtwQlbdtContext();
        [HttpGet]
        public IEnumerable<Product> GetAllProduct()
        {
            var sanPham = (from p in db.SanPhams
                           select new Product
                           {
                               MaSanPham = p.MaSanPham,
                               TenSanPham = p.TenSanPham,
                               AnhDaiDien = p.AnhDaiDien,
                               DonGiaBanRa = p.DonGiaBanRa,
                               MaHang = p.MaHang,
                           }).ToList();
            return sanPham;
        }
        [HttpGet("{mahang}")]
        public IEnumerable<Product> GetProductsByCategory(string mahang)
        {
            var sanPham = (from p in db.SanPhams
                           where p.MaHang == mahang
                           select new Product
                           {
                               MaSanPham = p.MaSanPham,
                               TenSanPham = p.TenSanPham,
                               AnhDaiDien = p.AnhDaiDien,
                               DonGiaBanRa = p.DonGiaBanRa,
                               MaHang = p.MaHang,
                           }).ToList();
            return sanPham;
        }
    }
}
