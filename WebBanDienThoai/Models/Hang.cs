using System;
using System.Collections.Generic;

namespace WebBanDienThoai.Models;

public partial class Hang
{
    public string MaHang { get; set; } = null!;

    public string? TenHang { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
