﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools.Inspects
{
	public enum InspectionTarget
	{
		None = 0,
		Chrome = 1,
		Pellcile45 = 2,
		Pellicle90 = 3,
		FrameTop = 4,
		FrameLeft = 5,
		FrameBottom = 6,
		FrameRight = 7,
		Glass = 8,
		SideInspection = 9,
		SideInspectionTop = 10,
		SideInspectionLeft = 11,
		SideInspectionBottom = 12,
		SideInspectionRight = 13,
	}
	public enum InspectionType
	{
		None = 0,
		AbsoluteSurface = 1,
		RelativeSurface = 2,
		D2D = 3,
		Strip = 4,
	}
}
