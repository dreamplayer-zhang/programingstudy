#pragma once

// standary library
#include <windows.h>
#include <vector>

// User Type Defines
public ref class Cpp_LabelParam
{
public:
	float centerX;
	float centerY;

	int boundTop;
	int boundBottom;
	int boundLeft;
	int boundRight;

	float width;
	float height;

	float area;
	float value;
};

public ref class Cpp_Point
{
public:
	int x;
	int y;
};

public ref class Cpp_Rect
{
public:
	int x; // left
	int y; // top
	int h;
	int w;
};
