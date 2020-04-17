#pragma once

#include  "pch.h"


// 각 Thread에서는 개개의 Insp 객체가 있어야함
 class InspBase
{
	 virtual bool PreInspect() {};
	 virtual bool Preproc() {};
	 virtual bool Postproc() {};

	 virtual bool Inspect() = 0; 
};

