using Microsoft.AspNetCore.Mvc;
using WebBanDienThoai.Repository;
namespace QLBanSach.ViewComponents
{
    public class HangSpMenuViewComponent: ViewComponent
    {
        private readonly IHangSpRepository _hangSp;

        public HangSpMenuViewComponent(IHangSpRepository hangSpRepository)
        {
            _hangSp = hangSpRepository;
        }
        public IViewComponentResult Invoke()
        {
            var hangsp= _hangSp.GetAllLoaiSp().OrderBy(x=>x.MaHang);
            return View(hangsp);
        }
    }
}
