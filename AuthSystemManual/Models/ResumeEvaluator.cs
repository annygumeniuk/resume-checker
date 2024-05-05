namespace ResumeCheckSystem.Models
{
    public class ResumeEvaluator
    {
        public int userFinalScore            { get; set; }   
        public int userScore                 { get; set; }
        public int selectedSkillsCount       { get; set; }
        Dictionary<int, int> resumeSkills    { get; set; }
        Dictionary<int, int> vacancySkills   { get; set; }


        public void SetData(int userScoreSet, int selectedSkillsCountSet, Dictionary<int, int> resumeSkillsSet, Dictionary<int, int> vacancySkillsSet)
        { 
            userScore = userScoreSet;
            selectedSkillsCount = selectedSkillsCountSet;
            resumeSkills = resumeSkillsSet;
            vacancySkills = vacancySkillsSet;
        }

        public int FindNeededSkills1()
        {            
            int userScore = 0;

            foreach (var skill in this.resumeSkills)
            {
                if (this.vacancySkills.ContainsKey(skill.Key))
                {                    
                    userScore += skill.Value;
                }
            }

            int userFinalScore = (100 * userScore) / (this.selectedSkillsCount * 3);

            return userFinalScore;
        }

        public int FindNeededSkills(Dictionary<int, int> resume, Dictionary<int, int> vacancy, int selectedSkills)
        {
            //int selectedSkills = vacancy.Count;
            int userScore = 0;

            foreach (var skill in resume)
            {
                if (vacancy.ContainsKey(skill.Key))
                {
                    Console.WriteLine($"{skill.Key}: {skill.Value}");
                    userScore += skill.Value;
                }
            }

            int userFinalScore = (100 * userScore) / (selectedSkills * 3);

            return userFinalScore;
        }
    }
}
