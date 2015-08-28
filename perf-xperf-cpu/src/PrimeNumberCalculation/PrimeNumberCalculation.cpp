// PrimeNumberCalculation.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <Windows.h>

struct PRIME_RANGE
{
	int Low;
	int High;
	int Count;
};

bool IsPrime(int number)
{
	if (number <= 2) return true;
	for (int i = 2; i < number; ++i)
		if (number % i == 0) return false;
	return true;
}

DWORD WINAPI CalculationThread(LPVOID prm)
{
	PRIME_RANGE* range = static_cast<PRIME_RANGE*>(prm);
	int count = 0;
	for (int i = range->Low; i < range->High; ++i)
	{
		if (IsPrime(i))
			++count;
	}
	return count;
}

int _tmain(int argc, _TCHAR* argv[])
{
	printf("Press ENTER to start execution\n");
	getchar();

	const int low = 10;
	const int high = 100000;
	const int P = 13;
	int chunkSize = (high - low) / P;

	HANDLE threads[P];
	PRIME_RANGE ranges[P];
	for (int i = 0; i < P; ++i)
	{
		ranges[i].Low = i*chunkSize;
		ranges[i].High = chunkSize + i*chunkSize;
		ranges[i].Count = 0;
		threads[i] = CreateThread(NULL, 0, CalculationThread, &ranges[i], 0, NULL);
	}
	printf("%d threads created\n", P);

	for (int i = 0; i < P; ++i)
	{
		WaitForSingleObject(threads[i], INFINITE);
		GetExitCodeThread(threads[i], (LPDWORD)&(ranges[i].Count));
	}

	for (int i = 0; i < P; ++i)
	{
		printf("Thread %d found %d primes\n", i, ranges[i].Count);
	}
	
	printf("Done. Press ENTER to quit\n");
	getchar();

	return 0;
}

