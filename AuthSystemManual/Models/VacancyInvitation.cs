namespace ResumeCheckSystem.Models
{
    public class VacancyInvitation
    {
        public int Id                  { get; set; }
        public int UserId              { get; set; }
        public int VacancyId           { get; set; }
        public string InvitationStatus { get; set; }

        public virtual Vacancy Vacancy { get; set; }           
    }
}

// INVITATION STATUS
// 1. Processing
// 2. Submitted
// 3. Denied
