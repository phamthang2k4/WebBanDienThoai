﻿using Microsoft.AspNetCore.Mvc;

namespace WebBanDienThoai.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
