using RootTools;
using RootTools.Control;
using RootTools.Camera.Dalsa;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RootTools.Camera.Matrox;
using Root_EFEM.Module;
using Root_EFEM;
using RootTools.GAFs;
using System;

namespace Root_WIND2.Module
{
	public class EdgeSideVision : ModuleBase, IWTRChild
	{
		public void SetAlarm()
		{
			alid_WaferExist.Run(true, "EdgeSideVision Wafer Exist Error");
		}

		#region ToolBox
		Axis axisRotate;
		Axis axisEdgeX;
		Axis axisEbrX;
		Axis axisEbrZ;
		DIO_O doVac;
		DIO_O doBlow;
		DIO_I diWaferExist;
		DIO_I diWaferExistVac;

		MemoryPool memoryPool;
		MemoryGroup memoryGroup;
		MemoryData memoryEdgeTop;
		MemoryData memoryEdgeSide;
		MemoryData memoryEdgeBtm;
		MemoryData memoryEBR;

		LightSet lightSet;
		Camera_Dalsa camEdgeTop;
		Camera_Dalsa camEdgeSide;
		Camera_Dalsa camEdgeBtm;
		Camera_Matrox camEBR;

		ALID alid_WaferExist;

		#region Getter/Setter
		public Axis AxisRotate { get => axisRotate; private set => axisRotate = value; }
		public Axis AxisEdgeX { get => axisEdgeX; private set => axisEdgeX = value; }
		public Axis AxisEbrX { get => axisEbrX; private set => axisEbrX = value; }
		public Axis AxisEbrZ { get => axisEbrZ; private set => axisEbrZ = value; }
		public DIO_O DoVac { get => doVac; private set => doVac = value; }
		public DIO_O DoBlow { get => doBlow; private set => doBlow = value; }
		public MemoryPool MemoryPool { get => memoryPool; private set => memoryPool = value; }
		public MemoryGroup MemoryGroup { get => memoryGroup; private set => memoryGroup = value; }
		public MemoryData MemoryEdgeTop { get => memoryEdgeTop; private set => memoryEdgeTop = value; }
		public MemoryData MemoryEdgeSide { get => memoryEdgeSide; private set => memoryEdgeSide = value; }
		public MemoryData MemoryEdgeBtm { get => memoryEdgeBtm; private set => memoryEdgeBtm = value; }
		public MemoryData MemoryEBR { get => memoryEBR; private set => memoryEBR = value; }
		public LightSet LightSet { get => lightSet; private set => lightSet = value; }
		public Camera_Dalsa CamEdgeTop { get => camEdgeTop; private set => camEdgeTop = value; }
		public Camera_Dalsa CamEdgeSide { get => camEdgeSide; private set => camEdgeSide = value; }
		public Camera_Dalsa CamEdgeBtm { get => camEdgeBtm; private set => camEdgeBtm = value; }
		public Camera_Matrox CamEBR { get => camEBR; private set => camEBR = value; }
		#endregion

		public override void GetTools(bool bInit)
		{
			p_sInfo = m_toolBox.GetAxis(ref axisRotate, this, "Axis Rotate");
			p_sInfo = m_toolBox.GetAxis(ref axisEdgeX, this, "Axis Edge X");
			p_sInfo = m_toolBox.GetAxis(ref axisEbrX, this, "Axis EBR X");
			p_sInfo = m_toolBox.GetAxis(ref axisEbrZ, this, "Axis EBR Z");
			p_sInfo = m_toolBox.GetDIO(ref doVac, this, "Stage Vacuum");
			p_sInfo = m_toolBox.GetDIO(ref doBlow, this, "Stage Blow");
			p_sInfo = m_toolBox.GetDIO(ref diWaferExist, this, "Wafer Exist");
			p_sInfo = m_toolBox.GetDIO(ref diWaferExistVac, this, "Wafer Exist -Vac");
			
			p_sInfo = m_toolBox.Get(ref memoryPool, this, "Memory", 1);
			p_sInfo = m_toolBox.GetCamera(ref camEdgeTop, this, "Cam EdgeTop");
			p_sInfo = m_toolBox.GetCamera(ref camEdgeSide, this, "Cam EdgeSide");
			p_sInfo = m_toolBox.GetCamera(ref camEdgeBtm, this, "Cam EdgeBottom");
            p_sInfo = m_toolBox.GetCamera(ref camEBR, this, "Cam EBR");
            p_sInfo = m_toolBox.Get(ref lightSet, this);
			memoryGroup = memoryPool.GetGroup(p_id);
			alid_WaferExist = m_gaf.GetALID(this, "Wafer Exist", "Wafer Exist");
		}
		#endregion

