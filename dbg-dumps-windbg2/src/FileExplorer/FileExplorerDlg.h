
// FileExplorerDlg.h : header file
//

#pragma once


// CFileExplorerDlg dialog
class CFileExplorerDlg : public CDialogEx
{
// Construction
public:
	CFileExplorerDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_FILEEXPLORER_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	void PopulateListRecursively(const CString& path);

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	CMFCEditBrowseCtrl m_EditBrowse;
	CListBox m_FileList;
	afx_msg void OnBnClickedButtonpopulate();
};
