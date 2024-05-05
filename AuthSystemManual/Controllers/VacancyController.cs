﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeCheckSystem.Models;
using System.Text.Json;

namespace ResumeCheckSystem.Controllers
{
    public class VacancyController : Controller
    {
        private readonly ResumeCheckDBContext _context;

        public VacancyController(ResumeCheckDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            HttpContext httpContext = HttpContext;
            if (SessionHelper.IsSessionActive(httpContext))
            {
                string userEmail = SessionHelper.GetUserEmail(httpContext);
                var user = _context.User.FirstOrDefault(u => u.Email == userEmail);

                var vacancy = _context.Vacancy
                           .Include(r => r.UserSkill)
                           .ThenInclude(us => us.Skill)
                           .Where(x => x.UserId == user.Id)
                           .ToList();
                
                var vacancyData = _context.Vacancy.FirstOrDefault(s => s.UserId == user.Id);

                if (vacancy.Count == 0)
                {
                    ViewBag.Display = "0";
                }
                else
                {
                    ViewBag.Title = $"{vacancyData.Title}";
                    ViewBag.Description = $"{vacancyData.Description}";
                }

                return View(vacancy);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public IActionResult CreateVacancy()
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

        void AddVacancyToDb(IEnumerable<string> selectedSkills, IEnumerable<int> skillLevels, string Title, string Description, int? userId)
        {
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

                    var vacancy = new Vacancy
                    {
                        Title = Title,
                        Description = Description,
                        UserId = Convert.ToInt32(userId),                        
                        UserSkillId = userSkill.Id
                    };
                    _context.Vacancy.Add(vacancy);
                }
            }
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult CreateVacancyForm(IEnumerable<string> selectedSkills, IEnumerable<int> skillLevels, string Title, string Description)
        {
            HttpContext httpContext = HttpContext;
            int? userId = SessionHelper.GetUserId(httpContext);

            AddVacancyToDb(selectedSkills, skillLevels, Title, Description, userId);

            return RedirectToAction("Index"); // redirecting to page with resume displayed
        }

        public IActionResult EditVacancy()
        {
            var categoryData = _context.SkillCategory.ToList();

            // sort skills by category
            var skillsByCategory = _context.Skill
            .GroupBy(s => s.CategoryId) // sorting with category id
            .Select(g => new { CategoryId = g.Key, Skills = g.ToList() })
            .ToList();

            var skillCategories = _context.SkillCategory.ToList();

            // get skills user already has in it`s resume
            HttpContext httpContext = HttpContext;
            int? userId = SessionHelper.GetUserId(httpContext);
            // Get resume skills for the user
            var vacancySkills = _context.Vacancy
             .Where(x => x.UserId == userId)
             .Include(x => x.UserSkill)
             .ToList();

            // Passing data to the page
            ViewBag.SkillFromVacancy = vacancySkills; // change in view
            ViewBag.SkillsByCategory = skillsByCategory;
            ViewBag.SkillCategories = skillCategories;

            var currentVacancy = _context.Vacancy.FirstOrDefault(s => s.UserId == userId);
            ViewBag.Title = $"{currentVacancy.Title}";
            ViewBag.Description = $"{currentVacancy.Description}";

            return View(categoryData);
        }

        // PROCESSING EDITING FORM
        [HttpPost]
        public IActionResult EditVacancyForm(IEnumerable<string> selectedSkills, IEnumerable<int> skillLevels, string Title, string Description)
        {
            HttpContext httpContext = HttpContext;
            int? userId = SessionHelper.GetUserId(httpContext);                        
            
            var currentVacancy = _context.Vacancy.FirstOrDefault(s => s.UserId == userId);
                                    
            if (Title == null)
            {
                Title = currentVacancy.Title;
            }
            if (Description == null) 
            {
                Description = currentVacancy.Description;
            }

            _context.Database.ExecuteSqlRaw("DELETE FROM Vacancy WHERE UserId = {0}", userId);            
            AddVacancyToDb(selectedSkills, skillLevels, Title, Description, userId);

            return RedirectToAction("Index", "Home");
        }

        // DELETE VACANCY
        public IActionResult DeleteVacancy()
        {            
            return View();
        }

        public IActionResult DeleteProcessing()
        {
            string selectedValue = Request.Form["choose"];

            if (selectedValue == "Yes")
            {
                HttpContext httpContext = HttpContext;
                int? userId = SessionHelper.GetUserId(httpContext); // get current user id to delete it`s vacancy
                var resume = _context.Resume.FirstOrDefault(s => s.UserId == userId); // findin`t the vacancy            
                _context.Database.ExecuteSqlRaw("DELETE FROM Vacancy WHERE UserId = {0}", userId);
                _context.SaveChanges(); // saving
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Vacancy");
            }
        }

        public List<int> GetAllUsersId()
        {
            List<int> userIdList = _context.User
                .Select(u => u.Id)
                .ToList(); // get all user`s id

            return userIdList;
        }     
                

        public IActionResult FindEmployee()
        {
            HttpContext httpContext = HttpContext;
            int? userId = SessionHelper.GetUserId(httpContext);

            var userIdToSkip = Convert.ToInt32(userId);
            var resumes = _context.Resume
                .Where(x => x.UserId != userIdToSkip)
                .Include(x => x.UserSkill);

            var currentVacancies = _context.Vacancy
                .Include(x => x.UserSkill)
                .Where(s => s.UserId == userId)
                .ToList(); // Execute the query to get actual Vacancy objects

            List<int> userIdList = GetAllUsersId();
            ResumeEvaluator resumeEvaluator = new ResumeEvaluator();

            // initialize dictionaries to store the skills, levels and result
            Dictionary<int, int> skillDictionaryVacancy = new Dictionary<int, int>();
            Dictionary<int, int> skillDictionaryResume = new Dictionary<int, int>();
            Dictionary<int?, int> evaluationResult = new Dictionary<int?, int>();

            int selectedSkillsCounter = 0;
            foreach (var vacancy in currentVacancies)
            {                
                var userSkill = vacancy.UserSkill;
                if (userSkill != null)
                {             
                    skillDictionaryVacancy.Add(userSkill.SkillId, userSkill.SkillLevel);
                    selectedSkillsCounter++;
                }
            }
            
            int currentId = userIdList[0];
            foreach (var resume in resumes)
            {
                int idInLoop = resume.UserId;

                if (currentId != idInLoop)
                {
                    currentId = idInLoop;
                    int userFinalScore = 0;                   
                    userFinalScore = resumeEvaluator.FindNeededSkills(skillDictionaryResume, skillDictionaryVacancy, selectedSkillsCounter);
                    evaluationResult.Add(currentId, userFinalScore);
                    // drop dictionary data to add resume skills for another user
                    skillDictionaryResume.Clear();
                }
                else
                {
                    var userSkill = resume.UserSkill;
                    if (userSkill != null)
                    {
                        // add all skills to the dictionary for the resumes with the same userid
                        skillDictionaryResume.Add(userSkill.SkillId, userSkill.SkillLevel);
                    }
                }                
            }

            var largestKeys = evaluationResult.Values.OrderByDescending(k => k).Take(3);
            
            return View("EvaluationResult");
        }

    }
}
