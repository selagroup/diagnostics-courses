// HeuristicsWhenOptimized.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <Windows.h>

HANDLE hEvent;

DWORD WINAPI SecondThread(LPVOID)
{
	WaitForSingleObject(hEvent, INFINITE);
	return 0;
}

int _tmain(int argc, _TCHAR* argv[])
{
	hEvent = CreateEvent(NULL, TRUE, FALSE, L"MyEvent");
	HANDLE hThread = CreateThread(NULL, 0, SecondThread, NULL, 0, NULL);
	HANDLE handles[] = { hEvent, hThread };
	WaitForMultipleObjects(2, handles, TRUE, 60000);

	return 0;
}

