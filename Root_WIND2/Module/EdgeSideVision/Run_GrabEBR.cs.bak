
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
		public GrabMode gmEBR = null;

		string m_sGrabModeEBR = "";

		#region [Getter/Setter]
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
		#endregion

		public double startDegree = 0;
		public double scanDegree = 360;
		public double scanAcc = 0.1;	//sec
		public int scanRate = 100;		// Camera Frame Spec 사용률 ? 1~100 %
		public int maxFrame = 100;

		public Run_GrabEBR(EdgeSideVision module)
		{
			this.module = module;
			InitModuleRun(module);
		}

		public override ModuleRunBase Clone()
		{
			Run_GrabEBR run = new Run_GrabEBR(module);
			run.p_sGrabModeEBR = p_sGrabModeEBR;
			run.startDegree = startDegree;
			run.scanDegree = scanDegree;
			run.scanRate = scanRate;
			run.scanAcc = scanAcc;
			run.maxFrame = maxFrame;
			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			startDegree = tree.Set(startDegree, startDegree, "Start Angle", "Degree", bVisible);
			scanDegree = tree.Set(scanDegree, scanDegree, "Scan Angle", "Degree", bVisible);

			scanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(scanRate, scanRate, "Scan Rate", "카메라 Frame 사용률 (1~ 100 %)", bVisible);
			maxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(maxFrame, maxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
			scanAcc = (tree.GetTree("Scan Velocity", false, bVisible)).Set(scanAcc, scanAcc, "Scan Acc", "Scan 축 가속도 (sec)", bVisible);

			p_sGrabModeEBR = tree.Set(p_sGrabModeEBR, p_sGrabModeEBR, module.p_asGrabMode, "Grab Mode : EBR", "Select GrabMode", bVisible);
			//if (m_gmEBR != null)
			//	m_gmEBR.RunTree(tree.GetTree("Grab Mode : EBR", false), bVisible, true);
		}

		public override string Run()
		{
			module.p_bStageVac = true;

			if (gmEBR == null) return "Grab Mode == null";

			try
			{
				gmEBR.SetLight(true);

				Axis axisR = module.AxisRotate;

				double pulsePerDegree = module.Pulse360 / 360;
				int camHeight = module.CamEBR.GetRoiSize().Y;
				int trigger = 1;
				int scanSpeed = Convert.ToInt32((double)maxFrame * camHeight * trigger * (double)scanRate / 100); //5000;

				//double currPos = axisR.p_posActual - axisR.p_posActual % m_module.dPulse360;
				//double triggerStart = currPos + (m_fStartDegree * pulsePerDegree);
				double triggerStart = startDegree * pulsePerDegree;
				double triggerDest = triggerStart + (scanDegree * pulsePerDegree);

				double moveStart = triggerStart - scanAcc * scanSpeed;   //y 축 이동 시작 지점 
				double moveEnd = triggerDest + scanAcc * scanSpeed;  // Y 축 이동 끝 지점.
				int grabCount = Convert.ToInt32(scanDegree * pulsePerDegree * module.EbrCamTriggerRatio);

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

				axisR.RunTrigger(false);
				gmEBR.StopGrab();
				return "OK";
			}
			finally
			{
				gmEBR.SetLight(false);
			}
		}
	}
}
