#pragma once

#include "..\RootTools_Cpp\\InspSurface.h"

namespace RootTools_CLR
{
	public ref class InspSurfaceWrapper
	{

	protected:
		InspSurface* m_pInspSurface;

	public:
		InspSurfaceWrapper()
		{
			m_pInspSurface = new InspSurface();
		}

		bool DoInsp(void* _param)
		{
			return m_pInspSurface->DoInsp(_param);
		}

		virtual ~InspSurfaceWrapper()
		{
			delete m_pInspSurface;
		}
	};
}