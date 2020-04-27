#pragma once
#include <string>
#include <iostream>

#include "Cpp_Memory.h"


class Cpp_Demo
{
public:
	Cpp_Memory m_memory; 
	int m_nAdd;
	int Add(int n0, int n1);
	void SetMemory(int nCount, int nByte, int xSize, int ySize, __int64 nAddress);
	bool OpenImage(std::string strFilepath);
};