#include "StdAfx.h"
#include "CPUInformation.h"

struct CPU_INFO_INTERNAL
{
	WCHAR AdditionalData[256];
	LONG Sizes[100];
};

CPUInformation::CPUInformation(void)
{
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
