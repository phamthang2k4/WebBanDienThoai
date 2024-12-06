using Microsoft.AspNetCore.Mvc;

namespace WebBanDienThoai.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
