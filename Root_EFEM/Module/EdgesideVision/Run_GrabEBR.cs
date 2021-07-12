
using RootTools;
using RootTools.Camera;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_EFEM.Module.EdgesideVision
{
	public class Run_GrabEBR : ModuleRunBase
	{
		Vision_Edgeside module;
		public GrabModeEdge gmEBR = null;

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

		public Run_GrabEBR(Vision_Edgeside module)
		{
			this.module = module;
			InitModuleRun(module);
		}

		public RootTools.GrabModeBase GetGrabMode()
		{
			return module.GetGrabMode(m_sGrabModeEBR);
		}

		public override ModuleRunBase Clone()
		{
			Run_GrabEBR run = new Run_GrabEBR(module);
			run.p_sGrabModeEBR = p_sGrabModeEBR;
			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
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
				Axis axisX = module.AxisEbrX;
				Axis axisZ = module.AxisEbrZ;

				double pulsePerDegree = module.Pulse360 / 360;
				int camHeight = module.CamEBR.GetRoiSize().Y;
				int scanSpeed = 10000;// Convert.ToInt32((double)gmEBR.m_nMaxFrame* camHeight * trigger * (double)gmEBR.m_nScanRate/ 100);

                //double currPos = axisR.p_posActual - axisR.p_posActual % m_module.dPulse360;
                //double triggerStart = currPos + (m_fStartDegree * pulsePerDegree);
                double triggerStart = gmEBR.m_nStartDegree * pulsePerDegree;
				double triggerDest = triggerStart + (gmEBR.m_nScanDegree * pulsePerDegree);
				
				// 시계 방향
				//double moveStart = triggerStart - axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc * scanSpeed*4;   //y 축 이동 시작 지점
				//double moveEnd = triggerDest + axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc * scanSpeed * 4;  // Y 축 이동 끝 지점
				// 반시계 방향
				double moveStart = triggerDest + axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc * scanSpeed * 4;  // Y 축 이동 끝 지점
                double moveEnd = triggerStart - axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc * scanSpeed * 4;   //y 축 이동 시작 지점
                
				int grabCount = Convert.ToInt32((gmEBR.m_nScanDegree / 360.0) * gmEBR.m_nWaferSize_mm * pulsePerDegree * Math.PI / gmEBR.m_dRealResX_um);

				if (module.Run(axisX.StartMove(gmEBR.m_nFocusX)))
					return p_sInfo;
				if (module.Run(axisX.WaitReady()))
					return p_sInfo;
				if (module.Run(axisZ.StartMove(gmEBR.m_nFocusZ)))
					return p_sInfo;
				if (module.Run(axisZ.WaitReady()))
					return p_sInfo;
				if (module.Run(axisR.StartMove(moveStart)))
					return p_sInfo;
				if (module.Run(axisR.WaitReady()))
					return p_sInfo;

                axisR.SetTrigger(triggerStart, triggerDest, 1, 10, true);
				gmEBR.StartGrab(gmEBR.m_memoryData, new CPoint(0, 0), grabCount, gmEBR.m_GD);
                gmEBR.Grabed += GmEBR_Grabed;

				if (module.Run(axisR.StartMove(moveEnd, scanSpeed, axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc, axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc)))
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

        private void GmEBR_Grabed(object sender, EventArgs e)
        {
			GrabedArgs ga = (GrabedArgs)e;
			module.p_nProgress = ga.nProgress;
		}
    }
}
