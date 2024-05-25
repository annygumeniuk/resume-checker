using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ResumeCheckSystem.Models;
using static Azure.Core.HttpHeader;

namespace ResumeCheckSystem.Controllers
{
    public class InvitationController : Controller
    {
        private readonly ResumeCheckDBContext _context;

        public InvitationController(ResumeCheckDBContext context)
        { 
            _context = context;
        }
       
        public IActionResult ProcessInvitation(string userName)
        {
            // Get currently user or user who created vacancy
            HttpContext httpContext = HttpContext;
            int? userId = SessionHelper.GetUserId(httpContext);

            // Get Vacancy id
            var vacancy = _context.Vacancy
                .FirstOrDefault(x => x.UserId == userId);
            
            // Get user data to sent invitation
            string[] fullName = userName.Split(' ');
            string firstName = fullName[0];
            string lastName = fullName[1];

            var userData = _context.User
                .FirstOrDefault(x => x.FirstName == firstName && x.LastName == lastName);

            VacancyInvitation vacancyInvitation = new VacancyInvitation
            {
                UserId = userData.Id,
                VacancyId = vacancy.Id,
                InvitationStatus = "Processing"
            };

            // Check if we already invited some user
            var checkIfExist = _context.VacancyInvitation
                .Where(x => x.UserId == vacancyInvitation.UserId && x.VacancyId == vacancyInvitation.VacancyId);
                        
            if (checkIfExist.IsNullOrEmpty())
            {             
                _context.Add(vacancyInvitation);
                _context.SaveChanges();
            }                      
            
            return RedirectToAction("Index", "Home");
        }

        // display vacancy details
        public IActionResult VacancyDetails(int ownerUserId)
        {
            var vacancy = _context.Vacancy
                         .Where(x => x.UserId == ownerUserId)
                         .Include(r => r.UserSkill)
                         .ThenInclude(us => us.Skill)
                         .ToList();

            var vac = _context.Vacancy.FirstOrDefault(x => x.UserId == ownerUserId);

            ViewBag.Title = vac.Title;
            ViewBag.Description = vac.Description;
            ViewBag.Data = vacancy;
            
            return View(vacancy);
        }

        public IActionResult SubmitInvitation(int invitationId)
        {
            var invitation = _context.VacancyInvitation.FirstOrDefault(x => x.Id == invitationId);

            if (invitation != null)
            {
                invitation.InvitationStatus = "Submitted";
                _context.SaveChanges();
            }            
            
            return RedirectToAction("Index", "Home");
        }

        public IActionResult DenyInvitation(int invitationId)
        {
            var invitation = _context.VacancyInvitation.FirstOrDefault(x => x.Id == invitationId);

            if (invitation != null)
            {
                invitation.InvitationStatus = "Denied";
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult DeleteInvitation(int invitationId)
        {
            var invitation = _context.VacancyInvitation.FirstOrDefault(x => x.Id == invitationId);
            
            if (invitation != null)
            {
                _context.Database.ExecuteSqlRaw("DELETE FROM VacancyInvitation WHERE Id = {0}", invitation.Id);
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Home");
        }

    }
}
