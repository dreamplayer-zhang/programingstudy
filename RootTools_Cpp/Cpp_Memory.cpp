#include "pch.h"
#include "Cpp_Memory.h"

Cpp_Memory::Cpp_Memory()
{
	m_ppBuf = nullptr; 
}

Cpp_Memory::~Cpp_Memory()
{
	if (m_ppBuf != nullptr) delete[] m_ppBuf; 
}

void Cpp_Memory::Init(int nCount, int nByte, int xSize, int ySize, __int64 nAddress)
{
	if (IsSame(nCount, nByte, xSize, ySize, nAddress) == false)
	{
		if (m_ppBuf != nullptr) delete[] m_ppBuf; 
		m_ppBuf = nullptr;
		if (nCount > 0) m_ppBuf = new __int64[nCount]; 
	}
	m_nCount = nCount; 
	m_nByte = nByte; 
	m_xSize = xSize; 
	m_ySize = xSize; 
	m_nLine = nByte * xSize; 
	__int64 lSize = m_nLine;
	lSize *= ySize; 
	for (int n = 0; n < m_nCount; n++, nAddress += lSize)
	{
		m_ppBuf[n] = nAddress; 
		nAddress += lSize;
	}
}

bool Cpp_Memory::IsSame(int nCount, int nByte, int xSize, int ySize, __int64 nAddress)
{
	if (m_nCount != nCount) return false; 
	if (m_nByte != nByte) return false; 
	if (m_xSize != xSize) return false; 
	if (m_ySize != ySize) return false;
	if (m_ppBuf == nullptr) return false; 
	if (m_ppBuf[0] != nAddress) return false; 
	return true; 
}

byte* Cpp_Memory::GetPtr(int nIndex)
{
	if ((nIndex < 0) || (nIndex >= m_nCount)) return nullptr; 
	return (byte*)m_ppBuf[nIndex]; 
}

byte* Cpp_Memory::GetPtr(int nIndex, int y)
{
	if ((y < 0) || (y >= m_ySize)) return nullptr;
	byte* p = GetPtr(nIndex);
	if (p == nullptr) return nullptr;
	int dBuf = (y * m_nLine); 
	return p + dBuf;
}

byte* Cpp_Memory::GetPtr(int nIndex, int y, int x)
{
	if ((x < 0) || (x >= m_ySize)) return nullptr;
	byte* p = GetPtr(nIndex, y);
	if (p == nullptr) return nullptr;
	int dBuf = (x * m_nByte);
	return p + dBuf;
}

