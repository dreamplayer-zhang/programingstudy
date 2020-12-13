
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
		GrabMode gmEBR = null;

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
				gmEBR = module.GetGrabMode(value);
			}
		}

		public Run_GrabEBR(EdgeSideVision module)
		{
			this.module = module;
			InitModuleRun(module);
		}

		double resolution = 1.7;    // um
		double startDegree = 0;
		double scanDegree = 360;
		double scanAcc = 0.1;       // sec
		int scanRate = 100;         // Camera Frame Spec 사용률 ? 1~100 %
		int maxFrame = 100;

		public override ModuleRunBase Clone()
		{
			Run_GrabEBR run = new Run_GrabEBR(module);
			run.p_sGrabModeEBR = p_sGrabModeEBR;

			run.resolution = resolution;
			run.startDegree = startDegree;
			run.scanDegree = scanDegree;
			run.scanAcc = scanAcc;
			run.scanRate = scanRate;
			run.maxFrame = maxFrame;
			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			startDegree = tree.Set(startDegree, startDegree, "Start Angle", "Degree", bVisible);
			scanDegree = tree.Set(scanDegree, scanDegree, "Scan Angle", "Degree", bVisible);
			scanRate = tree.Set(scanRate, scanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100%", bVisible);
			maxFrame = tree.Set(maxFrame, maxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
			scanAcc = tree.Set(scanAcc, scanAcc, "Scan Acc", "스캔 축 가속도 (sec)", bVisible);

			p_sGrabModeEBR = tree.Set(p_sGrabModeEBR, p_sGrabModeEBR, module.p_asGrabMode, "Grab Mode : EBR", "Select GrabMode", bVisible);
			if (gmEBR != null)
				gmEBR.RunTree(tree.GetTree("Grab Mode : EBR", false), bVisible, true);
		}

		public override string Run()
		{
			string rstCam = module.OpenCamera();
			if (rstCam != "OK")
			{
				return rstCam;
			}
			module.p_bStageVac = true;

			string rst = "None";
			rst = GrabEBR();
			if (rst != "OK")
				return rst;
			return rst;
		}

		private string GrabEBR()
		{
			Axis axisR = module.AxisRotate;

			try
			{
				gmEBR.SetLight(true);

				double pulsePerDegree = module.Pulse360 / 360;
				double currPos = axisR.p_posActual - axisR.p_posActual % module.Pulse360;
				double triggerStart = currPos + (startDegree * pulsePerDegree);
				double triggerDest = triggerStart + (scanDegree * pulsePerDegree);

				int trigger = 1;
				int scanSpeed = 5000;//Convert.ToInt32((double)m_nMaxFrame * trigger * (double)m_nScanRate / 100);
				double moveStart = triggerStart - scanAcc * scanSpeed;   //y 축 이동 시작 지점 
				double moveEnd = triggerDest + scanAcc * scanSpeed;  // Y 축 이동 끝 지점.
				int grabCount = Convert.ToInt32(scanDegree * pulsePerDegree * module.TriggerRatio);

				if (module.Run(axisR.StartMove(moveStart)))
					return p_sInfo;
				if (module.Run(axisR.WaitReady()))
					return p_sInfo;

				axisR.SetTrigger(triggerStart, triggerDest, trigger, true);
				gmEBR.StartGrab(gmEBR.m_memoryData, new CPoint(0, 0), grabCount);

				if (module.Run(axisR.StartMove(moveEnd, scanSpeed, scanAcc, scanAcc)))
					return p_sInfo;
				if (module.Run(axisR.WaitReady()))
					return p_sInfo;
				return "OK";
			}
			finally
			{
				axisR.RunTrigger(false);
				gmEBR.StopGrab();
				gmEBR.SetLight(false);
			}
		}
	}
}
