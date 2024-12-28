﻿using Microsoft.AspNetCore.Mvc;
using WebBanDienThoai.Helpers;
using WebBanDienThoai.Models;
using WebBanDienThoai.ViewModels;

namespace WebBanDienThoai.Controllers
{
    public class AccessController : Controller
    {
        //BtlLtwQlbdtContext db = new BtlLtwQlbdtContext();
        private readonly BtlLtwQlbdtContext db;

        public AccessController(BtlLtwQlbdtContext context)
        {
            db = context;

        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public IActionResult Login(TaiKhoan user)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                string hashedPassword = user.MatKhau.ToSHA256Hash("MySaltKey");

                // Kiểm tra tài khoản tồn tại
                var taiKhoan = db.TaiKhoans.FirstOrDefault(tk =>
                    tk.TenDangNhap == user.TenDangNhap &&
                    tk.MatKhau == hashedPassword);

                if (taiKhoan != null)
                {
                    if (taiKhoan.LoaiTaiKhoan == "customer")
                    {
                        // Xử lý đăng nhập cho khách hàng
                        var userInfo = (from tk in db.TaiKhoans
                                        join kh in db.KhachHangs on tk.TenDangNhap equals kh.TenDangNhap
                                        where tk.TenDangNhap == user.TenDangNhap
                                        select new
                                        {
                                            tk.TenDangNhap,
                                            kh.AnhDaiDien,
                                            kh.MaKhachHang,
                                            kh.TenKhachHang,
                                            kh.NgaySinh,
                                            kh.SoDienThoai,
                                            kh.DiaChi,
                                            kh.Email,
                                            kh.GhiChu
                                        }).FirstOrDefault();

                        if (userInfo != null)
                        {
                            HttpContext.Session.SetString("Username", userInfo.TenDangNhap);
                            HttpContext.Session.SetString("MaKhachHang", userInfo.MaKhachHang);
                            HttpContext.Session.SetString("HoTen", userInfo.TenKhachHang);
                            HttpContext.Session.SetString("NgaySinh", $"{userInfo.NgaySinh}");
                            HttpContext.Session.SetString("SoDienThoai", userInfo.SoDienThoai);
                            HttpContext.Session.SetString("DiaChi", userInfo.DiaChi);
                            HttpContext.Session.SetString("Email", userInfo.Email);
                            HttpContext.Session.SetString("GhiChu", userInfo.GhiChu ?? "");
                            HttpContext.Session.SetString("Avatar", Url.Content("~/Images/Customer/" + userInfo.AnhDaiDien));
                            HttpContext.Session.SetString("Role", "Customer");

                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (taiKhoan.LoaiTaiKhoan == "admin")
                    {
                        // Xử lý đăng nhập cho admin
                        var adminInfo = (from tk in db.TaiKhoans
                                         join nv in db.NhanViens on tk.TenDangNhap equals nv.TenDangNhap
                                         where tk.TenDangNhap == user.TenDangNhap
                                         select new
                                         {
                                             tk.TenDangNhap,
                                             nv.AnhDaiDien,
                                             nv.MaNhanVien,
                                             nv.TenNhanVien,
                                             nv.NgaySinh,
                                             nv.SoDienThoai,
                                             nv.DiaChi,
                                             nv.ChucVu,
                                             nv.GhiChu
                                         }).FirstOrDefault();

                        if (adminInfo != null)
                        {
                            HttpContext.Session.SetString("Username", adminInfo.TenDangNhap);
                            HttpContext.Session.SetString("MaNhanVien", adminInfo.MaNhanVien);
                            HttpContext.Session.SetString("HoTen", adminInfo.TenNhanVien);
                            HttpContext.Session.SetString("NgaySinh", $"{adminInfo.NgaySinh}");
                            HttpContext.Session.SetString("SoDienThoai", adminInfo.SoDienThoai);
                            HttpContext.Session.SetString("DiaChi", adminInfo.DiaChi);
                            HttpContext.Session.SetString("ChucVu", adminInfo.ChucVu);
                            HttpContext.Session.SetString("GhiChu", adminInfo.GhiChu ?? "");
                            HttpContext.Session.SetString("Avatar", Url.Content("~/Images/Admin/" + adminInfo.AnhDaiDien));
                            HttpContext.Session.SetString("Role", "Admin");

                            return RedirectToAction("Index", "HomeAdmin", new { area = "Admin" });
                        }
                    }
                }

                ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Xóa session
            HttpContext.Session.Clear();

            // Xóa cookies nếu có
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            // Redirect về trang login
            return RedirectToAction("Login", "Access");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterVM model, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên đăng nhập
                var existingAccount = db.TaiKhoans.FirstOrDefault(x => x.TenDangNhap == model.TaiKhoan);
                if (existingAccount != null)
                {
                    ModelState.AddModelError("TaiKhoan", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }


                var khachHang = new KhachHang
                {
                    MaKhachHang = MyUtil.GenerateRamdomKey(),
                    TenKhachHang = model.HoTen,
                    NgaySinh = model.NgaySinh,
                    SoDienThoai = model.DienThoai,
                    DiaChi = model.DiaChi,
                    Email = model.Email,
                    TenDangNhap = model.TaiKhoan,
                    // Thêm 2 trường tọa độ
                    DiaChiLatitude = model.DiaChiLatitude,
                    DiaChiLongitude = model.DiaChiLongitude
                };


                string hashedPassword = model.MatKhau.ToSHA256Hash("MySaltKey");
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.TaiKhoan,
                    MatKhau = hashedPassword,
                    LoaiTaiKhoan = "customer"
                };


                if (Hinh != null)
                {
                    khachHang.AnhDaiDien = MyUtil.UploadHinh(Hinh, "Customer");

                }

                db.KhachHangs.Add(khachHang);
                db.TaiKhoans.Add(taiKhoan);
                db.SaveChanges();

                // Set session values
                HttpContext.Session.SetString("Username", khachHang.TenDangNhap);
                HttpContext.Session.SetString("HoTen", khachHang.TenKhachHang);
                HttpContext.Session.SetString("NgaySinh", khachHang.NgaySinh.ToString());
                HttpContext.Session.SetString("SoDienThoai", khachHang.SoDienThoai);
                HttpContext.Session.SetString("DiaChi", khachHang.DiaChi);
                HttpContext.Session.SetString("Email", khachHang.Email);
                HttpContext.Session.SetString("GhiChu", khachHang.GhiChu ?? "");
                HttpContext.Session.SetString("Avatar", Url.Content("~/Images/Customer/" + khachHang.AnhDaiDien));
                HttpContext.Session.SetString("Role", "Customer");

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

    }
}
