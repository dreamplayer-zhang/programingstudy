#pragma once

typedef unsigned char byte; 

class Cpp_Memory
{
protected:
	bool IsSame(int nCount, int nByte, int xSize, int ySize, __int64 nAddress);
public:
	int m_nCount = 0;
	int m_nByte = 1;
	int m_xSize = 64; 
	int m_ySize = 64; 
	int m_nLine = 64; 
	__int64 *m_ppBuf = nullptr;

	Cpp_Memory(); 
	virtual ~Cpp_Memory(); 
	void Init(int nCount, int nByte, int xSize, int ySize, __int64 nAddress);
	byte* GetPtr(int nIndex);
	byte* GetPtr(int nIndex, int y); 
	byte* GetPtr(int nIndex, int y, int x);
};

