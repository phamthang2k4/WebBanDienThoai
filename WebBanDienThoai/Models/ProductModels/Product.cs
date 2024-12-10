namespace QLBanSach.Models.ProductModels
{
    public class Product
    {
        public string MaSanPham { get; set; } = null!;

        public string? TenSanPham { get; set; }

        public string? AnhDaiDien { get; set; }

        public decimal? DonGiaBanRa { get; set; }

        public string? MaHang { get; set; }
    }
}
