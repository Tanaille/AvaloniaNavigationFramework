using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaNavigationFramework.Models
{
    public class NavigationItem
    {
        public ObservableObject ViewModel { get; }
        public NavigationParameters Parameters { get; }

        public NavigationItem(ObservableObject viewModel, NavigationParameters parameters)
        {
            ViewModel = viewModel;
            Parameters = parameters;
        }
    }
}
