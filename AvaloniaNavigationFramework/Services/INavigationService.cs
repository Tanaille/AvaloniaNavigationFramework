﻿using Avalonia.Controls;
using AvaloniaNavigationFramework.Models;

namespace NavTest.Services
{
    public interface INavigationService
    {
        Task NavigateAsync(Type viewModelType);
        Task NavigateAsync(Type viewModelType, NavigationParameters parameters);
        Task GoBackAsync();
        Task GoBackAsync(NavigationParameters parameters);
        void Initialize(ContentControl contentControl, IServiceProvider serviceProvider);
    }
}