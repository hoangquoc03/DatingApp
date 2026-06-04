namespace DatingApp.DTOs
{
    public class OnboardingDto
    {
        public string? LookingFor { get; set; }
        public List<string>? Interests { get; set; }
        public string? Lifestyle { get; set; }
        public List<string>? Values { get; set; }
        public int? Distance { get; set; }
        public string? Vibe { get; set; }
        
        // New fields for extended onboarding
        public DatingApp.Enums.Gender? Gender { get; set; }
        public DatingApp.Enums.Gender? InterestedIn { get; set; }
        public int? Height { get; set; }
        public string? Smoking { get; set; }
        public string? Drinking { get; set; }
        public string? Education { get; set; }
        public string? Bio { get; set; }
    }
}
