
// BatteryMeterDlg.cpp : implementation file
//

#include "stdafx.h"
#include "BatteryMeter.h"
#include "BatteryMeterDlg.h"
#include "afxdialogex.h"
#include "BatteryInformation.h"
#include "CPUInformation.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CBatteryMeterDlg dialog




CBatteryMeterDlg::CBatteryMeterDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CBatteryMeterDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CBatteryMeterDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_PROGRESS1, m_BatteryLeft);
	DDX_Control(pDX, IDC_PROGRESS2, m_CPUTemp);
}

BEGIN_MESSAGE_MAP(CBatteryMeterDlg, CDialogEx)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_NOTIFY(UDN_DELTAPOS, IDC_SPIN2, &CBatteryMeterDlg::OnCPUSelectorChanged)
END_MESSAGE_MAP()


// CBatteryMeterDlg message handlers

CRITICAL_SECTION g_UpdaterCS;
CRITICAL_SECTION g_SelectorCS;
CRITICAL_SECTION g_HardwareCS;

static DWORD WINAPI TemperatureAndBatteryUpdaterThread(LPVOID pdlg)
{
	CBatteryMeterDlg* pDialog = (CBatteryMeterDlg*)pdlg;
	
	for (int i = 0; ; ++i)
	{
		EnterCriticalSection(&g_UpdaterCS);
		Sleep(10);
		BatteryInformation battery;
		CPUInformation cpu;
		if (i % 500 == 0)
		{
			pDialog->m_BatteryLeft.SetPos(pDialog->m_BatteryLeft.GetPos() - 1);
			pDialog->m_CPUTemp.SetPos(pDialog->m_CPUTemp.GetPos() + 1);
		}
		LeaveCriticalSection(&g_UpdaterCS);
	}

	return 0;

}

BOOL CBatteryMeterDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	m_BatteryLeft.SetRange(0, 100);
	m_BatteryLeft.SetPos(rand() % 91 + 9);
	m_CPUTemp.SetRange(0, 100);
	m_CPUTemp.SetPos(rand() % 39 + 20);

	InitializeCriticalSection(&g_HardwareCS);
	InitializeCriticalSection(&g_SelectorCS);
	InitializeCriticalSection(&g_UpdaterCS);

	CreateThread(NULL, 0, TemperatureAndBatteryUpdaterThread, this, 0, NULL);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CBatteryMeterDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialogEx::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CBatteryMeterDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

static void NotifySelector()
{
	Sleep(100);
	EnterCriticalSection(&g_SelectorCS);
	LeaveCriticalSection(&g_SelectorCS);
}
static void MonitorHardware()
{
	Sleep(100);
	EnterCriticalSection(&g_HardwareCS);
	LeaveCriticalSection(&g_HardwareCS);
}

static DWORD WINAPI CPUSelectorThread(LPVOID)
{
	EnterCriticalSection(&g_SelectorCS);
	MonitorHardware();
	LeaveCriticalSection(&g_SelectorCS);
	return 0;
}

static DWORD WINAPI HardwareChangeDetectorThread(LPVOID)
{
	EnterCriticalSection(&g_HardwareCS);
	NotifySelector();
	LeaveCriticalSection(&g_HardwareCS);

	return 0;
}

static DWORD WINAPI LocationAwarenessThread(LPVOID)
{
	HANDLE hEvent = OpenEvent(EVENT_ALL_ACCESS, FALSE, L"LocationChangeEvent");
	WaitForSingleObject(hEvent, 60000);
	return 0;
}

static DWORD WINAPI TemperaturePropagationThread(LPVOID p)
{
	HANDLE hLocationThread = (HANDLE)p;
	WaitForSingleObject(hLocationThread, 90000);
	return 0;
}

void CBatteryMeterDlg::OnCPUSelectorChanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMUPDOWN pNMUpDown = reinterpret_cast<LPNMUPDOWN>(pNMHDR);
	*pResult = 0;

	HANDLE hEvent = CreateEvent(NULL, TRUE, FALSE, L"LocationChangeEvent");

	HANDLE threads[4];
	threads[0] = CreateThread(NULL, 0, CPUSelectorThread, NULL, 0, NULL);
	threads[1] = CreateThread(NULL, 0, HardwareChangeDetectorThread, NULL, 0, NULL);
	threads[2] = CreateThread(NULL, 0, LocationAwarenessThread, NULL, 0, NULL);
	threads[3] = CreateThread(NULL, 0, TemperaturePropagationThread, threads[2], 0, NULL);
	WaitForMultipleObjects(ARRAYSIZE(threads), threads, TRUE, INFINITE);
}
