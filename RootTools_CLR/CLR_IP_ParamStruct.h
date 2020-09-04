#pragma once

// standary library
#include <windows.h>
#include <vector>

// User Type Defines
public ref class Cpp_LabelParam
{
public :
	int centerX;
	int centerY;
	
	int boundTop;
	int boundBottom;
	int boundLeft;
	int boundRight;
	
	LONG area;
	LONG value;
};
