// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Management.Deployment;

namespace PowerToys.FileLocksmithUI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private const string PowerToyName = "File Locksmith";
        private const string SettingsFileName = "file-locksmith-settings.json";
        private const string JsonKeyShowInExtendedContextMenu = "showInExtendedContextMenu";
        private const string ContextMenuPackageName = "FileLocksmithContextMenu";

        [ObservableProperty]
        private bool isContextMenuRegistered;

        [ObservableProperty]
        private bool showInExtendedContextMenu;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string statusMessage;

        public async Task InitializeAsync()
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            ShowInExtendedContextMenu = LoadExtendedMenuSetting();
            IsContextMenuRegistered = await Task.Run(IsContextMenuPackageInstalled);

            IsBusy = false;
        }

        public async Task SetContextMenuRegistrationAsync(bool enable)
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            var scriptName = enable ? "register.ps1" : "unregister.ps1";
            var scriptPath = Path.Combine(AppContext.BaseDirectory, scriptName);

            if (!File.Exists(scriptPath))
            {
                StatusMessage = $"未找到脚本：{scriptPath}";
                IsBusy = false;
                return;
            }

            try
            {
                await RunPowerShellElevatedAsync(scriptPath);
            }
            catch (Exception ex)
            {
                StatusMessage = $"执行脚本失败：{ex.Message}";
                IsBusy = false;
                return;
            }

            IsContextMenuRegistered = await Task.Run(IsContextMenuPackageInstalled);
            IsBusy = false;
        }

        public async Task SaveExtendedMenuSettingAsync()
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            await Task.Run(() => SaveExtendedMenuSetting(ShowInExtendedContextMenu));
            IsBusy = false;
        }

        public Task AddToPathAsync()
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            StatusMessage = AddToUserPath(AppContext.BaseDirectory);
            IsBusy = false;
            return Task.CompletedTask;
        }

        public Task RemoveFromPathAsync()
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            StatusMessage = RemoveFromUserPath(AppContext.BaseDirectory);
            IsBusy = false;
            return Task.CompletedTask;
        }

        public async Task ClearLogsAsync()
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            StatusMessage = await Task.Run(ClearLogsDirectory);
            IsBusy = false;
        }

        private static string AddToUserPath(string directory)
        {
            try
            {
                var normalized = directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var current = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? string.Empty;
                var paths = current
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                if (paths.Any(p => string.Equals(p, normalized, StringComparison.OrdinalIgnoreCase)))
                {
                    return "PATH 已包含当前目录，无需重复添加。";
                }

                var newValue = string.IsNullOrWhiteSpace(current)
                    ? normalized
                    : current.TrimEnd(';') + ";" + normalized;

                Environment.SetEnvironmentVariable("Path", newValue, EnvironmentVariableTarget.User);
                return "已添加到 PATH（当前用户）。请重新打开终端后使用。";
            }
            catch (Exception ex)
            {
                return $"写入 PATH 失败：{ex.Message}";
            }
        }

        private static string RemoveFromUserPath(string directory)
        {
            try
            {
                var normalized = directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var current = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? string.Empty;
                var entries = current
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                    .ToArray();

                if (!entries.Any(p => string.Equals(p, normalized, StringComparison.OrdinalIgnoreCase)))
                {
                    return "PATH 未包含当前目录，无需移除。";
                }

                var newValue = string.Join(";", entries.Where(p => !string.Equals(p, normalized, StringComparison.OrdinalIgnoreCase)));
                Environment.SetEnvironmentVariable("Path", newValue, EnvironmentVariableTarget.User);
                return "已从 PATH 移除当前目录。请重新打开终端后生效。";
            }
            catch (Exception ex)
            {
                return $"移除 PATH 失败：{ex.Message}";
            }
        }

        private static string ClearLogsDirectory()
        {
            try
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var logsPath = Path.Combine(basePath, "ALp_Studio", "FileLocksmith", "Logs");

                if (!Directory.Exists(logsPath))
                {
                    return "日志目录不存在。";
                }

                Directory.Delete(logsPath, true);
                return "已删除日志目录。";
            }
            catch (Exception ex)
            {
                return $"删除日志目录失败：{ex.Message}";
            }
        }

        private static bool IsContextMenuPackageInstalled()
        {
            try
            {
                var pm = new PackageManager();
                return pm.FindPackagesForUser(string.Empty)
                    .Any(p => string.Equals(p.Id.Name, ContextMenuPackageName, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        private static async Task RunPowerShellElevatedAsync(string scriptPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"",
                Verb = "runas",
                UseShellExecute = true,
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                await Task.Run(() => process.WaitForExit());
            }
        }

        private static string GetSettingsFilePath()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(basePath, "ALp_Studio", "FileLocksmith", SettingsFileName);
        }

        private static bool LoadExtendedMenuSetting()
        {
            try
            {
                var path = GetSettingsFilePath();
                if (!File.Exists(path))
                {
                    return false;
                }

                var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject;
                if (json is null)
                {
                    return false;
                }

                if (json.TryGetPropertyValue(JsonKeyShowInExtendedContextMenu, out var value) && value is JsonValue jsonValue)
                {
                    return jsonValue.GetValue<bool>();
                }
            }
            catch
            {
            }

            return false;
        }

        private static void SaveExtendedMenuSetting(bool value)
        {
            var path = GetSettingsFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            JsonObject json;
            if (File.Exists(path))
            {
                json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? new JsonObject();
            }
            else
            {
                json = new JsonObject();
            }

            json[JsonKeyShowInExtendedContextMenu] = value;
            File.WriteAllText(path, json.ToJsonString());
        }
    }
}
