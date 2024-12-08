
using WebBanDienThoai.Models;

namespace WebBanDienThoai.Repository
{
    public interface IHangSpRepository
    {
        Hang Add(Hang hangSp);
        Hang Update(Hang hangSp);
        Hang Delete(String maHangSp);
        Hang Get(String maHangSp);
        IEnumerable<Hang> GetAllLoaiSp();
    }
}
