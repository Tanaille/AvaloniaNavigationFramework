using Avalonia.Controls;

namespace NavTest.Services
{
    public interface INavigationService
    {
        Task NavigateAsync(Type viewModelType, CancellationToken cancellationToken = default);
        Task NavigateAsync(Type viewModelType, object parameter, CancellationToken cancellationToken = default);
        Task GoBackAsync(CancellationToken cancellationToken = default);
        void Initialize(ContentControl contentControl, IServiceProvider serviceProvider);
    }
}