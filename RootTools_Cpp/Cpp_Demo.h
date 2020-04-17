#pragma once
#include <string>
#include <iostream>

#include "Cpp_Memory.h"

using namespace std;

class Cpp_Demo
{
public:
	Cpp_Memory m_memory; 
	int m_nAdd;
	int Add(int n0, int n1);
	void SetMemory(int nCount, int nByte, int xSize, int ySize, __int64 nAddress);
	bool OpenImage(string strFilepath);
};