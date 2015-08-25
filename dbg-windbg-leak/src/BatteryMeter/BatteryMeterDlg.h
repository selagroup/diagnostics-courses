
// BatteryMeterDlg.h : header file
//

#pragma once
#include "afxcmn.h"


// CBatteryMeterDlg dialog
class CBatteryMeterDlg : public CDialogEx
{
// Construction
public:
	CBatteryMeterDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_BATTERYMETER_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	CProgressCtrl m_BatteryLeft;
	CProgressCtrl m_CPUTemp;
	afx_msg void OnCPUSelectorChanged(NMHDR *pNMHDR, LRESULT *pResult);

private:
	short m_InitialBatteryLeft;
	short m_InitialCPUTemperature;
};
