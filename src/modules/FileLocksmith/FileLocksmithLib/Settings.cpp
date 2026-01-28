#include "pch.h"
#include "Settings.h"
#include "Constants.h"

#include <filesystem>
#include <ShlObj.h>
#include <common/utils/json.h>

namespace
{
    constexpr wchar_t PolicyRegistryPath[] = L"Software\\Policies\\FileLocksmith";
    constexpr wchar_t PolicyEnabledValueName[] = L"Enabled";

    enum class PolicyState
    {
        NotConfigured,
        Enabled,
        Disabled,
    };

    PolicyState ReadPolicyEnabled()
    {
        DWORD value = 0;
        DWORD valueSize = sizeof(value);
        auto status = RegGetValueW(
            HKEY_LOCAL_MACHINE,
            PolicyRegistryPath,
            PolicyEnabledValueName,
            RRF_RT_REG_DWORD,
            nullptr,
            &value,
            &valueSize);

        if (status != ERROR_SUCCESS)
        {
            return PolicyState::NotConfigured;
        }

        if (value == 1)
        {
            return PolicyState::Enabled;
        }

        if (value == 0)
        {
            return PolicyState::Disabled;
        }

        return PolicyState::NotConfigured;
    }

    std::wstring GetFileLocksmithRoot()
    {
        PWSTR localAppData = nullptr;
        if (FAILED(SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, nullptr, &localAppData)) || !localAppData)
        {
            return {};
        }

        std::wstring root{ localAppData };
        CoTaskMemFree(localAppData);
        root += L"\\ALp_Studio\\FileLocksmith";
        std::filesystem::create_directories(root);
        return root;
    }
}

static bool LastModifiedTime(const std::wstring& filePath, FILETIME* lpFileTime)
{
    WIN32_FILE_ATTRIBUTE_DATA attr{};
    if (GetFileAttributesExW(filePath.c_str(), GetFileExInfoStandard, &attr))
    {
        *lpFileTime = attr.ftLastWriteTime;
        return true;
    }
    return false;
}

FileLocksmithSettings::FileLocksmithSettings()
{
    std::wstring savePath = GetFileLocksmithRoot();
    jsonFilePath = savePath + constants::nonlocalizable::DataFilePath;
    Load();
}

bool FileLocksmithSettings::GetEnabled()
{
    switch (ReadPolicyEnabled())
    {
    case PolicyState::Enabled:
        return true;
    case PolicyState::Disabled:
        return false;
    case PolicyState::NotConfigured:
    default:
        break;
    }

    Reload();
    return settings.enabled;
}

void FileLocksmithSettings::Save()
{
    json::JsonObject jsonData;

    jsonData.SetNamedValue(constants::nonlocalizable::JsonKeyEnabled, json::value(settings.enabled));
    jsonData.SetNamedValue(constants::nonlocalizable::JsonKeyShowInExtendedContextMenu, json::value(settings.showInExtendedContextMenu));

    json::to_file(jsonFilePath, jsonData);
    GetSystemTimeAsFileTime(&lastLoadedTime);
}

void FileLocksmithSettings::Load()
{
    if (!std::filesystem::exists(jsonFilePath))
    {
        Save();
    }
    else
    {
        ParseJson();
    }
}

void FileLocksmithSettings::Reload()
{
    // Load json settings from data file if it is modified in the meantime.
    FILETIME lastModifiedTime{};
    if (LastModifiedTime(jsonFilePath, &lastModifiedTime) &&
        CompareFileTime(&lastModifiedTime, &lastLoadedTime) == 1)
    {
        Load();
    }
}

void FileLocksmithSettings::ParseJson()
{
    auto json = json::from_file(jsonFilePath);
    if (json)
    {
        const json::JsonObject& jsonSettings = json.value();
        try
        {
            if (json::has(jsonSettings, constants::nonlocalizable::JsonKeyEnabled, json::JsonValueType::Boolean))
            {
                settings.enabled = jsonSettings.GetNamedBoolean(constants::nonlocalizable::JsonKeyEnabled);
            }

            if (json::has(jsonSettings, constants::nonlocalizable::JsonKeyShowInExtendedContextMenu, json::JsonValueType::Boolean))
            {
                settings.showInExtendedContextMenu = jsonSettings.GetNamedBoolean(constants::nonlocalizable::JsonKeyShowInExtendedContextMenu);
            }
        }
        catch (const winrt::hresult_error&)
        {
        }
    }
    GetSystemTimeAsFileTime(&lastLoadedTime);
}

FileLocksmithSettings& FileLocksmithSettingsInstance()
{
    static FileLocksmithSettings instance;
    return instance;
}
