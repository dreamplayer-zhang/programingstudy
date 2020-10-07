#pragma once

#include "windows.h"
#include "memoryapi.h"
#include <stdio.h>
#include <list>
#include "..\RootTools_Cpp\\IP.h"
#include <iostream>

namespace RootTools_CLR
{
	public ref class CLR_IP
	{
	protected:

	public:
		static void ContourFitEllipse(array<byte>^ pSrcImg, int nW, int nH, array<byte>^ pDstImg);

	};
}

#pragma once
