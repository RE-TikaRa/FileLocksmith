#include "pch.h"

#include "IPC.h"
#include "Constants.h"

#include <filesystem>
#include <ShlObj.h>

constexpr DWORD DefaultPipeBufferSize = 8192;
constexpr DWORD DefaultPipeTimeoutMillis = 200;

namespace ipc
{
    namespace
    {
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

    Writer::Writer()
    {
        start();
    }

    Writer::~Writer()
    {
        finish();
    }

    HRESULT Writer::start()
    {
        std::wstring path = GetFileLocksmithRoot();
        path += constants::nonlocalizable::LastRunPath;

        try
        {
            m_stream = std::ofstream(path);
            return S_OK;
        }
        catch (...)
        {
            return E_FAIL;
        }
    }

    HRESULT Writer::add_path(LPCWSTR path)
    {
        int length = lstrlenW(path);
        if (!m_stream.write(reinterpret_cast<const char*>(path), length * sizeof(WCHAR)))
        {
            return E_FAIL;
        }

        WCHAR line_break = L'\n';
        if (!m_stream.write(reinterpret_cast<const char*>(&line_break), sizeof(WCHAR)))
        {
            return E_FAIL;
        }

        return S_OK;
    }

    void Writer::finish()
    {
        add_path(L"");
        m_stream.close();
    }
}
