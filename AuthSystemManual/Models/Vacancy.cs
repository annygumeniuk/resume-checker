using System.ComponentModel.DataAnnotations;

namespace ResumeCheckSystem.Models
{
    public class Vacancy
    {
        public int Id             { get; set; }
        public int UserId         { get; set; }
        public int UserSkillId    { get; set; }
        [Required]
        public string Title       { get; set; }
        [Required]
        public string Description { get; set; }

        public virtual User User           { get; set; }
        public virtual UserSkill UserSkill { get; set; }
    }
}
