using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using WebApplication1.Data;
using WebApplication1.Models;
using System;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDBContext _db;

        public LoginController(ApplicationDBContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(User obj) 
        {

            if (ModelState.IsValid) 
            {
                var user = _db.User.FirstOrDefault(u => u.UserName == obj.UserName && u.Password == obj.Password);

                if (user != null)
                {
                    HttpContext.Session.SetString("Status", "Login");
                    HttpContext.Session.SetString("UserName", user.UserName);
                    HttpContext.Session.SetInt32("ID", user.Id); 
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            return View(obj); 
        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User obj)
        {
            try
            {

                List<string> items = new List<string> 
                { 
                    "https://img2.pic.in.th/pic/11efd141fe89bd1982.png",
                    "https://img2.pic.in.th/pic/22ea893238db82eeed.png",
                    "https://img5.pic.in.th/file/secure-sv1/3357cec7fd7d01f25f.png",
                    "https://img2.pic.in.th/pic/447a878b979614d151.png",
                    "https://img5.pic.in.th/file/secure-sv1/55395bcc72f4abe112.png",
                    "https://img5.pic.in.th/file/secure-sv1/66a43be3ea6ecc74c8.png",
                    "https://img2.pic.in.th/pic/88b2654adefc96e522.png",
                    "https://img5.pic.in.th/file/secure-sv1/99fabb6c8f1c876281.png"
                };
                Random random = new Random();
                int index = random.Next(items.Count);
                string randomItem = items[index];
                obj.User_Image_Url = randomItem;
                _db.User.Add(obj); 
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return View("Error", ex.Message);
            }

            return RedirectToAction("Index");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Status");
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("ID");
            return RedirectToAction("Index", "Home"); // เปลี่ยนไปที่หน้า Home
        }
    }
}
