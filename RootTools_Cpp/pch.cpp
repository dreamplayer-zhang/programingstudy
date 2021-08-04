// pch.cpp: 미리 컴파일된 헤더에 해당하는 소스 파일

#include "pch.h"

// 미리 컴파일된 헤더를 사용하는 경우 컴파일이 성공하려면 이 소스 파일이 필요합니다.

void DebugTxt(const void* s, int size)
{
#ifdef _DEBUG
	CFile f;
	if (f.Open(L"D:\\CLRD.txt", CFile::modeCreate| CFile::modeNoTruncate| CFile::modeReadWrite))
	{
		f.SeekToEnd();
		f.Write(s, size);
		f.Close();
	}	
#endif
}