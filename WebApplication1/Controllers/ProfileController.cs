using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ProfileController : Controller
    {

        private readonly ApplicationDBContext _db;

        public ProfileController(ApplicationDBContext db)
        {
            _db = db;
        }
        [HttpGet]
        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("ID");
            ViewBag.UserStatus = HttpContext.Session.GetString("Status");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            var user = _db.User.FirstOrDefault(p => p.Id == userId);
            if (user == null)
            {
                return NotFound(); // Return 404 if post not found
            }
            return View(user); // Show the post if found
        }
        public IActionResult EditProfile()
        {
            int? userId = HttpContext.Session.GetInt32("ID");
            ViewBag.UserStatus = HttpContext.Session.GetString("Status");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            var user = _db.User.FirstOrDefault(p => p.Id == userId);
            if (user == null)
            {
                return NotFound(); // Return 404 if post not found
            }
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(User obj)
        {
            int? userId = HttpContext.Session.GetInt32("ID");
            var user = _db.User.FirstOrDefault(p => p.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            // อัปเดตข้อมูลผู้ใช้โดยไม่ต้องตรวจสอบรหัสผ่าน
            user.User_Image_Url = obj.User_Image_Url;  // เปลี่ยนชื่อให้ตรงกับโมเดล
            user.UserName = obj.UserName;
            user.Age = obj.Age;
            user.Email = obj.Email;
            if (obj.Description != null){
                user.Description = obj.Description;
            }
            _db.SaveChanges();

            HttpContext.Session.SetString("UserName", obj.UserName);
            return RedirectToAction("Index");
        }

            
            
        }


}
