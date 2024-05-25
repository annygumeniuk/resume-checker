using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeCheckSystem.Models
{
    public class Skill
    {
        public int Id           { get; set; }
        public int CategoryId   { get; set; }
        public string SkillName { get; set; }

        [ForeignKey("CategoryId")]
        public virtual SkillCategory SkillCategory { get; set; }
    }
}
