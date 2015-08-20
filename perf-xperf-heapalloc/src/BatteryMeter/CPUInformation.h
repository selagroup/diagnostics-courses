#pragma once
class CPUInformation
{
public:
	CPUInformation(void);
	short GetCPUTemperature();
	~CPUInformation(void);

private:
	void* m_Info;
};

