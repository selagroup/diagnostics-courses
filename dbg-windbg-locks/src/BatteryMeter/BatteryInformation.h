#pragma once
class BatteryInformation
{
public:
	BatteryInformation(void);
	short GetBatteryPercentLeft();
	~BatteryInformation(void);

private:
	void* m_Info;
};

