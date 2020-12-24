
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.Module
{
	public class Run_GrabEBR : ModuleRunBase
	{
		EdgeSideVision module;
		GrabMode grabMode = null;

		string m_sGrabModeEBR = "";
		string p_sGrabModeEBR
		{
			get
			{
				return m_sGrabModeEBR;
			}
			set
			{
				m_sGrabModeEBR = value;
				grabMode = module.GetGrabMode(value);
			}
		}

		public Run_GrabEBR(EdgeSideVision module)
		{
			this.module = module;
			InitModuleRun(module);
		}

		double startDegree = 0;
		double scanDegree = 360;
		double scanAcc = 1;			// sec
		int scanRate = 100;         // Camera Frame Spec 사용률 ? 1~100 %
		int maxFrame = 100;
		double resolution = 1.7;    // um/pixel
		//double triggerRatio = 2/3;	// trigger 분주비

		public override ModuleRunBase Clone()
		{
			Run_GrabEBR run = new Run_GrabEBR(module);
			run.p_sGrabModeEBR = p_sGrabModeEBR;

			run.startDegree = startDegree;
			run.scanDegree = scanDegree;
			run.maxFrame = maxFrame;
			run.scanRate = scanRate;
			run.scanAcc = scanAcc;
			run.resolution = resolution;
			//run.triggerRatio = triggerRatio;
			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			startDegree = tree.Set(startDegree, startDegree, "Start Angle", "Degree", bVisible);
			scanDegree = tree.Set(scanDegree, scanDegree, "Scan Angle", "Degree", bVisible);
			maxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(maxFrame, maxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
			scanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(scanRate, scanRate, "Scan Rate", "카메라 Frame 사용률 (1~ 100 %)", bVisible);
			scanAcc = (tree.GetTree("Scan Velocity", false, bVisible)).Set(scanAcc, scanAcc, "Scan Acc", "Scan 축 가속도 (sec)", bVisible);
			resolution = tree.Set(resolution, resolution, "Resolution", "um / pixel", bVisible);
			//triggerRatio = tree.Set(triggerRatio, triggerRatio, "분주비", "Camera 분주비", bVisible);

			p_sGrabModeEBR = tree.Set(p_sGrabModeEBR, p_sGrabModeEBR, module.p_asGrabMode, "Grab Mode : EBR", "Select GrabMode", bVisible);
			if (grabMode != null)
				grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
		}

		public override string Run()
		{
			/*
			string rstCam = module.OpenCamera();
			if (rstCam != "OK")
			{
				return rstCam;
			}
			module.p_bStageVac = true;
			*/

			if (grabMode == null) return "Grab Mode == null";

			try
			{
				grabMode.SetLight(true);

				Axis axisR = module.AxisRotate;
				double pulsePerDegree = module.Pulse360 / 360;
				int camHeight = module.CamEBR.GetRoiSize().Y;
				int trigger = 1;
				int scanSpeed = Convert.ToInt32((double)maxFrame * camHeight * trigger * (double)scanRate / 100); //5000;

				//double currPos = axisR.p_posActual - axisR.p_posActual % m_module.dPulse360;
				//double triggerStart = currPos + (m_fStartDegree * pulsePerDegree);
				double triggerStart = startDegree * pulsePerDegree;
				double triggerDest = triggerStart + (scanDegree * pulsePerDegree);
				double moveStart = triggerStart - (scanAcc * scanSpeed);   //y 축 이동 시작 지점 
				double moveEnd = triggerDest + (scanAcc * scanSpeed);  // Y 축 이동 끝 지점.
				int grabCount = Convert.ToInt32(scanDegree * pulsePerDegree * module.EbrCamTriggerRatio);

				if (module.Run(axisR.StartMove(moveStart)))
					return p_sInfo;
				if (module.Run(axisR.WaitReady()))
					return p_sInfo;

				axisR.SetTrigger(triggerStart, triggerDest, trigger, true);
				grabMode.StartGrab(grabMode.m_memoryData, new CPoint(0, 0), grabCount);

				if (module.Run(axisR.StartMove(moveEnd, scanSpeed, scanAcc, scanAcc)))
					return p_sInfo;
				if (module.Run(axisR.WaitReady()))
					return p_sInfo;

				axisR.RunTrigger(false);
				return "OK";
			}
			finally
			{
				grabMode.StopGrab();
				grabMode.SetLight(false);
			}
		}
	}
}
