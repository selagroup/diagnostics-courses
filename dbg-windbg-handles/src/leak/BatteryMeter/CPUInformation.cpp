#include "StdAfx.h"
#include "CPUInformation.h"

struct CPU_INFO_INTERNAL
{
	WCHAR AdditionalData[256];
	LONG Sizes[100];
};

CPUInformation::CPUInformation(LPCWSTR lpszSyncEventName /*= NULL*/)
{
	WCHAR lpszCurrentDir[MAX_PATH];
	GetCurrentDirectory(ARRAYSIZE(lpszCurrentDir), lpszCurrentDir);
	WCHAR lpszTempFilename[MAX_PATH];
	GetTempFileName(lpszCurrentDir, L"SyncEvent_", 0, lpszTempFilename);
	m_SyncEvent = CreateFile(lpszTempFilename, GENERIC_READ|GENERIC_WRITE,
		FILE_SHARE_READ, NULL, CREATE_ALWAYS, 0, NULL);

	m_Info = new CPU_INFO_INTERNAL[5];
	for (int i = 0; i < 5; ++i)
		wcscpy_s(((CPU_INFO_INTERNAL*)m_Info)[i].AdditionalData, 100, L"CPU Internal Info Structure");
}

short CPUInformation::GetCPUTemperature()
{
	return 42;
}

CPUInformation::~CPUInformation(void)
{
	delete m_Info;
}
