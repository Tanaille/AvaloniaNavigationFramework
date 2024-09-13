namespace NavTest.Services
{
    public interface INavigationAware
    {
        Task OnNavigatedToAsync(object parameter);
        Task OnNavigatedFromAsync();
        Task<bool> CanNavigateAwayAsync();
    }
}