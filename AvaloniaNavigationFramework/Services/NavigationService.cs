using Avalonia.Controls;
using AvaloniaNavigationFramework.Models;
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

        public Task NavigateAsync(Type viewModelType)
        {
            return NavigateAsync(viewModelType, new NavigationParameters());
        }

        public async Task NavigateAsync(Type viewModelType, NavigationParameters parameters)
        {
            EnsureInitialized();

            if (!_viewModelToViewMappings.ContainsKey(viewModelType))
            {
                throw new NavigationServiceException($"No view registered for ViewModel type {viewModelType}");
            }

            try
            {
                var viewModel = (ObservableObject)_serviceProvider.GetRequiredService(viewModelType);

                if (CurrentViewModel is INavigationAware currentNavigationAware)
                {
                    await currentNavigationAware.OnNavigatedFromAsync(parameters);
                }

                if (viewModel is INavigationAware navigationAware)
                {
                    await navigationAware.OnNavigatedToAsync(parameters);
                }

                var view = (Control)_serviceProvider.GetRequiredService(_viewModelToViewMappings[viewModelType]);
                view.DataContext = viewModel;

                _contentControl.Content = view;
                _navigationStack.Push(new NavigationItem(viewModel, parameters));

                CurrentViewModel = viewModel;
            }
            catch (Exception ex)
            {
                throw new NavigationServiceException($"Failed to navigate to {viewModelType}", ex);
            }
        }

        public Task GoBackAsync()
        {
            return GoBackAsync(new NavigationParameters());
        }

        public async Task GoBackAsync(NavigationParameters parameters)
        {
            EnsureInitialized();

            if (_navigationStack.Count <= 1)
            {
                throw new NavigationServiceException("Cannot go back. This is the first page in the navigation stack.");
            }

            try
            {
                if (CurrentViewModel is INavigationAware currentNavigationAware)
                {
                    await currentNavigationAware.OnNavigatedFromAsync(parameters);
                }

                _navigationStack.Pop();
                var previousItem = _navigationStack.Peek();

                if (previousItem.ViewModel is INavigationAware previousNavigationAware)
                {
                    await previousNavigationAware.OnNavigatedToAsync(parameters);
                }

                var view = (Control)_serviceProvider.GetRequiredService(_viewModelToViewMappings[previousItem.ViewModel.GetType()]);

                view.DataContext = previousItem.ViewModel;

                _contentControl.Content = view;
                CurrentViewModel = previousItem.ViewModel;
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
    }
}