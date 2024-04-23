namespace ResumeCheckSystem.Models
{
    public class UserSkill
    {
        public int Id { get; set; }
        public int SkillId { get; set; }
        public int SkillLevel { get; set; }

        public virtual Skill Skill { get; set; }
    }
}
