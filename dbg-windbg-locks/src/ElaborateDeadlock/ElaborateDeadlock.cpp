// ElaborateDeadlock.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <Windows.h>

HANDLE hHelperThread, hWaiterThread, hSomeOtherThread, hAnnoyingThread;
HANDLE hHelperMutex, hAnnoyingMutex;

DWORD WINAPI HelperThread(LPVOID)
{
	//acquire mutex needed by WaiterThread
	WaitForSingleObject(hHelperMutex, INFINITE);
	//wait for mutex owned by AnnoyingThread
	WaitForSingleObject(hAnnoyingMutex, INFINITE);
	ReleaseMutex(hHelperMutex);
	ReleaseMutex(hAnnoyingMutex);
	return 0;
}

DWORD WINAPI WaiterThread(LPVOID)
{
	//acquire mutex owned by HelperThread
	WaitForSingleObject(hHelperMutex, INFINITE);
	ReleaseMutex(hHelperMutex);
	return 0;
}

DWORD WINAPI SomeOtherThread(LPVOID)
{
	//wait for WaiterThread
	WaitForSingleObject(hWaiterThread, INFINITE);
	return 0;
}

DWORD WINAPI AnnoyingThread(LPVOID)
{
	//acquire mutex needed by HelperThread
	WaitForSingleObject(hAnnoyingMutex, INFINITE);
	//wait for SomeOtherThread
	WaitForSingleObject(hSomeOtherThread, INFINITE);
	ReleaseMutex(hAnnoyingMutex);
	return 0;
}

int _tmain(int argc, _TCHAR* argv[])
{
	hHelperThread = CreateThread(NULL, 0, HelperThread, NULL, CREATE_SUSPENDED, NULL);
	hWaiterThread = CreateThread(NULL, 0, WaiterThread, NULL, CREATE_SUSPENDED, NULL);
	hSomeOtherThread = CreateThread(NULL, 0, SomeOtherThread, NULL, CREATE_SUSPENDED, NULL);
	hAnnoyingThread = CreateThread(NULL, 0, AnnoyingThread, NULL, CREATE_SUSPENDED, NULL);

	hHelperMutex = CreateMutex(NULL, FALSE, L"HelperMutex");
	hAnnoyingMutex = CreateMutex(NULL, FALSE, L"AnnoyingMutex");

	ResumeThread(hSomeOtherThread);
	Sleep(100);
	ResumeThread(hAnnoyingThread);
	Sleep(100);
	ResumeThread(hHelperThread);
	Sleep(100);
	ResumeThread(hWaiterThread);
	Sleep(100);

	HANDLE handles[4] = {hHelperThread, hWaiterThread, hSomeOtherThread, hAnnoyingThread};
	WaitForMultipleObjects(4, handles, TRUE, INFINITE);

	return 0;
}

