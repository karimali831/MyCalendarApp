

namespace Appology.Website.ViewModels
{
    public class BootBox
    {
        public string Title { get; set; }
        public string[] Description { get; set; }
        public bool Reload { get; set; } = false;
        public int TimeoutMs { get; set; } = 3000;
        public (string ActionName, string Controller)? Redirect { get; set; }
    }
}