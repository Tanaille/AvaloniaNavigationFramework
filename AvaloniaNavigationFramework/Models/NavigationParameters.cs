namespace AvaloniaNavigationFramework.Models
{
    public class NavigationParameters : Dictionary<string, object>
    {
        public NavigationParameters() : base() { }
        public NavigationParameters(IDictionary<string, object> dictionary) : base(dictionary) { }
    }
}
