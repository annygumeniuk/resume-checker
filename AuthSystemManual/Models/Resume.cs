namespace ResumeCheckSystem.Models
{
    public class Resume
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int UserSkillId { get; set; }
        
        public virtual User User { get; set; }
        public virtual UserSkill UserSkill { get; set; }
        // public virtual Skill Skill { get; set; }
    }
}
