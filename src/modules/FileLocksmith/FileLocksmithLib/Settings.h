#pragma once

#include "pch.h"
class FileLocksmithSettings
{
public:
    FileLocksmithSettings();
    bool GetEnabled();

    inline bool GetShowInExtendedContextMenu() const
    {
        return settings.showInExtendedContextMenu;
    }

    inline void SetExtendedContextMenuOnly(bool extendedOnly)
    {
        settings.showInExtendedContextMenu = extendedOnly;
    }

    void Save();
    void Load();

private:
    struct Settings
    {
        bool enabled{ true };
        bool showInExtendedContextMenu{ false };
    };

    void Reload();
    void ParseJson();

    Settings settings;
    std::wstring jsonFilePath;
    FILETIME lastLoadedTime{};
};

FileLocksmithSettings& FileLocksmithSettingsInstance();
