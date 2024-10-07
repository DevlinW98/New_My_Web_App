using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Linq;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDBContext _db;

        public PostController(ApplicationDBContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("ID");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserStatus = HttpContext.Session.GetString("Status");

            var posts = _db.Post.ToList();
            var userIds = posts.Select(p => p.Post_by_id).Distinct().ToList();
            var usernames = _db.User
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.UserName);
            

            var comment_PostIds = posts.Select(p => p.ID).Distinct().ToList();
            var comments = _db.Comments.Where(c => comment_PostIds.Contains(c.PostID)).ToList();

            ViewBag.Usernames = usernames;
            
            ViewBag.Id = userId;
            ViewBag.Comments = comments;

            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> Manage_Participants(int id)
        {
            int? userId = HttpContext.Session.GetInt32("ID");
            ViewBag.userId = userId;
            ViewBag.UserStatus = HttpContext.Session.GetString("Status");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var joinEvents = await _db.Join_Event.Where(u => u.Post_ID == id).ToListAsync();
            var post_in_user = await _db.Post.FirstOrDefaultAsync(p => p.ID == id);
            

            var userIds = joinEvents.Select(p => p.UserID).Distinct().ToList();
            var usernames = await _db.User
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "Unknown User");
            
            var user_Image_Url = _db.User
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.User_Image_Url);

                
            
            var model = Tuple.Create(joinEvents);

            if (post_in_user.Post_by_id != userId){
                return RedirectToAction("Index", "Post");
            }

            ViewBag.user_Image_Url = user_Image_Url;
            ViewBag.Usernames = usernames;
            ViewBag.post = post_in_user;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Open_Status(int id)
        {
            int? userId = HttpContext.Session.GetInt32("ID");

            var postInDb = _db.Post.FirstOrDefault(p => p.ID == id);
            
            if (postInDb == null)
            {
                return NotFound();
            }

            
            postInDb.Status = "Open";

            _db.SaveChanges();

            return RedirectToAction("Manage_Participants", "Post", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> Close_Status(int id)
        {
            int? userId = HttpContext.Session.GetInt32("ID");

            var postInDb = _db.Post.FirstOrDefault(p => p.ID == id);
            
            if (postInDb == null)
            {
                return NotFound();
            }

            
            postInDb.Status = "Close";
            
            _db.SaveChanges();

            return RedirectToAction("Manage_Participants", "Post", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            int? userId = HttpContext.Session.GetInt32("ID");

            if (userId == null)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            var joinEventInDb = _db.Join_Event.FirstOrDefault(p => p.Join_ID == id);
            
            if (joinEventInDb == null)
            {
                return NotFound();
            }

            var postInDb = _db.Post.FirstOrDefault(p => p.ID == joinEventInDb.Post_ID);
            
            if (postInDb == null)
            {
                return NotFound();
            }

            joinEventInDb.Status = "Approve";
            postInDb.Participants += 1;

            if (postInDb.Participants <= postInDb.Capacity){
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Manage_Participants", "Post", new { id = postInDb.ID });
        }


        [HttpPost]
        public IActionResult Out(int id)
        {
            int? userId = HttpContext.Session.GetInt32("ID");
            

            
            if (userId == null)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            var joinEventInDb = _db.Join_Event.FirstOrDefault(p => p.Join_ID == id );
            var postInDb = _db.Post.FirstOrDefault(p => p.ID == joinEventInDb.Post_ID);
            
            Console.WriteLine(joinEventInDb.Status);
            
            if (joinEventInDb == null)
            {
                return NotFound();
            }

            joinEventInDb.Status = "Request";
            postInDb.Participants -= 1;
            
            _db.SaveChanges();

            return RedirectToAction("Manage_Participants", "Post", new { id = postInDb.ID });
        }

        [HttpPost]
        public IActionResult Deny(int id)
        {
            int? userId = HttpContext.Session.GetInt32("ID");

            if (userId == null)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            var joinEventInDb = _db.Join_Event.FirstOrDefault(p => p.Join_ID == id);
            if (joinEventInDb == null)
            {
                return Json(new { success = false, message = "Join Event not found." });
            }

            var postInDb = _db.Post.FirstOrDefault(p => p.ID == joinEventInDb.Post_ID);
            if (postInDb == null)
            {
                return Json(new { success = false, message = "Post not found." });
            }

            _db.Join_Event.Remove(joinEventInDb);
            _db.SaveChanges();

            return RedirectToAction("Manage_Participants", "Post", new { id = joinEventInDb.Post_ID });
        }







        public IActionResult CreatePost()
        {
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserStatus = HttpContext.Session.GetString("Status");
            int? userId = HttpContext.Session.GetInt32("ID");

            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePost(Post obj)
        {
            int? userId = HttpContext.Session.GetInt32("ID");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserStatus = HttpContext.Session.GetString("Status");

            if (userId.HasValue)
            {   
                if (string.IsNullOrEmpty(obj.Post_img) && obj.Location == "Luck and Roll")
                {
                    obj.Post_img = "https://img5.pic.in.th/file/secure-sv1/1710cbac47d36f599.png";
                }
                else if (string.IsNullOrEmpty(obj.Post_img) && obj.Location == "KUMO cafe & board game")
                {
                    obj.Post_img = "https://img2.pic.in.th/pic/2f741cbb21105026a.png";
                }
                else if (string.IsNullOrEmpty(obj.Post_img) && obj.Location == "KMITL Lifelong Learning Center")
                {
                    obj.Post_img = "https://scontent.fbkk6-1.fna.fbcdn.net/v/t39.30808-6/312563861_202121498842408_5468716625227351501_n.jpg?_nc_cat=105&ccb=1-7&_nc_sid=6ee11a&_nc_eui2=AeE52MCz87Ric4ck8cUaSKZRQXmP9b57uSZBeY_1vnu5Jvr_rtQvlG60eVutMy4dbFCqY7pIa1ypBuHjVhm9jNbQ&_nc_ohc=449568RqaxgQ7kNvgGeE0zF&_nc_ht=scontent.fbkk6-1.fna&_nc_gid=Apys6uHNKTeub3zJNn7Pp5e&oh=00_AYBkFgso3nG-xukbRGpFS3TrN35IiDiPu42OcHjYo7QGyw&oe=67090B99";
                }
                
                obj.Post_Detail ??= "";
                obj.Post_by_id = userId.Value;
                obj.Participants = 1;
                obj.Status = "Open";
                _db.Post.Add(obj);
                _db.SaveChanges();

                if (obj.ID == null || userId == null)
                {
                    return Json(new { success = false, message = "Post ID or User ID is missing." });
                }

                // Check if the user has already requested to join
                var existingRequest = _db.Join_Event
                    .FirstOrDefault(r => r.Post_ID == obj.ID && r.UserID == userId.Value);

                if (existingRequest != null)
                {
                    return Json(new { success = false, message = "You have already requested to join this post." });
                }

                // Save a new join request
                var request = new Join_Event
                {
                    Post_ID = obj.ID, // Use Post_ID here
                    UserID = userId.Value,
                    Status = "Approve"
                };

                _db.Join_Event.Add(request);
                _db.SaveChanges();


                return RedirectToAction("Index");
            }
            else
            {
                return View("Error", "User ID not found in session.");
            }
        }

        [HttpGet]
        public IActionResult GetPostById(int id)
        {
            var post = _db.Post.FirstOrDefault(p => p.ID == id);
            if (post == null)
            {
                return NotFound();
            }

            var username = _db.User
                .Where(u => u.Id == post.Post_by_id)
                .Select(u => u.UserName)
                .FirstOrDefault();

            ViewBag.Username = username;

            return View(post);
        }

        public IActionResult Edit(int id)
        {
            int? userId = HttpContext.Session.GetInt32("ID");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserStatus = HttpContext.Session.GetString("Status");
            var post = _db.Post.FirstOrDefault(p => p.ID == id && p.Post_by_id == userId);
            if (post == null)
            {
                return NotFound();
            }
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserStatus = HttpContext.Session.GetString("Status");
            return View(post);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Post post)
        {
            int? userId = HttpContext.Session.GetInt32("ID");
            if (ModelState.IsValid)
            {
                var postInDb = _db.Post.FirstOrDefault(p => p.ID == post.ID && p.Post_by_id == userId);
                if (postInDb == null)
                {
                    return NotFound();
                }

                postInDb.Post_name = post.Post_name;
                postInDb.Post_Detail = post.Post_Detail ?? "";
                postInDb.Capacity = post.Capacity;
                postInDb.Date = post.Date;
                postInDb.Location = post.Location;
                postInDb.Post_img = string.IsNullOrEmpty(post.Post_img) ? "" : post.Post_img;

                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(post);
        }

        [HttpGet]
        public IActionResult Delete_Post(int id)
        {
            int? userId = HttpContext.Session.GetInt32("ID");
            var post = _db.Post.FirstOrDefault(p => p.ID == id && p.Post_by_id == userId);
            if (post == null)
            {
                return NotFound();
            }

            var joinEvents = _db.Join_Event.Where(je => je.Post_ID == id).ToList();
            _db.Join_Event.RemoveRange(joinEvents);

            _db.Post.Remove(post);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult CreateComment(string CommentText, int? id)
        {
            int? userId = HttpContext.Session.GetInt32("ID");
            Console.WriteLine($"User ID: {userId}");
            Console.WriteLine("Received Post ID: " + id);


            if (id == null || userId == null)
            {
                return Json(new { success = false, message = "Post ID or User ID is missing." ,id,userId});
            }

            if (string.IsNullOrWhiteSpace(CommentText))
            {
                return Json(new { success = false, message = "Comment text is required." });
            }

            int maxCommentId = _db.Comments.Any() ? _db.Comments.Max(c => c.CommentID) : 0;

            Comment obj = new Comment
            {
                PostID = id.Value,
                UserID = userId.Value,
                CommentText = CommentText,
                CreatedAt = DateTime.Now
            };

            _db.Comments.Add(obj);
            _db.SaveChanges();

            var user = _db.User.FirstOrDefault(u => u.Id == userId);
            string username = user != null ? user.UserName : "Unknown User";

            return RedirectToAction("Index");
        }

        public IActionResult GetComments(int id)
        {
            var comments = _db.Comments
                .AsNoTracking()
                .Where(c => c.PostID == id)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new Comment
                {
                    CommentID = c.CommentID,
                    CommentText = c.CommentText ?? string.Empty,
                    CreatedAt = c.CreatedAt,
                    PostID = c.PostID,
                    UserID = c.UserID
                })
                .ToList();

            var usernames = _db.User
                .AsNoTracking()
                .Where(u => comments.Select(c => c.UserID).Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.UserName ?? "Unknown User");

            ViewBag.Usernames = usernames;
            return PartialView("_CommentsPartial", comments);  // Create a partial view for comments
        }
        
    }
}