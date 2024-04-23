using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeCheckSystem.Models;
using Microsoft.AspNetCore.Http;

namespace ResumeCheckSystem.Controllers
{
    public class ResumeController : Controller
    {
        private readonly ResumeCheckDBContext _context;        

        public ResumeController( ResumeCheckDBContext context)
        {        
            _context = context;
        }      

        // DISPLAY RESUME
        public IActionResult Index() 
        {
            HttpContext httpContext = HttpContext;
            if (SessionHelper.IsSessionActive(httpContext))
            {
                string userEmail = SessionHelper.GetUserEmail(httpContext);
                var user = _context.User.FirstOrDefault(u => u.Email == userEmail);                

                var resume = _context.Resume
                           .Include(r => r.UserSkill)
                           .ThenInclude(us => us.Skill)
                           .Where(x => x.UserId == user.Id)
                           .ToList();

                ViewBag.UserEmail = userEmail;
                ViewBag.FullName = $"{user.FirstName} {user.LastName}"; 
                                
                return View(resume);
            }
            else 
            {
                return RedirectToAction("Login", "Account");
            }                                    
        }

        // CREATE RESUME
        public IActionResult CreateResume()
        {
            var categoryData = _context.SkillCategory.ToList();

            var skillsByCategory = _context.Skill
            .GroupBy(s => s.CategoryId)
            .Select(g => new { CategoryId = g.Key, Skills = g.ToList() })
            .ToList();

            var skillCategories = _context.SkillCategory.ToList();

            ViewBag.SkillsByCategory = skillsByCategory;
            ViewBag.SkillCategories = skillCategories;            
            return View(categoryData);
        }


        // RESUME CREATING FORM PROSESSING
        [HttpPost]
        public IActionResult CreateResumeForm(IEnumerable<string> selectedSkills, IEnumerable<int> skillLevels)
        {
            HttpContext httpContext = HttpContext;                        
            int? userId = SessionHelper.GetUserId(httpContext);

            foreach (var pair in selectedSkills.Zip(skillLevels, (skill, level) => new { Skill = skill, Level = level }))
            {
                // Find the SkillId corresponding to the selected skill
                var skillId = _context.Skill.FirstOrDefault(s => s.SkillName == pair.Skill)?.Id;

                // If the skill exists, create a new UserSkill object and add it to the context
                if (skillId != null)
                {
                    var userSkill = new UserSkill
                    {
                        SkillId = skillId.Value,
                        SkillLevel = pair.Level
                    };

                    _context.UserSkill.Add(userSkill);
                    _context.SaveChanges();
                    
                    var resume = new Resume
                    {
                        UserId = Convert.ToInt32(userId),
                        // Use the UserSkill ID instead of SkillId
                        UserSkillId = userSkill.Id
                    };
                    _context.Resume.Add(resume);
                }
            }

            // Save changes to the database
            _context.SaveChanges();

            return RedirectToAction("Index"); // redirecting to page with resume displayed
        }

        public IActionResult DeleteProsessing()
        {
            string selectedValue = Request.Form["choose"];
            
            if (selectedValue == "Yes")
            {
                HttpContext httpContext = HttpContext;
                int? userId = SessionHelper.GetUserId(httpContext); // get current user id to delete it`s resume
                var resume = _context.Resume.FirstOrDefault(s => s.UserId == userId); // findin`t the resume            
                _context.Database.ExecuteSqlRaw("DELETE FROM Resume WHERE UserId = {0}", userId);
                _context.SaveChanges(); // saving
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Resume");
            }            
        }

        // DELETE RESUME
        public IActionResult DeleteResume()
        {
            ViewBag.Delete = "Are you sure?";
            return View();
        }

        public IActionResult EditResume()
        { 
            return View();
        }

        // EDIT RESUME
    }
}