		#region GrabMode
		int m_lGrabMode = 0;
		public ObservableCollection<GrabModeEdge> m_aGrabMode = new ObservableCollection<GrabModeEdge>();

		public List<string> p_asGrabMode
		{
			get
			{
				List<string> asGrabMode = new List<string>();
				foreach (GrabModeBase grabMode in m_aGrabMode)
					asGrabMode.Add(grabMode.p_sName);
				return asGrabMode;
			}
		}

		public GrabModeEdge GetGrabMode(string sGrabMode)
		{
			foreach (GrabModeEdge grabMode in m_aGrabMode)
			{
				if (sGrabMode == grabMode.p_sName)
					return grabMode;
			}
			return null;
		}

		public GrabModeEdge GetGrabMode(int index)
		{
			if (m_aGrabMode?.Count > 0 && (m_aGrabMode.Count - 1 >= index))
			{
				return m_aGrabMode[index];
			}
			return null;
		}

		void RunTreeGrabMode(Tree tree)
		{
			m_lGrabMode = tree.Set(m_lGrabMode, m_lGrabMode, "Count", "Grab Mode Count");
			while (m_aGrabMode.Count < m_lGrabMode)
			{
				string id = "Mode." + m_aGrabMode.Count.ToString("00");
				GrabModeEdge grabMode = new GrabModeEdge(id, m_cameraSet, lightSet, memoryPool);
				m_aGrabMode.Add(grabMode);
			}
			while (m_aGrabMode.Count > m_lGrabMode)
				m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
			foreach (GrabModeEdge grabMode in m_aGrabMode)
				grabMode.RunTreeName(tree.GetTree("Name", false));
			foreach (GrabModeEdge grabMode in m_aGrabMode)
				grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
		}
		#endregion

		#region DIO
		public bool p_bStageVac
		{
			get { return doVac.p_bOut; }
			set
			{
				if (doVac.p_bOut == value) return;
				doVac.Write(value);
			}
		}

		public bool p_bStageBlow
		{
			get { return doBlow.p_bOut; }
			set
			{
				if (doBlow.p_bOut == value) return;
				doBlow.Write(value);
			}
		}

		public void RunBlow(int msDelay)
		{
			doBlow.DelayOff(msDelay);
		}
		#endregion

		#region override
		public override void Reset()
		{
			base.Reset();
		}

		double pulse360 = 360000;
		public double Pulse360 { get => pulse360; set => pulse360 = value; }
		//double edgeCamTriggerRatio = 1; //캠익에서 트리거 분주비
		//public double EdgeCamTriggerRatio { get => edgeCamTriggerRatio; set => edgeCamTriggerRatio = value; }
		//double ebrCamTriggerRatio = 3.0 / 4;
		//public double EbrCamTriggerRatio { get => ebrCamTriggerRatio; set => ebrCamTriggerRatio = value; }
		//double margin = 36000;
		//public double Margin { get => margin; set => margin = value; }

		public override void InitMemorys()
		{
			int nImageX = 1000; //camEdgeTop.GetRoiSize().X;
			int nImageY = 1000; //(int)(pulse360 * edgeCamTriggerRatio + margin);
			memoryGroup = memoryPool.GetGroup(p_id);
			memoryEdgeTop = memoryPool.GetGroup(p_id).CreateMemory(EDGE_TYPE.EdgeTop.ToString(), 3, 1, nImageX, nImageY);
			memoryEdgeSide = memoryPool.GetGroup(p_id).CreateMemory(EDGE_TYPE.EdgeSide.ToString(), 3, 1, nImageX, nImageY);
			memoryEdgeBtm = memoryPool.GetGroup(p_id).CreateMemory(EDGE_TYPE.EdgeBottom.ToString(), 3, 1, nImageX, nImageY);

			int ebrImageX = 1000; //camEBR.GetRoiSize().X;
			int ebrImageY = 1000; //(int)(pulse360 * ebrCamTriggerRatio + margin);
			memoryEBR = memoryPool.GetGroup(p_id).CreateMemory(EDGE_TYPE.EBR.ToString(), 1, 1, ebrImageX, ebrImageY);
		}
		#endregion

