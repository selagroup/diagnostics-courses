// NumberCruncher.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <Windows.h>
#include <vector>
#include <cmath>

struct CRUNCHER_PARAMS
{
	int Start;
	int End;
	std::vector<int>* Output;
};

bool IsPrime(unsigned int number)
{
	if (number == 0) return false;
	if (number <= 2) return true;
	if ((number & 1) == 0) return false;
	int root = (int)std::sqrt((float)number);
	for (int i = 3; i <= root; i += 2)
		if (number % i == 0) return false;
	return true;
}

DWORD WINAPI CrunchingThread(LPVOID param)
{
	CRUNCHER_PARAMS* cruncherParams = static_cast<CRUNCHER_PARAMS*>(param);
	for (int i = cruncherParams->Start; i < cruncherParams->End; ++i)
	{
		if (IsPrime(i))
			cruncherParams->Output->push_back(i);
	}
	return 0;
}

int _tmain(int argc, _TCHAR* argv[])
{
	CRUNCHER_PARAMS params;
	params.Start = 2;
	params.End = 1000000;

	HANDLE rghThreads[2];
	for (int i = 0; i < 2; ++i)
	{
		rghThreads[i] = CreateThread(NULL, 0, CrunchingThread, &params, 0, NULL);
		std::vector<int> result;
		params.Output = &result;
	}
	WaitForMultipleObjects(ARRAYSIZE(rghThreads), rghThreads, TRUE, INFINITE);

	return 0;
}

