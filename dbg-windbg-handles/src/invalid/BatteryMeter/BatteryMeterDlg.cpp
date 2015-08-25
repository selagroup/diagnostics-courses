
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

static DWORD WINAPI TemperatureAndBatteryUpdaterThread(LPVOID pdlg)
{
	CBatteryMeterDlg* pDialog = (CBatteryMeterDlg*)pdlg;
	
	for (int i = 1; ; ++i)
	{
		Sleep(10);
		if (i % 100 == 0)
		{
			BatteryInformation battery;
			CPUInformation cpu;
			pDialog->m_BatteryLeft.SetPos(pDialog->m_BatteryLeft.GetPos() - 1);
			pDialog->m_CPUTemp.SetPos(pDialog->m_CPUTemp.GetPos() + 1);
		}
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

__declspec(noinline) static void MakeSureHandleIsValid(HANDLE handle)
{
	HANDLE newHandle;
	DuplicateHandle(
		GetCurrentProcess(), handle, NULL, &newHandle,
		0, FALSE, DUPLICATE_SAME_ACCESS);
	if (CloseHandle(handle) == FALSE)
		throw "We got an invalid handle!";
}

HANDLE g_hSwitcherDoneEvent;

static DWORD WINAPI CPUSwitcherThread(LPVOID)
{
	Sleep(500);

	MakeSureHandleIsValid(g_hSwitcherDoneEvent);
	SetEvent(g_hSwitcherDoneEvent);
	return 0;
}

void CBatteryMeterDlg::OnCPUSelectorChanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMUPDOWN pNMUpDown = reinterpret_cast<LPNMUPDOWN>(pNMHDR);
	*pResult = 0;

	g_hSwitcherDoneEvent = CreateEvent(NULL, TRUE, FALSE, L"SwitcherDoneEvent");

	HANDLE handles[2];
	handles[0] = CreateThread(NULL, 0, CPUSwitcherThread, NULL, 0, NULL);
	handles[1] = g_hSwitcherDoneEvent;

	WaitForMultipleObjects(ARRAYSIZE(handles), handles, TRUE, INFINITE);
}