		#region Axis
		private int pulseRound = 1000;
		public int PulseRound { get => pulseRound; set => pulseRound = value; }
		void RunTreeAxis(Tree tree)
		{
			pulseRound = tree.Set(pulseRound, pulseRound, "Rotate Pulse / Round", "Rotate Axis Pulse / 1 Round (pulse)");
		}

		public enum eAxisPosEdge
		{
			Ready,
		}
		void InitPosAxis()
		{
			AxisRotate.AddPos(Enum.GetNames(typeof(eAxisPosEdge)));
		}
		#endregion

		#region IWTRChild
		bool _bLock = false;
		public bool p_bLock
		{
			get
			{
				return _bLock;
			}
			set
			{
				if (_bLock == value)
					return;
				_bLock = value;
			}
		}

		bool IsLock()
		{
			for (int n = 0; n < 10; n++)
			{
				if (p_bLock == false)
					return false;
				Thread.Sleep(100);
			}
			return true;
		}

		public List<string> p_asChildSlot
		{
			get
			{
				return null;
			}
		}

		public InfoWafer GetInfoWafer(int nID)
		{
			return p_infoWafer;
		}

		public void SetInfoWafer(int nID, InfoWafer infoWafer)
		{
			p_infoWafer = infoWafer;
		}

		public string IsGetOK(int nID)
		{
			if (p_eState != eState.Ready)
				return p_id + " eState not Ready";
			//if (p_infoWafer == null)
			//	return p_id + " IsGetOK - InfoWafer not Exist";
			return "OK";
		}

		public string IsPutOK(InfoWafer infoWafer, int nID)
		{
			if (p_eState != eState.Ready)
				return p_id + " eState not Ready";
			//if (p_infoWafer != null)
			//	return p_id + " IsPutOK - InfoWafer Exist";
			//if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false)
			//	return p_id + " not Enable Wafer Size";
			return "OK";
		}

		public int GetTeachWTR(InfoWafer infoWafer = null)
		{
			if (infoWafer == null)
				infoWafer = p_infoWafer;
			return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
		}

		public string BeforeGet(int nID)
		{
			//            string info = MoveReadyPos();
			//            if (info != "OK") return info;
			axisRotate.StartHome();
			if (axisRotate.WaitReady() != "OK")
			{
				p_bStageVac = false;
				p_eState = eState.Error;
				return "Fail";
			}
			axisRotate.StartMove(eAxisPosEdge.Ready);
			if (axisRotate.WaitReady() != "OK")
			{
				p_bStageVac = false;
				p_eState = eState.Error;
				return "Fail";
			}
			p_bStageVac = false;
			return "OK";
		}

		public string BeforePut(int nID)
		{
			//            string info = MoveReadyPos();
			//            if (info != "OK") return info;
			axisRotate.StartHome();
			if (axisRotate.WaitReady() != "OK")
			{
				p_bStageVac = false;
				p_eState = eState.Error;
				return "OK";
			}
			axisRotate.StartMove(eAxisPosEdge.Ready);
			if (axisRotate.WaitReady() != "OK")
			{
				p_bStageVac = false;
				p_eState = eState.Error;
				return "Fail";
			}
			p_bStageVac = false;
			return "OK";
		}

		public string AfterGet(int nID)
		{
			return "OK";
		}

		public string AfterPut(int nID)
		{
			doVac.Write(true);
			//if (!diWaferExist.p_bIn || !diWaferExistVac.p_bIn)
			//	alid_WaferExist.Run(true, "Wafer Check Error");
			return "OK";
		}

		enum eCheckWafer
		{
			InfoWafer,
			Sensor
		}
		eCheckWafer m_eCheckWafer = eCheckWafer.InfoWafer;
		public bool IsWaferExist(int nID)
		{
			switch (m_eCheckWafer)
			{
				case eCheckWafer.Sensor: return false; // m_diWaferExist.p_bIn;
				default: return (p_infoWafer != null);
			}
		}

