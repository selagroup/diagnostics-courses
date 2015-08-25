// ILikeToThrow.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <exception>

class my_elaborate_exception : public std::exception
{
private:
	const char* _error_message;
	int _error_code;
public:
	my_elaborate_exception(int error_code)
		: _error_code(error_code)
	{
		create_error_message();
	}
	virtual const char* what() const
	{
		return _error_message;
	}
private:
	void create_error_message()
	{
		char buf[50];
		sprintf_s(buf, "error: %d %x", _error_code, _error_code);
		_error_message = _strdup(buf);
	}
};

__declspec(noinline) void baz()
{
	throw my_elaborate_exception(1244);
}

__declspec(noinline) void bar()
{
	baz();
}

__declspec(noinline) void foo()
{
	bar();
}

int _tmain(int argc, _TCHAR* argv[])
{
	foo();
	return 0;
}

