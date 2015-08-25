// DebuggingDemo.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <vector>
#include <Windows.h>

void PrintArrayRepeatedly()
{
	int* arr = new int[10];
	for (int i = 0; i < 10; ++i)
	{
		arr[i] = i;
	}
	while (true)
	{
		Sleep(1000);
		for (int i = 0; i < 10; ++i)
		{
			printf("%d ", arr[i]);
		}
	}
}

void PrintVectorRepeatedly()
{
	std::vector<int> vec;
	for (int i = 0; i < 10; ++i)
	{
		vec.push_back(i);
	}
	while (true)
	{
		Sleep(1500);
		for (const auto& i : vec)
		{
			printf("%d ", i);
		}
	}
}

DWORD WINAPI Thread1(LPVOID)
{
	PrintArrayRepeatedly();
	return 0;
}

DWORD WINAPI Thread2(LPVOID)
{
	PrintVectorRepeatedly();
	return 0;
}

int _tmain(int argc, _TCHAR* argv[])
{
	CreateThread(NULL, 0, Thread1, NULL, 0, NULL);
	CreateThread(NULL, 0, Thread2, NULL, 0, NULL);
	getchar();
	return 0;
}

