// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ManagedCommon;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerToys.FileLocksmithUI.Helpers;
using PowerToys.FileLocksmithUI.Views;
using WinUIEx;

namespace FileLocksmithUI
{
    public sealed partial class ManagementWindow : WindowEx
    {
        public ManagementWindow()
        {
            InitializeComponent();
            SetTitleBar(titleBar);
            ExtendsContentIntoTitleBar = true;
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            AppWindow.SetIcon("Assets/FileLocksmith/Icon.ico");
            WindowHelpers.ForceTopBorder1PixelInsetOnWindows10(this.GetWindowHandle());

            Title = "File Locksmith";
            titleBar.Title = "File Locksmith";

            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(HomePage));
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
            {
                switch (tag)
                {
                    case "home":
                        ContentFrame.Navigate(typeof(HomePage));
                        break;
                    case "settings":
                        ContentFrame.Navigate(typeof(SettingsPage));
                        break;
                    case "about":
                        ContentFrame.Navigate(typeof(AboutPage));
                        break;
                }
            }
        }
    }
}
