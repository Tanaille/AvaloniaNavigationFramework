using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using NavTest.Exceptions;

namespace NavTest.Services
{
    public class NavigationService : ObservableObject, INavigationService
    {
        private readonly Dictionary<Type, Type> _viewModelToViewMappings = new Dictionary<Type, Type>();
        private readonly Stack<NavigationItem> _navigationStack = new Stack<NavigationItem>();
        private ContentControl _contentControl;
        private IServiceProvider _serviceProvider;

        private ObservableObject _currentViewModel;
        public ObservableObject CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public void Initialize(ContentControl contentControl, IServiceProvider serviceProvider)
        {
            _contentControl = contentControl ?? throw new ArgumentNullException(nameof(contentControl));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void RegisterView<TViewModel, TView>()
            where TViewModel : ObservableObject
            where TView : Control
        {
            if (_viewModelToViewMappings.ContainsKey(typeof(TViewModel)))
            {
                throw new NavigationServiceException($"A view is already registered for ViewModel type {typeof(TViewModel)}");
            }

            _viewModelToViewMappings[typeof(TViewModel)] = typeof(TView);
        }

        public Task NavigateAsync(Type viewModelType, CancellationToken cancellationToken = default)
        {
            return NavigateAsync(viewModelType, null, cancellationToken);
        }

        public async Task NavigateAsync(Type viewModelType, object parameter, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            if (!_viewModelToViewMappings.ContainsKey(viewModelType))
            {
                throw new NavigationServiceException($"No view registered for ViewModel type {viewModelType}");
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var viewModel = (ObservableObject)_serviceProvider.GetRequiredService(viewModelType);

                if (CurrentViewModel is INavigationAware currentNavigationAware)
                {
                    if (!await currentNavigationAware.CanNavigateAwayAsync())
                    {
                        throw new NavigationCanceledException("Navigation was cancelled by the current view model.");
                    }
                    await currentNavigationAware.OnNavigatedFromAsync();
                }

                if (viewModel is INavigationAware navigationAware)
                {
                    await navigationAware.OnNavigatedToAsync(parameter);
                }

                var view = (Control)_serviceProvider.GetRequiredService(_viewModelToViewMappings[viewModelType]);
                view.DataContext = viewModel;

                _contentControl.Content = view;
                _navigationStack.Push(new NavigationItem(viewModel, parameter));
                CurrentViewModel = viewModel;
            }
            catch (OperationCanceledException)
            {
                throw new NavigationCanceledException("Navigation was cancelled.");
            }
            catch (Exception ex)
            {
                throw new NavigationServiceException($"Failed to navigate to {viewModelType}", ex);
            }
        }

        public async Task GoBackAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            if (_navigationStack.Count <= 1)
            {
                throw new NavigationServiceException("Cannot go back. This is the first page in the navigation stack.");
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (CurrentViewModel is INavigationAware currentNavigationAware)
                {
                    if (!await currentNavigationAware.CanNavigateAwayAsync())
                    {
                        throw new NavigationCanceledException("Navigation was cancelled by the current view model.");
                    }
                    await currentNavigationAware.OnNavigatedFromAsync();
                }

                _navigationStack.Pop();
                var previousItem = _navigationStack.Peek();

                if (previousItem.ViewModel is INavigationAware previousNavigationAware)
                {
                    await previousNavigationAware.OnNavigatedToAsync(previousItem.Parameter);
                }

                var view = (Control)_serviceProvider.GetRequiredService(_viewModelToViewMappings[previousItem.ViewModel.GetType()]);
                view.DataContext = previousItem.ViewModel;

                _contentControl.Content = view;
                CurrentViewModel = previousItem.ViewModel;
            }
            catch (OperationCanceledException)
            {
                throw new NavigationCanceledException("Navigation was cancelled.");
            }
            catch (Exception ex)
            {
                throw new NavigationServiceException("Failed to navigate back", ex);
            }
        }

        private void EnsureInitialized()
        {
            if (_contentControl == null || _serviceProvider == null)
            {
                throw new NavigationServiceException("NavigationService has not been properly initialized. Make sure Initialize method is called with a valid ContentControl and IServiceProvider.");
            }
        }

        private class NavigationItem
        {
            public ObservableObject ViewModel { get; }
            public object Parameter { get; }

            public NavigationItem(ObservableObject viewModel, object parameter)
            {
                ViewModel = viewModel;
                Parameter = parameter;
            }
        }
    }

    public class NavigationCanceledException : Exception
    {
        public NavigationCanceledException(string message) : base(message) { }
    }
}