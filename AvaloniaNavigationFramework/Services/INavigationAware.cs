using AvaloniaNavigationFramework.Models;

namespace NavTest.Services
{
    public interface INavigationAware
    {
        Task OnNavigatedToAsync(NavigationParameters parameters);
        Task OnNavigatedFromAsync(NavigationParameters parameters);
    }
}