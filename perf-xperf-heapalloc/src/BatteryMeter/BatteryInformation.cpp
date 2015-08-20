#include "StdAfx.h"
#include "BatteryInformation.h"

struct BATTERY_INFO_INTERNAL
{
	WCHAR Data[1024];
	FLOAT CalcBuf[256];
};

BatteryInformation::BatteryInformation(void)
{
	m_Info = new BATTERY_INFO_INTERNAL;
	wcscpy_s(((BATTERY_INFO_INTERNAL*)m_Info)->Data, 100, L"Battery Internal Info Structure");
}

short BatteryInformation::GetBatteryPercentLeft()
{
	return 42;
}

BatteryInformation::~BatteryInformation(void)
{
}
