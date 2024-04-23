using Microsoft.AspNetCore.Mvc;
using ResumeCheckSystem.Models;
using Microsoft.IdentityModel.Tokens;

namespace ResumeCheckSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ResumeCheckDBContext _context;

        public AccountController(ResumeCheckDBContext context)
        {
            _context = context;
        }

        public IActionResult SignUp(User newUser)
        {
            ViewBag.EmailError = null;

            if (_context.User.Any(x => x.Email == newUser.Email))
            {
                ViewBag.EmailError = "This email is already used";
                return View();
            }

            // Check if any required fields are null or empty
            if (string.IsNullOrEmpty(newUser.FirstName) ||
                string.IsNullOrEmpty(newUser.LastName) ||
                string.IsNullOrEmpty(newUser.Email) ||
                string.IsNullOrEmpty(newUser.Password))
            {
                ViewBag.EmailError = "All fields are required";
                return View();
            }

            // If all checks pass, add the user to the database
            _context.User.Add(newUser);
            _context.SaveChanges();

            HttpContext.Session.SetString("UserIdValue", newUser.Id.ToString()); // UserId
            HttpContext.Session.SetString("UserEmail", newUser.Email);

            return RedirectToAction("Index", "Home"); // Redirect to home page after successful sign up
        }

        public IActionResult LogIn(User existUser)
        {
            var user = _context.User.FirstOrDefault(x => x.Email == existUser.Email && x.Password == existUser.Password);
            
            if (user != null)
            {
                HttpContext.Session.SetString("UserIdValue", user.Id.ToString());
                HttpContext.Session.SetString("UserEmail", existUser.Email);

                return RedirectToAction("Index", "Home");
            }
            return View();           
        }

        public IActionResult LogOut() 
        {
            HttpContext.Session.Remove("UserEmail");
            return RedirectToAction("Index", "Home");
        }
    }
}
