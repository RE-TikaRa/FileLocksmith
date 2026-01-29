// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace PowerToys.FileLocksmithUI.Views
{
    public sealed partial class AboutPage : Page
    {
        private const string WebsiteUrl = "https://re-tikara.fun";
        private const string CreatedDate = "2026-01-29";
        private const string PowerToysVersion = "0.97.1";

        public AboutPage()
        {
            InitializeComponent();
        }

        public string CreatedDateHeaderText => $"创建日期 {CreatedDate}";

        public string CreatedDateValueText => CreatedDate;

        public string PowerToysVersionText => PowerToysVersion;

        private void OnCopyCreatedDateClicked(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(CreatedDateValueText);
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
