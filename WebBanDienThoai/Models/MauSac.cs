using System;
using System.Collections.Generic;

namespace WebBanDienThoai.Models;

public partial class MauSac
{
    public string MaMau { get; set; } = null!;

    public string? TenMau { get; set; }

    public string? MaSanPham { get; set; }

    public virtual SanPham? MaSanPhamNavigation { get; set; }
}
