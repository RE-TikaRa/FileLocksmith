// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;

using ManagedCommon;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Win32;

namespace FileLocksmithUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            string appLanguage = LanguageHelper.LoadLanguage();
            if (!string.IsNullOrEmpty(appLanguage))
            {
                Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = appLanguage;
            }

            Logger.InitializeLogger(Path.Combine(GetFileLocksmithRoot(), "Logs"));

            this.InitializeComponent();

            UnhandledException += App_UnhandledException;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (IsPolicyDisabled())
            {
                Logger.LogWarning("Tried to start with a GPO policy setting the utility to always be disabled. Please contact your systems administrator.");
                Environment.Exit(0); // Current.Exit won't work until there's a window opened.
                return;
            }

            bool isElevated = PowerToys.FileLocksmithLib.Interop.NativeMethods.IsProcessElevated();

            if (isElevated)
            {
                if (!PowerToys.FileLocksmithLib.Interop.NativeMethods.SetDebugPrivilege())
                {
                    Logger.LogWarning("Couldn't set debug privileges to see system processes.");
                }
            }

            var arguments = args?.Arguments ?? string.Empty;
            if (arguments.Contains("--native", StringComparison.OrdinalIgnoreCase) ||
                arguments.Contains("--elevated", StringComparison.OrdinalIgnoreCase))
            {
                _window = new MainWindow(isElevated);
            }
            else
            {
                _window = new ManagementWindow();
            }
            _window.Activate();
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger.LogError("Unhandled exception", e.Exception);
        }

        private Window _window;

        private static string GetFileLocksmithRoot()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var root = Path.Combine(basePath, "ALp_Studio", "FileLocksmith");
            Directory.CreateDirectory(root);
            return root;
        }

        private static bool IsPolicyDisabled()
        {
            const string policyKey = @"SOFTWARE\Policies\FileLocksmith";
            const string policyValue = "Enabled";

            using var key = Registry.LocalMachine.OpenSubKey(policyKey);
            if (key == null)
            {
                return false;
            }

            var value = key.GetValue(policyValue);
            if (value is int intValue)
            {
                return intValue == 0;
            }

            if (value is string stringValue && int.TryParse(stringValue, out var parsed))
            {
                return parsed == 0;
            }

            return false;
        }
    }
}
