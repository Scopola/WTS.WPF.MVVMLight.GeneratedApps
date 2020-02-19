﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

using ForcedLogin.Contracts.Services;
using ForcedLogin.Contracts.ViewModels;
using ForcedLogin.Core.Contracts.Services;
using ForcedLogin.Models;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace ForcedLogin.ViewModels
{
    // TODO WTS: Change the URL for your privacy policy in the appsettings.json file, currently set to https://YourPrivacyUrlGoesHere
    public class SettingsViewModel : ViewModelBase, INavigationAware
    {
        private readonly AppConfig _config;
        private readonly IUserDataService _userDataService;
        private readonly IIdentityService _identityService;
        private readonly IThemeSelectorService _themeSelectorService;
        private readonly ISystemService _systemService;
        private AppTheme _theme;
        private string _versionDescription;
        private UserViewModel _user;
        private ICommand _setThemeCommand;
        private ICommand _privacyStatementCommand;
        private ICommand _logOutCommand;

        public AppTheme Theme
        {
            get { return _theme; }
            set { Set(ref _theme, value); }
        }

        public string VersionDescription
        {
            get { return _versionDescription; }
            set { Set(ref _versionDescription, value); }
        }

        public UserViewModel User
        {
            get { return _user; }
            set { Set(ref _user, value); }
        }

        public ICommand SetThemeCommand => _setThemeCommand ?? (_setThemeCommand = new RelayCommand<string>(OnSetTheme));

        public ICommand PrivacyStatementCommand => _privacyStatementCommand ?? (_privacyStatementCommand = new RelayCommand(OnPrivacyStatement));

        public ICommand LogOutCommand => _logOutCommand ?? (_logOutCommand = new RelayCommand(OnLogOut));

        public SettingsViewModel(AppConfig config, IThemeSelectorService themeSelectorService, ISystemService systemService, IUserDataService userDataService, IIdentityService identityService)
        {
            _config = config;
            _themeSelectorService = themeSelectorService;
            _systemService = systemService;
            _userDataService = userDataService;
            _identityService = identityService;
        }

        public void OnNavigatedTo(object parameter)
        {
            VersionDescription = GetVersionDescription();
            Theme = _themeSelectorService.GetCurrentTheme();
            _identityService.LoggedOut += OnLoggedOut;
            _userDataService.UserDataUpdated += OnUserDataUpdated;
            User = _userDataService.GetUser();
        }

        public void OnNavigatedFrom()
        {
            UnregisterEvents();
        }

        private void UnregisterEvents()
        {
            _identityService.LoggedOut -= OnLoggedOut;
            _userDataService.UserDataUpdated -= OnUserDataUpdated;
        }

        private string GetVersionDescription()
        {
            var appName = "ForcedLogin";
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var versionInfo = FileVersionInfo.GetVersionInfo(assemblyLocation);
            return $"{appName} - {versionInfo.FileVersion}";
        }

        private void OnSetTheme(string themeName)
        {
            var theme = (AppTheme)Enum.Parse(typeof(AppTheme), themeName);
            _themeSelectorService.SetTheme(theme);
        }

        private void OnPrivacyStatement()
            => _systemService.OpenInWebBrowser(_config.PrivacyStatement);

        private async void OnLogOut()
        {
            await _identityService.LogoutAsync();
        }

        private void OnUserDataUpdated(object sender, UserViewModel userData)
        {
            User = userData;
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            UnregisterEvents();
        }
    }
}
