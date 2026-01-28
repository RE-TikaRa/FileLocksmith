// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerToys.FileLocksmithUI.ViewModels;

namespace PowerToys.FileLocksmithUI.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }

        private async void ContextMenuToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsBusy)
            {
                return;
            }

            await ViewModel.SetContextMenuRegistrationAsync(ViewModel.IsContextMenuRegistered);
        }

        private async void ExtendedMenuToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsBusy)
            {
                return;
            }

            await ViewModel.SaveExtendedMenuSettingAsync();
        }
    }
}
