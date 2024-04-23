using ResumeCheckSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace ResumeCheckSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ResumeCheckDBContext _context;

        public HomeController(ILogger<HomeController> logger, ResumeCheckDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail.IsNullOrEmpty())
            {
                ViewBag.Message = "SignUp or LogIn to use services";
                return View();
            }
            else
            {
                ViewBag.UserEmail = userEmail;
                var data = _context.User.Where(x=> x.Email == userEmail).ToList();
                var user = _context.User.FirstOrDefault(u => u.Email == userEmail);
                int userId = user.Id;                
                return View(data);
            }            
        }
       
        public IActionResult Privacy()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserEmail = userEmail;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
