using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeCheckSystem.Models;
using System.Diagnostics;
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

            ViewBag.SkillsByCategory = skillsByCategory;
            ViewBag.SkillCategories = categoryData;
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
            ViewBag.SkillCategories = categoryData;

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

        public string GetNormalSkillLevel(int lvl)
        {
            if      (lvl == 1) { return "Basic"; }
            else if (lvl == 2) { return "Intermediate"; }
            else               { return "Advanced"; }
        }



        public void DisplayEmployee(IEnumerable<KeyValuePair<int?, int>> biggestResult)
        {
            List<string> userNames = new List<string>();           
            List<double> scores = new List<double>();            
            Dictionary<string, List<string>> nameAndSkills = new Dictionary<string, List<string>>();
            List<string> additionalSkills = new List<string>();

            foreach (var kvp in biggestResult)
            {
                var user = _context.User.FirstOrDefault(x => x.Id == kvp.Key);

                if (user != null)
                {
                    userNames.Add($"{user.FirstName} {user.LastName}");
                    scores.Add(kvp.Value);
                    var userResume = _context.Resume
                        .Where(x => x.UserId == user.Id)
                        .Include(x => x.UserSkill)                        
                        .ThenInclude(x => x.Skill).ThenInclude(x => x.SkillCategory);

                    List<string> info = new List<string>();
                    foreach (var skl in userResume)
                    {                           
                        info.Add($"{skl.UserSkill.Skill.SkillName} - {GetNormalSkillLevel(skl.UserSkill.SkillLevel)}:: {skl.UserSkill.Skill.SkillCategory.CategoryName}");

                        if (skl.AdditionalSkills != null)
                        {
                            if (!additionalSkills.Contains(skl.AdditionalSkills))
                            {
                                additionalSkills.Add(skl.AdditionalSkills);
                            }                            
                        }
                        else
                        {
                            additionalSkills.Add("");
                        }
                    }
                    
                    nameAndSkills.Add($"{user.FirstName} {user.LastName}", info);                                       
                }
            }           

            ViewBag.UserNames = userNames;           
            ViewBag.Scores = scores;
            ViewBag.NameAndSkills = nameAndSkills;
            ViewBag.AdditionalSkills = additionalSkills;
        }

        public int FindNeededSkills(Dictionary<int, int> resume, Dictionary<int, int> vacancy, int selectedSkills, int currId)
        {
            //int selectedSkills = vacancy.Count;
            int userScore = 0;

            // get user just to write name in debug window
            var user = _context.User.FirstOrDefault(x => x.Id == currId);            

            foreach (var skill in resume)
            {
                if (vacancy.ContainsKey(skill.Key))
                {
                    //Console.WriteLine($"{skill.Key}: {skill.Value}");
                    string name = _context.Skill.FirstOrDefault(x => x.Id == skill.Key).SkillName;                    
                    userScore += skill.Value;
                }
            }

            int userFinalScore = (100 * userScore) / (selectedSkills * 3);            

            return userFinalScore;
        }

        public Dictionary<int, int> FillResumeDictionary(int id)//Resume? resume)
        {
            Dictionary<int, int> skillDictionaryResume = new Dictionary<int, int>();

            var resumes = _context.Resume.Where(x => x.UserId == id).Include(x => x.UserSkill);

            foreach (var res in resumes)
            {
                var userSkill = res.UserSkill;                
                if (userSkill != null)
                {
                    skillDictionaryResume.Add(userSkill.SkillId, userSkill.SkillLevel);                    
                }
            }

            return skillDictionaryResume;
        }

        public IActionResult FindEmployee()
        {
            HttpContext httpContext = HttpContext;
            int? userId = SessionHelper.GetUserId(httpContext);

            var userIdToSkip = Convert.ToInt32(userId); // skip vacancy owner
            
            var resumes = _context.Resume  // get all resumes
                .Where(x => x.UserId != userIdToSkip)
                .Include(x => x.UserSkill);

            var currentVacancies = _context.Vacancy // get current vacancy
                .Include(x => x.UserSkill)
                .Where(s => s.UserId == userId)
                .ToList();

            var users = _context.User // get all users
               .Where(x => x.Id != userIdToSkip)
               .ToList();

            List<int> userIdList = GetAllUsersId();           

            // initialize dictionaries to store the skills, levels and result
            Dictionary<int, int> skillDictionaryVacancy = new Dictionary<int, int>(); // here we store skills and levels from vacancy which we looking for candidate
            Dictionary<int, int> skillDictionaryResume = new Dictionary<int, int>(); // here we store resume skills and levels which we pass to get resume grade
            Dictionary<int?, int> evaluationResult = new Dictionary<int?, int>(); // store user id and its final grade

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

            foreach (var userC in users)
            {                
                skillDictionaryResume = FillResumeDictionary(userC.Id);                

                //evaluate result
                int userFinalScore = FindNeededSkills(skillDictionaryResume, skillDictionaryVacancy, selectedSkillsCounter, userC.Id);
                
                // add the results to dictionary
                evaluationResult.Add(userC.Id, userFinalScore);
                skillDictionaryResume.Clear();
            }

            var biggestResult = evaluationResult.OrderByDescending(kv => kv.Value).Take(3);
            DisplayEmployee(biggestResult);

            return View("EvaluationResult");
        }

    }
}
