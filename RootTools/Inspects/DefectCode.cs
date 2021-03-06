using System;
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
		SideInspectionRight = 12,
		SideInspectionBottom = 13,
		BevelInspection = 14,
		BevelInspectionTop = 15,
		BevelInspectionLeft = 16,
		BevelInspectionRight = 17,
		BevelInspectionBottom = 18,
		D2D = 19,
		EBR = 20,
	}
	public enum InspectionType
	{
		None = 0,
		DarkInspection = 1,//1~3까지는 Merge
		AbsoluteSurfaceDark = 2,
		RelativeSurfaceDark = 3,
		D2D = 3,
		Strip = 4,
		BrightInspection = 5,//5~7까지는 Merge
		AbsoluteSurfaceBright = 6,
		RelativeSurfaceBright = 7,
	}
}
