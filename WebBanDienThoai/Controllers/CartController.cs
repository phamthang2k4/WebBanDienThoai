using Microsoft.AspNetCore.Mvc;

namespace WebBanDienThoai.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
