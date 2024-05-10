using Microsoft.EntityFrameworkCore;

namespace ResumeCheckSystem.Models
{
    public class ResumeCheckDBContext : DbContext
    {
        public DbSet<User>              User              { get; set; }
        public DbSet<SkillCategory>     SkillCategory     { get; set; }
        public DbSet<Skill>             Skill             { get; set; }
        public DbSet<UserSkill>         UserSkill         { get; set; }
        public DbSet<Resume>            Resume            { get; set; }
        public DbSet<Vacancy>           Vacancy           { get; set; }
        public DbSet<VacancyInvitation> VacancyInvitation { get; set; }

        public ResumeCheckDBContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}
