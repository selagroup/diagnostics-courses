#pragma once
class CPUInformation
{
public:
	CPUInformation();
	short GetCPUTemperature();
	~CPUInformation(void);

private:
	void* m_Info;
};

