
// BatteryMeter.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CBatteryMeterApp:
// See BatteryMeter.cpp for the implementation of this class
//

class CBatteryMeterApp : public CWinApp
{
public:
	CBatteryMeterApp();

// Overrides
public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CBatteryMeterApp theApp;