		InfoWafer.WaferSize m_waferSize;
		public void RunTreeTeach(Tree tree)
		{
			m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
		}

		string m_sInfoWafer = "";
		InfoWafer _infoWafer = null;
		public InfoWafer p_infoWafer
		{
			get
			{
				return _infoWafer;
			}
			set
			{
				m_sInfoWafer = (value == null) ? "" : value.p_id;
				_infoWafer = value;
				if (m_reg != null)
					m_reg.Write("sInfoWafer", m_sInfoWafer);
				OnPropertyChanged();
			}
		}

		Registry m_reg = null;
		public void ReadInfoWafer_Registry()
		{
			m_reg = new Registry(p_id + ".InfoWafer");
			m_sInfoWafer = m_reg.Read("sInfoWafer", m_sInfoWafer);
			p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
		}
		#endregion

		#region State Home
		public string OpenCamera()
		{
			if (camEdgeTop != null && camEdgeTop.p_CamInfo.p_eState == RootTools.Camera.Dalsa.eCamState.Init)
				camEdgeTop.Connect();
			if (camEdgeSide != null && camEdgeSide.p_CamInfo.p_eState == RootTools.Camera.Dalsa.eCamState.Init)
				camEdgeSide.Connect();
			if (camEdgeBtm != null && camEdgeBtm.p_CamInfo.p_eState == RootTools.Camera.Dalsa.eCamState.Init)
				camEdgeBtm.Connect();
			if (camEBR != null && camEBR.p_CamInfo.p_eState == RootTools.Camera.Matrox.eCamState.Init)
				camEBR.Connect();
			return "OK";
		}

		public override string StateHome()
		{
			if (EQ.p_bSimulate) return "OK";
			//            p_bStageBlow = false;
			//            p_bStageVac = true;

			OpenCamera();
			p_bStageVac = true;
			
			axisEdgeX.StartHome();
			axisEbrX.StartHome();
			axisEbrZ.StartHome();
			if (axisEdgeX.WaitReady() != "OK")
			{
				p_bStageVac = false;
				p_eState = eState.Error;
				return "OK";
			}
			if (axisEbrX.WaitReady() != "OK")
			{
				p_bStageVac = false;
				p_eState = eState.Error;
				return "OK";
			}
			if (axisEbrZ.WaitReady() != "OK")
			{
				p_bStageVac = false;
				p_eState = eState.Error;
				return "OK";
			}

			Thread.Sleep(200);
			axisRotate.StartHome();
			if (axisRotate.WaitReady() == "OK")
			{
				p_eState = eState.Ready;
				return "OK";
			}

			p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
			
			if (diWaferExist.p_bIn == false)
				p_bStageVac = false;

			return p_sInfo;
		}
		#endregion

		#region Tree
		public override void RunTree(Tree tree)
		{
			base.RunTree(tree);
			m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");

			RunTreeGrabMode(tree.GetTree("Grab Mode", false));
		}
		#endregion

		public enum EDGE_TYPE
		{
			EdgeTop,
			EdgeSide,
			EdgeBottom,
			EBR,
		}

		public EdgeSideVision(string id, IEngineer engineer)
		{ 
			base.InitBase(id, engineer);
			InitPosAxis();
			m_waferSize = new InfoWafer.WaferSize(id, false, false);
		}
				
		public override void ThreadStop()
		{
			base.ThreadStop();
		}

		#region ModuleRun
		protected override void InitModuleRuns()
		{
			AddModuleRunList(new Run_GrabEdge(this), true, "Run Grab Edge");
			AddModuleRunList(new Run_GrabEBR(this), true, "Run Grab EBR");
			AddModuleRunList(new Run_InspectEdge(this), true, "Run Inspect Edge");
			AddModuleRunList(new Run_InspectEBR(this), true, "Run Inspect EBR");
		}

		public ImageData GetMemoryData(EDGE_TYPE data)
		{
			ImageData result = new ImageData(memoryPool.GetMemory(p_id, data.ToString()));
			return result;
		}
		#endregion

	}
}
