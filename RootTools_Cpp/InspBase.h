#pragma once

#include  "pch.h"


// �� Thread������ ������ Insp ��ü�� �־����
 class InspBase
{
	 virtual bool PreInspect() {};
	 virtual bool Preproc() {};
	 virtual bool Postproc() {};

	 virtual bool Inspect() = 0; 
};

