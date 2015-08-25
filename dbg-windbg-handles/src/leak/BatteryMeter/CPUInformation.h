#pragma once
class CPUInformation
{
public:
	CPUInformation(LPCWSTR lpszSyncEventName = NULL);
	short GetCPUTemperature();
	~CPUInformation(void);

private:
	void* m_Info;
	HANDLE m_SyncEvent;
};

