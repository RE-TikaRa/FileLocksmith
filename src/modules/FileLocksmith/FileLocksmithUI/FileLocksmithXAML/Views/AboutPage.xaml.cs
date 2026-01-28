// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;

namespace PowerToys.FileLocksmithUI.Views
{
    public sealed partial class AboutPage : Page
    {
        private const string WebsiteUrl = "https://re-tikara.fun";

        public AboutPage()
        {
            InitializeComponent();
        }

        public string VersionText => $"版本 {VersionNumberText}";

        public string VersionNumberText
        {
            get
            {
                var version = GetVersion();
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }

        public string BuildNumberText
        {
            get
            {
                var version = GetVersion();
                return version.Build.ToString();
            }
        }

        private static Version GetVersion()
        {
            try
            {
                var version = Package.Current.Id.Version;
                return new Version(version.Major, version.Minor, version.Build, version.Revision);
            }
            catch
            {
                return Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
            }
        }

        private void OnCopyVersionClicked(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(VersionNumberText);
        }

        private void OnCopyWebsiteClicked(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(WebsiteUrl);
        }

        private static void CopyToClipboard(string text)
        {
            var data = new DataPackage();
            data.SetText(text);
            Clipboard.SetContent(data);
            Clipboard.Flush();
        }
    }
}
