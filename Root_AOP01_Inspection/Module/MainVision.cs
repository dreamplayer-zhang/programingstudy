using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Root_AOP01_Inspection.Recipe;
using Root_EFEM;
using Root_EFEM.Module;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Control.Ajin;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static RootTools.Control.Axis;
using MBrushes = System.Windows.Media.Brushes;
using DPoint = System.Drawing.Point;
using RootTools_CLR;
using System.Linq;
using RootTools.Inspects;

namespace Root_AOP01_Inspection.Module
{
	public class MainVision : ModuleBase, IWTRChild
	{
		public Dispatcher dispatcher;   // RecipeLADS_ViewModel 페이지에서 LADS Heatmap 바인딩을 하려면 Dispatcher 필요

		#region ToolBox
		public Axis m_axisRotate;
		public Axis m_axisZ;
		public Axis m_axisSideZ;
		public AxisXY m_axisXY;
		public DIO_I m_diExistVision;
		public DIO_I m_diReticleTiltCheck;
		public DIO_I m_diReticleFrameCheck;

		public DIO_O m_do45DTrigger;

		public MemoryPool m_memoryPool;
		public MemoryGroup m_memoryGroup;
		public MemoryData m_memoryMain;
		public MemoryData m_memorySideLeft;
		public MemoryData m_memorySideRight;
		public MemoryData m_memorySideTop;
		public MemoryData m_memorySideBottom;
		public MemoryData m_memoryTDI45;
		public MemoryData m_memoryLADS;

		public LightSet m_lightSet;
		public Camera_Dalsa m_CamTDI90;
		public Camera_Dalsa m_CamTDI45;
		public Camera_Dalsa m_CamTDISide;
		public Camera_Basler m_CamLADS;

		class LADSInfo//한 줄에 대한 정보
		{
			public double[] m_Heightinfo;
			public RPoint axisPos;//시작점의 x,y
			public double endYPos;//끝점의 y 정보

			LADSInfo() { }
			public LADSInfo(RPoint _axisPos, double _endYPos, int arrcap/*heightinfo capacity*/)
			{
				axisPos = _axisPos;
				endYPos = _endYPos;
				m_Heightinfo = new double[arrcap];
			}
		}

		static List<LADSInfo> ladsinfos;

		public override void GetTools(bool bInit)
		{
			p_sInfo = m_toolBox.Get(ref m_diExistVision, this, "Reticle Exist on Vision");
			p_sInfo = m_toolBox.Get(ref m_diReticleTiltCheck, this, "Reticle Tilt Check");
			p_sInfo = m_toolBox.Get(ref m_diReticleFrameCheck, this, "Reticle Frame Check");
			p_sInfo = m_toolBox.Get(ref m_do45DTrigger, this, "45D Trigger");
			p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Axis Rotate");
			p_sInfo = m_toolBox.Get(ref m_axisSideZ, this, "Axis Side Z");
			p_sInfo = m_toolBox.Get(ref m_axisZ, this, "Axis Z");
			p_sInfo = m_toolBox.Get(ref m_axisXY, this, "Axis XY");
			p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Vision Memory", 1);
			p_sInfo = m_toolBox.Get(ref m_lightSet, this);
			p_sInfo = m_toolBox.Get(ref m_CamTDI90, this, "TDI 90");
			p_sInfo = m_toolBox.Get(ref m_CamTDI45, this, "TDI 45");
			p_sInfo = m_toolBox.Get(ref m_CamTDISide, this, "TDI Side");
			p_sInfo = m_toolBox.Get(ref m_CamLADS, this, "LADS");
			m_axisRotate.StartMove(1000);
		}
		#endregion

		#region Grab Mode
		int m_lGrabMode = 0;
		public ObservableCollection<GrabMode> m_aGrabMode = new ObservableCollection<GrabMode>();
		public List<string> p_asGrabMode
		{
			get
			{
				List<string> asGrabMode = new List<string>();
				foreach (GrabMode grabMode in m_aGrabMode) asGrabMode.Add(grabMode.p_sName);
				return asGrabMode;
			}
		}

		public GrabMode GetGrabMode(string sGrabMode)
		{
			foreach (GrabMode grabMode in m_aGrabMode)
			{
				if (sGrabMode == grabMode.p_sName) return grabMode;
			}
			return null;
		}

		void RunTreeGrabMode(Tree tree)
		{
			m_lGrabMode = tree.Set(m_lGrabMode, m_lGrabMode, "Count", "Grab Mode Count");
			while (m_aGrabMode.Count < m_lGrabMode)
			{
				string id = "Mode." + m_aGrabMode.Count.ToString("00");
				GrabMode grabMode = new GrabMode(id, m_cameraSet, m_lightSet, m_memoryPool);
				m_aGrabMode.Add(grabMode);
			}
			while (m_aGrabMode.Count > m_lGrabMode) m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
			foreach (GrabMode grabMode in m_aGrabMode) grabMode.RunTreeName(tree.GetTree("Name", false));
			foreach (GrabMode grabMode in m_aGrabMode) grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
		}
		#endregion

		#region Axis Position
		public enum eAxisPos
		{
			ReadyPos,   // RTR 티칭포인트
			ScanPos,
		}

		void InitPosAlign()
		{
			m_axisZ.AddPos(Enum.GetNames(typeof(eAxisPos)));
			m_axisRotate.AddPos(Enum.GetNames(typeof(eAxisPos)));
			m_axisSideZ.AddPos(Enum.GetNames(typeof(eAxisPos)));
			m_axisXY.AddPos(Enum.GetNames(typeof(eAxisPos)));
		}
		#endregion

		#region InfoWafer
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

		#region InfoWafer UI
		InfoWaferChild_UI m_ui;
		void InitInfoWaferUI()
		{
			m_ui = new InfoWaferChild_UI();
			m_ui.Init(this);
			m_aTool.Add(m_ui);
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
			if (p_infoWafer == null)
				return p_id + " IsGetOK - InfoWafer not Exist";
			return "OK";
		}

		public string IsPutOK(InfoWafer infoWafer, int nID)
		{
			if (p_eState != eState.Ready)
				return p_id + " eState not Ready";
			if (p_infoWafer != null)
				return p_id + " IsPutOK - InfoWafer Exist";
			if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false)
				return p_id + " not Enable Wafer Size";
			return "OK";
		}

		public int GetTeachWTR(InfoWafer infoWafer = null)
		{
			if (infoWafer == null)
				infoWafer = p_infoWafer;
			return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
		}

		private string MoveReadyPos()
		{
			if (Run(m_axisXY.p_axisX.StartMove(eAxisPos.ReadyPos)))
				return p_sInfo;
			if (Run(m_axisXY.p_axisY.StartMove(eAxisPos.ReadyPos)))
				return p_sInfo;
			if (Run(m_axisZ.StartMove(eAxisPos.ReadyPos)))
				return p_sInfo;
			if (Run(m_axisRotate.StartMove(eAxisPos.ReadyPos)))
				return p_sInfo;

			if (Run(m_axisRotate.WaitReady()))
				return p_sInfo;
			if (Run(m_axisZ.WaitReady()))
				return p_sInfo;
			if (Run(m_axisXY.WaitReady()))
				return p_sInfo;
			return "OK";
		}

		public string BeforeGet(int nID)
		{
			string info = MoveReadyPos();
			if (info != "OK")
				return info;
			return "OK";
		}

		public string BeforePut(int nID)
		{
			string info = MoveReadyPos();
			if (info != "OK")
				return info;
			return "OK";
		}

		public string AfterGet(int nID)
		{
			return "OK";
		}

		public string AfterPut(int nID)
		{
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
				case eCheckWafer.Sensor: return false; //m_diWaferExist.p_bIn;
				default: return (p_infoWafer != null);
			}
		}

		InfoWafer.WaferSize m_waferSize;
		public void RunTreeTeach(Tree tree)
		{
			m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
		}
		#endregion

		#region override
		public override void InitMemorys()
		{
			//MainVision.Main.
			m_memoryGroup = m_memoryPool.GetGroup(p_id);
			m_memoryMain = m_memoryGroup.CreateMemory(App.mMainMem, 1, 1, 1000, 1000);

			m_memorySideLeft = m_memoryGroup.CreateMemory(App.mSideLeftMem, 1, 1, 1000, 1000);
			m_memorySideBottom = m_memoryGroup.CreateMemory(App.mSideBotMem, 1, 1, 1000, 1000);
			m_memorySideRight = m_memoryGroup.CreateMemory(App.mSideRightMem, 1, 1, 1000, 1000);
			m_memorySideTop = m_memoryGroup.CreateMemory(App.mSideTopMem, 1, 1, 1000, 1000);

			m_memoryTDI45 = m_memoryGroup.CreateMemory("TDI45", 1, 1, 1000, 1000);
			m_memoryLADS = m_memoryGroup.CreateMemory("LADS", 1, 1, 1000, 1000);
		}
		#endregion

		#region State Home
		public override string StateHome()
		{
			if (EQ.p_bSimulate)
				return "OK";
			//            p_bStageBlow = false;
			//            p_bStageVac = true;
			Thread.Sleep(200);

			if (m_CamTDI90 != null && m_CamTDI90.p_CamInfo.p_eState == eCamState.Init)
				m_CamTDI90.Connect();
			if (m_CamTDI45 != null && m_CamTDI45.p_CamInfo.p_eState == eCamState.Init)
				m_CamTDI45.Connect();
			if (m_CamLADS.p_CamInfo._OpenStatus == false)
				m_CamLADS.Connect();
			if (m_CamTDISide != null && m_CamTDISide.p_CamInfo.p_eState == eCamState.Init)
				m_CamTDISide.Connect();

			// Theta와 SideZ 충돌 방지를 위해 SideZ축을 Safety 위치로 우선 이동하도록 Home 시퀀스 수정
			if (base.p_eState == eState.Run) return "Invalid State : Run";
			if (EQ.IsStop()) return "Home Stop";
			if (m_axisSideZ != null) m_axisSideZ.ServoOn(true);
			Thread.Sleep(200);
			if (EQ.IsStop()) return "Home Stop";
			if (m_axisSideZ != null) base.p_sInfo = m_axisSideZ.StartHome();
			while (true)
			{
				Thread.Sleep(10);
				if (EQ.IsStop(1000)) return "Home Stop";
				bool bDone = true;
				if ((m_axisSideZ != null) && (m_axisSideZ.p_eState == Axis.eState.Home)) bDone = false;
				if (bDone) break;
			}

			if (m_axisSideZ.WaitReady() != "OK")
				return "Error";

			m_axisSideZ.StartMove(eAxisPos.ReadyPos);
			if (m_axisSideZ.WaitReady() != "OK")
				return "Error";

			var listAxis = new List<Axis>(base.m_listAxis);
			for (int i = 0; i < listAxis.Count; i++)
			{
				if (listAxis[i].p_id == "MainVision.Axis Side Z")
				{
					listAxis.RemoveAt(i);
					break;
				}
			}
			p_sInfo = base.StateHome(listAxis);
			//

			p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
			//p_bStageVac = false;

			//m_axisRotate.StartMove(eAxisPos.ReadyPos);
			//if (m_axisRotate.WaitReady() != "OK")
			//    return "Error";

			return "OK";
		}
		#endregion

		#region Tree
		public override void RunTree(Tree tree)
		{
			base.RunTree(tree);
			RunTreeSetup(tree.GetTree("Setup", false));
			RunTreeGrabMode(tree.GetTree("Grab Mode", false));
		}

		void RunTreeSetup(Tree tree)
		{
			m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");
		}
		#endregion

		#region Inspection Result
		bool m_bAlignKeyPass = true;
		public bool p_bAlignKeyPass
		{
			get { return m_bAlignKeyPass; }
			set
			{
				m_bAlignKeyPass = value;
				OnPropertyChanged();
			}
		}

		bool m_bPatterhShiftPass = true;
		public bool p_bPatternShiftPass
		{
			get { return m_bPatterhShiftPass; }
			set
			{
				m_bPatterhShiftPass = value;
				OnPropertyChanged();
			}
		}

		bool m_bPellicleShiftPass = true;
		public bool p_bPellicleShiftPass
		{
			get { return m_bPellicleShiftPass; }
			set
			{
				m_bPellicleShiftPass = value;
				OnPropertyChanged();
			}
		}

		internal void SetRectInfo(ObservableCollection<TRect> rectList, string mainModuleName)
		{
			//TODO 여기서 직접 가져와서 수정하자!
			var targetidx = -1;
			for (int i = 0; i < m_asModuleRun.Count; i++)
			{
				if (m_asModuleRun[i] == mainModuleName)
				{
					targetidx = i;
					break;
				}
			}
			if(targetidx == -1)
			{
				return;
			}

			for (int i = 0; i < ((Run_SurfaceInspection)m_aModuleRun[targetidx]).EdgeList.Length; i++)
			{
				((Run_SurfaceInspection)m_aModuleRun[targetidx]).EdgeList[i] = new TRect(rectList[i]);
			}
			((Run_SurfaceInspection)m_aModuleRun[targetidx]).UpdeteTree();
		}
		internal void SetSurfaceParam(bool isBright, int GV, int size, string mainModuleName)
		{
			var targetidx = -1;
			for (int i = 0; i < m_asModuleRun.Count; i++)
			{
				if (m_asModuleRun[i] == mainModuleName)
				{
					targetidx = i;
					break;
				}
			}
			if (targetidx == -1)
			{
				return;
			}
			((Run_SurfaceInspection)m_aModuleRun[targetidx]).BrightGV = isBright;
			((Run_SurfaceInspection)m_aModuleRun[targetidx]).SurfaceGV = GV;
			((Run_SurfaceInspection)m_aModuleRun[targetidx]).SurfaceSize = size;

			((Run_SurfaceInspection)m_aModuleRun[targetidx]).UpdeteTree();
		}

		double m_dPatternShiftDistance = 0.0;
		public double p_dPatternShiftDistance
		{
			get { return m_dPatternShiftDistance; }
			set
			{
				m_dPatternShiftDistance = value;
				OnPropertyChanged();
			}
		}

		double m_dPatternShiftAngle = 0.0;
		public double p_dPatternShiftAngle
		{
			get { return m_dPatternShiftAngle; }
			set
			{
				m_dPatternShiftAngle = value;
				OnPropertyChanged();
			}
		}

		double m_dPellicleShiftDistance = 0.0;
		public double p_dPellicleShiftDistance
		{
			get { return m_dPellicleShiftDistance; }
			set
			{
				m_dPellicleShiftDistance = value;
				OnPropertyChanged();
			}
		}

		double m_dPellicleShiftAngle = 0.0;
		public double p_dPellicleShiftAngle
		{
			get { return m_dPellicleShiftAngle; }
			set
			{
				m_dPellicleShiftAngle = value;
				OnPropertyChanged();
			}
		}

		bool m_bBarcodePass = true;
		public bool p_bBarcodePass
		{
			get { return m_bBarcodePass; }
			set
			{
				m_bBarcodePass = value;
				OnPropertyChanged();
			}
		}

		bool m_bPellicleExpandingPass = true;
		public bool p_bPellicleExpandingPass
		{
			get { return m_bPellicleExpandingPass; }
			set
			{
				m_bPellicleExpandingPass = value;
				OnPropertyChanged();
			}
		}

		double m_dPellicleExpandingMax = 0.0;
		public double p_dPellicleExpandingMax
		{
			get { return m_dPellicleExpandingMax; }
			set
			{
				m_dPellicleExpandingMax = value;
				OnPropertyChanged();
			}
		}

		double m_dPellicleExpandingMin = 0.0;
		public double p_dPellicleExpandingMin
		{
			get { return m_dPellicleExpandingMin; }
			set
			{
				m_dPellicleExpandingMin = value;
				OnPropertyChanged();
			}
		}

		BitmapImage m_bmpImgPellicleHeatmap = new BitmapImage();
		public BitmapImage p_bmpImgPellicleHeatmap
		{
			get { return m_bmpImgPellicleHeatmap; }
			set
			{
				m_bmpImgPellicleHeatmap = value;
				OnPropertyChanged();
			}
		}
		#endregion

		#region ProgressBar
		int m_nPellicleExpandingProgressValue = 0;
		public int p_nPellicleExpandingProgressValue
		{
			get { return m_nPellicleExpandingProgressValue; }
			set
			{
				m_nPellicleExpandingProgressValue = value;
				OnPropertyChanged();
			}
		}

		int m_nPellicleExpandingProgressMin = 0;
		public int p_nPellicleExpandingProgressMin
		{
			get { return m_nPellicleExpandingProgressMin; }
			set
			{
				m_nPellicleExpandingProgressMin = value;
				OnPropertyChanged();
			}
		}

		int m_nPellicleExpandingProgressMax = 100;
		public int p_nPellicleExpandingProgressMax
		{
			get { return m_nPellicleExpandingProgressMax; }
			set
			{
				m_nPellicleExpandingProgressMax = value;
				OnPropertyChanged();
			}
		}

		int m_nPellicleExpandingProgressPercent = 0;
		public int p_nPellicleExpandingProgressPercent
		{
			get { return m_nPellicleExpandingProgressPercent; }
			set
			{
				m_nPellicleExpandingProgressPercent = value;
				OnPropertyChanged();
			}
		}

		int m_nBarcodeInspectionProgressValue = 0;
		public int p_nBarcodeInspectionProgressValue
		{
			get { return m_nBarcodeInspectionProgressValue; }
			set
			{
				m_nBarcodeInspectionProgressValue = value;
				OnPropertyChanged();
			}
		}

		int m_nBarcodeInspectionProgressMin = 0;
		public int p_nBarcodeInspectionProgressMin
		{
			get { return m_nBarcodeInspectionProgressMin; }
			set
			{
				m_nBarcodeInspectionProgressMin = value;
				OnPropertyChanged();
			}
		}

		int m_nBarcodeInspectionProgressMax = 100;
		public int p_nBarcodeInspectionProgressMax
		{
			get { return m_nBarcodeInspectionProgressMax; }
			set
			{
				m_nBarcodeInspectionProgressMax = value;
				OnPropertyChanged();
			}
		}

		int m_nBarcodeInspectionProgressPercent = 0;
		public int p_nBarcodeInspectionProgressPercent
		{
			get { return m_nBarcodeInspectionProgressPercent; }
			set
			{
				m_nBarcodeInspectionProgressPercent = value;
				OnPropertyChanged();
			}
		}

		int m_nPatternShiftProgressValue = 0;
		public int p_nPatternShiftProgressValue
		{
			get { return m_nPatternShiftProgressValue; }
			set
			{
				m_nPatternShiftProgressValue = value;
				OnPropertyChanged();
			}
		}

		int m_nPatternShiftProgressMin = 0;
		public int p_nPatternShiftProgressMin
		{
			get { return m_nPatternShiftProgressMin; }
			set
			{
				m_nPatternShiftProgressMin = value;
				OnPropertyChanged();
			}
		}

		int m_nPatternShiftProgressMax = 100;
		public int p_nPatternShiftProgressMax
		{
			get { return m_nPatternShiftProgressMax; }
			set
			{
				m_nPatternShiftProgressMax = value;
				OnPropertyChanged();
			}
		}

		int m_nPatternShiftProgressPercent = 0;
		public int p_nPatternShiftProgressPercent
		{
			get { return m_nPatternShiftProgressPercent; }
			set
			{
				m_nPatternShiftProgressPercent = value;
				OnPropertyChanged();
			}
		}

		int m_nPellicleShiftProgressValue = 0;
		public int p_nPellicleShiftProgressValue
		{
			get { return m_nPellicleShiftProgressValue; }
			set
			{
				m_nPellicleShiftProgressValue = value;
				OnPropertyChanged();
			}
		}

		int m_nPellicleShiftProgressMin = 0;
		public int p_nPellicleShiftProgressMin
		{
			get { return m_nPellicleShiftProgressMin; }
			set
			{
				m_nPellicleShiftProgressMin = value;
				OnPropertyChanged();
			}
		}

		int m_nPellicleShiftProgressMax = 100;
		public int p_nPellicleShiftProgressMax
		{
			get { return m_nPellicleShiftProgressMax; }
			set
			{
				m_nPellicleShiftProgressMax = value;
				OnPropertyChanged();
			}
		}

		int m_nPellicleShiftProgressPercent = 0;
		public int p_nPellicleShiftProgressPercent
		{
			get { return m_nPellicleShiftProgressPercent; }
			set
			{
				m_nPellicleShiftProgressPercent = value;
				OnPropertyChanged();
			}
		}

		int m_nAlignKeyProgressValue = 0;
		public int p_nAlignKeyProgressValue
		{
			get { return m_nAlignKeyProgressValue; }
			set
			{
				m_nAlignKeyProgressValue = value;
				OnPropertyChanged();
			}
		}

		int m_nAlignKeyProgressMin = 0;
		public int p_nAlignKeyProgressMin
		{
			get { return m_nAlignKeyProgressMin; }
			set
			{
				m_nAlignKeyProgressMin = value;
				OnPropertyChanged();
			}
		}

		int m_nAlignKeyProgressMax = 100;
		public int p_nAlignKeyProgressMax
		{
			get { return m_nAlignKeyProgressMax; }
			set
			{
				m_nAlignKeyProgressMax = value;
				OnPropertyChanged();
			}
		}

		int m_nAlignKeyProgressPercent = 0;
		public int p_nAlignKeyProgressPercent
		{
			get { return m_nAlignKeyProgressPercent; }
			set
			{
				m_nAlignKeyProgressPercent = value;
				OnPropertyChanged();
			}
		}
		#endregion

		#region Public Variable
		public int[] m_narrSideEdgeOffset = new int[4];
		public double m_dThetaAlignOffset = 0;
		#endregion

		#region Vision Algorithm
		Mat GetMatImage(MemoryData mem, CRect crtROI)
		{
			if (crtROI.Width < 1 || crtROI.Height < 1) return null;
			if (crtROI.Left < 0 || crtROI.Top < 0) return null;
			ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
			IntPtr p = mem.GetPtr();
			img.SetData(p, crtROI, (int)mem.W);
			Mat matReturn = new Mat((int)img.p_Size.Y, (int)img.p_Size.X, Emgu.CV.CvEnum.DepthType.Cv8U, img.p_nByte, img.GetPtr(), (int)img.p_Stride);

			return matReturn;
		}

		Image<Gray, byte> GetGrayByteImageFromMemory(MemoryData mem, CRect crtROI)
		{
			if (crtROI.Width < 1 || crtROI.Height < 1) return null;
			if (crtROI.Left < 0 || crtROI.Top < 0) return null;
			if (crtROI.Width % 4 != 0)
			{
				int nSpare = 0;
				while (((crtROI.Width + nSpare) % 4) != 0) nSpare++;
				crtROI = new CRect(crtROI.Left, crtROI.Top, crtROI.Right + nSpare, crtROI.Bottom);
			}
			ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
			IntPtr p = mem.GetPtr();
			img.SetData(p, crtROI, (int)mem.W);

			byte[] barr = img.GetByteArray();
			System.Drawing.Bitmap bmp = img.GetBitmapToArray(crtROI.Width, crtROI.Height, barr);
			Image<Gray, byte> imgReturn = new Image<Gray, byte>(bmp);

			return imgReturn;
		}

		bool TemplateMatching(MemoryData mem, CRect crtSearchArea, Image<Gray, byte> imgSrc, Image<Gray, byte> imgTemplate, out CPoint cptCenter, double dMatchScore)
		{
			// variable
			int nWidthDiff = 0;
			int nHeightDiff = 0;
			Point ptMaxRelative = new Point();
			float fMaxScore = float.MinValue;
			bool bFoundTemplate = false;

			// implement
			Image<Gray, float> imgResult = imgSrc.MatchTemplate(imgTemplate, TemplateMatchingType.CcorrNormed);
			nWidthDiff = imgSrc.Width - imgResult.Width;
			nHeightDiff = imgSrc.Height - imgResult.Height;
			float[,,] matches = imgResult.Data;

			for (int x = 0; x < matches.GetLength(1); x++)
			{
				for (int y = 0; y < matches.GetLength(0); y++)
				{
					if (fMaxScore < matches[y, x, 0] && dMatchScore <= matches[y, x, 0])
					{
						fMaxScore = matches[y, x, 0];
						ptMaxRelative.X = x;
						ptMaxRelative.Y = y;
						bFoundTemplate = true;
					}
				}
			}
			cptCenter = new CPoint();
			cptCenter.X = (int)(crtSearchArea.Left + ptMaxRelative.X) + (int)(nWidthDiff / 2);
			cptCenter.Y = (int)(crtSearchArea.Top + ptMaxRelative.Y) + (int)(nHeightDiff / 2);

			return bFoundTemplate;
		}

		public enum eSearchDirection
		{
			TopToBottom = 0,
			LeftToRight,
			RightToLeft,
			BottomToTop,
		}

		unsafe int GetEdge(MemoryData mem, CRect crtROI, int nProfileSize, eSearchDirection eDirection, int nThreshold, bool bDarkBackground)
		{
			if (nProfileSize > crtROI.Width) return 0;
			if (nProfileSize > crtROI.Height) return 0;

			// variable
			ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
			IntPtr p = mem.GetPtr();
			byte* bp;

			// implement
			img.SetData(p, crtROI, (int)mem.W);
			int nCount = 0;
			switch (eDirection)
			{
				case eSearchDirection.TopToBottom:
					for (int y = 0; y < img.p_Size.Y; y++)
					{
						nCount = 0;
						bp = (byte*)img.GetPtr() + y * img.p_Stride + (img.p_Size.X / 2);
						for (int x = -(nProfileSize / 2); x < (nProfileSize / 2); x++)
						{
							byte* bpCurrent = bp + x;
							if (bDarkBackground)
							{
								if (*bpCurrent > nThreshold) nCount++;
							}
							else
							{
								if (*bpCurrent < nThreshold) nCount++;
							}
						}
						if (nCount == nProfileSize) return y;
					}
					break;
				case eSearchDirection.LeftToRight:
					for (int x = 0; x < img.p_Size.X; x++)
					{
						nCount = 0;
						bp = (byte*)img.GetPtr() + x + (img.p_Size.Y / 2) * img.p_Stride;
						for (int y = -(nProfileSize / 2); y < (nProfileSize / 2); y++)
						{
							byte* bpCurrent = bp + y * img.p_Stride;
							if (bDarkBackground)
							{
								if (*bpCurrent > nThreshold) nCount++;
							}
							else
							{
								if (*bpCurrent < nThreshold) nCount++;
							}
						}
						if (nCount == nProfileSize) return x;
					}
					break;
				case eSearchDirection.RightToLeft:
					for (int x = img.p_Size.X - 1; x >= 0; x--)
					{
						nCount = 0;
						bp = (byte*)img.GetPtr() + x + (img.p_Size.Y / 2) * img.p_Stride;
						for (int y = -(nProfileSize / 2); y < (nProfileSize / 2); y++)
						{
							byte* bpCurrent = bp + y * img.p_Stride;
							if (bDarkBackground)
							{
								if (*bpCurrent > nThreshold) nCount++;
							}
							else
							{
								if (*bpCurrent < nThreshold) nCount++;
							}
						}
						if (nCount == nProfileSize) return x;
					}
					break;
				case eSearchDirection.BottomToTop:
					for (int y = img.p_Size.Y - 2; y >= 0; y--) // img의 마지막줄은 0으로 채워질 수 있기 때문에 마지막의 전줄부터 탐색
					{
						nCount = 0;
						bp = (byte*)img.GetPtr() + y * img.p_Stride + (img.p_Size.X / 2);
						for (int x = -(nProfileSize / 2); x < (nProfileSize / 2); x++)
						{
							byte* bpCurrent = bp + x;
							if (bDarkBackground)
							{
								if (*bpCurrent > nThreshold) nCount++;
							}
							else
							{
								if (*bpCurrent < nThreshold) nCount++;
							}
						}
						if (nCount == nProfileSize) return y;
					}
					break;
			}

			return 0;
		}

		double GetDistanceOfTwoPoint(CPoint cpt1, CPoint cpt2)
		{
			// variable
			double dX1, dX2, dY1, dY2;
			double dResultDistance = 0;

			// implement
			dX1 = cpt1.X;
			dX2 = cpt2.X;
			dY1 = cpt1.Y;
			dY2 = cpt2.Y;

			dResultDistance = Math.Sqrt(((dX1 - dX2) * (dX1 - dX2)) + ((dY1 - dY2) * (dY1 - dY2)));

			return dResultDistance;
		}
		#endregion

		#region ModuleRun

		protected override void InitModuleRuns()
		{
			//switch (currentMgmName)
			//{
			//                   case App.SideLeftInspMgRegName:
			//                       targetViewModel = UIManager.Instance.SetupViewModel.m_RecipeEdge.p_ImageViewerLeft_VM;
			//                       break;
			//                   case App.SideTopInspMgRegName:
			//                       targetViewModel = UIManager.Instance.SetupViewModel.m_RecipeEdge.p_ImageViewerTop_VM;
			//                       break;
			//                   case App.SideBotInspMgRegName:
			//                       targetViewModel = UIManager.Instance.SetupViewModel.m_RecipeEdge.p_ImageViewerBot_VM;
			//                       break;
			//                   case App.SideRightInspMgRegName:
			//                       targetViewModel = UIManager.Instance.SetupViewModel.m_RecipeEdge.p_ImageViewerRight_VM;
			//                       break;
			//                   case App.PellInspMgRegName:
			//                       targetViewModel = UIManager.Instance.SetupViewModel.m_Recipe45D.p_ImageViewer_VM;
			//                       break;
			//	//case App.BackInspMgRegName:
			//	//	targetList = new List<TRect>(mainEdgeList[6]);
			//	//	targetViewModel = UIManager.Instance.SetupViewModel.m_RecipeFrontSide.p_ImageViewer_VM;
			//	//	break;
			//	case App.MainInspMgRegName:
			//                   default:
			//                       targetViewModel = ;
			//                       break;
			//               }
			AddModuleRunList(new Run_Grab(this), true, "Run Grab");
			AddModuleRunList(new Run_GrabBacksideScan(this), true, "Run Backside Scan");
			AddModuleRunList(new Run_Grab45(this), true, "Run Grab 45");
			AddModuleRunList(new Run_GrabSideScan(this), true, "Run Side Scan");
			AddModuleRunList(new Run_LADS(this), true, "Run LADS");
			AddModuleRunList(new Run_BarcodeInspection(this), true, "Run Barcode Inspection");
			AddModuleRunList(new Run_MakeAlignTemplateImage(this), true, "Run MakeAlignTemplateImage");
			AddModuleRunList(new Run_PatternAlign(this), true, "Run PatternAlign");
			AddModuleRunList(new Run_PatternShiftAndRotation(this), true, "Run PatternShiftAndRotation");
			AddModuleRunList(new Run_AlignKeyInspection(this), true, "Run AlignKeyInspection");
			AddModuleRunList(new Run_PellicleShiftAndRotation(this), true, "Run PellicleShiftAndRotation");
			AddModuleRunList(new Run_PellicleExpandingInspection(this), true, "Run PellicleExpanding");

			var main = new Run_SurfaceInspection(this, App.MainRecipeRegName, App.MainInspMgRegName);
			main.m_sModuleRun = App.MainModuleName;
			AddModuleRunList(main, true, "Run " + App.MainModuleName);


			var pell = new Run_PellSideInspection(this, App.PellRecipeRegName, App.PellInspMgRegName);
			pell.m_sModuleRun = App.PellicleModuleName;
			AddModuleRunList(pell, true, "Run " + App.PellicleModuleName);


			var left = new Run_LeftSideInspection(this, App.SideRecipeRegName, App.SideLeftInspMgRegName);
			left.m_sModuleRun = App.SideLeftModuleName;
			AddModuleRunList(left, true, "Run " + App.SideLeftModuleName);

			var top = new Run_TopSideInspection(this, App.SideRecipeRegName, App.SideTopInspMgRegName);
			top.m_sModuleRun = App.SideTopModuleName;
			AddModuleRunList(top, true, "Run " + App.SideTopModuleName);

			var right = new Run_RightSideInspection(this, App.SideRecipeRegName, App.SideRightInspMgRegName);
			right.m_sModuleRun = App.SideRightModuleName;
			AddModuleRunList(right, true, "Run " + App.SideRightModuleName);

			var bottom = new Run_BotSideInspection(this, App.SideRecipeRegName, App.SideBotInspMgRegName);
			bottom.m_sModuleRun = App.SideBotModuleName;
			AddModuleRunList(bottom, true, "Run " + App.SideBotModuleName);


			AddModuleRunList(new Run_TestPellicle(this), true, "Run Delay");
		}
		#endregion

		public MainVision(string id, IEngineer engineer)
		{
			base.InitBase(id, engineer);
			m_waferSize = new InfoWafer.WaferSize(id, false, false);
			ladsinfos = new List<LADSInfo>();
			InitMemorys();
			InitPosAlign();
		}

		public override void ThreadStop()
		{
			base.ThreadStop();
		}
		public class Run_TestPellicle : ModuleRunBase
		{
			MainVision m_module;
			public RPoint m_rpAxisCenter = new RPoint();    // Side Center Position
			public Run_TestPellicle(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);

			}
			public override ModuleRunBase Clone()
			{
				Run_TestPellicle run = new Run_TestPellicle(m_module);
				run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
				return run;
			}
			string m_sFlip = "Test";
			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
				m_sFlip = tree.Set(m_sFlip, m_sFlip, "Test", "Bottom", bVisible, true);
			}
			public override string Run()
			{
				Thread.Sleep(1000);
				AxisXY axisXY = m_module.m_axisXY;
				double dStartPosY = m_rpAxisCenter.Y;



				double dPosX = m_rpAxisCenter.X;


				if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
					return p_sInfo;
				Thread.Sleep(10000);
				//m_module.p_eState = eState.Ready;
				return "OK";
			}
		}
		public class Run_LeftSideInspection : Run_SurfaceInspection
		{
			public Run_LeftSideInspection(MainVision module, string rcpName, string inspMgmName) : base(module, rcpName, inspMgmName)
			{
			}
		}
		public class Run_TopSideInspection : Run_SurfaceInspection
		{
			public Run_TopSideInspection(MainVision module, string rcpName, string inspMgmName) : base(module, rcpName, inspMgmName)
			{
			}
		}
		public class Run_RightSideInspection : Run_SurfaceInspection
		{
			public Run_RightSideInspection(MainVision module, string rcpName, string inspMgmName) : base(module, rcpName, inspMgmName)
			{
			}
		}
		public class Run_BotSideInspection : Run_SurfaceInspection
		{
			public Run_BotSideInspection(MainVision module, string rcpName, string inspMgmName) : base(module, rcpName, inspMgmName)
			{
			}
		}
		public class Run_PellSideInspection : Run_SurfaceInspection
		{
			public Run_PellSideInspection(MainVision module, string rcpName, string inspMgmName) : base(module, rcpName, inspMgmName)
			{
			}
		}

		public class Run_SurfaceInspection : ModuleRunBase
		{

			MainVision m_module;
			string currentRcpName;
			string currentMgmName;

			public Run_SurfaceInspection(MainVision module, string rcpName, string inspMgmName)
			{
				EdgeList = new TRect[6];
				for (int j = 0; j < 6; j++)
				{
					EdgeList[j] = new TRect();
				}
				currentRcpName = rcpName;
				currentMgmName = inspMgmName;
				m_module = module;
				InitModuleRun(module);
			}

			#region Surface Inspection Parameter

			public bool BrightGV;
			public int SurfaceGV;
			public int SurfaceSize;
			CPoint CenterPoint;
			public int InspectionOffsetX_Left;
			public int InspectionOffsetX_Right;
			public int InspectionOffsetY;
			public int BlockSizeWidth;
			public int BlockSizeHeight;

			public TRect[] EdgeList = new TRect[6];
			//List<TRect> sideLeftEdgeList;
			//List<TRect> sideRightEdgeList;
			//List<TRect> sideTopEdgeList;
			//List<TRect> sideBotEdgeList;

			#endregion

			public override ModuleRunBase Clone()
			{
				Run_SurfaceInspection run = new Run_SurfaceInspection(m_module, this.currentRcpName, this.currentMgmName);
				run.EdgeList = EdgeList;
				run.BrightGV = BrightGV;
				run.SurfaceGV = SurfaceGV;
				run.SurfaceSize = SurfaceSize;
				run.CenterPoint = CenterPoint;
				run.InspectionOffsetX_Left = InspectionOffsetX_Left;
				run.InspectionOffsetX_Right = InspectionOffsetX_Right;
				run.InspectionOffsetY = InspectionOffsetY;

				return run;
			}
			//string[] keywords = new string[] { "Main", "SideLeft", "SideTop", "SideBot", "SideRight", "Pellicle"};
			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				string defeaultName = currentMgmName + " EdgeBox #{0}";
				for (int i = 0; i < 6; i++)
				{
					defeaultName = currentMgmName + " EdgeBox #{0} Left";
					EdgeList[i].MemoryRect.Left = tree.Set(EdgeList[i].MemoryRect.Left, EdgeList[i].MemoryRect.Left, string.Format(defeaultName, i), "EdgeBox's Left Position (pixel. MemoryRect)", bVisible);
					defeaultName = currentMgmName + " EdgeBox #{0} Top";
					EdgeList[i].MemoryRect.Top = tree.Set(EdgeList[i].MemoryRect.Top, EdgeList[i].MemoryRect.Top, string.Format(defeaultName, i), "EdgeBox's Top Position (pixel. MemoryRect)", bVisible);
					defeaultName = currentMgmName + " EdgeBox #{0} Width";
					EdgeList[i].MemoryRect.Width = tree.Set(EdgeList[i].MemoryRect.Width, EdgeList[i].MemoryRect.Width, string.Format(defeaultName, i), "EdgeBox's Width (pixel. MemoryRect)", bVisible);
					defeaultName = currentMgmName + " EdgeBox #{0} Height";
					EdgeList[i].MemoryRect.Height = tree.Set(EdgeList[i].MemoryRect.Height, EdgeList[i].MemoryRect.Height, string.Format(defeaultName, i), "EdgeBox's Height (pixel. MemoryRect)", bVisible);
				}
				BrightGV = tree.Set(BrightGV, BrightGV, "Use Bright Inspection", "Use Bright Inspection", bVisible);
				SurfaceGV = tree.Set(SurfaceGV, SurfaceGV, "Target Inspection GV", "Target Inspection GV", bVisible);
				SurfaceSize = tree.Set(SurfaceSize, SurfaceSize, "Target Inspection Size", "Target Inspection Size", bVisible);
				InspectionOffsetX_Left = tree.Set(InspectionOffsetX_Left, InspectionOffsetX_Left, "Insepction Area X-Left Offset", "Insepction Area X-Left Offset", bVisible);
				InspectionOffsetX_Right = tree.Set(InspectionOffsetX_Right, InspectionOffsetX_Right, "Insepction Area X-Right Offset", "Insepction Area X-Right Offset", bVisible);
				InspectionOffsetY = tree.Set(InspectionOffsetY, InspectionOffsetY, "Inspection Y Offset", "Inspection Y Offset", bVisible);
				BlockSizeWidth = tree.Set(BlockSizeWidth, BlockSizeWidth, "Insepction BlockSize Width", "Insepction BlockSize Width", bVisible);
				BlockSizeHeight = tree.Set(BlockSizeHeight, BlockSizeHeight, "Insepction BlockSize Height", "Insepction BlockSize Height", bVisible);

			}

			private bool _StartRecipeTeaching(TRect[] tempList, RootViewer_ViewModel viewer)
			{
#if !DEBUG
			try
			{
#endif
				CenterPoint = new CPoint();
				int memH = viewer.p_ImageData.p_Size.Y;
				int memW = viewer.p_ImageData.p_Size.X;

				float centX = CenterPoint.X; // 레시피 티칭 값 가지고오기
				float centY = CenterPoint.Y;

				float outOriginX, outOriginY;
				float outChipSzX, outChipSzY;
				float outRadius = memH / 2;
				if (memH > memW)
					outRadius = memW / 2;
				bool isIncludeMode = true;

				IntPtr MainImage = new IntPtr();
				if (viewer.p_ImageData.p_nByte == 3)
				{
					if (viewer.m_eColorViewMode != RootViewer_ViewModel.eColorViewMode.All)
						MainImage = viewer.p_ImageData.GetPtr((int)viewer.p_eColorViewMode - 1);
					else
						MainImage = viewer.p_ImageData.GetPtr(0);
				}
				else
				{ // All 일때는 R채널로...
					MainImage = viewer.p_ImageData.GetPtr(0);
				}

				List<Cpp_Point> WaferEdge = new List<Cpp_Point>();
				int[] mapData = null;
				unsafe
				{
					int DownSample = 40;
					int outmap_x, outmap_y;

					fixed (byte* pImg = new byte[(long)(memW / DownSample) * (long)(memH / DownSample)]) // 원본 이미지 너무 커서 안열림
					{
						CLR_IP.Cpp_SubSampling((byte*)MainImage, pImg, memW, memH, 0, 0, memW, memH, DownSample);
						var area = searchArea(tempList.ToList(), viewer.p_ImageData, InspectionOffsetX_Left, InspectionOffsetY, InspectionOffsetX_Right, InspectionOffsetY);


						string sInspectionID = RootTools.Database.DatabaseManager.Instance.GetInspectionID();
						string outputSize = string.Format("Left:{0} Top:{1} Right:{2} Bottom:{3}", area.Left, area.Top, area.Right, area.Bottom);
						if (!System.IO.Directory.Exists(System.IO.Path.Combine(App.AOPImageRootPath, sInspectionID)))
						{
							System.IO.Directory.CreateDirectory(System.IO.Path.Combine(App.AOPImageRootPath, sInspectionID));
						}
						System.IO.File.WriteAllText(System.IO.Path.Combine(App.AOPImageRootPath, sInspectionID, currentRcpName + "_TotalSize.txt"), outputSize);
						//그다음 이미지 축소 저장
						//Image<Gray, byte> mapImage = new Image<Gray, byte>(memW / DownSample, memH / DownSample, memW / DownSample, (IntPtr)pImg);
						//mapImage.Save(System.IO.Path.Combine(sDefectimagePath, sInspectionID, idx.ToString() + "mapImage.bmp"));

						// Param Down Scale
						centX /= DownSample; centY /= DownSample;
						outRadius /= DownSample;
						memW /= DownSample; memH /= DownSample;

						if (BlockSizeWidth != 0)
							outmap_x = area.Width / BlockSizeWidth;
						else
							outmap_x = area.Width / 500;
						if (BlockSizeHeight != 0)
							outmap_y = area.Height / BlockSizeHeight;
						else
							outmap_y = area.Height / 500;

						if (outmap_x == 0)
						{
							outmap_x = 40;
						}
						if (outmap_y == 0)
						{
							outmap_y = 40;
						}

						WaferEdge.Add(new Cpp_Point(area.Left / DownSample, area.Top / DownSample));
						WaferEdge.Add(new Cpp_Point(area.Left / DownSample, area.Bottom / DownSample));
						WaferEdge.Add(new Cpp_Point(area.Right / DownSample, area.Bottom / DownSample));
						WaferEdge.Add(new Cpp_Point(area.Right / DownSample, area.Top / DownSample));

						mapData = CLR_IP.Cpp_GenerateMapData(
							WaferEdge.ToArray(),
							&outOriginX,
							&outOriginY,
							&outChipSzX,
							&outChipSzY,
							&outmap_x,
							&outmap_y,
							memW, memH,
							1,
							isIncludeMode
							);
						//OutmapX = outmap_x;
						//OutmapY = outmap_y;
					}

					//// Param Up Scale
					centX *= DownSample; centY *= DownSample;
					outRadius *= DownSample;
					outOriginX *= DownSample; outOriginY *= DownSample;
					outChipSzX *= DownSample; outChipSzY *= DownSample;

					// Save Recipe
					SetRecipeMapData(mapData, (int)outmap_x, (int)outmap_y, (int)outOriginX, (int)outOriginY, (int)outChipSzX, (int)outChipSzY);

					//GlobalObjects.Instance.Get<BacksideRecipe>().CenterX = (int)centX;
					//GlobalObjects.Instance.Get<BacksideRecipe>().CenterY = (int)centY;
					//GlobalObjects.Instance.Get<BacksideRecipe>().Radius = (int)outRadius;

					//SaveContourMap((int)centX, (int)centY, (int)outRadius);
				}
				return true;
#if !DEBUG
			}
			catch (Exception ex)
			{
				return false;
			}
#endif
			}

			CRect searchArea(List<TRect> tempList, ImageData data, int x_left_margin, int y_top_margin, int x_right_margin, int y_bot_margin)
			{
				// variable
				List<Rect> arcROIs = new List<Rect>();
				List<DPoint> aptEdges = new List<DPoint>();
				//RecipeFrontside_Viewer_ViewModel ivvm = m_ImageViewer_VM;
				RootTools.Inspects.eEdgeFindDirection eTempDirection = RootTools.Inspects.eEdgeFindDirection.TOP;
				DPoint ptLeft1, ptLeft2, ptBottom, ptRight1, ptRight2, ptTop;
				DPoint ptLT, ptRT, ptLB, ptRB;


				arcROIs.Clear();
				aptEdges.Clear();
				for (int j = 0; j < 6; j++)
				{
					if (tempList.Count < 6) break;
					arcROIs.Add(new Rect(
						tempList[j].MemoryRect.Left,
						tempList[j].MemoryRect.Top,
						tempList[j].MemoryRect.Width,
						tempList[j].MemoryRect.Height));
				}
				if (arcROIs.Count < 6) return new CRect(-1, -1, -1, -1);
				for (int j = 0; j < arcROIs.Count; j++)
				{
					eTempDirection = InspectionManager_AOP.GetDirection(data, arcROIs[j]);
					aptEdges.Add(InspectionManager_AOP.GetEdge(data, arcROIs[j], eTempDirection, true, true, 30));
				}
				// aptEeges에 있는 DPoint들을 좌표에 맞게 분배
				List<DPoint> aSortedByX = aptEdges.OrderBy(x => x.X).ToList();
				List<DPoint> aSortedByY = aptEdges.OrderBy(x => x.Y).ToList();
				if (aSortedByX[0].Y < aSortedByX[1].Y)
				{
					ptLeft1 = aSortedByX[0];
					ptLeft2 = aSortedByX[1];
				}
				else
				{
					ptLeft1 = aSortedByX[1];
					ptLeft2 = aSortedByX[0];
				}
				if (aSortedByX[4].Y < aSortedByX[5].Y)
				{
					ptRight1 = aSortedByX[4];
					ptRight2 = aSortedByX[5];
				}
				else
				{
					ptRight1 = aSortedByX[5];
					ptRight2 = aSortedByX[4];
				}
				ptTop = aSortedByY[0];
				ptBottom = aSortedByY[5];

				ptLT = new DPoint(ptLeft1.X, ptTop.Y);
				ptLB = new DPoint(ptLeft2.X, ptBottom.Y);
				ptRB = new DPoint(ptRight2.X, ptBottom.Y);
				ptRT = new DPoint(ptRight1.X, ptTop.Y);

				//m_ImageViewer_VM.DrawLine(ptLT, ptLB, MBrushes.Lime);
				//DrawLine(ptRB, ptRT, MBrushes.Lime);
				//DrawLine(ptLT, ptRT, MBrushes.Lime);
				//DrawLine(ptLB, ptRB, MBrushes.Lime);

				//m_ImageViewer_VM.DrawRect(new CPoint(ptLeft1.X - 10, ptLeft1.Y - 10), new CPoint(ptLeft1.X + 10, ptLeft1.Y + 10), RecipeFrontside_Viewer_ViewModel.ColorType.Defect);
				//m_ImageViewer_VM.DrawRect(new CPoint(ptLeft2.X - 10, ptLeft2.Y - 10), new CPoint(ptLeft2.X + 10, ptLeft2.Y + 10), RecipeFrontside_Viewer_ViewModel.ColorType.Defect);
				//m_ImageViewer_VM.DrawRect(new CPoint(ptBottom.X - 10, ptBottom.Y - 10), new CPoint(ptBottom.X + 10, ptBottom.Y + 10), RecipeFrontside_Viewer_ViewModel.ColorType.Defect);
				//m_ImageViewer_VM.DrawRect(new CPoint(ptRight1.X - 10, ptRight1.Y - 10), new CPoint(ptRight1.X + 10, ptRight1.Y + 10), RecipeFrontside_Viewer_ViewModel.ColorType.Defect);
				//m_ImageViewer_VM.DrawRect(new CPoint(ptRight2.X - 10, ptRight2.Y - 10), new CPoint(ptRight2.X + 10, ptRight2.Y + 10), RecipeFrontside_Viewer_ViewModel.ColorType.Defect);
				//m_ImageViewer_VM.DrawRect(new CPoint(ptTop.X - 100, ptTop.Y - 100), new CPoint(ptTop.X + 100, ptTop.Y + 100), RecipeFrontside_Viewer_ViewModel.ColorType.Defect);

				switch (currentMgmName)
				{
					case App.SideLeftInspMgRegName:
						var left = UIManager.Instance.SetupViewModel.m_RecipeEdge.p_ImageViewerLeft_VM;
						break;
					case App.SideTopInspMgRegName:
						var top = UIManager.Instance.SetupViewModel.m_RecipeEdge.p_ImageViewerTop_VM;
						break;
					case App.SideBotInspMgRegName:
						var bot = UIManager.Instance.SetupViewModel.m_RecipeEdge.p_ImageViewerBot_VM;
						break;
					case App.SideRightInspMgRegName:
						var right = UIManager.Instance.SetupViewModel.m_RecipeEdge.p_ImageViewerRight_VM;
						break;
					case App.PellInspMgRegName:
						var pell = UIManager.Instance.SetupViewModel.m_Recipe45D.p_ImageViewer_VM;
						break;
					//case App.BackInspMgRegName:
					//	targetList = new List<TRect>(mainEdgeList[6]);
					//	targetViewModel = UIManager.Instance.SetupViewModel.m_RecipeFrontSide.p_ImageViewer_VM;
					//	break;
					case App.MainInspMgRegName:
					default:
						var main = UIManager.Instance.SetupViewModel.m_RecipeFrontSide.p_ImageViewer_VM;
						main.currectDispatcher.Invoke(new Action(delegate ()
						{
							main.DrawRect(new CPoint(ptLT.X, ptLT.Y), new CPoint(ptRB.X, ptRB.Y), RecipeFrontside_Viewer_ViewModel.ColorType.ChipFeature);
						}));
						break;
				}

				return new CRect(new Point(ptLT.X + x_left_margin, ptLT.Y + y_top_margin), new Point(ptRB.X - x_right_margin, ptRB.Y - y_top_margin));
			}

			private void SetRecipeMapData(int[] mapData, int mapX, int mapY, int originX, int originY, int chipSzX, int chipSzY)
			{
				// Map Data Recipe 생성
				GlobalObjects.Instance.GetNamed<AOP_RecipeSurface>(currentRcpName).GetItem<BacksideRecipe>().OriginX = originX;
				GlobalObjects.Instance.GetNamed<AOP_RecipeSurface>(currentRcpName).GetItem<BacksideRecipe>().OriginY = originY;
				GlobalObjects.Instance.GetNamed<AOP_RecipeSurface>(currentRcpName).GetItem<BacksideRecipe>().DiePitchX = chipSzX;
				GlobalObjects.Instance.GetNamed<AOP_RecipeSurface>(currentRcpName).GetItem<BacksideRecipe>().DiePitchY = chipSzY;

				OriginRecipe originRecipe = GlobalObjects.Instance.GetNamed<AOP_RecipeSurface>(currentRcpName).GetItem<OriginRecipe>();
				originRecipe.DiePitchX = chipSzX;
				originRecipe.DiePitchY = chipSzY;
				originRecipe.OriginX = originX;
				originRecipe.OriginY = originY;

				RecipeType_WaferMap mapInfo = new RecipeType_WaferMap(mapX, mapY, mapData);
				int x = 0; int y = 0;
				for (int i = 0; i < mapX * mapY; i++)
				{
					if (y == 0 || y == mapY - 1)
					{
						mapInfo.Data[i] = 0;
					}
					else if (x == 0 || x == mapX - 1)
					{
						mapInfo.Data[i] = 0;
					}
					x++;
					if (x >= mapX)
					{
						y++;
						x = 0;
					}
				}

				GlobalObjects.Instance.GetNamed<AOP_RecipeSurface>(currentRcpName).WaferMap = mapInfo;

				if (false) // Display Map Data Option화
					DrawMapData(mapInfo, mapData, mapX, mapY, originX, originY, chipSzX, chipSzY);
			}
			private void DrawMapData(RecipeType_WaferMap mapInfo, int[] mapData, int mapX, int mapY, int OriginX, int OriginY, int ChipSzX, int ChipSzY)
			{
				// Map Display
				List<RootTools.Database.Defect> rectList = new List<RootTools.Database.Defect>();
				int offsetY = 0;
				bool isOrigin = true;

				for (int x = 0; x < mapX; x++)
					for (int y = 0; y < mapY; y++)
						if (mapData[y * mapX + x] == 1)
						{
							if (isOrigin)
							{
								offsetY = OriginY - (y + 1) * ChipSzY;
								mapInfo.MasterDieX = x;
								mapInfo.MasterDieY = y;
								isOrigin = false;
							}
							var data = new RootTools.Database.Defect();

							var left = OriginX + x * ChipSzX;
							var top = offsetY + y * ChipSzY;
							var right = OriginX + (x + 1) * ChipSzX;
							var bot = offsetY + (y + 1) * ChipSzY;

							var width = right - left;
							var height = bot - top;
							//left = (int)(left - width / 2.0);
							//top = (int)(top - height / 2.0);

							data.p_rtDefectBox = new Rect(left, top, width, height);
							rectList.Add(data);
						}


				//m_ImageViewer_VM.DrawRect(rectList, Recipe45D_ImageViewer_ViewModel.ColorType.MapData);
				var main = UIManager.Instance.SetupViewModel.m_RecipeFrontSide.p_ImageViewer_VM;
				main.currectDispatcher.Invoke(new Action(delegate ()
				{
					GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(currentMgmName).AddRect(rectList, null, new System.Windows.Media.Pen(System.Windows.Media.Brushes.Green, 2));
				}));
			}
			public override string Run()
			{
				try
				{
					GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(currentMgmName).ClearDefect();
					//m_Setup.PatternInspectionManager.ResetWorkManager();
					//GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(currentMgmName).InitInspectionInfo();
					GlobalObjects.Instance.GetNamed<AOP_RecipeSurface>(currentRcpName).WaferMap.Clear();

					RootViewer_ViewModel targetViewModel;

					int defectCode = 10000;

					switch (currentMgmName)
					{
						case App.SideLeftInspMgRegName:
							targetViewModel = UIManager.Instance.SetupViewModel.m_RecipeEdge.p_ImageViewerLeft_VM;
							defectCode = InspectionManager.MakeDefectCode(InspectionTarget.SideInspectionLeft, InspectionType.AbsoluteSurface, 0);
							break;
						case App.SideTopInspMgRegName:
							targetViewModel = UIManager.Instance.SetupViewModel.m_RecipeEdge.p_ImageViewerTop_VM;
							defectCode = InspectionManager.MakeDefectCode(InspectionTarget.SideInspectionTop, InspectionType.AbsoluteSurface, 0);
							break;
						case App.SideBotInspMgRegName:
							targetViewModel = UIManager.Instance.SetupViewModel.m_RecipeEdge.p_ImageViewerBot_VM;
							defectCode = InspectionManager.MakeDefectCode(InspectionTarget.SideInspectionBottom, InspectionType.AbsoluteSurface, 0);
							break;
						case App.SideRightInspMgRegName:
							targetViewModel = UIManager.Instance.SetupViewModel.m_RecipeEdge.p_ImageViewerRight_VM;
							defectCode = InspectionManager.MakeDefectCode(InspectionTarget.SideInspectionRight, InspectionType.AbsoluteSurface, 0);
							break;
						case App.PellInspMgRegName:
							targetViewModel = UIManager.Instance.SetupViewModel.m_Recipe45D.p_ImageViewer_VM;
							defectCode = InspectionManager.MakeDefectCode(InspectionTarget.Pellcile45, InspectionType.AbsoluteSurface, 0);
							break;
						//case App.BackInspMgRegName:
						//	targetList = new List<TRect>(mainEdgeList[6]);
						//	targetViewModel = UIManager.Instance.SetupViewModel.m_RecipeFrontSide.p_ImageViewer_VM;
						//	break;
						case App.MainInspMgRegName:
						default:
							targetViewModel = UIManager.Instance.SetupViewModel.m_RecipeFrontSide.p_ImageViewer_VM;
							defectCode = InspectionManager.MakeDefectCode(InspectionTarget.Chrome, InspectionType.AbsoluteSurface, 0);
							break;
					}

					_StartRecipeTeaching(EdgeList, targetViewModel);

					ReticleSurfaceParameter surParam = GlobalObjects.Instance.GetNamed<AOP_RecipeSurface>(currentRcpName).GetItem<ReticleSurfaceParameter>();
					surParam.IsBright = BrightGV;
					surParam.Intensity = SurfaceGV;
					surParam.DefectCode = defectCode;
					//surParam.DiffFilter = DiffFilterMethod.Gaussian;
					surParam.Size = SurfaceSize;

					GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(currentMgmName).Start();

					return "OK";
				}
				finally
				{

				}
			}
		}

		public class Run_GrabSideScan : ModuleRunBase
		{
			MainVision m_module;

			public RPoint m_rpAxisCenter = new RPoint();    // Side Center Position
			public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
			public double m_dResX_um = 1;                   // Camera Resolution X
			public double m_dResY_um = 1;                   // Camera Resolution Y
			double m_dDegree;                    //Rotate Degree
			public int m_nFocusPosZ = 0;                    // Focus Position Z
			public int m_nReticleSize_mm = 1000;              // Reticle Size (mm)
			public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
			public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
			public int m_nRotatePulse = 1000;
			public GrabMode m_grabMode = null;
			string m_sGrabMode = "";

			public int m_nLeftOffsetX = 0;
			public int m_nTopOffsetX = 0;
			public int m_nRightOffsetX = 0;
			public int m_nBottomOffsetX = 0;

			public double p_dDegree
			{
				get
				{
					return m_dDegree;
				}
				set
				{
					if (value > 360)
						m_dDegree = value - 360;
					else
						m_dDegree = value;
				}
			}
			public string p_sGrabMode
			{
				get { return m_sGrabMode; }
				set
				{
					m_sGrabMode = value;
					m_grabMode = m_module.GetGrabMode(value);
				}
			}
			public Run_GrabSideScan(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}
			public override ModuleRunBase Clone()
			{
				Run_GrabSideScan run = new Run_GrabSideScan(m_module);
				run.m_dDegree = m_dDegree;
				run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
				run.m_cpMemoryOffset = new CPoint(m_cpMemoryOffset);
				run.m_dResX_um = m_dResX_um;
				run.m_dResY_um = m_dResY_um;
				run.m_nFocusPosZ = m_nFocusPosZ;
				run.m_nReticleSize_mm = m_nReticleSize_mm;
				run.m_nMaxFrame = m_nMaxFrame;
				run.m_nScanRate = m_nScanRate;
				run.p_sGrabMode = p_sGrabMode;

				run.m_nLeftOffsetX = m_nLeftOffsetX;
				run.m_nTopOffsetX = m_nTopOffsetX;
				run.m_nRightOffsetX = m_nRightOffsetX;
				run.m_nBottomOffsetX = m_nBottomOffsetX;

				return run;
			}
			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
				p_dDegree = tree.Set(p_dDegree, p_dDegree, "Degree", "Rotation Degree(0 ~ 360)", bVisible);
				m_nRotatePulse = tree.Set(m_nRotatePulse, m_nRotatePulse, "Theta Pulse", "Theta Pulse", bVisible);
				m_cpMemoryOffset = tree.Set(m_cpMemoryOffset, m_cpMemoryOffset, "Memory Offset", "Grab Start Memory Position (px)", bVisible);
				m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Cam X Resolution", "X Resolution (um)", bVisible);
				m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Cam Y Resolution", "Y Resolution (um)", bVisible);
				m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);
				m_nReticleSize_mm = tree.Set(m_nReticleSize_mm, m_nReticleSize_mm, "Reticle Size Y", "Reticle Size Y", bVisible);
				m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
				m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
				p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);

				m_nLeftOffsetX = (tree.GetTree("Scan Offset", false, bVisible)).Set(m_nLeftOffsetX, m_nLeftOffsetX, "Left Offset X", "Left Offset X", bVisible);
				m_nTopOffsetX = (tree.GetTree("Scan Offset", false, bVisible)).Set(m_nTopOffsetX, m_nTopOffsetX, "Top Offset X", "Top Offset X", bVisible);
				m_nRightOffsetX = (tree.GetTree("Scan Offset", false, bVisible)).Set(m_nRightOffsetX, m_nRightOffsetX, "Right Offset X", "Right Offset X", bVisible);
				m_nBottomOffsetX = (tree.GetTree("Scan Offset", false, bVisible)).Set(m_nBottomOffsetX, m_nBottomOffsetX, "Bottom Offset X", "Bottom Offset X", bVisible);
			}
			public override string Run()
			{
				if (m_grabMode == null) return "Grab Mode == null";

				try
				{
					m_grabMode.SetLight(true);

					AxisXY axisXY = m_module.m_axisXY;
					Axis axisSizeZ = m_module.m_axisSideZ;
					Axis axisRotate = m_module.m_axisRotate;

					CPoint cpMemoryOffset = new CPoint(m_cpMemoryOffset);
					int nScanLine = 0;
					int nMMPerUM = 1000;
					int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

					m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
					int nReticleSizeY_px = Convert.ToInt32(m_nReticleSize_mm * nMMPerUM / m_dResY_um);  // 레티클 영역의 Y픽셀 갯수
					int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nReticleSizeY_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
					int nScanOffset_pulse = 100000; //가속버퍼구간
					int nDirection = 4;
					while (nDirection > nScanLine)
					{
						if (EQ.IsStop())
							return "OK";
						double nRotate = m_nRotatePulse * (p_dDegree * nScanLine) + m_module.m_dThetaAlignOffset - 2500;
						if (m_module.Run(axisRotate.StartMove(nRotate)))
							return p_sInfo;
						if (m_module.Run(axisRotate.WaitReady()))
							return p_sInfo;

						double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
						double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

						m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

						double dPosX = m_rpAxisCenter.X;// + (m_module.m_narrSideEdgeOffset[nScanLine] * 5);

						if (m_module.Run(axisSizeZ.StartMove(m_nFocusPosZ)))
							return p_sInfo;
						if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
							return p_sInfo;
						if (m_module.Run(axisRotate.WaitReady()))
							return p_sInfo;
						if (m_module.Run(axisXY.WaitReady()))
							return p_sInfo;
						if (m_module.Run(axisSizeZ.WaitReady()))
							return p_sInfo;

						double dTriggerStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2;
						double dTriggerEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;
						axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

						string strPool = m_grabMode.m_memoryPool.p_id;
						string strGroup = m_grabMode.m_memoryGroup.p_id;
						GrabMode.eScanPos curScanPos = (GrabMode.eScanPos)nScanLine;
						string strMemory = curScanPos.ToString();
						MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
						int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_nScanRate / 100);
						m_grabMode.StartGrab(mem, cpMemoryOffset, nReticleSizeY_px, 0, m_grabMode.m_bUseBiDirectionScan);

						if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
							return p_sInfo;
						if (m_module.Run(axisXY.WaitReady()))
							return p_sInfo;
						axisXY.p_axisY.RunTrigger(false);

						nScanLine++;
					}
					m_grabMode.m_camera.StopGrab();
					return "OK";
				}
				finally
				{
					m_grabMode.SetLight(false);
				}
			}
		}
		public class Run_Grab : ModuleRunBase
		{
			MainVision m_module;

			public RPoint m_rpAxisCenter = new RPoint();    // Reticle Center Position
			public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
			public double m_dResX_um = 1;                   // Camera Resolution X
			public double m_dResY_um = 1;                   // Camera Resolution Y
			public int m_nFocusPosZ = 0;                    // Focus Position Z
			public int m_nReticleSize_mm = 1000;              // Reticle Size (mm)
			public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
			public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
			public GrabMode m_grabMode = null;
			string m_sGrabMode = "";

			public string p_sGrabMode
			{
				get { return m_sGrabMode; }
				set
				{
					m_sGrabMode = value;
					m_grabMode = m_module.GetGrabMode(value);
				}
			}
			public Run_Grab(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}
			public override ModuleRunBase Clone()
			{
				Run_Grab run = new Run_Grab(m_module);
				run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
				run.m_cpMemoryOffset = new CPoint(m_cpMemoryOffset);
				run.m_dResX_um = m_dResX_um;
				run.m_dResY_um = m_dResY_um;
				run.m_nFocusPosZ = m_nFocusPosZ;
				run.m_nReticleSize_mm = m_nReticleSize_mm;
				run.m_nMaxFrame = m_nMaxFrame;
				run.m_nScanRate = m_nScanRate;
				run.p_sGrabMode = p_sGrabMode;
				return run;
			}
			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
				m_cpMemoryOffset = tree.Set(m_cpMemoryOffset, m_cpMemoryOffset, "Memory Offset", "Grab Start Memory Position (px)", bVisible);
				m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Cam X Resolution", "X Resolution (um)", bVisible);
				m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Cam Y Resolution", "Y Resolution (um)", bVisible);
				m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);
				m_nReticleSize_mm = tree.Set(m_nReticleSize_mm, m_nReticleSize_mm, "Reticle Size Y", "Reticle Size Y", bVisible);
				m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
				m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
				p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
			}
			public override string Run()
			{
				if (m_grabMode == null) return "Grab Mode == null";

				try
				{
					m_grabMode.SetLight(true);

					//((Camera_Dalsa)m_grabMode.m_camera).p_CamParam.p_eDir = DalsaParameterSet.eDir.Reverse;
					//((Camera_Dalsa)m_grabMode.m_camera).p_CamParam.p_eTriggerMode = DalsaParameterSet.eTriggerMode.External;

					AxisXY axisXY = m_module.m_axisXY;
					Axis axisZ = m_module.m_axisZ;
					Axis axisRotate = m_module.m_axisRotate;
					CPoint cpMemoryOffset = new CPoint(m_cpMemoryOffset);
					int nScanLine = 0;
					int nMMPerUM = 1000;
					int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
					int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

					double dXScale = m_dResX_um * 10;
					cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth;
					m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
					int nReticleSizeY_px = Convert.ToInt32(m_nReticleSize_mm * nMMPerUM / m_dResY_um);  // 레티클 영역의 Y픽셀 갯수
					int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nReticleSizeY_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
					int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_nScanRate / 100);
					int nScanOffset_pulse = Convert.ToInt32(nScanSpeed * 0.3); //가속버퍼구간

					while (m_grabMode.m_ScanLineNum > nScanLine)
					{
						if (EQ.IsStop())
							return "OK";

						double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
						double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

						m_grabMode.m_eGrabDirection = eGrabDirection.Forward;


						double dPosX = m_rpAxisCenter.X + nReticleSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

						// Theta축 0으로
						double dTheta = axisRotate.GetPosValue(eAxisPos.ScanPos.ToString());
						dTheta += m_module.m_dThetaAlignOffset;
						if (m_module.Run(axisRotate.StartMove(dTheta)))
							return p_sInfo;
						if (m_module.Run(axisRotate.WaitReady()))
							return p_sInfo;
						//
						if (m_module.Run(axisZ.StartMove(m_nFocusPosZ)))
							return p_sInfo;
						if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
							return p_sInfo;
						if (m_module.Run(axisXY.WaitReady()))
							return p_sInfo;
						if (m_module.Run(axisZ.WaitReady()))
							return p_sInfo;

						double dTriggerStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2;
						double dTriggerEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;
						axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

						string strPool = m_grabMode.m_memoryPool.p_id;
						string strGroup = m_grabMode.m_memoryGroup.p_id;
						string strMemory = m_grabMode.m_memoryData.p_id;

						MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

						m_grabMode.StartGrab(mem, cpMemoryOffset, nReticleSizeY_px, 0, m_grabMode.m_bUseBiDirectionScan);

						if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
							return p_sInfo;
						if (m_module.Run(axisXY.WaitReady()))
							return p_sInfo;
						axisXY.p_axisY.RunTrigger(false);

						nScanLine++;
						cpMemoryOffset.X += nCamWidth;
					}
					m_grabMode.m_camera.StopGrab();

					return "OK";
				}
				finally
				{
					m_grabMode.SetLight(false);
				}
			}
		}

		public class Run_GrabBacksideScan : ModuleRunBase
		{
			MainVision m_module;

			public RPoint m_rpAxisCenter = new RPoint();    // Reticle Center Position
			public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
			public double m_dResX_um = 1;                   // Camera Resolution X
			public double m_dResY_um = 1;                   // Camera Resolution Y
			public int m_nFocusPosZ = 0;                    // Focus Position Z
			public int m_nReticleSize_mm = 1000;              // Reticle Size (mm)
			public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
			public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
			public GrabMode m_grabMode = null;
			string m_sGrabMode = "";

			public string p_sGrabMode
			{
				get { return m_sGrabMode; }
				set
				{
					m_sGrabMode = value;
					m_grabMode = m_module.GetGrabMode(value);
				}
			}

			public CPoint m_cpLeftEdgeCenterPos = new CPoint();
			public CPoint m_cpTopEdgeCenterPos = new CPoint();
			public CPoint m_cpRightEdgeCenterPos = new CPoint();
			public CPoint m_cpBottomEdgeCenterPos = new CPoint();
			public int m_nSearchArea = 1000;
			public int m_nEdgeThreshold = 70;

			public Run_GrabBacksideScan(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_GrabBacksideScan run = new Run_GrabBacksideScan(m_module);
				run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
				run.m_cpMemoryOffset = new CPoint(m_cpMemoryOffset);
				run.m_dResX_um = m_dResX_um;
				run.m_dResY_um = m_dResY_um;
				run.m_nFocusPosZ = m_nFocusPosZ;
				run.m_nReticleSize_mm = m_nReticleSize_mm;
				run.m_nMaxFrame = m_nMaxFrame;
				run.m_nScanRate = m_nScanRate;
				run.p_sGrabMode = p_sGrabMode;

				run.m_cpLeftEdgeCenterPos = m_cpLeftEdgeCenterPos;
				run.m_cpTopEdgeCenterPos = m_cpTopEdgeCenterPos;
				run.m_cpRightEdgeCenterPos = m_cpRightEdgeCenterPos;
				run.m_cpBottomEdgeCenterPos = m_cpBottomEdgeCenterPos;
				run.m_nSearchArea = m_nSearchArea;
				run.m_nEdgeThreshold = m_nEdgeThreshold;

				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
				m_cpMemoryOffset = tree.Set(m_cpMemoryOffset, m_cpMemoryOffset, "Memory Offset", "Grab Start Memory Position (px)", bVisible);
				m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Cam X Resolution", "X Resolution (um)", bVisible);
				m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Cam Y Resolution", "Y Resolution (um)", bVisible);
				m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);
				m_nReticleSize_mm = tree.Set(m_nReticleSize_mm, m_nReticleSize_mm, "Reticle Size Y", "Reticle Size Y", bVisible);
				m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
				m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
				p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);

				m_cpLeftEdgeCenterPos = (tree.GetTree("Edge Position", false, bVisible)).Set(m_cpLeftEdgeCenterPos, m_cpLeftEdgeCenterPos, "Left Edge Position", "Left Edge Position", bVisible);
				m_cpTopEdgeCenterPos = (tree.GetTree("Edge Position", false, bVisible)).Set(m_cpTopEdgeCenterPos, m_cpTopEdgeCenterPos, "Top Edge Position", "Top Edge Position", bVisible);
				m_cpRightEdgeCenterPos = (tree.GetTree("Edge Position", false, bVisible)).Set(m_cpRightEdgeCenterPos, m_cpRightEdgeCenterPos, "Right Edge Position", "Right Edge Position", bVisible);
				m_cpBottomEdgeCenterPos = (tree.GetTree("Edge Position", false, bVisible)).Set(m_cpBottomEdgeCenterPos, m_cpBottomEdgeCenterPos, "Bottom Edge Position", "Bottom Edge Position", bVisible);
				m_nSearchArea = (tree.GetTree("Edge Position", false, bVisible)).Set(m_nSearchArea, m_nSearchArea, "Search Area Size", "Search Area Size", bVisible);
				m_nEdgeThreshold = (tree.GetTree("Edge Position", false, bVisible)).Set(m_nEdgeThreshold, m_nEdgeThreshold, "Edge Threshold", "Edge Threshold", bVisible);
			}

			public override string Run()
			{
				if (m_grabMode == null) return "Grab Mode == null";

				try
				{
					m_grabMode.SetLight(true);

					AxisXY axisXY = m_module.m_axisXY;
					Axis axisZ = m_module.m_axisZ;
					CPoint cpMemoryOffset = new CPoint(m_cpMemoryOffset);
					int nScanLine = 0;
					int nMMPerUM = 1000;
					int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
					int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

					double dXScale = m_dResX_um * 10;
					cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth;
					m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
					int nReticleSizeY_px = Convert.ToInt32(m_nReticleSize_mm * nMMPerUM / m_dResY_um);  // 레티클 영역의 Y픽셀 갯수
					int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nReticleSizeY_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
					int nScanOffset_pulse = 100000; //가속버퍼구간

					string strPool = m_grabMode.m_memoryPool.p_id;
					string strGroup = m_grabMode.m_memoryGroup.p_id;
					string strMemory = m_grabMode.m_memoryData.p_id;
					MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

					while (m_grabMode.m_ScanLineNum > nScanLine)
					{
						if (EQ.IsStop())
							return "OK";

						double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
						double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

						m_grabMode.m_eGrabDirection = eGrabDirection.Forward;


						double dPosX = m_rpAxisCenter.X + nReticleSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

						if (m_module.Run(axisZ.StartMove(m_nFocusPosZ)))
							return p_sInfo;
						if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
							return p_sInfo;
						if (m_module.Run(axisXY.WaitReady()))
							return p_sInfo;
						if (m_module.Run(axisZ.WaitReady()))
							return p_sInfo;

						double dTriggerStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2;
						double dTriggerEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;
						axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

						int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_nScanRate / 100);
						m_grabMode.StartGrab(mem, cpMemoryOffset, nReticleSizeY_px, 0, m_grabMode.m_bUseBiDirectionScan);

						if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
							return p_sInfo;
						if (m_module.Run(axisXY.WaitReady()))
							return p_sInfo;
						axisXY.p_axisY.RunTrigger(false);

						nScanLine++;
						cpMemoryOffset.X += nCamWidth;
					}
					m_grabMode.m_camera.StopGrab();

					// SideScan용 Edge Offset 구하기
					if (m_cpLeftEdgeCenterPos.X == 0 || m_cpLeftEdgeCenterPos.Y == 0) return "Fail";
					if (m_cpTopEdgeCenterPos.X == 0 || m_cpTopEdgeCenterPos.Y == 0) return "Fail";
					if (m_cpRightEdgeCenterPos.X == 0 || m_cpRightEdgeCenterPos.Y == 0) return "Fail";
					if (m_cpBottomEdgeCenterPos.X == 0 || m_cpBottomEdgeCenterPos.Y == 0) return "Fail";

					CRect crtLeftROI = new CRect(m_cpLeftEdgeCenterPos.X, m_cpLeftEdgeCenterPos.Y, m_nSearchArea);
					CRect crtTopROI = new CRect(m_cpTopEdgeCenterPos.X, m_cpTopEdgeCenterPos.Y, m_nSearchArea);
					CRect crtRightROI = new CRect(m_cpRightEdgeCenterPos.X, m_cpRightEdgeCenterPos.Y, m_nSearchArea);
					CRect crtBottomROI = new CRect(m_cpBottomEdgeCenterPos.X, m_cpBottomEdgeCenterPos.Y, m_nSearchArea);

					//int nLeftEdge = crtLeftROI.Left + m_module.GetEdge(mem, crtLeftROI, 100, eSearchDirection.LeftToRight, m_nEdgeThreshold, true);
					//int nTopEdge = crtTopROI.Top + m_module.GetEdge(mem, crtTopROI, 100, eSearchDirection.TopToBottom, m_nEdgeThreshold, true);
					//int nRightEdge = crtRightROI.Left + m_module.GetEdge(mem, crtRightROI, 100, eSearchDirection.RightToLeft, m_nEdgeThreshold, true);
					//int nBottomEdge = crtBottomROI.Top + m_module.GetEdge(mem, crtBottomROI, 100, eSearchDirection.BottomToTop, m_nEdgeThreshold, true);

					int nLeftStandardFocusPos = 2266;
					int nTopStandardFocusPos = 1751;
					int nRightStandardFocusPos = 32370;
					int nBottomStandardFocusPos = 31957;

					m_module.m_narrSideEdgeOffset[0] = nLeftStandardFocusPos - crtLeftROI.Left + m_module.GetEdge(mem, crtLeftROI, 100, eSearchDirection.LeftToRight, m_nEdgeThreshold, true);
					m_module.m_narrSideEdgeOffset[1] = nTopStandardFocusPos - crtTopROI.Top + m_module.GetEdge(mem, crtTopROI, 100, eSearchDirection.TopToBottom, m_nEdgeThreshold, true);
					m_module.m_narrSideEdgeOffset[2] = nRightStandardFocusPos - crtRightROI.Left + m_module.GetEdge(mem, crtRightROI, 100, eSearchDirection.RightToLeft, m_nEdgeThreshold, true);
					m_module.m_narrSideEdgeOffset[3] = nBottomStandardFocusPos - crtBottomROI.Top + m_module.GetEdge(mem, crtBottomROI, 100, eSearchDirection.BottomToTop, m_nEdgeThreshold, true);


					//int nLeftStandardFocusPos = 2454;
					//int nBottomStandardFocusPos = 31982;
					//int nRightStandardFocusPos = 32382;
					//int nTopStandardFocusPos = 1663;

					//m_module.m_narrSideEdgeOffset[0] = nLeftStandardFocusPos - (crtLeftROI.Left + m_module.GetEdge(mem, crtLeftROI, crtLeftROI.Height / 2, eSearchDirection.LeftToRight, m_nEdgeThreshold, true));
					//m_module.m_narrSideEdgeOffset[1] = nBottomStandardFocusPos - (crtBottomROI.Top + m_module.GetEdge(mem, crtBottomROI, crtBottomROI.Width / 2, eSearchDirection.BottomToTop, m_nEdgeThreshold, true));
					//m_module.m_narrSideEdgeOffset[2] = nRightStandardFocusPos - (crtRightROI.Left + m_module.GetEdge(mem, crtRightROI, crtRightROI.Height / 2, eSearchDirection.RightToLeft, m_nEdgeThreshold, true));
					//m_module.m_narrSideEdgeOffset[3] = nTopStandardFocusPos - (crtTopROI.Top + m_module.GetEdge(mem, crtTopROI, crtTopROI.Width / 2, eSearchDirection.TopToBottom, m_nEdgeThreshold, true));

					return "OK";
				}
				finally
				{
					m_grabMode.SetLight(false);
				}
			}
		}

		public class Run_Grab45 : ModuleRunBase
		{
			MainVision m_module;

			public RPoint m_rpAxisCenter = new RPoint();    // Reticle Center Position
			public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
			public double m_dResX_um = 1;                   // Camera Resolution X
			public double m_dResY_um = 1;                   // Camera Resolution Y
			public int m_nFocusPosZ = 0;                    // Focus Position Z
			public int m_nReticleSize_mm = 1000;              // Reticle Size (mm)
			public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
			public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
			public GrabMode m_grabMode = null;
			string m_sGrabMode = "";

			public string p_sGrabMode
			{
				get { return m_sGrabMode; }
				set
				{
					m_sGrabMode = value;
					m_grabMode = m_module.GetGrabMode(value);
				}
			}
			public Run_Grab45(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}
			public override ModuleRunBase Clone()
			{
				Run_Grab45 run = new Run_Grab45(m_module);
				run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
				run.m_cpMemoryOffset = new CPoint(m_cpMemoryOffset);
				run.m_dResX_um = m_dResX_um;
				run.m_dResY_um = m_dResY_um;
				run.m_nFocusPosZ = m_nFocusPosZ;
				run.m_nReticleSize_mm = m_nReticleSize_mm;
				run.m_nMaxFrame = m_nMaxFrame;
				run.m_nScanRate = m_nScanRate;
				run.p_sGrabMode = p_sGrabMode;
				return run;
			}
			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
				m_cpMemoryOffset = tree.Set(m_cpMemoryOffset, m_cpMemoryOffset, "Memory Offset", "Grab Start Memory Position (px)", bVisible);
				m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Cam X Resolution", "X Resolution (um)", bVisible);
				m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Cam Y Resolution", "Y Resolution (um)", bVisible);
				m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);
				m_nReticleSize_mm = tree.Set(m_nReticleSize_mm, m_nReticleSize_mm, "Reticle Size Y", "Reticle Size Y", bVisible);
				m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
				m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
				p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
			}
			public override string Run()
			{
				if (m_grabMode == null) return "Grab Mode == null";

				try
				{
					AxisXY axisXY = m_module.m_axisXY;
					Axis axisZ = m_module.m_axisZ;
					Axis axisRotate = m_module.m_axisRotate;
					m_grabMode.SetLight(true);
					m_module.m_do45DTrigger.Write(true);
					if (m_grabMode.pUseRADS)
					{
						if (!axisZ.EnableCompensation(1))
							return "Axis Y Compensation disabled";

					}
					CPoint cpMemoryOffset = new CPoint(m_cpMemoryOffset);
					int nScanLine = 0;
					int nMMPerUM = 1000;
					int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
					int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

					double dXScale = m_dResX_um * 10;
					cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth;
					m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
					int nReticleSizeY_px = Convert.ToInt32(m_nReticleSize_mm * nMMPerUM / m_dResY_um);  // 레티클 영역의 Y픽셀 갯수
					int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nReticleSizeY_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
					int nScanOffset_pulse = 200000; //가속버퍼구간

					while (m_grabMode.m_ScanLineNum > nScanLine)
					{
						if (EQ.IsStop())
							return "OK";

						double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
						double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

						m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

						double dPosX = m_rpAxisCenter.X/*중심축값*/ + (nReticleSizeY_px * (double)m_grabMode.m_dTrigger / 2 /*레티클 절반*/) - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

						// Theta축 0으로
						double dTheta = axisRotate.GetPosValue(eAxisPos.ScanPos.ToString());
						dTheta += m_module.m_dThetaAlignOffset;
						if (m_module.Run(axisRotate.StartMove(dTheta)))
							return p_sInfo;
						if (m_module.Run(axisRotate.WaitReady()))
							return p_sInfo;
						//

						if (m_module.Run(axisZ.StartMove(m_nFocusPosZ)))
							return p_sInfo;
						if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
							return p_sInfo;
						if (m_module.Run(axisXY.WaitReady()))
							return p_sInfo;
						if (m_module.Run(axisZ.WaitReady()))
							return p_sInfo;

						double dTriggerStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2;
						double dTriggerEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;
						axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

						//double dTriggerDistance = Math.Abs(dTriggerEndPosY - dTriggerStartPosY);
						//double dSection = dTriggerDistance / ladsinfos[nScanLine].m_Heightinfo.Length;
						//double[] darrScanAxisPos = new double[ladsinfos[nScanLine].m_Heightinfo.Length];
						//for (int i = 0; i < darrScanAxisPos.Length; i++)
						//{
						//    if (dTriggerStartPosY > dTriggerEndPosY)
						//        darrScanAxisPos[i] = dTriggerStartPosY - (dSection * i);
						//    else
						//        darrScanAxisPos[i] = dTriggerStartPosY + (dSection * i);
						//}
						//SetFocusMap(((AjinAxis)axisXY.p_axisY).m_nAxis, ((AjinAxis)axisZ).m_nAxis, darrScanAxisPos, ladsinfos[nScanLine].m_Heightinfo, ladsinfos[nScanLine].m_Heightinfo.Length, false);

						string strPool = m_grabMode.m_memoryPool.p_id;
						string strGroup = m_grabMode.m_memoryGroup.p_id;
						string strMemory = m_grabMode.m_memoryData.p_id;

						MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
						int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_nScanRate / 100);
						m_grabMode.StartGrab(mem, cpMemoryOffset, nReticleSizeY_px, 0, m_grabMode.m_bUseBiDirectionScan);

						//CAXM.AxmContiStart(((AjinAxis)axisXY.p_axisY).m_nAxis, 0, 0);
						//Thread.Sleep(10);
						//uint unRunning = 0;
						//while (true)
						//{
						//    CAXM.AxmContiIsMotion(((AjinAxis)axisXY.p_axisY).m_nAxis, ref unRunning);
						//    if (unRunning == 0) break;
						//    Thread.Sleep(100);
						//}

						if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
							return p_sInfo;
						if (m_module.Run(axisXY.WaitReady()))
							return p_sInfo;
						axisXY.p_axisY.RunTrigger(false);

						nScanLine++;
						cpMemoryOffset.X += nCamWidth;
					}
					m_grabMode.m_camera.StopGrab();
					return "OK";
				}
				finally
				{
					m_grabMode.SetLight(false);
				}
			}

			private void SetFocusMap(int nScanAxisNo, int nZAxisNo, double[] darrScanAxisPos, double[] darrZAxisPos, int nPointCount, bool bReverse)
			{
				// variable
				int iIdxScan = 0;
				int iIdxZ = 1;
				int[] narrAxisNo = new int[2];
				double[] darrPosition = new double[2];
				double dMaxVelocity = m_module.m_axisXY.p_axisY.GetSpeedValue(eSpeed.Move).m_v;
				double dMaxAccel = m_module.m_axisXY.p_axisY.GetSpeedValue(eSpeed.Move).m_acc;
				double dMaxDecel = m_module.m_axisXY.p_axisY.GetSpeedValue(eSpeed.Move).m_dec;

				// implement
				if (nZAxisNo < nScanAxisNo)
				{
					iIdxZ = 0;
					iIdxScan = 1;
				}
				narrAxisNo[iIdxScan] = nScanAxisNo;
				narrAxisNo[iIdxZ] = nZAxisNo;

				// Min-Max 구하기
				double dMin = 480;
				double dMax = 0;
				double dCenter = 240;
				double dPixelPerPulse = 500;
				for (int i = 0; i < darrZAxisPos.Length; i++)
				{
					if (darrZAxisPos[i] < dMin) dMin = darrZAxisPos[i];
					if (darrZAxisPos[i] > dMax) dMax = darrZAxisPos[i];
				}
				dCenter = ((dMax - dMin) / 2) + dMin;

				// Queue 초기화
				CAXM.AxmContiWriteClear(nScanAxisNo);
				// 보간구동 축 맵핑
				CAXM.AxmContiSetAxisMap(nScanAxisNo, (uint)narrAxisNo.Length, narrAxisNo);
				// 구동모드 설정 -> [0] : 절대위치구동, [1] : 상대위치구동
				uint unAbsRelMode = 0;
				CAXM.AxmContiSetAbsRelMode(nScanAxisNo, unAbsRelMode);
				// Conti 작성 시작 -> AxmContiBeginNode ~ AxmContiEndNode 사이의 AXM관련 함수들이 Conti Queue에 등록된다.
				CAXM.AxmContiBeginNode(nScanAxisNo);
				// 축별 구동위치 등록
				if (bReverse)
				{
					for (int i = nPointCount - 1; i >= 0; i--)
					{
						darrPosition[iIdxScan] = darrScanAxisPos[i];
						darrPosition[iIdxZ] = ((darrZAxisPos[i] - dCenter) * dPixelPerPulse) + m_nFocusPosZ;//m_module.m_axisZ.GetPosValue(eAxisPos.ScanPos);
						CAXM.AxmLineMove(nScanAxisNo, darrPosition, dMaxVelocity, dMaxAccel, dMaxDecel);
					}
				}
				else
				{
					for (int i = 0; i < nPointCount; i++)
					{
						darrPosition[iIdxScan] = darrScanAxisPos[i];
						darrPosition[iIdxZ] = ((darrZAxisPos[i] - dCenter) * dPixelPerPulse) + m_nFocusPosZ;//m_module.m_axisZ.GetPosValue(eAxisPos.ScanPos);
						CAXM.AxmLineMove(nScanAxisNo, darrPosition, dMaxVelocity, dMaxAccel, dMaxDecel);
					}
				}
				// Conti 작성 종료
				CAXM.AxmContiEndNode(nScanAxisNo);

				return;
			}
		}
		public class Run_LADS : ModuleRunBase
		{
			MainVision m_module;

			public int m_nUptime = 40;                      // Trigger Uptime
			public RPoint m_rpAxisCenter = new RPoint();    // Reticle Center Position
			public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
			public double m_dResX_um = 1;                   // Camera Resolution X
			public double m_dResY_um = 1;                   // Camera Resolution Y
			public int m_nFocusPosZ = 0;                    // Focus Position Z
			public int m_nReticleSize_mm = 1000;              // Reticle Size (mm)
			public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
			public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
			public GrabMode m_grabMode = null;
			string m_sGrabMode = "";

			public string p_sGrabMode
			{
				get { return m_sGrabMode; }
				set
				{
					m_sGrabMode = value;
					m_grabMode = m_module.GetGrabMode(value);
				}
			}
			public Run_LADS(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}
			public override ModuleRunBase Clone()
			{
				Run_LADS run = new Run_LADS(m_module);
				run.m_nUptime = m_nUptime;
				run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
				run.m_cpMemoryOffset = new CPoint(m_cpMemoryOffset);
				run.m_dResX_um = m_dResX_um;
				run.m_dResY_um = m_dResY_um;
				run.m_nFocusPosZ = m_nFocusPosZ;
				run.m_nReticleSize_mm = m_nReticleSize_mm;
				run.m_nMaxFrame = m_nMaxFrame;
				run.m_nScanRate = m_nScanRate;
				run.p_sGrabMode = p_sGrabMode;
				return run;
			}
			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_nUptime = tree.Set(m_nUptime, m_nUptime, "Trigger Uptime", "Trigger Uptime", bVisible);
				m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
				m_cpMemoryOffset = tree.Set(m_cpMemoryOffset, m_cpMemoryOffset, "Memory Offset", "Grab Start Memory Position (px)", bVisible);
				m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Cam X Resolution", "X Resolution (um)", bVisible);
				m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Cam Y Resolution", "Y Resolution (um)", bVisible);
				m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);
				m_nReticleSize_mm = tree.Set(m_nReticleSize_mm, m_nReticleSize_mm, "Reticle Size Y", "Reticle Size Y", bVisible);
				m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
				m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
				p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
			}

			public override string Run()
			{
				if (m_grabMode == null) return "Grab Mode == null";

				try
				{
					m_grabMode.SetLight(true);
					ladsinfos.Clear();

					AxisXY axisXY = m_module.m_axisXY;
					Axis axisZ = m_module.m_axisZ;
					Axis axisRotate = m_module.m_axisRotate;
					CPoint cpMemoryOffset = new CPoint(m_cpMemoryOffset);
					int nScanLine = 0;
					int nMMPerUM = 1000;
					int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
					int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

					double dXScale = m_dResX_um * 10;
					cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth;
					m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
					int nReticleSizeY_px = Convert.ToInt32(m_nReticleSize_mm * nMMPerUM / m_dResY_um);  // 레티클 영역의 Y픽셀 갯수
					int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nReticleSizeY_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
					int nScanOffset_pulse = 100000; //가속버퍼구간

					while (m_grabMode.m_ScanLineNum > nScanLine)
					{
						if (EQ.IsStop())
							return "OK";

						double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
						double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

						m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

						double dPosX = m_rpAxisCenter.X + nReticleSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

						// Theta축 0으로
						double dTheta = axisRotate.GetPosValue(eAxisPos.ScanPos.ToString());
						dTheta += m_module.m_dThetaAlignOffset;
						if (m_module.Run(axisRotate.StartMove(dTheta)))
							return p_sInfo;
						if (m_module.Run(axisRotate.WaitReady()))
							return p_sInfo;
						//

						if (m_module.Run(axisZ.StartMove(m_nFocusPosZ)))
							return p_sInfo;
						if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
							return p_sInfo;
						if (m_module.Run(axisXY.WaitReady()))
							return p_sInfo;
						if (m_module.Run(axisZ.WaitReady()))
							return p_sInfo;

						double dTriggerStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2;
						double dTriggerEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2;
						axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger * nCamHeight, m_nUptime, true);

						string strPool = m_grabMode.m_memoryPool.p_id;
						string strGroup = m_grabMode.m_memoryGroup.p_id;
						string strMemory = m_grabMode.m_memoryData.p_id;

						MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
						int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_nScanRate / 100);
						m_grabMode.StartGrab(mem, cpMemoryOffset, nReticleSizeY_px, 0, m_grabMode.m_bUseBiDirectionScan);

						if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
							return p_sInfo;
						if (m_module.Run(axisXY.WaitReady()))
							return p_sInfo;
						axisXY.p_axisY.RunTrigger(false);
						m_grabMode.m_camera.StopGrab();

						nScanLine++;
						cpMemoryOffset.X += nCamWidth;
					}
					m_grabMode.m_camera.StopGrab();
					return "OK";
				}
				finally
				{
					m_grabMode.SetLight(false);
				}
			}
		}

		#region Barcode Inspection
		public class Run_BarcodeInspection : ModuleRunBase
		{
			public enum eSearchDirection
			{
				TopToBottom = 0,
				LeftToRight,
				RightToLeft,
				BottomToTop,
			}

			MainVision m_module;
			public CPoint m_cptBarcodeLTPoint = new CPoint(0, 0);
			public int m_nROIWidth = 1000;
			public int m_nROIHeight = 5000;
			public bool m_bDarkBackground = true;
			public int m_nThreshold = 70;
			public int m_nSubImageThreshold = 70;
			public double m_dNGSpecScratchLength_mm = 1;

			public Run_BarcodeInspection(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_BarcodeInspection run = new Run_BarcodeInspection(m_module);
				run.m_cptBarcodeLTPoint = m_cptBarcodeLTPoint;
				run.m_nROIWidth = m_nROIWidth;
				run.m_nROIHeight = m_nROIHeight;
				run.m_bDarkBackground = m_bDarkBackground;
				run.m_nThreshold = m_nThreshold;
				run.m_nSubImageThreshold = m_nSubImageThreshold;
				run.m_dNGSpecScratchLength_mm = m_dNGSpecScratchLength_mm;
				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_cptBarcodeLTPoint = (tree.GetTree("Barcode ROI", false, bVisible)).Set(m_cptBarcodeLTPoint, m_cptBarcodeLTPoint, "Left Top Point", "Left Top Point", bVisible);
				m_nROIWidth = (tree.GetTree("Barcode ROI", false, bVisible)).Set(m_nROIWidth, m_nROIWidth, "Barcode ROI Width", "Barcode ROI Width", bVisible);
				m_nROIHeight = (tree.GetTree("Barcode ROI", false, bVisible)).Set(m_nROIHeight, m_nROIHeight, "Barcode ROI Height", "Barcode ROI Height", bVisible);
				m_bDarkBackground = tree.Set(m_bDarkBackground, m_bDarkBackground, "Dark Background", "Dark Background", bVisible);
				m_nThreshold = tree.Set(m_nThreshold, m_nThreshold, "Find Edge Threshold", "Find Edge Threshold", bVisible);
				m_nSubImageThreshold = tree.Set(m_nSubImageThreshold, m_nSubImageThreshold, "Sub Image Threshold", "Sub Image Threshold", bVisible);
				m_dNGSpecScratchLength_mm = tree.Set(m_dNGSpecScratchLength_mm, m_dNGSpecScratchLength_mm, "Scratch Spec [mm]", "Scratch Spec [mm]", bVisible);
			}

			public override string Run()
			{
				MemoryData mem = m_module.m_engineer.GetMemory(App.mPool, App.mGroup, App.mMainMem);
				CPoint cptStartROIPoint = m_cptBarcodeLTPoint;
				CPoint cptEndROIPoint = new CPoint(cptStartROIPoint.X + m_nROIWidth, cptStartROIPoint.Y + m_nROIHeight);
				CRect crtROI = new CRect(cptStartROIPoint, cptEndROIPoint);
				CRect crtHalfLeft = new CRect(cptStartROIPoint, new CPoint(crtROI.Center().X, cptEndROIPoint.Y));
				CRect crtHalfRight = new CRect(new CPoint(crtROI.Center().X, cptStartROIPoint.Y), cptEndROIPoint);

				// ROI따기
				int nTop = GetEdge(mem, crtROI, 50, eSearchDirection.TopToBottom, m_nThreshold, m_bDarkBackground);
				int nBottom = GetEdge(mem, crtROI, 50, eSearchDirection.BottomToTop, m_nThreshold, m_bDarkBackground);
				int nLeft = GetBarcodeSideEdge(mem, crtROI, 10, eSearchDirection.LeftToRight, m_nThreshold, m_bDarkBackground);
				int nRight = GetBarcodeSideEdge(mem, crtROI, 10, eSearchDirection.RightToLeft, m_nThreshold, m_bDarkBackground);
				CRect crtBarcode = new CRect(m_cptBarcodeLTPoint.X + nLeft, m_cptBarcodeLTPoint.Y + nTop, m_cptBarcodeLTPoint.X + nRight, m_cptBarcodeLTPoint.Y + nBottom);
				Image<Gray, byte> imgBarcode = m_module.GetGrayByteImageFromMemory(mem, crtBarcode);
				Mat matBarcode = imgBarcode.Mat;
				matBarcode.Save("D:\\AOP01\\BarcodeInspection\\BeforeRotation.bmp");

				// 회전각도 알아내기
				int nLeftTop = GetEdge(mem, crtHalfLeft, 10, eSearchDirection.TopToBottom, m_nThreshold, m_bDarkBackground);
				CPoint cptLeftTop = new CPoint(crtHalfLeft.Center().X, nLeftTop);
				int nRightTop = GetEdge(mem, crtHalfRight, 10, eSearchDirection.TopToBottom, m_nThreshold, m_bDarkBackground);
				CPoint cptRightTop = new CPoint(crtHalfRight.Center().X, nRightTop);
				double dThetaRadian = Math.Atan2((double)(cptRightTop.Y - cptLeftTop.Y), (double)(cptRightTop.X - cptLeftTop.X));
				double dThetaDegree = dThetaRadian * (180 / Math.PI);

				// Barcode 회전
				Mat matAffine = new Mat();
				Mat matRotation = new Mat();
				CvInvoke.GetRotationMatrix2D(new System.Drawing.PointF(matBarcode.Width / 2, matBarcode.Height / 2), dThetaDegree, 1.0, matAffine);
				CvInvoke.WarpAffine(matBarcode, matRotation, matAffine, new System.Drawing.Size(matBarcode.Width, matBarcode.Height));
				matRotation.Save("D:\\AOP01\\BarcodeInspection\\AfterRotation.bmp");

				// 회전 후 외곽영역 Cutting
				int y1 = 100;
				int y2 = matRotation.Rows - 100;
				int x1 = 100;
				int x2 = matRotation.Cols - 100;
				Mat matCutting = new Mat(matRotation, new Range(y1, y2), new Range(x1, x2));
				matCutting.Save("D:\\AOP01\\BarcodeInspection\\Cutting.bmp");

				// Profile 구하기
				Mat matSub = GetRowProfileMat(matCutting);

				// 차영상 구하기
				Mat matResult;// = new Mat(matCutting.Rows, matCutting.Cols, matCutting.Depth, matCutting.NumberOfChannels);
				Mat matResult2;
				Mat matResult3;

				matResult2 = matCutting - matSub;
				matResult3 = matSub - matCutting;
				matResult = matResult2 + matResult3;

				matResult.Save("D:\\AOP01\\BarcodeInspection\\Result.bmp");

				// 차영상에서 Blob Labeling
				Mat matBinary = new Mat();
				CvInvoke.Threshold(matResult, matBinary, m_nSubImageThreshold, 255, ThresholdType.Binary);
				matBinary.Save("D:\\AOP01\\BarcodeInspection\\BinaryResult.bmp");
				CvBlobs blobs = new CvBlobs();
				CvBlobDetector blobDetector = new CvBlobDetector();
				Image<Gray, byte> img = matBinary.ToImage<Gray, byte>();
				blobDetector.Detect(img, blobs);

				int nMMPerUM = 1000;
				foreach (CvBlob blob in blobs.Values)
				{
					if (blob.BoundingBox.Width * 5/*TDI90 Resolution = 5*/ > m_dNGSpecScratchLength_mm * nMMPerUM || blob.BoundingBox.Height * 5/*TDI90 Resolution = 5*/ > m_dNGSpecScratchLength_mm * nMMPerUM)
					{
						m_module.p_bBarcodePass = false;
						break;
					}
					else m_module.p_bBarcodePass = true;
				}
				return "OK";
			}

			unsafe int GetEdge(MemoryData mem, CRect crtROI, int nProfileSize, eSearchDirection eDirection, int nThreshold, bool bDarkBackground)
			{
				if (nProfileSize > crtROI.Width) return 0;
				if (nProfileSize > crtROI.Height) return 0;

				// variable
				ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
				IntPtr p = mem.GetPtr();
				byte* bp;

				// implement
				img.SetData(p, crtROI, (int)mem.W);
				int nCount = 0;
				switch (eDirection)
				{
					case eSearchDirection.TopToBottom:
						for (int y = 0; y < img.p_Size.Y; y++)
						{
							nCount = 0;
							bp = (byte*)img.GetPtr() + y * img.p_Stride + (img.p_Size.X / 2);
							for (int x = -(nProfileSize / 2); x < (nProfileSize / 2); x++)
							{
								byte* bpCurrent = bp + x;
								if (bDarkBackground)
								{
									if (*bpCurrent > nThreshold) nCount++;
								}
								else
								{
									if (*bpCurrent < nThreshold) nCount++;
								}
							}
							if (nCount == nProfileSize) return y;
						}
						break;
					case eSearchDirection.LeftToRight:
						for (int x = 0; x < img.p_Size.X; x++)
						{
							nCount = 0;
							bp = (byte*)img.GetPtr() + x + (img.p_Size.Y / 2) * img.p_Stride;
							for (int y = -(nProfileSize / 2); y < (nProfileSize / 2); y++)
							{
								byte* bpCurrent = bp + y * img.p_Stride;
								if (bDarkBackground)
								{
									if (*bpCurrent > nThreshold) nCount++;
								}
								else
								{
									if (*bpCurrent < nThreshold) nCount++;
								}
							}
							if (nCount == nProfileSize) return x;
						}
						break;
					case eSearchDirection.RightToLeft:
						for (int x = img.p_Size.X - 1; x >= 0; x--)
						{
							nCount = 0;
							bp = (byte*)img.GetPtr() + x + (img.p_Size.Y / 2) * img.p_Stride;
							for (int y = -(nProfileSize / 2); y < (nProfileSize / 2); y++)
							{
								byte* bpCurrent = bp + y * img.p_Stride;
								if (bDarkBackground)
								{
									if (*bpCurrent > nThreshold) nCount++;
								}
								else
								{
									if (*bpCurrent < nThreshold) nCount++;
								}
							}
							if (nCount == nProfileSize) return x;
						}
						break;
					case eSearchDirection.BottomToTop:
						for (int y = img.p_Size.Y - 2; y >= 0; y--) // img의 마지막줄은 0으로 채워질 수 있기 때문에 마지막의 전줄부터 탐색
						{
							nCount = 0;
							bp = (byte*)img.GetPtr() + y * img.p_Stride + (img.p_Size.X / 2);
							for (int x = -(nProfileSize / 2); x < (nProfileSize / 2); x++)
							{
								byte* bpCurrent = bp + x;
								if (bDarkBackground)
								{
									if (*bpCurrent > nThreshold) nCount++;
								}
								else
								{
									if (*bpCurrent < nThreshold) nCount++;
								}
							}
							if (nCount == nProfileSize) return y;
						}
						break;
				}

				return 0;
			}

			unsafe int GetBarcodeSideEdge(MemoryData mem, CRect crtROI, int nProfileSize, eSearchDirection eDirection, int nThreshold, bool bDarkBackground)
			{
				if (nProfileSize > crtROI.Width) return 0;
				if (nProfileSize > crtROI.Height) return 0;

				// variable
				ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
				IntPtr p = mem.GetPtr();
				byte* bp;

				// implement
				img.SetData(p, crtROI, (int)mem.W);
				int nFlipCount = 0;
				bool bCurrentDark = false;
				bool bPreDark = true;

				switch (eDirection)
				{
					case eSearchDirection.LeftToRight:

						for (int x = 0; x < img.p_Size.X; x++)
						{
							nFlipCount = 0;
							for (int y = 0; y < img.p_Size.Y; y++)
							{
								bp = (byte*)img.GetPtr() + y * img.p_Stride + x;
								if (*bp > nThreshold) bCurrentDark = false;
								else bCurrentDark = true;
								if (bPreDark != bCurrentDark)
								{
									nFlipCount++;
									bPreDark = bCurrentDark;
								}
							}
							if (nFlipCount > 30) return x;
						}
						return 0;

					//break;
					case eSearchDirection.RightToLeft:

						for (int x = img.p_Size.X - 1; x >= 0; x--)
						{
							nFlipCount = 0;
							for (int y = 0; y < img.p_Size.Y; y++)
							{
								bp = (byte*)img.GetPtr() + y * img.p_Stride + x;
								if (*bp > nThreshold) bCurrentDark = false;
								else bCurrentDark = true;
								if (bPreDark != bCurrentDark)
								{
									nFlipCount++;
									bPreDark = bCurrentDark;
								}
							}
							if (nFlipCount > 30) return x;
						}
						return 0;

					//break;
					default:
						return 0;
						//break;
				}

				//return 0;
			}

			unsafe Mat GetRowProfileMat(Mat matSrc)
			{
				// variable
				byte* bp = null;
				Mat matReturn = new Mat(matSrc.Size, matSrc.Depth, matSrc.NumberOfChannels);
				Image<Gray, byte> img = matReturn.ToImage<Gray, byte>();

				// implement
				m_module.p_nBarcodeInspectionProgressValue = 0;
				m_module.p_nBarcodeInspectionProgressMin = 0;
				m_module.p_nBarcodeInspectionProgressMax = matSrc.Rows;
				if (m_module.p_nBarcodeInspectionProgressMax - m_module.p_nBarcodeInspectionProgressMin > 0)
					m_module.p_nBarcodeInspectionProgressPercent = (int)((double)m_module.p_nBarcodeInspectionProgressValue / (double)(m_module.p_nBarcodeInspectionProgressMax - m_module.p_nBarcodeInspectionProgressMin) * 100);

				int nProgress = 0;
				Parallel.For(0, matSrc.Rows, (y) =>
				{
					long lSum = 0;
					for (int x = 0; x < matSrc.Cols; x++)
					{
						bp = (byte*)matSrc.DataPointer + y * matSrc.Step + x;
						lSum += *bp;
					}
					for (int x = 0; x < matReturn.Cols; x++)
					{
						img.Data[y, x, 0] = (byte)(lSum / matSrc.Cols);
					}
					m_module.p_nBarcodeInspectionProgressValue = ++nProgress;
					if (m_module.p_nBarcodeInspectionProgressMax - m_module.p_nBarcodeInspectionProgressMin > 0)
						m_module.p_nBarcodeInspectionProgressPercent = (int)((double)m_module.p_nBarcodeInspectionProgressValue / (double)(m_module.p_nBarcodeInspectionProgressMax - m_module.p_nBarcodeInspectionProgressMin) * 100);
				});
				matReturn = img.Mat;

				m_module.p_nBarcodeInspectionProgressPercent = 100;

				return matReturn;
			}
		}
		#endregion

		#region Make Align Template Image
		public class Run_MakeAlignTemplateImage : ModuleRunBase
		{
			public enum eSelectedTemplate
			{
				AlignMark,
				InOutFeature,
				AlignKey,
			}

			MainVision m_module;

			public eSelectedTemplate m_eSelectedTemplate = eSelectedTemplate.AlignMark;

			public CPoint m_cptTopAlignMarkCenterPos = new CPoint();
			public int m_nTopWidth = 500;
			public int m_nTopHeight = 500;
			public CPoint m_cptBottomAlignMarkCenterPos = new CPoint();
			public int m_nBottomWidth = 500;
			public int m_nBottomHeight = 500;

			public CPoint m_cptOutLTCenterPos = new CPoint();
			public int m_nOutLTWidth = 500;
			public int m_nOutLTHeight = 500;
			public CPoint m_cptOutRTCenterPos = new CPoint();
			public int m_nOutRTWidth = 500;
			public int m_nOutRTHeight = 500;
			public CPoint m_cptOutRBCenterPos = new CPoint();
			public int m_nOutRBWidth = 500;
			public int m_nOutRBHeight = 500;
			public CPoint m_cptOutLBCenterPos = new CPoint();
			public int m_nOutLBWidth = 500;
			public int m_nOutLBHeight = 500;

			public CPoint m_cptInLTCenterPos = new CPoint();
			public int m_nInLTWidth = 500;
			public int m_nInLTHeight = 500;
			public CPoint m_cptInRTCenterPos = new CPoint();
			public int m_nInRTWidth = 500;
			public int m_nInRTHeight = 500;
			public CPoint m_cptInRBCenterPos = new CPoint();
			public int m_nInRBWidth = 500;
			public int m_nInRBHeight = 500;
			public CPoint m_cptInLBCenterPos = new CPoint();
			public int m_nInLBWidth = 500;
			public int m_nInLBHeight = 500;

			// Align Key
			public CPoint m_cptLTAlignKeyCenterPos = new CPoint();
			public int m_nLTAlignKeyWidth = 500;
			public int m_nLTAlignKeyHeight = 500;
			public CPoint m_cptRTAlignKeyCenterPos = new CPoint();
			public int m_nRTAlignKeyWidth = 500;
			public int m_nRTAlignKeyHeight = 500;
			public CPoint m_cptRBAlignKeyCenterPos = new CPoint();
			public int m_nRBAlignKeyWidth = 500;
			public int m_nRBAlignKeyHeight = 500;
			public CPoint m_cptLBAlignKeyCenterPos = new CPoint();
			public int m_nLBAlignKeyWidth = 500;
			public int m_nLBAlignKeyHeight = 500;

			public Run_MakeAlignTemplateImage(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_MakeAlignTemplateImage run = new Run_MakeAlignTemplateImage(m_module);

				run.m_eSelectedTemplate = m_eSelectedTemplate;

				run.m_cptTopAlignMarkCenterPos = m_cptTopAlignMarkCenterPos;
				run.m_nTopWidth = m_nTopWidth;
				run.m_nTopHeight = m_nTopHeight;
				run.m_cptBottomAlignMarkCenterPos = m_cptBottomAlignMarkCenterPos;
				run.m_nBottomWidth = m_nBottomWidth;
				run.m_nBottomHeight = m_nBottomHeight;

				run.m_cptOutLTCenterPos = m_cptOutLTCenterPos;
				run.m_nOutLTWidth = m_nOutLTWidth;
				run.m_nOutLTHeight = m_nOutLTHeight;
				run.m_cptOutRTCenterPos = m_cptOutRTCenterPos;
				run.m_nOutRTWidth = m_nOutRTWidth;
				run.m_nOutRTHeight = m_nOutRTHeight;
				run.m_cptOutRBCenterPos = m_cptOutRBCenterPos;
				run.m_nOutRBWidth = m_nOutRBWidth;
				run.m_nOutRBHeight = m_nOutRBHeight;
				run.m_cptOutLBCenterPos = m_cptOutLBCenterPos;
				run.m_nOutLBWidth = m_nOutLBWidth;
				run.m_nOutLBHeight = m_nOutLBHeight;

				run.m_cptInLTCenterPos = m_cptInLTCenterPos;
				run.m_nInLTWidth = m_nInLTWidth;
				run.m_nInLTHeight = m_nInLTHeight;
				run.m_cptInRTCenterPos = m_cptInRTCenterPos;
				run.m_nInRTWidth = m_nInRTWidth;
				run.m_nInRTHeight = m_nInRTHeight;
				run.m_cptInRBCenterPos = m_cptInRBCenterPos;
				run.m_nInRBWidth = m_nInRBWidth;
				run.m_nInRBHeight = m_nInRBHeight;
				run.m_cptInLBCenterPos = m_cptInLBCenterPos;
				run.m_nInLBWidth = m_nInLBWidth;
				run.m_nInLBHeight = m_nInLBHeight;

				// Align Key
				run.m_cptLTAlignKeyCenterPos = m_cptLTAlignKeyCenterPos;
				run.m_nLTAlignKeyWidth = m_nLTAlignKeyWidth;
				run.m_nLTAlignKeyHeight = m_nLTAlignKeyHeight;
				run.m_cptRTAlignKeyCenterPos = m_cptRTAlignKeyCenterPos;
				run.m_nRTAlignKeyWidth = m_nRTAlignKeyWidth;
				run.m_nRTAlignKeyHeight = m_nRTAlignKeyHeight;
				run.m_cptRBAlignKeyCenterPos = m_cptRBAlignKeyCenterPos;
				run.m_nRBAlignKeyWidth = m_nRBAlignKeyWidth;
				run.m_nRBAlignKeyHeight = m_nRBAlignKeyHeight;
				run.m_cptLBAlignKeyCenterPos = m_cptLBAlignKeyCenterPos;
				run.m_nLBAlignKeyWidth = m_nLBAlignKeyWidth;
				run.m_nLBAlignKeyHeight = m_nLBAlignKeyHeight;

				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_eSelectedTemplate = (eSelectedTemplate)tree.Set(m_eSelectedTemplate, m_eSelectedTemplate, "Selected Template", "Selected Template", bVisible);

				m_cptTopAlignMarkCenterPos = ((tree.GetTree("Align Mark", false, bVisible)).GetTree("Top Align Mark ROI", false, bVisible)).Set(m_cptTopAlignMarkCenterPos, m_cptTopAlignMarkCenterPos, "Top Align Mark Center Position", "Top Align Mark Center Position", bVisible);
				m_nTopWidth = ((tree.GetTree("Align Mark", false, bVisible)).GetTree("Top Align Mark ROI", false, bVisible)).Set(m_nTopWidth, m_nTopWidth, "Top Align Mark Width", "Top Align Mark Width", bVisible);
				m_nTopHeight = ((tree.GetTree("Align Mark", false, bVisible)).GetTree("Top Align Mark ROI", false, bVisible)).Set(m_nTopHeight, m_nTopHeight, "Top Align Mark Height", "Top Align Mark Height", bVisible);
				m_cptBottomAlignMarkCenterPos = ((tree.GetTree("Align Mark", false, bVisible)).GetTree("Bottom Align Mark ROI", false, bVisible)).Set(m_cptBottomAlignMarkCenterPos, m_cptBottomAlignMarkCenterPos, "Bottom Align Mark Center Position", "Bottom Align Mark Center Position", bVisible);
				m_nBottomWidth = ((tree.GetTree("Align Mark", false, bVisible)).GetTree("Bottom Align Mark ROI", false, bVisible)).Set(m_nBottomWidth, m_nBottomWidth, "Bottom Align Mark Width", "Bottom Align Mark Width", bVisible);
				m_nBottomHeight = ((tree.GetTree("Align Mark", false, bVisible)).GetTree("Bottom Align Mark ROI", false, bVisible)).Set(m_nBottomHeight, m_nBottomHeight, "Bottom Align Mark Height", "Bottom Align Mark Height", bVisible);

				m_cptOutLTCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Left Top", false, bVisible)).Set(m_cptOutLTCenterPos, m_cptOutLTCenterPos, "Out Left Top Center Position", "Out Left Top Center Position", bVisible);
				m_nOutLTWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Left Top", false, bVisible)).Set(m_nOutLTWidth, m_nOutLTWidth, "Out Left Top Width", "Out Left Top Width", bVisible);
				m_nOutLTHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Left Top", false, bVisible)).Set(m_nOutLTHeight, m_nOutLTHeight, "Out Left Top Height", "Out Left Top Height", bVisible);
				m_cptOutRTCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Right Top", false, bVisible)).Set(m_cptOutRTCenterPos, m_cptOutRTCenterPos, "Out Right Top Center Position", "Out Right Top Center Position", bVisible);
				m_nOutRTWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Right Top", false, bVisible)).Set(m_nOutRTWidth, m_nOutRTWidth, "Out Right Top Width", "Out Right Top Width", bVisible);
				m_nOutRTHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Right Top", false, bVisible)).Set(m_nOutRTHeight, m_nOutRTHeight, "Out Right Top Height", "Out Right Top Height", bVisible);
				m_cptOutRBCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Right Bottom", false, bVisible)).Set(m_cptOutRBCenterPos, m_cptOutRBCenterPos, "Out Right Bottom Center Position", "Out Right Bottom Center Position", bVisible);
				m_nOutRBWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Right Bottom", false, bVisible)).Set(m_nOutRBWidth, m_nOutRBWidth, "Out Right Bottom Width", "Out Right Bottom Width", bVisible);
				m_nOutRBHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Right Bottom", false, bVisible)).Set(m_nOutRBHeight, m_nOutRBHeight, "Out Right Bottom Height", "Out Right Bottom Height", bVisible);
				m_cptOutLBCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Left Bottom", false, bVisible)).Set(m_cptOutLBCenterPos, m_cptOutLBCenterPos, "Out Left Bottom Center Position", "Out Left Bottom Center Position", bVisible);
				m_nOutLBWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Left Bottom", false, bVisible)).Set(m_nOutLBWidth, m_nOutLBWidth, "Out Left Bottom Width", "Out Left Bottom Width", bVisible);
				m_nOutLBHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Left Bottom", false, bVisible)).Set(m_nOutLBHeight, m_nOutLBHeight, "Out Left Bottom Height", "Out Left Bottom Height", bVisible);

				m_cptInLTCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Left Top", false, bVisible)).Set(m_cptInLTCenterPos, m_cptInLTCenterPos, "In Left Top Center Position", "In Left Top Center Position", bVisible);
				m_nInLTWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Left Top", false, bVisible)).Set(m_nInLTWidth, m_nInLTWidth, "In Left Top Width", "In Left Top Width", bVisible);
				m_nInLTHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Left Top", false, bVisible)).Set(m_nInLTHeight, m_nInLTHeight, "In Left Top Height", "In Left Top Height", bVisible);
				m_cptInRTCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Right Top", false, bVisible)).Set(m_cptInRTCenterPos, m_cptInRTCenterPos, "In Right Top Center Position", "In Right Top Center Position", bVisible);
				m_nInRTWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Right Top", false, bVisible)).Set(m_nInRTWidth, m_nInRTWidth, "In Right Top Width", "In Right Top Width", bVisible);
				m_nInRTHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Right Top", false, bVisible)).Set(m_nInRTHeight, m_nInRTHeight, "In Right Top Height", "In Right Top Height", bVisible);
				m_cptInRBCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Right Bottom", false, bVisible)).Set(m_cptInRBCenterPos, m_cptInRBCenterPos, "In Right Bottom Center Position", "In Right Bottom Center Position", bVisible);
				m_nInRBWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Right Bottom", false, bVisible)).Set(m_nInRBWidth, m_nInRBWidth, "In Right Bottom Width", "In Right Bottom Width", bVisible);
				m_nInRBHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Right Bottom", false, bVisible)).Set(m_nInRBHeight, m_nInRBHeight, "In Right Bottom Height", "In Right Bottom Height", bVisible);
				m_cptInLBCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Left Bottom", false, bVisible)).Set(m_cptInLBCenterPos, m_cptInLBCenterPos, "In Left Bottom Center Position", "In Left Bottom Center Position", bVisible);
				m_nInLBWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Left Bottom", false, bVisible)).Set(m_nInLBWidth, m_nInLBWidth, "In Left Bottom Width", "In Left Bottom Width", bVisible);
				m_nInLBHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Left Bottom", false, bVisible)).Set(m_nInLBHeight, m_nInLBHeight, "In Left Bottom Height", "In Left Bottom Height", bVisible);

				// Align Key
				m_cptLTAlignKeyCenterPos = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Left Top Align Key ROI", false, bVisible)).Set(m_cptLTAlignKeyCenterPos, m_cptLTAlignKeyCenterPos, "Left Top Center Position", "Left Top Center Position", bVisible);
				m_nLTAlignKeyWidth = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Left Top Align Key ROI", false, bVisible)).Set(m_nLTAlignKeyWidth, m_nLTAlignKeyWidth, "Left Top Width", "Left Top Width", bVisible);
				m_nLTAlignKeyHeight = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Left Top Align Key ROI", false, bVisible)).Set(m_nLTAlignKeyHeight, m_nLTAlignKeyHeight, "Left Top Height", "Left Top Height", bVisible);
				m_cptRTAlignKeyCenterPos = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Right Top Align Key ROI", false, bVisible)).Set(m_cptRTAlignKeyCenterPos, m_cptRTAlignKeyCenterPos, "Right Top Center Position", "Right Top Center Position", bVisible);
				m_nRTAlignKeyWidth = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Right Top Align Key ROI", false, bVisible)).Set(m_nRTAlignKeyWidth, m_nRTAlignKeyWidth, "Right Top Width", "Right Top Width", bVisible);
				m_nRTAlignKeyHeight = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Right Top Align Key ROI", false, bVisible)).Set(m_nRTAlignKeyHeight, m_nRTAlignKeyHeight, "Right Top Height", "Right Top Height", bVisible);
				m_cptRBAlignKeyCenterPos = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Right Bottom Align Key ROI", false, bVisible)).Set(m_cptRBAlignKeyCenterPos, m_cptRBAlignKeyCenterPos, "Right Bottom Center Position", "Right Bottom Center Position", bVisible);
				m_nRBAlignKeyWidth = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Right Bottom Align Key ROI", false, bVisible)).Set(m_nRBAlignKeyWidth, m_nRBAlignKeyWidth, "Right Bottom Width", "Right Bottom Width", bVisible);
				m_nRBAlignKeyHeight = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Right Bottom Align Key ROI", false, bVisible)).Set(m_nRBAlignKeyHeight, m_nRBAlignKeyHeight, "Right Bottom Height", "Right Bottom Height", bVisible);
				m_cptLBAlignKeyCenterPos = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Left Bottom Align Key ROI", false, bVisible)).Set(m_cptLBAlignKeyCenterPos, m_cptLBAlignKeyCenterPos, "Left Bottom Center Position", "Left Bottom Center Position", bVisible);
				m_nLBAlignKeyWidth = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Left Bottom Align Key ROI", false, bVisible)).Set(m_nLBAlignKeyWidth, m_nLBAlignKeyWidth, "Left Bottom Width", "Left Bottom Width", bVisible);
				m_nLBAlignKeyHeight = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Left Bottom Align Key ROI", false, bVisible)).Set(m_nLBAlignKeyHeight, m_nLBAlignKeyHeight, "Left Bottom Height", "Left Bottom Height", bVisible);
			}

			public override string Run()
			{
				// variable
				Mat matTopTemplateImage = new Mat();
				Mat matBottomTemplateImage = new Mat();
				MemoryData mem = m_module.m_engineer.GetMemory(App.mPool, App.mGroup, App.mMainMem);
				string strAlignMarkPath = "D:\\AlignMarkTemplateImage\\";
				string strInOutFeaturePath = "D:\\FeatureTemplateImage\\";
				string strAlignKeyPath = "D:\\AlignKeyTemplateImage\\";

				// implement
				switch (m_eSelectedTemplate)
				{
					case eSelectedTemplate.AlignMark:

						if (!Directory.Exists(strAlignMarkPath))
							Directory.CreateDirectory(strAlignMarkPath);

						Mat matTopAlignMarkImage = new Mat();
						Mat matBottomAlignMarkImage = new Mat();
						CRect crtTopAlignMarkROI = new CRect(m_cptTopAlignMarkCenterPos, m_nTopWidth, m_nTopHeight);
						CRect crtBottomAlignMarkROI = new CRect(m_cptBottomAlignMarkCenterPos, m_nBottomWidth, m_nBottomHeight);

						matTopAlignMarkImage = m_module.GetMatImage(mem, crtTopAlignMarkROI);
						if (matTopAlignMarkImage != null) matTopAlignMarkImage.Save(Path.Combine(strAlignMarkPath, "TopTemplateImage.bmp"));
						matBottomAlignMarkImage = m_module.GetMatImage(mem, crtBottomAlignMarkROI);
						if (matBottomAlignMarkImage != null) matBottomAlignMarkImage.Save(Path.Combine(strAlignMarkPath, "BottomTemplateImage.bmp"));

						break;
					case eSelectedTemplate.InOutFeature:

						if (!Directory.Exists(strInOutFeaturePath))
							Directory.CreateDirectory(strInOutFeaturePath);

						Mat matOutLTImage = new Mat();
						Mat matOutRTImage = new Mat();
						Mat matOutRBImage = new Mat();
						Mat matOutLBImage = new Mat();
						Mat matInLTImage = new Mat();
						Mat matInRTImage = new Mat();
						Mat matInRBImage = new Mat();
						Mat matInLBImage = new Mat();
						CRect crtOutLTROI = new CRect(m_cptOutLTCenterPos, m_nOutLTWidth, m_nOutLTHeight);
						CRect crtOutRTROI = new CRect(m_cptOutRTCenterPos, m_nOutRTWidth, m_nOutRTHeight);
						CRect crtOutRBROI = new CRect(m_cptOutRBCenterPos, m_nOutRBWidth, m_nOutRBHeight);
						CRect crtOutLBROI = new CRect(m_cptOutLBCenterPos, m_nOutLBWidth, m_nOutLBHeight);
						CRect crtInLTROI = new CRect(m_cptInLTCenterPos, m_nInLTWidth, m_nInLTHeight);
						CRect crtInRTROI = new CRect(m_cptInRTCenterPos, m_nInRTWidth, m_nInRTHeight);
						CRect crtInRBROI = new CRect(m_cptInRBCenterPos, m_nInRBWidth, m_nInRBHeight);
						CRect crtInLBROI = new CRect(m_cptInLBCenterPos, m_nInLBWidth, m_nInLBHeight);

						matOutLTImage = m_module.GetMatImage(mem, crtOutLTROI);
						if (matOutLTImage != null) matOutLTImage.Save(Path.Combine(strInOutFeaturePath, "OutLT.bmp"));
						matOutRTImage = m_module.GetMatImage(mem, crtOutRTROI);
						if (matOutRTImage != null) matOutRTImage.Save(Path.Combine(strInOutFeaturePath, "OutRT.bmp"));
						matOutRBImage = m_module.GetMatImage(mem, crtOutRBROI);
						if (matOutRBImage != null) matOutRBImage.Save(Path.Combine(strInOutFeaturePath, "OutRB.bmp"));
						matOutLBImage = m_module.GetMatImage(mem, crtOutLBROI);
						if (matOutLBImage != null) matOutLBImage.Save(Path.Combine(strInOutFeaturePath, "OutLB.bmp"));
						matInLTImage = m_module.GetMatImage(mem, crtInLTROI);
						if (matInLTImage != null) matInLTImage.Save(Path.Combine(strInOutFeaturePath, "InLT.bmp"));
						matInRTImage = m_module.GetMatImage(mem, crtInRTROI);
						if (matInRTImage != null) matInRTImage.Save(Path.Combine(strInOutFeaturePath, "InRT.bmp"));
						matInRBImage = m_module.GetMatImage(mem, crtInRBROI);
						if (matInRBImage != null) matInRBImage.Save(Path.Combine(strInOutFeaturePath, "InRB.bmp"));
						matInLBImage = m_module.GetMatImage(mem, crtInLBROI);
						if (matInLBImage != null) matInLBImage.Save(Path.Combine(strInOutFeaturePath, "InLB.bmp"));

						break;
					case eSelectedTemplate.AlignKey:

						if (!Directory.Exists(strAlignKeyPath))
							Directory.CreateDirectory(strAlignKeyPath);

						Mat matLTAlignKeyImage = new Mat();
						Mat matRTAlignKeyImage = new Mat();
						Mat matRBAlignKeyImage = new Mat();
						Mat matLBAlignKeyImage = new Mat();
						CRect crtLTAlignKeyROI = new CRect(m_cptLTAlignKeyCenterPos, m_nLTAlignKeyWidth, m_nLTAlignKeyHeight);
						CRect crtRTAlignKeyROI = new CRect(m_cptRTAlignKeyCenterPos, m_nRTAlignKeyWidth, m_nRTAlignKeyHeight);
						CRect crtRBAlignKeyROI = new CRect(m_cptRBAlignKeyCenterPos, m_nRBAlignKeyWidth, m_nRBAlignKeyHeight);
						CRect crtLBAlignKeyROI = new CRect(m_cptLBAlignKeyCenterPos, m_nLBAlignKeyWidth, m_nLBAlignKeyHeight);

						matLTAlignKeyImage = m_module.GetMatImage(mem, crtLTAlignKeyROI);
						if (matLTAlignKeyImage != null) matLTAlignKeyImage.Save(Path.Combine(strAlignKeyPath, "LT.bmp"));
						matRTAlignKeyImage = m_module.GetMatImage(mem, crtRTAlignKeyROI);
						if (matRTAlignKeyImage != null) matRTAlignKeyImage.Save(Path.Combine(strAlignKeyPath, "RT.bmp"));
						matRBAlignKeyImage = m_module.GetMatImage(mem, crtRBAlignKeyROI);
						if (matRBAlignKeyImage != null) matRBAlignKeyImage.Save(Path.Combine(strAlignKeyPath, "RB.bmp"));
						matLBAlignKeyImage = m_module.GetMatImage(mem, crtLBAlignKeyROI);
						if (matLBAlignKeyImage != null) matLBAlignKeyImage.Save(Path.Combine(strAlignKeyPath, "LB.bmp"));

						break;
				}

				return "OK";
			}
		}
		#endregion

		#region PatternAlign
		public class Run_PatternAlign : ModuleRunBase
		{
			MainVision m_module;
			public int m_nSearchAreaSize = 1000;
			public double m_dMatchScore = 0.4;
			public string m_strTopTemplateImageFilePath = "D:\\AlignMarkTemplateImage\\TopTemplateImage.bmp";
			public string m_strBottomTemplateImageFilePath = "D:\\AlignMarkTemplateImage\\BottomTemplateImage.bmp";

			public Run_PatternAlign(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_PatternAlign run = new Run_PatternAlign(m_module);
				run.m_nSearchAreaSize = m_nSearchAreaSize;
				run.m_dMatchScore = m_dMatchScore;
				run.m_strTopTemplateImageFilePath = m_strTopTemplateImageFilePath;
				run.m_strBottomTemplateImageFilePath = m_strBottomTemplateImageFilePath;
				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_nSearchAreaSize = tree.Set(m_nSearchAreaSize, m_nSearchAreaSize, "Template Matching Search Area Size", "Template Matching Search Area Size", bVisible);
				m_dMatchScore = tree.Set(m_dMatchScore, m_dMatchScore, "Template Matching Pass Score", "Template Matching Pass Score", bVisible);
				m_strTopTemplateImageFilePath = (tree.GetTree("Template Image Path", false, bVisible)).SetFile(m_strTopTemplateImageFilePath, m_strTopTemplateImageFilePath, "bmp", "Top Template Image Path", "Top Template Image Path", bVisible);
				m_strBottomTemplateImageFilePath = (tree.GetTree("Template Image Path", false, bVisible)).SetFile(m_strBottomTemplateImageFilePath, m_strBottomTemplateImageFilePath, "bmp", "Bottom Template Image Path", "Bottom Template Image Path", bVisible);
			}

			public override string Run()
			{
				// variable
				Image<Gray, byte> imgTop = new Image<Gray, byte>(m_strTopTemplateImageFilePath);
				Image<Gray, byte> imgBottom = new Image<Gray, byte>(m_strBottomTemplateImageFilePath);
				CPoint cptTopCenter = new CPoint();
				CPoint cptBottomCenter = new CPoint();
				bool bFoundTop = false;
				bool bFoundBottom = false;
				CPoint cptTopResultCenter;
				CPoint cptBottomResultCenter;

				// implement
				// Align Mark 1Line Scan
				Run_Grab moduleRunGrab = (Run_Grab)m_module.CloneModuleRun("Grab");
				AxisXY axisXY = m_module.m_axisXY;
				Axis axisZ = m_module.m_axisZ;
				Axis axisRotate = m_module.m_axisRotate;
				CPoint cpMemoryOffset = new CPoint(moduleRunGrab.m_cpMemoryOffset);
				int nScanLine = 0;
				int nMMPerUM = 1000;
				int nCamWidth = moduleRunGrab.m_grabMode.m_camera.GetRoiSize().X;
				int nCamHeight = moduleRunGrab.m_grabMode.m_camera.GetRoiSize().Y;
				double dXScale = moduleRunGrab.m_dResX_um * 10;
				cpMemoryOffset.X += (nScanLine + moduleRunGrab.m_grabMode.m_ScanStartLine) * nCamWidth;
				moduleRunGrab.m_grabMode.m_dTrigger = Convert.ToInt32(10 * moduleRunGrab.m_dResY_um);
				int nReticleSizeY_px = Convert.ToInt32(moduleRunGrab.m_nReticleSize_mm * nMMPerUM / moduleRunGrab.m_dResY_um);  // 레티클 영역의 Y픽셀 갯수
				int nTotalTriggerCount = Convert.ToInt32(moduleRunGrab.m_grabMode.m_dTrigger * nReticleSizeY_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
				int nScanSpeed = Convert.ToInt32((double)moduleRunGrab.m_nMaxFrame * moduleRunGrab.m_grabMode.m_dTrigger * nCamHeight * moduleRunGrab.m_nScanRate / 100);
				int nScanOffset_pulse = Convert.ToInt32(nScanSpeed * 0.3); //가속버퍼구간

				if (EQ.IsStop())
					return "OK";

				moduleRunGrab.m_grabMode.SetLight(true);

				double dStartPosY = moduleRunGrab.m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
				double dEndPosY = moduleRunGrab.m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

				moduleRunGrab.m_grabMode.m_eGrabDirection = eGrabDirection.Forward;


				double dPosX = moduleRunGrab.m_rpAxisCenter.X + nReticleSizeY_px * (double)moduleRunGrab.m_grabMode.m_dTrigger / 2 - (nScanLine + moduleRunGrab.m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

				if (m_module.Run(axisRotate.StartMove(eAxisPos.ScanPos)))
					return p_sInfo;
				if (m_module.Run(axisZ.StartMove(moduleRunGrab.m_nFocusPosZ)))
					return p_sInfo;
				if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
					return p_sInfo;
				if (m_module.Run(axisRotate.WaitReady()))
					return p_sInfo;
				if (m_module.Run(axisZ.WaitReady()))
					return p_sInfo;
				if (m_module.Run(axisXY.WaitReady()))
					return p_sInfo;


				double dTriggerStartPosY = moduleRunGrab.m_rpAxisCenter.Y - nTotalTriggerCount / 2;
				double dTriggerEndPosY = moduleRunGrab.m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;
				axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, moduleRunGrab.m_grabMode.m_dTrigger, true);

				string strPool = moduleRunGrab.m_grabMode.m_memoryPool.p_id;
				string strGroup = moduleRunGrab.m_grabMode.m_memoryGroup.p_id;
				string strMemory = moduleRunGrab.m_grabMode.m_memoryData.p_id;

				MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

				moduleRunGrab.m_grabMode.StartGrab(mem, cpMemoryOffset, nReticleSizeY_px, 0, moduleRunGrab.m_grabMode.m_bUseBiDirectionScan);

				if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
					return p_sInfo;
				if (m_module.Run(axisXY.WaitReady()))
					return p_sInfo;
				axisXY.p_axisY.RunTrigger(false);
				moduleRunGrab.m_grabMode.SetLight(false);
				//

				Run_MakeAlignTemplateImage moduleRun = (Run_MakeAlignTemplateImage)m_module.CloneModuleRun("MakeAlignTemplateImage");
				cptTopCenter.X = moduleRun.m_cptTopAlignMarkCenterPos.X;
				cptTopCenter.Y = moduleRun.m_cptTopAlignMarkCenterPos.Y;
				cptBottomCenter.X = moduleRun.m_cptBottomAlignMarkCenterPos.X;
				cptBottomCenter.Y = moduleRun.m_cptBottomAlignMarkCenterPos.Y;

				// Top Template Image Processing
				Point ptStart = new Point(cptTopCenter.X - (m_nSearchAreaSize / 2), cptTopCenter.Y - (m_nSearchAreaSize / 2));
				Point ptEnd = new Point(cptTopCenter.X + (m_nSearchAreaSize / 2), cptTopCenter.Y + (m_nSearchAreaSize / 2));
				CRect crtSearchArea = new CRect(ptStart, ptEnd);
				//Mat matSearchArea = m_module.GetMatImage(mem, crtSearchArea);
				//Image<Gray, byte> imgSrc = matSearchArea.ToImage<Gray, byte>();
				Image<Gray, byte> imgSrc = m_module.GetGrayByteImageFromMemory(mem, crtSearchArea);
				bFoundTop = m_module.TemplateMatching(mem, crtSearchArea, imgSrc, imgTop, out cptTopResultCenter, m_dMatchScore);

				// Bottom Template Image Processing
				ptStart = new Point(cptBottomCenter.X - (m_nSearchAreaSize / 2), cptBottomCenter.Y - (m_nSearchAreaSize / 2));
				ptEnd = new Point(cptBottomCenter.X + (m_nSearchAreaSize / 2), cptBottomCenter.Y + (m_nSearchAreaSize / 2));
				crtSearchArea = new CRect(ptStart, ptEnd);
				//matSearchArea = m_module.GetMatImage(mem, crtSearchArea);
				//imgSrc = matSearchArea.ToImage<Gray, byte>();
				imgSrc = m_module.GetGrayByteImageFromMemory(mem, crtSearchArea);
				bFoundBottom = m_module.TemplateMatching(mem, crtSearchArea, imgSrc, imgTop, out cptBottomResultCenter, m_dMatchScore);

				// Calculate Theta
				if (bFoundTop && bFoundBottom)  // Top & Bottom 모두 Template Matching 성공했을 경우
				{
					double dThetaRadian = Math.Atan2((double)(cptBottomResultCenter.Y - cptTopResultCenter.Y), (double)(cptBottomResultCenter.X - cptTopResultCenter.X));
					double dThetaDegree = dThetaRadian * (180 / Math.PI);
					dThetaDegree -= 90;
					// 1000 Pulse = 1 Degree
					double dThetaPulse = dThetaDegree * 1000;

					//Theta축 회전
					double dActualPos = axisRotate.p_posActual;
					if (m_module.Run(axisRotate.StartMove(dActualPos + dThetaPulse)))
						return p_sInfo;
					if (m_module.Run(axisRotate.WaitReady()))
						return p_sInfo;
					m_module.m_dThetaAlignOffset = dThetaPulse;

					//// 회전이미지 
					//Mat matSrc = GetMatImage(mem, new CRect(1000, 1000, 7000, 33000));
					//Mat matAffine = new Mat();
					//Mat matRotation = new Mat();
					//CvInvoke.GetRotationMatrix2D(new System.Drawing.PointF(matSrc.Width / 2, matSrc.Height / 2), dThetaDegree, 1.0, matAffine);
					//CvInvoke.WarpAffine(matSrc, matRotation, matAffine, new System.Drawing.Size(matSrc.Width, matSrc.Height));
					//matRotation.Save("D:\\ROTATIONTEST.BMP");

					return "OK";
				}
				else
					return "Fail";
			}
		}
		#endregion

		#region PatternArrayShift & Rotation 검사
		public class Run_PatternShiftAndRotation : ModuleRunBase
		{
			MainVision m_module;
			public int m_nSearchArea = 2000;
			public double m_dMatchScore = 0.95;
			public double m_dNGSpecDistance_mm = 500.0;
			public double m_dNGSpecDegree = 0.5;

			public Run_PatternShiftAndRotation(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_PatternShiftAndRotation run = new Run_PatternShiftAndRotation(m_module);
				run.m_nSearchArea = m_nSearchArea;
				run.m_dMatchScore = m_dMatchScore;
				run.m_dNGSpecDistance_mm = m_dNGSpecDistance_mm;
				run.m_dNGSpecDegree = m_dNGSpecDegree;
				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_nSearchArea = tree.Set(m_nSearchArea, m_nSearchArea, "Search Area Size [px]", "Search Area Size [px]", bVisible);
				m_dMatchScore = tree.Set(m_dMatchScore, m_dMatchScore, "Template Matching Score [0.0~1.0]", "Template Matching Score [0.0~1.0]", bVisible);
				m_dNGSpecDistance_mm = tree.GetTree("NG Spec", false, bVisible).Set(m_dNGSpecDistance_mm, m_dNGSpecDistance_mm, "Distance NG Spec [mm]", "Distance NG Spec [mm]", bVisible);
				m_dNGSpecDegree = tree.GetTree("NG Spec", false, bVisible).Set(m_dNGSpecDegree, m_dNGSpecDegree, "Degree NG Spec", "Degree NG Spec", bVisible);
			}

			public enum eSearchPoint
			{
				LT,
				RT,
				RB,
				LB,
				Count,
			}
			public override string Run()
			{
				// variable
				string strPool = "MainVision.Vision Memory";
				string strGroup = "MainVision";
				string strMemory = "Main";
				MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
				Run_MakeAlignTemplateImage moduleRun = (Run_MakeAlignTemplateImage)m_module.CloneModuleRun("MakeAlignTemplateImage");
				string strFeatureTemplateImagePath = "D:\\FeatureTemplateImage\\";
				Image<Gray, byte> imgSearchArea;
				Image<Gray, byte> imgTemplate;
				Point ptStart, ptEnd;
				CRect crtSearchArea;
				//Mat matSearchArea;
				CPoint cptSearchAreaCenter;
				bool bFound = false;
				CPoint[] cptarrOutResultCenterPositions = new CPoint[(int)eSearchPoint.Count];
				CPoint[] cptarrInResultCenterPositions = new CPoint[(int)eSearchPoint.Count];
				CPoint cptOutFeatureCentroid;
				CPoint cptInFeatureCentroid;

				// implement
				m_module.p_bPatternShiftPass = true;

				m_module.p_nPatternShiftProgressValue = 0;
				m_module.p_nPatternShiftProgressMin = 0;
				m_module.p_nPatternShiftProgressMax = (int)eSearchPoint.Count * 2;
				if (m_module.p_nPatternShiftProgressMax - m_module.p_nPatternShiftProgressMin > 0)
					m_module.p_nPatternShiftProgressPercent = (int)((double)m_module.p_nPatternShiftProgressValue / (double)(m_module.p_nPatternShiftProgressMax - m_module.p_nPatternShiftProgressMin) * 100);

				// 1. Outside Feature(LT, RT, RB, LB) TemplateMatching
				for (int i = 0; i < (int)eSearchPoint.Count; i++)
				{
					switch (i)
					{
						case (int)eSearchPoint.LT:
							cptSearchAreaCenter = new CPoint(moduleRun.m_cptOutLTCenterPos);
							imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "OutLT.bmp"));
							break;
						case (int)eSearchPoint.RT:
							cptSearchAreaCenter = new CPoint(moduleRun.m_cptOutRTCenterPos);
							imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "OutRT.bmp"));
							break;
						case (int)eSearchPoint.RB:
							cptSearchAreaCenter = new CPoint(moduleRun.m_cptOutRBCenterPos);
							imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "OutRB.bmp"));
							break;
						case (int)eSearchPoint.LB:
							cptSearchAreaCenter = new CPoint(moduleRun.m_cptOutLBCenterPos);
							imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "OutLB.bmp"));
							break;
						default:
							cptSearchAreaCenter = new CPoint();
							imgTemplate = new Image<Gray, byte>(m_nSearchArea, m_nSearchArea);
							break;
					}

					ptStart = new Point(cptSearchAreaCenter.X - (m_nSearchArea / 2), cptSearchAreaCenter.Y - (m_nSearchArea / 2));
					ptEnd = new Point(cptSearchAreaCenter.X + (m_nSearchArea / 2), cptSearchAreaCenter.Y + (m_nSearchArea / 2));
					crtSearchArea = new CRect(ptStart, ptEnd);
					//matSearchArea = m_module.GetMatImage(mem, crtSearchArea);
					//imgSearchArea = matSearchArea.ToImage<Gray, byte>();
					imgSearchArea = m_module.GetGrayByteImageFromMemory(mem, crtSearchArea);
					CPoint cptFoundCenter;
					bFound = m_module.TemplateMatching(mem, crtSearchArea, imgSearchArea, imgTemplate, out cptFoundCenter, m_dMatchScore);
					if (bFound) cptarrOutResultCenterPositions[i] = new CPoint(cptFoundCenter);

					m_module.p_nPatternShiftProgressValue++;
					if (m_module.p_nPatternShiftProgressMax - m_module.p_nPatternShiftProgressMin > 0)
						m_module.p_nPatternShiftProgressPercent = (int)((double)m_module.p_nPatternShiftProgressValue / (double)(m_module.p_nPatternShiftProgressMax - m_module.p_nPatternShiftProgressMin) * 100);
				}
				cptOutFeatureCentroid = GetCentroidFromPolygonPointArray(cptarrOutResultCenterPositions);

				// 2. Inside Feature(LT, RT, RB, LB) TemplateMatching
				for (int i = 0; i < (int)eSearchPoint.Count; i++)
				{
					switch (i)
					{
						case (int)eSearchPoint.LT:
							cptSearchAreaCenter = new CPoint(moduleRun.m_cptInLTCenterPos);
							imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "InLT.bmp"));
							break;
						case (int)eSearchPoint.RT:
							cptSearchAreaCenter = new CPoint(moduleRun.m_cptInRTCenterPos);
							imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "InRT.bmp"));
							break;
						case (int)eSearchPoint.RB:
							cptSearchAreaCenter = new CPoint(moduleRun.m_cptInRBCenterPos);
							imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "InRB.bmp"));
							break;
						case (int)eSearchPoint.LB:
							cptSearchAreaCenter = new CPoint(moduleRun.m_cptInLBCenterPos);
							imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "InLB.bmp"));
							break;
						default:
							cptSearchAreaCenter = new CPoint();
							imgTemplate = new Image<Gray, byte>(m_nSearchArea, m_nSearchArea);
							break;
					}

					ptStart = new Point(cptSearchAreaCenter.X - (m_nSearchArea / 2), cptSearchAreaCenter.Y - (m_nSearchArea / 2));
					ptEnd = new Point(cptSearchAreaCenter.X + (m_nSearchArea / 2), cptSearchAreaCenter.Y + (m_nSearchArea / 2));
					crtSearchArea = new CRect(ptStart, ptEnd);
					//matSearchArea = m_module.GetMatImage(mem, crtSearchArea);
					//imgSearchArea = matSearchArea.ToImage<Gray, byte>();
					imgSearchArea = m_module.GetGrayByteImageFromMemory(mem, crtSearchArea);
					imgSearchArea.Save("D:\\TEST.BMP");
					CPoint cptFoundCenter;
					bFound = m_module.TemplateMatching(mem, crtSearchArea, imgSearchArea, imgTemplate, out cptFoundCenter, m_dMatchScore);
					if (bFound) cptarrInResultCenterPositions[i] = new CPoint(cptFoundCenter);

					m_module.p_nPatternShiftProgressValue++;
					if (m_module.p_nPatternShiftProgressMax - m_module.p_nPatternShiftProgressMin > 0)
						m_module.p_nPatternShiftProgressPercent = (int)((double)m_module.p_nPatternShiftProgressValue / (double)(m_module.p_nPatternShiftProgressMax - m_module.p_nPatternShiftProgressMin) * 100);
				}
				cptInFeatureCentroid = GetCentroidFromPolygonPointArray(cptarrInResultCenterPositions);

				// Get distance From InFeatureCentroid & OutFeatureCentroid
				Run_Grab moduleRunGrab = (Run_Grab)m_module.CloneModuleRun("Grab");
				double dResultDistance = m_module.GetDistanceOfTwoPoint(cptInFeatureCentroid, cptOutFeatureCentroid);
				m_module.p_dPatternShiftDistance = dResultDistance * moduleRunGrab.m_dResY_um / 1000;

				// Get Degree
				CPoint cptOutLeftCenter = new CPoint((cptarrOutResultCenterPositions[(int)eSearchPoint.LT].X + cptarrOutResultCenterPositions[(int)eSearchPoint.LB].X) / 2,
													 (cptarrOutResultCenterPositions[(int)eSearchPoint.LT].Y + cptarrOutResultCenterPositions[(int)eSearchPoint.LB].Y) / 2);
				CPoint cptOutRightCenter = new CPoint((cptarrOutResultCenterPositions[(int)eSearchPoint.RT].X + cptarrOutResultCenterPositions[(int)eSearchPoint.RB].X) / 2,
													  (cptarrOutResultCenterPositions[(int)eSearchPoint.RT].Y + cptarrOutResultCenterPositions[(int)eSearchPoint.RB].Y) / 2);
				CPoint cptInLeftCenter = new CPoint((cptarrInResultCenterPositions[(int)eSearchPoint.LT].X + cptarrInResultCenterPositions[(int)eSearchPoint.LB].X) / 2,
													 (cptarrInResultCenterPositions[(int)eSearchPoint.LT].Y + cptarrInResultCenterPositions[(int)eSearchPoint.LB].Y) / 2);
				CPoint cptInRightCenter = new CPoint((cptarrInResultCenterPositions[(int)eSearchPoint.RT].X + cptarrInResultCenterPositions[(int)eSearchPoint.RB].X) / 2,
													 (cptarrInResultCenterPositions[(int)eSearchPoint.RT].Y + cptarrInResultCenterPositions[(int)eSearchPoint.RB].Y) / 2);
				double dOutLineThetaRadian = Math.Atan2((double)(cptOutLeftCenter.Y - cptOutRightCenter.Y),
														(double)(cptOutLeftCenter.X - cptOutRightCenter.X));
				double dOutLineThetaDegree = dOutLineThetaRadian * (180 / Math.PI);

				double dInLineThetaRadian = Math.Atan2((double)(cptInLeftCenter.Y - cptInRightCenter.Y),
													   (double)(cptInLeftCenter.X - cptInRightCenter.X));
				double dInLineThetaDegree = dInLineThetaRadian * (180 / Math.PI);

				m_module.p_dPatternShiftAngle = Math.Abs(dOutLineThetaDegree - dInLineThetaDegree);
				//double dThetaRadian = Math.Atan2((double)(cptarrOutResultCenterPositions[(int)eSearchPoint.RT].Y - cptarrOutResultCenterPositions[(int)eSearchPoint.LT].Y),
				//                                 (double)(cptarrOutResultCenterPositions[(int)eSearchPoint.RT].X - cptarrOutResultCenterPositions[(int)eSearchPoint.LT].X));
				//double dThetaDegree = dThetaRadian * (180 / Math.PI);
				//m_module.p_dPatternShiftAngle = dThetaDegree;

				// Judgement
				if (m_dNGSpecDistance_mm < (dResultDistance * moduleRunGrab.m_dResY_um / 1000))
				{
					m_module.p_bPatternShiftPass = false;
				}
				if (m_dNGSpecDegree < m_module.p_dPatternShiftAngle)
				{
					m_module.p_bPatternShiftPass = false;
				}

				return "OK";
			}

			// 다각형의 면적과 무게중심을 구하는 알고리즘 * 출처 https://lsit81.tistory.com/entry/%EB%8B%A4%EA%B0%81%ED%98%95-%EB%A9%B4%EC%A0%81%EA%B3%BC-%EB%AC%B4%EA%B2%8C-%EC%A4%91%EC%8B%AC-%EA%B5%AC%ED%95%98%EA%B8%B0
			CPoint GetCentroidFromPolygonPointArray(CPoint[] cptarr)
			{
				// variable
				int j = 0;
				CPoint cpt1, cpt2;
				double dArea = 0.0;
				CPoint cptCentroid = new CPoint();
				double dX1, dX2, dY1, dY2;
				double dCentroidX = 0, dCentroidY = 0;

				// implement
				for (int i = 0; i < cptarr.Length; i++)
				{
					j = (i + 1) % cptarr.Length;
					cpt1 = cptarr[i];
					cpt2 = cptarr[j];
					dX1 = cpt1.X;
					dX2 = cpt2.X;
					dY1 = cpt1.Y;
					dY2 = cpt2.Y;
					dArea += ((dX1 * dY2) - (dX2 * dY1));

					dCentroidX += ((dX1 + dX2) * ((dX1 * dY2) - (dX2 * dY1)));
					dCentroidY += ((dY1 + dY2) * ((dX1 * dY2) - (dX2 * dY1)));
				}

				dArea /= 2.0;
				dArea = Math.Abs(dArea);

				dCentroidX = (dCentroidX / (6.0 * dArea));
				dCentroidY = (dCentroidY / (6.0 * dArea));

				cptCentroid.X = (int)dCentroidX;
				cptCentroid.Y = (int)dCentroidY;

				return cptCentroid;
			}
		}
		#endregion

		#region Align Key 검사
		public class Run_AlignKeyInspection : ModuleRunBase
		{
			public enum eSearchPoint
			{
				LT,
				RT,
				RB,
				LB,
				Count,
			}

			MainVision m_module;
			public int m_nSearchArea = 2000;
			public double m_dMatchScore = 0.95;
			public int m_nThreshold = 70;
			public int m_nNGSpec_um = 30;

			public Run_AlignKeyInspection(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_AlignKeyInspection run = new Run_AlignKeyInspection(m_module);
				run.m_nSearchArea = m_nSearchArea;
				run.m_dMatchScore = m_dMatchScore;
				run.m_nThreshold = m_nThreshold;
				run.m_nNGSpec_um = m_nNGSpec_um;
				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_nSearchArea = tree.Set(m_nSearchArea, m_nSearchArea, "Search Area Size [px]", "Search Area Size [px]", bVisible);
				m_dMatchScore = tree.Set(m_dMatchScore, m_dMatchScore, "Template Matching Score [0.0~1.0]", "Template Matching Score [0.0~1.0]", bVisible);
				m_nThreshold = tree.Set(m_nThreshold, m_nThreshold, "Binary Threshold [GV]", "Binary Threshold [GV]", bVisible);
				m_nNGSpec_um = tree.Set(m_nNGSpec_um, m_nNGSpec_um, "NG Spec [um]", "NG Spec [um]", bVisible);
			}

			public override string Run()
			{
				// variable
				string strPool = "MainVision.Vision Memory";
				string strGroup = "MainVision";
				string strMemory = "Main";
				MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
				Run_MakeAlignTemplateImage moduleRun = (Run_MakeAlignTemplateImage)m_module.CloneModuleRun("MakeAlignTemplateImage");
				string strAlignKeyTemplateImagePath = "D:\\AlignKeyTemplateImage\\";
				Image<Gray, byte> imgSearchArea;
				Image<Gray, byte> imgTemplate;
				Point ptStart, ptEnd;
				CRect crtSearchArea;
				CPoint cptSearchAreaCenter;
				bool bFound = false;
				Mat[] matarr = new Mat[4];

				// implement
				m_module.p_nAlignKeyProgressValue = 0;
				m_module.p_nAlignKeyProgressMin = 0;
				m_module.p_nAlignKeyProgressMax = 7;
				if (m_module.p_nAlignKeyProgressMax - m_module.p_nAlignKeyProgressMin > 0)
					m_module.p_nAlignKeyProgressPercent = (int)((double)m_module.p_nAlignKeyProgressValue / (double)(m_module.p_nAlignKeyProgressMax - m_module.p_nAlignKeyProgressMin) * 100);

				for (int i = 0; i < (int)eSearchPoint.Count; i++)
				{
					switch (i)
					{
						case (int)eSearchPoint.LT:
							cptSearchAreaCenter = new CPoint(moduleRun.m_cptLTAlignKeyCenterPos);
							imgTemplate = new Image<Gray, byte>(Path.Combine(strAlignKeyTemplateImagePath, "LT.bmp"));
							break;
						case (int)eSearchPoint.RT:
							cptSearchAreaCenter = new CPoint(moduleRun.m_cptRTAlignKeyCenterPos);
							imgTemplate = new Image<Gray, byte>(Path.Combine(strAlignKeyTemplateImagePath, "RT.bmp"));
							break;
						case (int)eSearchPoint.RB:
							cptSearchAreaCenter = new CPoint(moduleRun.m_cptRBAlignKeyCenterPos);
							imgTemplate = new Image<Gray, byte>(Path.Combine(strAlignKeyTemplateImagePath, "RB.bmp"));
							break;
						case (int)eSearchPoint.LB:
							cptSearchAreaCenter = new CPoint(moduleRun.m_cptLBAlignKeyCenterPos);
							imgTemplate = new Image<Gray, byte>(Path.Combine(strAlignKeyTemplateImagePath, "LB.bmp"));
							break;
						default:
							cptSearchAreaCenter = new CPoint();
							imgTemplate = new Image<Gray, byte>(m_nSearchArea, m_nSearchArea);
							break;
					}

					ptStart = new Point(cptSearchAreaCenter.X - (m_nSearchArea / 2), cptSearchAreaCenter.Y - (m_nSearchArea / 2));
					ptEnd = new Point(cptSearchAreaCenter.X + (m_nSearchArea / 2), cptSearchAreaCenter.Y + (m_nSearchArea / 2));
					crtSearchArea = new CRect(ptStart, ptEnd);
					imgSearchArea = m_module.GetGrayByteImageFromMemory(mem, crtSearchArea);
					CPoint cptFoundCenter;
					bFound = m_module.TemplateMatching(mem, crtSearchArea, imgSearchArea, imgTemplate, out cptFoundCenter, m_dMatchScore);

					if (bFound) // Template Matching 성공
					{
						ptStart = new Point(cptFoundCenter.X - (imgTemplate.Width / 2), cptFoundCenter.Y - (imgTemplate.Height / 2));
						ptEnd = new Point(cptFoundCenter.X + (imgTemplate.Width / 2), cptFoundCenter.Y + (imgTemplate.Height / 2));
						CRect crtFoundRect = new CRect(ptStart, ptEnd);
						Image<Gray, byte> imgFound = m_module.GetGrayByteImageFromMemory(mem, crtFoundRect);
						Image<Gray, byte> imgBinary = imgFound.ThresholdBinaryInv(new Gray(m_nThreshold), new Gray(128));
						Mat matBinary = imgBinary.Mat;
						CvBlobs blobs = new CvBlobs();
						CvBlobDetector blobDetector = new CvBlobDetector();
						blobDetector.Detect(imgBinary, blobs);
						int nMaxArea = 0;
						System.Drawing.Point[] ptsContour = new System.Drawing.Point[1];
						foreach (CvBlob blob in blobs.Values)
						{
							if (blob.Area > nMaxArea)
							{
								nMaxArea = blob.Area;
								ptsContour = blob.GetContour();
							}
						}
						CRect crtBoundingBox;
						Mat matResult = FloodFill(matBinary, ptsContour[0], 255, out crtBoundingBox, Connectivity.EightConnected);
						matResult = matResult - matBinary;
						if (i == (int)eSearchPoint.RT)  // Flip Horizontal
						{
							CvInvoke.Flip(matResult, matResult, FlipType.Horizontal);
						}
						else if (i == (int)eSearchPoint.RB) // Flip Horizontal & Vertical
						{
							CvInvoke.Flip(matResult, matResult, FlipType.Horizontal);
							CvInvoke.Flip(matResult, matResult, FlipType.Vertical);
						}
						else if (i == (int)eSearchPoint.LB) // Flip Vertical
						{
							CvInvoke.Flip(matResult, matResult, FlipType.Vertical);
						}
						matarr[i] = matResult.Clone();
					}

					m_module.p_nAlignKeyProgressValue++;
					if (m_module.p_nAlignKeyProgressMax - m_module.p_nAlignKeyProgressMin > 0)
						m_module.p_nAlignKeyProgressPercent = (int)((double)m_module.p_nAlignKeyProgressValue / (double)(m_module.p_nAlignKeyProgressMax - m_module.p_nAlignKeyProgressMin) * 100);
				}

				// Compare All Image
				for (int i = 0; i < 3; i++)
				{
					Mat matMaster = matarr[i].Clone();
					Image<Gray, byte> imgMaster = matMaster.ToImage<Gray, byte>();
					for (int j = i + 1; j < 4; j++)
					{
						Mat matSlave = matarr[j].Clone();
						Image<Gray, byte> imgSlave = matSlave.ToImage<Gray, byte>();
						CvBlobs blobs = new CvBlobs();
						CvBlobDetector blobDetector = new CvBlobDetector();
						blobDetector.Detect(imgSlave, blobs);
						foreach (CvBlob blob in blobs.Values)
						{
							Mat matMiniTemplate = new Mat(matSlave, blob.BoundingBox);
							Image<Gray, byte> imgMiniTemplate = matMiniTemplate.ToImage<Gray, byte>();
							Image<Gray, float> imgMatchResult = imgMaster.MatchTemplate(imgMiniTemplate, TemplateMatchingType.CcorrNormed);
							float[,,] matches = imgMatchResult.Data;
							float fMaxScore = float.MinValue;
							CPoint cptMaxRelative = new CPoint();
							for (int x = 0; x < matches.GetLength(1); x++)
							{
								for (int y = 0; y < matches.GetLength(0); y++)
								{
									if (fMaxScore < matches[y, x, 0] && m_dMatchScore < matches[y, x, 0])
									{
										fMaxScore = matches[y, x, 0];
										cptMaxRelative.X = x;
										cptMaxRelative.Y = y;
									}
								}
							}
							Image<Gray, byte> imgMasterClone = imgMaster.Clone();
							byte[,,] barrMaster = imgMasterClone.Data;
							byte[,,] barrMiniTemplate = imgMiniTemplate.Data;
							for (int x = 0; x < imgMiniTemplate.Width; x++)
							{
								for (int y = 0; y < imgMiniTemplate.Height; y++)
								{
									barrMaster[y + cptMaxRelative.Y, x + cptMaxRelative.X, 0] -= barrMiniTemplate[y, x, 0];
								}
							}
							Image<Gray, byte> imgSub = new Image<Gray, byte>(barrMaster);
							imgSub = imgSub.Erode(1);

							// 차영상 Blob 결과
							bool bResult = GetResultFromImage(imgSub);

							string strName = "";
							if (i == (int)eSearchPoint.LT) strName += eSearchPoint.LT.ToString() + "-";
							else if (i == (int)eSearchPoint.RT) strName += eSearchPoint.RT.ToString() + "-";
							else if (i == (int)eSearchPoint.RB) strName += eSearchPoint.RB.ToString() + "-";
							else strName += eSearchPoint.LB.ToString() + "-";

							if (j == (int)eSearchPoint.LT) strName += eSearchPoint.LT;
							else if (j == (int)eSearchPoint.RT) strName += eSearchPoint.RT;
							else if (j == (int)eSearchPoint.RB) strName += eSearchPoint.RB;
							else strName += eSearchPoint.LB;

							imgSub.Save("D:\\AOP01\\AlignKeyInspection\\ESCHO_" + strName + ".BMP");

							//if (bResult == false)
							//{
							//    m_module.p_bAlignKeyPass = false;
							//    return "Fail";
							//}
						}
					}

					m_module.p_nAlignKeyProgressValue++;
					if (m_module.p_nAlignKeyProgressMax - m_module.p_nAlignKeyProgressMin > 0)
						m_module.p_nAlignKeyProgressPercent = (int)((double)m_module.p_nAlignKeyProgressValue / (double)(m_module.p_nAlignKeyProgressMax - m_module.p_nAlignKeyProgressMin) * 100);
				}
				m_module.p_bAlignKeyPass = true;
				return "OK";
			}

			bool GetResultFromImage(Image<Gray, byte> img)
			{
				// variable
				CvBlobs blobs = new CvBlobs();
				CvBlobDetector blobDetector = new CvBlobDetector();

				// implement
				blobDetector.Detect(img, blobs);
				foreach (CvBlob blob in blobs.Values)
				{
					if (blob.BoundingBox.Width > m_nNGSpec_um / 5/*Resolution*/ || blob.BoundingBox.Height > m_nNGSpec_um / 5/*Resolution*/) return false;
				}

				return true;
			}

			Mat FloodFill(Mat matSrc, System.Drawing.Point ptSeed, int nPaintValue, out CRect crtBoundingBox, Connectivity connect)
			{
				// variable
				Queue<System.Drawing.Point> q = new Queue<System.Drawing.Point>();
				bool[,] barrVisited = new bool[matSrc.Height, matSrc.Width];
				int nL = matSrc.Width - 1;
				int nT = matSrc.Height - 1;
				int nR = 0;
				int nB = 0;

				// implement
				Image<Gray, byte> img = matSrc.ToImage<Gray, byte>();
				byte[,,] imgarr = img.Data;
				for (int y = 0; y < matSrc.Height; y++)
				{
					for (int x = 0; x < matSrc.Width; x++)
					{
						barrVisited[y, x] = false;
					}
				}

				// BFS 시작
				q.Enqueue(ptSeed);
				barrVisited[ptSeed.Y, ptSeed.X] = true;
				imgarr[ptSeed.Y, ptSeed.X, 0] = (byte)nPaintValue;
				if (connect == Connectivity.FourConnected)
				{
					while (q.Count != 0)
					{
						System.Drawing.Point ptTemp = q.Dequeue();
						// 상,우,하,좌
						if (ptTemp.Y - 1 >= 0)
						{
							if (barrVisited[ptTemp.Y - 1, ptTemp.X] == false && imgarr[ptTemp.Y - 1, ptTemp.X, 0] != 0)
							{
								barrVisited[ptTemp.Y - 1, ptTemp.X] = true;
								imgarr[ptTemp.Y - 1, ptTemp.X, 0] = (byte)nPaintValue;
								q.Enqueue(new System.Drawing.Point(ptTemp.X, ptTemp.Y - 1));
								if (nL > ptTemp.X) nL = ptTemp.X;
								if (nT > ptTemp.Y) nT = ptTemp.Y;
								if (nR < ptTemp.X) nR = ptTemp.X;
								if (nB < ptTemp.Y) nB = ptTemp.Y;
							}
						}
						if (ptTemp.X + 1 < matSrc.Width)
						{
							if (barrVisited[ptTemp.Y, ptTemp.X + 1] == false && imgarr[ptTemp.Y, ptTemp.X + 1, 0] != 0)
							{
								barrVisited[ptTemp.Y, ptTemp.X + 1] = true;
								imgarr[ptTemp.Y, ptTemp.X + 1, 0] = (byte)nPaintValue;
								q.Enqueue(new System.Drawing.Point(ptTemp.X + 1, ptTemp.Y));
								if (nL > ptTemp.X) nL = ptTemp.X;
								if (nT > ptTemp.Y) nT = ptTemp.Y;
								if (nR < ptTemp.X) nR = ptTemp.X;
								if (nB < ptTemp.Y) nB = ptTemp.Y;
							}
						}
						if (ptTemp.Y + 1 < matSrc.Height)
						{
							if (barrVisited[ptTemp.Y + 1, ptTemp.X] == false && imgarr[ptTemp.Y + 1, ptTemp.X, 0] != 0)
							{
								barrVisited[ptTemp.Y + 1, ptTemp.X] = true;
								imgarr[ptTemp.Y + 1, ptTemp.X, 0] = (byte)nPaintValue;
								q.Enqueue(new System.Drawing.Point(ptTemp.X, ptTemp.Y + 1));
								if (nL > ptTemp.X) nL = ptTemp.X;
								if (nT > ptTemp.Y) nT = ptTemp.Y;
								if (nR < ptTemp.X) nR = ptTemp.X;
								if (nB < ptTemp.Y) nB = ptTemp.Y;
							}
						}
						if (ptTemp.X - 1 >= 0)
						{
							if (barrVisited[ptTemp.Y, ptTemp.X - 1] == false && imgarr[ptTemp.Y, ptTemp.X - 1, 0] != 0)
							{
								barrVisited[ptTemp.Y, ptTemp.X - 1] = true;
								imgarr[ptTemp.Y, ptTemp.X - 1, 0] = (byte)nPaintValue;
								q.Enqueue(new System.Drawing.Point(ptTemp.X - 1, ptTemp.Y));
								if (nL > ptTemp.X) nL = ptTemp.X;
								if (nT > ptTemp.Y) nT = ptTemp.Y;
								if (nR < ptTemp.X) nR = ptTemp.X;
								if (nB < ptTemp.Y) nB = ptTemp.Y;
							}
						}
					}
				}
				else
				{
					while (q.Count != 0)
					{
						System.Drawing.Point ptTemp = q.Dequeue();
						// 좌상,상,우상,우,우하,하,좌하,좌
						for (int y = -1; y <= 1; y++)
						{
							for (int x = -1; x <= 1; x++)
							{
								if (ptTemp.X + x >= 0 && ptTemp.X + x < matSrc.Width && ptTemp.Y + y >= 0 && ptTemp.Y + y < matSrc.Height)
								{
									if (barrVisited[ptTemp.Y + y, ptTemp.X + x] == false && imgarr[ptTemp.Y + y, ptTemp.X + x, 0] != 0)
									{
										barrVisited[ptTemp.Y + y, ptTemp.X + x] = true;
										imgarr[ptTemp.Y + y, ptTemp.X + x, 0] = (byte)nPaintValue;
										q.Enqueue(new System.Drawing.Point(ptTemp.X + x, ptTemp.Y + y));
										if (nL > ptTemp.X) nL = ptTemp.X;
										if (nT > ptTemp.Y) nT = ptTemp.Y;
										if (nR < ptTemp.X) nR = ptTemp.X;
										if (nB < ptTemp.Y) nB = ptTemp.Y;
									}
								}
							}
						}
					}
				}
				crtBoundingBox = new CRect(nL, nT, nR, nB);
				Image<Gray, byte> imgResult = new Image<Gray, byte>(imgarr);
				Mat matResult = imgResult.Mat;
				return matResult;
			}
		}
		#endregion

		#region Pellicle Shift & Rotation 검사
		public class Run_PellicleShiftAndRotation : ModuleRunBase
		{
			MainVision m_module;
			public int m_nLeftFrameScanLine = 0;
			public int m_nRightFrameScanLine = 1;
			public int m_nFrameheight = 5;

			public int m_nReticleEdgeThreshold = 20;
			public int m_nFrameEdgeThreshold = 40;
			public int m_nSearchArea = 100;

			public double m_dNGSpecDistance_mm = 0.3;
			public double m_dNGSpecDegree = 0.5;

			public CPoint m_cptReticleEdgeTLROI = new CPoint();
			public CPoint m_cptReticleEdgeTRROI = new CPoint();
			public CPoint m_cptReticleEdgeRTROI = new CPoint();
			public CPoint m_cptReticleEdgeRBROI = new CPoint();
			public CPoint m_cptReticleEdgeBRROI = new CPoint();
			public CPoint m_cptReticleEdgeBLROI = new CPoint();
			public CPoint m_cptReticleEdgeLBROI = new CPoint();
			public CPoint m_cptReticleEdgeLTROI = new CPoint();

			public CPoint m_cptFrameEdgeTLROI = new CPoint();
			public CPoint m_cptFrameEdgeTRROI = new CPoint();
			public CPoint m_cptFrameEdgeRTROI = new CPoint();
			public CPoint m_cptFrameEdgeRBROI = new CPoint();
			public CPoint m_cptFrameEdgeBRROI = new CPoint();
			public CPoint m_cptFrameEdgeBLROI = new CPoint();
			public CPoint m_cptFrameEdgeLBROI = new CPoint();
			public CPoint m_cptFrameEdgeLTROI = new CPoint();

			public Run_PellicleShiftAndRotation(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_PellicleShiftAndRotation run = new Run_PellicleShiftAndRotation(m_module);
				run.m_nLeftFrameScanLine = m_nLeftFrameScanLine;
				run.m_nRightFrameScanLine = m_nRightFrameScanLine;
				run.m_nFrameheight = m_nFrameheight;

				run.m_nReticleEdgeThreshold = m_nReticleEdgeThreshold;
				run.m_nFrameEdgeThreshold = m_nFrameEdgeThreshold;
				run.m_nSearchArea = m_nSearchArea;

				run.m_dNGSpecDistance_mm = m_dNGSpecDistance_mm;
				run.m_dNGSpecDegree = m_dNGSpecDegree;

				run.m_cptReticleEdgeTLROI = m_cptReticleEdgeTLROI;
				run.m_cptReticleEdgeTRROI = m_cptReticleEdgeTRROI;
				run.m_cptReticleEdgeRTROI = m_cptReticleEdgeRTROI;
				run.m_cptReticleEdgeRBROI = m_cptReticleEdgeRBROI;
				run.m_cptReticleEdgeBRROI = m_cptReticleEdgeBRROI;
				run.m_cptReticleEdgeBLROI = m_cptReticleEdgeBLROI;
				run.m_cptReticleEdgeLBROI = m_cptReticleEdgeLBROI;
				run.m_cptReticleEdgeLTROI = m_cptReticleEdgeLTROI;

				run.m_cptFrameEdgeTLROI = m_cptFrameEdgeTLROI;
				run.m_cptFrameEdgeTRROI = m_cptFrameEdgeTRROI;
				run.m_cptFrameEdgeRTROI = m_cptFrameEdgeRTROI;
				run.m_cptFrameEdgeRBROI = m_cptFrameEdgeRBROI;
				run.m_cptFrameEdgeBRROI = m_cptFrameEdgeBRROI;
				run.m_cptFrameEdgeBLROI = m_cptFrameEdgeBLROI;
				run.m_cptFrameEdgeLBROI = m_cptFrameEdgeLBROI;
				run.m_cptFrameEdgeLTROI = m_cptFrameEdgeLTROI;

				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_nLeftFrameScanLine = tree.Set(m_nLeftFrameScanLine, m_nLeftFrameScanLine, "Left Frame Scan Line Number", "Left Frame Scan Line Number", bVisible);
				m_nRightFrameScanLine = tree.Set(m_nRightFrameScanLine, m_nRightFrameScanLine, "Right Frame Scan Line Number", "Right Frame Scan Line Number", bVisible);
				m_nFrameheight = tree.Set(m_nFrameheight, m_nFrameheight, "Frame Height [mm]", "Frame Height [mm]", bVisible);

				m_nReticleEdgeThreshold = tree.Set(m_nReticleEdgeThreshold, m_nReticleEdgeThreshold, "Reticle Edge Threshold", "Reticle Edge Threshold", bVisible);
				m_nFrameEdgeThreshold = tree.Set(m_nFrameEdgeThreshold, m_nFrameEdgeThreshold, "Frame Edge Threshold", "Frame Edge Threshold", bVisible);
				m_nSearchArea = tree.Set(m_nSearchArea, m_nSearchArea, "Search Area", "Search Area", bVisible);

				m_dNGSpecDistance_mm = tree.Set(m_dNGSpecDistance_mm, m_dNGSpecDistance_mm, "Distance NG Spec [mm]", "Distance NG Spec [mm]", bVisible);
				m_dNGSpecDegree = tree.Set(m_dNGSpecDegree, m_dNGSpecDegree, "Degree NG Spec", "Degree NG Spec", bVisible);

				m_cptReticleEdgeTLROI = tree.Set(m_cptReticleEdgeTLROI, m_cptReticleEdgeTLROI, "TL Reticle Edge", "TL Reticle Edge", bVisible);
				m_cptReticleEdgeTRROI = tree.Set(m_cptReticleEdgeTRROI, m_cptReticleEdgeTRROI, "TR Reticle Edge", "TR Reticle Edge", bVisible);
				m_cptReticleEdgeRTROI = tree.Set(m_cptReticleEdgeRTROI, m_cptReticleEdgeRTROI, "RT Reticle Edge", "RT Reticle Edge", bVisible);
				m_cptReticleEdgeRBROI = tree.Set(m_cptReticleEdgeRBROI, m_cptReticleEdgeRBROI, "RB Reticle Edge", "RB Reticle Edge", bVisible);
				m_cptReticleEdgeBRROI = tree.Set(m_cptReticleEdgeBRROI, m_cptReticleEdgeBRROI, "BR Reticle Edge", "BR Reticle Edge", bVisible);
				m_cptReticleEdgeBLROI = tree.Set(m_cptReticleEdgeBLROI, m_cptReticleEdgeBLROI, "BL Reticle Edge", "BL Reticle Edge", bVisible);
				m_cptReticleEdgeLBROI = tree.Set(m_cptReticleEdgeLBROI, m_cptReticleEdgeLBROI, "LB Reticle Edge", "LB Reticle Edge", bVisible);
				m_cptReticleEdgeLTROI = tree.Set(m_cptReticleEdgeLTROI, m_cptReticleEdgeLTROI, "LT Reticle Edge", "LT Reticle Edge", bVisible);

				m_cptFrameEdgeTLROI = tree.Set(m_cptFrameEdgeTLROI, m_cptFrameEdgeTLROI, "TL Frame Edge", "TL Frame Edge", bVisible);
				m_cptFrameEdgeTRROI = tree.Set(m_cptFrameEdgeTRROI, m_cptFrameEdgeTRROI, "TR Frame Edge", "TR Frame Edge", bVisible);
				m_cptFrameEdgeRTROI = tree.Set(m_cptFrameEdgeRTROI, m_cptFrameEdgeRTROI, "RT Frame Edge", "RT Frame Edge", bVisible);
				m_cptFrameEdgeRBROI = tree.Set(m_cptFrameEdgeRBROI, m_cptFrameEdgeRBROI, "RB Frame Edge", "RB Frame Edge", bVisible);
				m_cptFrameEdgeBRROI = tree.Set(m_cptFrameEdgeBRROI, m_cptFrameEdgeBRROI, "BR Frame Edge", "BR Frame Edge", bVisible);
				m_cptFrameEdgeBLROI = tree.Set(m_cptFrameEdgeBLROI, m_cptFrameEdgeBLROI, "BL Frame Edge", "BL Frame Edge", bVisible);
				m_cptFrameEdgeLBROI = tree.Set(m_cptFrameEdgeLBROI, m_cptFrameEdgeLBROI, "LB Frame Edge", "LB Frame Edge", bVisible);
				m_cptFrameEdgeLTROI = tree.Set(m_cptFrameEdgeLTROI, m_cptFrameEdgeLTROI, "LT Frame Edge", "LT Frame Edge", bVisible);
			}

			public override string Run()
			{
				MemoryData mem = m_module.m_engineer.GetMemory(App.mPool, App.mGroup, App.mMainMem);
				VectorOfPoint contour = new VectorOfPoint();
				double dReticleAngle = 0;
				double dFrameAngle = 0;

				m_module.p_bPellicleShiftPass = true;
				m_module.p_nPellicleShiftProgressValue = 0;
				m_module.p_nPellicleShiftProgressMin = 0;
				m_module.p_nPellicleShiftProgressMax = 16;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);

				// Reticle Edge
				CRect crtReticleEdgeTL = new CRect(new CPoint(m_cptReticleEdgeTLROI.X - (m_nSearchArea / 2), m_cptReticleEdgeTLROI.Y - (m_nSearchArea / 2)),
												   new CPoint(m_cptReticleEdgeTLROI.X + (m_nSearchArea / 2), m_cptReticleEdgeTLROI.Y + (m_nSearchArea / 2)));
				CRect crtReticleEdgeTR = new CRect(new CPoint(m_cptReticleEdgeTRROI.X - (m_nSearchArea / 2), m_cptReticleEdgeTRROI.Y - (m_nSearchArea / 2)),
												   new CPoint(m_cptReticleEdgeTRROI.X + (m_nSearchArea / 2), m_cptReticleEdgeTRROI.Y + (m_nSearchArea / 2)));
				CRect crtReticleEdgeRT = new CRect(new CPoint(m_cptReticleEdgeRTROI.X - (m_nSearchArea / 2), m_cptReticleEdgeRTROI.Y - (m_nSearchArea / 2)),
												   new CPoint(m_cptReticleEdgeRTROI.X + (m_nSearchArea / 2), m_cptReticleEdgeRTROI.Y + (m_nSearchArea / 2)));
				CRect crtReticleEdgeRB = new CRect(new CPoint(m_cptReticleEdgeRBROI.X - (m_nSearchArea / 2), m_cptReticleEdgeRBROI.Y - (m_nSearchArea / 2)),
												   new CPoint(m_cptReticleEdgeRBROI.X + (m_nSearchArea / 2), m_cptReticleEdgeRBROI.Y + (m_nSearchArea / 2)));
				CRect crtReticleEdgeBR = new CRect(new CPoint(m_cptReticleEdgeBRROI.X - (m_nSearchArea / 2), m_cptReticleEdgeBRROI.Y - (m_nSearchArea / 2)),
												   new CPoint(m_cptReticleEdgeBRROI.X + (m_nSearchArea / 2), m_cptReticleEdgeBRROI.Y + (m_nSearchArea / 2)));
				CRect crtReticleEdgeBL = new CRect(new CPoint(m_cptReticleEdgeBLROI.X - (m_nSearchArea / 2), m_cptReticleEdgeBLROI.Y - (m_nSearchArea / 2)),
												   new CPoint(m_cptReticleEdgeBLROI.X + (m_nSearchArea / 2), m_cptReticleEdgeBLROI.Y + (m_nSearchArea / 2)));
				CRect crtReticleEdgeLB = new CRect(new CPoint(m_cptReticleEdgeLBROI.X - (m_nSearchArea / 2), m_cptReticleEdgeLBROI.Y - (m_nSearchArea / 2)),
												   new CPoint(m_cptReticleEdgeLBROI.X + (m_nSearchArea / 2), m_cptReticleEdgeLBROI.Y + (m_nSearchArea / 2)));
				CRect crtReticleEdgeLT = new CRect(new CPoint(m_cptReticleEdgeLTROI.X - (m_nSearchArea / 2), m_cptReticleEdgeLTROI.Y - (m_nSearchArea / 2)),
												   new CPoint(m_cptReticleEdgeLTROI.X + (m_nSearchArea / 2), m_cptReticleEdgeLTROI.Y + (m_nSearchArea / 2)));

				System.Drawing.Point[] ptsReticleEdge = new System.Drawing.Point[8];
				int nTL = m_module.GetEdge(mem, crtReticleEdgeTL, m_nSearchArea / 2, eSearchDirection.TopToBottom, m_nReticleEdgeThreshold, true);
				ptsReticleEdge[0] = new System.Drawing.Point(m_cptReticleEdgeTLROI.X, m_cptReticleEdgeTLROI.Y - (m_nSearchArea / 2) + nTL);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				int nTR = m_module.GetEdge(mem, crtReticleEdgeTR, m_nSearchArea / 2, eSearchDirection.TopToBottom, m_nReticleEdgeThreshold, true);
				ptsReticleEdge[1] = new System.Drawing.Point(m_cptReticleEdgeTRROI.X, m_cptReticleEdgeTRROI.Y - (m_nSearchArea / 2) + nTR);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				int nRT = m_module.GetEdge(mem, crtReticleEdgeRT, m_nSearchArea / 2, eSearchDirection.RightToLeft, m_nReticleEdgeThreshold, true);
				ptsReticleEdge[2] = new System.Drawing.Point(m_cptReticleEdgeRTROI.X - (m_nSearchArea / 2) + nRT, m_cptReticleEdgeRTROI.Y);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				int nRB = m_module.GetEdge(mem, crtReticleEdgeRB, m_nSearchArea / 2, eSearchDirection.RightToLeft, m_nReticleEdgeThreshold, true);
				ptsReticleEdge[3] = new System.Drawing.Point(m_cptReticleEdgeRBROI.X - (m_nSearchArea / 2) + nRB, m_cptReticleEdgeRBROI.Y);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				int nBR = m_module.GetEdge(mem, crtReticleEdgeBR, m_nSearchArea / 2, eSearchDirection.BottomToTop, m_nReticleEdgeThreshold, true);
				ptsReticleEdge[4] = new System.Drawing.Point(m_cptReticleEdgeBRROI.X, m_cptReticleEdgeBRROI.Y - (m_nSearchArea / 2) + nBR);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				int nBL = m_module.GetEdge(mem, crtReticleEdgeBL, m_nSearchArea / 2, eSearchDirection.BottomToTop, m_nReticleEdgeThreshold, true);
				ptsReticleEdge[5] = new System.Drawing.Point(m_cptReticleEdgeBLROI.X, m_cptReticleEdgeBLROI.Y - (m_nSearchArea / 2) + nBL);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				int nLB = m_module.GetEdge(mem, crtReticleEdgeLB, m_nSearchArea / 2, eSearchDirection.LeftToRight, m_nReticleEdgeThreshold, true);
				ptsReticleEdge[6] = new System.Drawing.Point(m_cptReticleEdgeLBROI.X - (m_nSearchArea / 2) + nLB, m_cptReticleEdgeLBROI.Y);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				int nLT = m_module.GetEdge(mem, crtReticleEdgeLT, m_nSearchArea / 2, eSearchDirection.LeftToRight, m_nReticleEdgeThreshold, true);
				ptsReticleEdge[7] = new System.Drawing.Point(m_cptReticleEdgeLTROI.X - (m_nSearchArea / 2) + nLT, m_cptReticleEdgeLTROI.Y);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);

				contour.Push(ptsReticleEdge);
				RotatedRect rtReticleEdge = CvInvoke.MinAreaRect(contour);
				dReticleAngle = rtReticleEdge.Angle;
				while (true)
				{
					if (dReticleAngle <= 10 && dReticleAngle >= -10)
					{
						break;
					}
					else if (dReticleAngle > 10)
					{
						dReticleAngle -= 90;
					}
					else if (dReticleAngle < -10)
					{
						dReticleAngle += 90;
					}
				}

				contour.Clear();

				// Frame Edge
				CRect crtFrameEdgeTL = new CRect(new CPoint(m_cptFrameEdgeTLROI.X - (m_nSearchArea / 2), m_cptFrameEdgeTLROI.Y - (m_nSearchArea / 2)),
												 new CPoint(m_cptFrameEdgeTLROI.X + (m_nSearchArea / 2), m_cptFrameEdgeTLROI.Y + (m_nSearchArea / 2)));
				CRect crtFrameEdgeTR = new CRect(new CPoint(m_cptFrameEdgeTRROI.X - (m_nSearchArea / 2), m_cptFrameEdgeTRROI.Y - (m_nSearchArea / 2)),
												 new CPoint(m_cptFrameEdgeTRROI.X + (m_nSearchArea / 2), m_cptFrameEdgeTRROI.Y + (m_nSearchArea / 2)));
				CRect crtFrameEdgeRT = new CRect(new CPoint(m_cptFrameEdgeRTROI.X - (m_nSearchArea / 2), m_cptFrameEdgeRTROI.Y - (m_nSearchArea / 2)),
												 new CPoint(m_cptFrameEdgeRTROI.X + (m_nSearchArea / 2), m_cptFrameEdgeRTROI.Y + (m_nSearchArea / 2)));
				CRect crtFrameEdgeRB = new CRect(new CPoint(m_cptFrameEdgeRBROI.X - (m_nSearchArea / 2), m_cptFrameEdgeRBROI.Y - (m_nSearchArea / 2)),
												 new CPoint(m_cptFrameEdgeRBROI.X + (m_nSearchArea / 2), m_cptFrameEdgeRBROI.Y + (m_nSearchArea / 2)));
				CRect crtFrameEdgeBR = new CRect(new CPoint(m_cptFrameEdgeBRROI.X - (m_nSearchArea / 2), m_cptFrameEdgeBRROI.Y - (m_nSearchArea / 2)),
												 new CPoint(m_cptFrameEdgeBRROI.X + (m_nSearchArea / 2), m_cptFrameEdgeBRROI.Y + (m_nSearchArea / 2)));
				CRect crtFrameEdgeBL = new CRect(new CPoint(m_cptFrameEdgeBLROI.X - (m_nSearchArea / 2), m_cptFrameEdgeBLROI.Y - (m_nSearchArea / 2)),
												 new CPoint(m_cptFrameEdgeBLROI.X + (m_nSearchArea / 2), m_cptFrameEdgeBLROI.Y + (m_nSearchArea / 2)));
				CRect crtFrameEdgeLB = new CRect(new CPoint(m_cptFrameEdgeLBROI.X - (m_nSearchArea / 2), m_cptFrameEdgeLBROI.Y - (m_nSearchArea / 2)),
												 new CPoint(m_cptFrameEdgeLBROI.X + (m_nSearchArea / 2), m_cptFrameEdgeLBROI.Y + (m_nSearchArea / 2)));
				CRect crtFrameEdgeLT = new CRect(new CPoint(m_cptFrameEdgeLTROI.X - (m_nSearchArea / 2), m_cptFrameEdgeLTROI.Y - (m_nSearchArea / 2)),
												 new CPoint(m_cptFrameEdgeLTROI.X + (m_nSearchArea / 2), m_cptFrameEdgeLTROI.Y + (m_nSearchArea / 2)));

				System.Drawing.Point[] ptsFrameEdge = new System.Drawing.Point[8];
				nTL = m_module.GetEdge(mem, crtFrameEdgeTL, m_nSearchArea / 2, eSearchDirection.BottomToTop, m_nFrameEdgeThreshold, false);
				ptsFrameEdge[0] = new System.Drawing.Point(m_cptFrameEdgeTLROI.X, m_cptFrameEdgeTLROI.Y - (m_nSearchArea / 2) + nTL);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				nTR = m_module.GetEdge(mem, crtFrameEdgeTR, m_nSearchArea / 2, eSearchDirection.BottomToTop, m_nFrameEdgeThreshold, false);
				ptsFrameEdge[1] = new System.Drawing.Point(m_cptFrameEdgeTRROI.X, m_cptFrameEdgeTRROI.Y - (m_nSearchArea / 2) + nTR);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				nRT = m_module.GetEdge(mem, crtFrameEdgeRT, m_nSearchArea / 2, eSearchDirection.LeftToRight, m_nFrameEdgeThreshold, false);
				ptsFrameEdge[2] = new System.Drawing.Point(m_cptFrameEdgeRTROI.X - (m_nSearchArea / 2) + nRT, m_cptFrameEdgeRTROI.Y);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				nRB = m_module.GetEdge(mem, crtFrameEdgeRB, m_nSearchArea / 2, eSearchDirection.LeftToRight, m_nFrameEdgeThreshold, false);
				ptsFrameEdge[3] = new System.Drawing.Point(m_cptFrameEdgeRBROI.X - (m_nSearchArea / 2) + nRB, m_cptFrameEdgeRBROI.Y);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				nBR = m_module.GetEdge(mem, crtFrameEdgeBR, m_nSearchArea / 2, eSearchDirection.TopToBottom, m_nFrameEdgeThreshold, false);
				ptsFrameEdge[4] = new System.Drawing.Point(m_cptFrameEdgeBRROI.X, m_cptFrameEdgeBRROI.Y - (m_nSearchArea / 2) + nBR);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				nBL = m_module.GetEdge(mem, crtFrameEdgeBL, m_nSearchArea / 2, eSearchDirection.TopToBottom, m_nFrameEdgeThreshold, false);
				ptsFrameEdge[5] = new System.Drawing.Point(m_cptFrameEdgeBLROI.X, m_cptFrameEdgeBLROI.Y - (m_nSearchArea / 2) + nBL);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				nLB = m_module.GetEdge(mem, crtFrameEdgeLB, m_nSearchArea / 2, eSearchDirection.RightToLeft, m_nFrameEdgeThreshold, false);
				ptsFrameEdge[6] = new System.Drawing.Point(m_cptFrameEdgeLBROI.X - (m_nSearchArea / 2) + nLB, m_cptFrameEdgeLBROI.Y);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);
				nLT = m_module.GetEdge(mem, crtFrameEdgeLT, m_nSearchArea / 2, eSearchDirection.RightToLeft, m_nFrameEdgeThreshold, false);
				ptsFrameEdge[7] = new System.Drawing.Point(m_cptFrameEdgeLTROI.X - (m_nSearchArea / 2) + nLT, m_cptFrameEdgeLTROI.Y);
				m_module.p_nPellicleShiftProgressValue++;
				if (m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin > 0)
					m_module.p_nPellicleShiftProgressPercent = (int)((double)m_module.p_nPellicleShiftProgressValue / ((double)(m_module.p_nPellicleShiftProgressMax - m_module.p_nPellicleShiftProgressMin)) * 100);

				contour.Push(ptsFrameEdge);
				RotatedRect rtFrameEdge = CvInvoke.MinAreaRect(contour);
				dFrameAngle = rtFrameEdge.Angle;
				while (true)
				{
					if (dFrameAngle <= 10 && dFrameAngle >= -10)
					{
						break;
					}
					else if (dFrameAngle > 10)
					{
						dFrameAngle -= 90;
					}
					else if (dFrameAngle < -10)
					{
						dFrameAngle += 90;
					}
				}
				contour.Clear();

				// Judgement
				double dResultDistance = m_module.GetDistanceOfTwoPoint(new CPoint((int)rtReticleEdge.Center.X, (int)rtReticleEdge.Center.Y), new CPoint((int)rtFrameEdge.Center.X, (int)rtFrameEdge.Center.Y));
				double dResultAngle = Math.Abs(dFrameAngle - dReticleAngle);

				Run_Grab moduleRunGrab = (Run_Grab)m_module.CloneModuleRun("Grab");
				if (m_dNGSpecDistance_mm < (dResultDistance * moduleRunGrab.m_dResY_um)) m_module.p_bPellicleShiftPass = false;
				if (m_dNGSpecDegree < m_module.p_dPatternShiftAngle) m_module.p_bPellicleShiftPass = false;

				m_module.p_dPellicleShiftDistance = dResultDistance * moduleRunGrab.m_dResY_um / 1000;
				m_module.p_dPellicleShiftAngle = dResultAngle;

				return "OK";
			}
		}
		#endregion

		public class Run_PellicleExpandingInspection : ModuleRunBase
		{
			MainVision m_module;
			int m_nLaserThreshold = 70;
			public int m_nNGSpec_um = 100;

			public Run_PellicleExpandingInspection(MainVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_PellicleExpandingInspection run = new Run_PellicleExpandingInspection(m_module);
				run.m_nLaserThreshold = m_nLaserThreshold;
				run.m_nNGSpec_um = m_nNGSpec_um;
				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_nLaserThreshold = tree.Set(m_nLaserThreshold, m_nLaserThreshold, "Laser Threshold [GV]", "Laser Threshold [GV]", bVisible);
				m_nNGSpec_um = tree.Set(m_nNGSpec_um, m_nNGSpec_um, "Pellicle Expanding NG Spec [um]", "Pellicle Expanding NG Spec [um]", bVisible);
			}

			public override string Run()
			{
				// variable
				Run_LADS moduleRunLADS = (Run_LADS)m_module.CloneModuleRun("LADS");
				GrabMode grabMode = m_module.GetGrabMode("LADS");
				string strPool = grabMode.m_memoryPool.p_id;
				string strGroup = grabMode.m_memoryGroup.p_id;
				string strMemory = grabMode.m_memoryData.p_id;
				MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
				int nMMPerUM = 1000;
				int nReticleSizeY_px = Convert.ToInt32(moduleRunLADS.m_nReticleSize_mm * nMMPerUM / moduleRunLADS.m_dResY_um);  // 레티클 영역의 Y픽셀 갯수
				int nCamHeight = 100;//grabMode.m_camera.GetRoiSize().Y;

				// implement
				ladsinfos.Clear();
				m_module.p_nPellicleExpandingProgressValue = 0;
				m_module.p_nPellicleExpandingProgressMin = 0;
				m_module.p_nPellicleExpandingProgressMax = grabMode.m_ScanLineNum - 1;
				if (m_module.p_nPellicleExpandingProgressMax - m_module.p_nPellicleExpandingProgressMin > 0)
					m_module.p_nPellicleExpandingProgressPercent = (int)((double)m_module.p_nPellicleExpandingProgressValue / (double)(m_module.p_nPellicleExpandingProgressMax - m_module.p_nPellicleExpandingProgressMin) * 100);
				for (int i = 0; i < grabMode.m_ScanLineNum; i++)
				{
					CalculateHeight(mem, grabMode.m_ScanStartLine + i, nReticleSizeY_px);
					m_module.p_nPellicleExpandingProgressValue = i;
					if (m_module.p_nPellicleExpandingProgressMax - m_module.p_nPellicleExpandingProgressMin > 0)
						m_module.p_nPellicleExpandingProgressPercent = (int)((double)m_module.p_nPellicleExpandingProgressValue / (double)(m_module.p_nPellicleExpandingProgressMax - m_module.p_nPellicleExpandingProgressMin) * 100);
				}
				SaveFocusMapImage(grabMode.m_ScanLineNum, nReticleSizeY_px / nCamHeight);

				return "OK";
			}
			#region Sub Function
			unsafe void CalculateHeight(MemoryData mem, int nCurrentLine, int nReticleHeight_px)
			{
				GrabMode grabMode = m_module.GetGrabMode("LADS");
				IntPtr p = mem.GetPtr();
				int nCamWidth = 100; //grabMode.m_camera.GetRoiSize().X;
				int nCamHeight = 100; //grabMode.m_camera.GetRoiSize().Y;
				int nCount = nReticleHeight_px / nCamHeight;
				LADSInfo ladsinfo = new LADSInfo(new RPoint(), 0, nCount);

				for (int i = 0; i < nCount; i++)
				{
					int nLeft = nCurrentLine * nCamWidth;
					int nTop = i * nCamHeight;
					int nRight = nLeft + nCamWidth;
					int nBottom = nTop + nCamHeight;
					CRect crtROI = new CRect(nLeft, nTop, nRight, nBottom);
					ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
					img.SetData(p, crtROI, (int)mem.W);
					ladsinfo.m_Heightinfo[i] = GetLaserHeight(img);
				}
				ladsinfos.Add(ladsinfo);
			}

			unsafe double GetLaserHeight(ImageData img)
			{
				// variable
				int nImgWidth = 100; //grabMode.m_camera.GetRoiSize().X;
				int nImgHeight = 100; //grabMode.m_camera.GetRoiSize().Y;
				double[] profile = new double[nImgWidth];
				IntPtr dp = img.GetPtr();

				// implement
				// make profile
				for (int i = 0; i < nImgHeight; i++)
				{
					byte* p = (byte*)(dp + nImgWidth * i);
					long sum = 0;
					for (int j = 0; j < nImgWidth; j++)
					{
						byte b = *(p + j);
						sum += b;
					}
					profile[i] = sum / nImgWidth;
				}

				double dReturnHeight = 0.0;
				double max = 0;
				for (int k = 1; k < profile.Length - 1; k++)
				{
					if (max < profile[k])
					{
						max = profile[k];
						dReturnHeight = (profile[k] * k + profile[k - 1] * (k - 1) + profile[k + 1] * (k + 1)) / (profile[k] + profile[k - 1] + profile[k + 1]);
					}
				}

				return dReturnHeight;
			}

			#region CalculatingHeight(구)
			unsafe double CalculatingHeight_ESCH(ImageData img)
			{
				// variable
				GrabMode grabMode = m_module.GetGrabMode("LADS");
				int nImgWidth = 100; //grabMode.m_camera.GetRoiSize().X;
				int nImgHeight = 100; //grabMode.m_camera.GetRoiSize().Y;
				double[] daHeight = new double[nImgWidth];
				// implement
				byte* pSrc = (byte*)img.GetPtr().ToPointer();
				for (int x = 0; x < nImgWidth; x++, pSrc++)
				{
					byte* pSrcY = pSrc;
					int nSum = 0;
					int nYSum = 0;

					for (int y = 0; y < nImgHeight; y++, pSrcY += nImgWidth)
					{
						int b = *pSrcY;
						if (b < m_nLaserThreshold) continue;
						nSum += b;
						nYSum += b * y;
					}
					int iIndex = x;
					daHeight[iIndex] = (nSum != 0) ? ((double)nYSum / (double)nSum) : 0.0;
				}

				return GetHeightAverage(daHeight);
			}

			double GetHeightAverage(double[] daHeight)
			{
				// variable
				double dSum = 0.0;
				int nHitCount = 0;

				// implement
				for (int i = 0; i < daHeight.Length; i++)
				{
					if (daHeight[i] < double.Epsilon) continue;
					nHitCount++;
					dSum += daHeight[i];
				}
				if (nHitCount == 0) return -1;
				return dSum / nHitCount;
			}
			#endregion

			private void SaveFocusMapImage(int nX, int nY)
			{
				GrabMode grabMode = m_module.GetGrabMode("LADS");
				int thumsize = 30;
				int nCamHeight = grabMode.m_camera.GetRoiSize().Y;
				Mat ResultMat = new Mat();

				// Min-Max 값 알아내기
				int nMin = 100;
				int nMax = 0;
				for (int x = 0; x < nX; x++)
				{
					for (int y = 0; y < nY; y++)
					{
						if (ladsinfos[x].m_Heightinfo[y] < nMin && ladsinfos[x].m_Heightinfo[y] > -1) nMin = (int)ladsinfos[x].m_Heightinfo[y];
						if (ladsinfos[x].m_Heightinfo[y] > nMax) nMax = (int)ladsinfos[x].m_Heightinfo[y];
					}
				}

				//StringBuilder strBuilder = new StringBuilder();
				//for (int y = 0; y < nY; y++)
				//{
				//    for (int x = 0; x < nX; x++)
				//    {
				//        strBuilder.Append(ladsinfos[x].m_Heightinfo[y].ToString() + ",");
				//    }
				//    strBuilder.AppendLine();
				//}
				//File.WriteAllText("D:\\test.csv", strBuilder.ToString());

				for (int x = 0; x < nX; x++)
				{
					Mat Vmat = new Mat();
					for (int y = 0; y < nY; y++)
					{
						Mat ColorImg = new Mat(thumsize, thumsize, DepthType.Cv8U, 3);
						MCvScalar color = HeatColor(ladsinfos[x].m_Heightinfo[y], nMin - 2, nMax + 2);
						ColorImg.SetTo(color);

						if (y == 0)
							Vmat = ColorImg;
						else
							CvInvoke.VConcat(ColorImg, Vmat, Vmat);
					}
					if (x == 0)
						ResultMat = Vmat;
					else
						CvInvoke.HConcat(ResultMat, Vmat, ResultMat);

					CvInvoke.Imwrite(@"D:\Test\" + x + ".bmp", ResultMat);

				}
				CvInvoke.Imwrite(@"D:\FocusMap.bmp", ResultMat);

				// Image Binding
				System.Drawing.Bitmap bmp = ResultMat.Bitmap;
				if (m_module.dispatcher != null)
				{
					m_module.dispatcher.Invoke(new Action(delegate ()
					{
						m_module.p_bmpImgPellicleHeatmap = GetBitmapImageFromBitmap(bmp);
					}));
				}

				m_module.p_dPellicleExpandingMax = nMax;
				m_module.p_dPellicleExpandingMin = nMin;
				if ((nMax - nMin) * 400 / 10/*1px당 Pulse = 400*/ > m_nNGSpec_um) m_module.p_bPellicleExpandingPass = false;
				else m_module.p_bPellicleExpandingPass = true;
			}

			BitmapImage GetBitmapImageFromBitmap(System.Drawing.Bitmap bmp)
			{
				// Memory Stream 준비
				MemoryStream ms = new MemoryStream();
				bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
				ms.Position = 0;

				// BitmapImage로 변환
				var bmpImg = new BitmapImage();
				bmpImg.BeginInit();
				bmpImg.StreamSource = ms;
				bmpImg.CacheOption = BitmapCacheOption.OnLoad;
				bmpImg.EndInit();

				return bmpImg;
			}

			MCvScalar HeatColor(double dValue, double dMin, double dMax)
			{
				double r = 0, g = 0, b = 0;
				double x = (dValue - dMin) / (dMax - dMin);
				//r = 255 * (-4 * Math.Abs(x - 0.75) + 2);
				//g = 255 * (-4 * Math.Abs(x - 0.50) + 2);
				//b = 255 * (-4 * Math.Abs(x) + 2);

				r = 255 * (-4 * Math.Abs(1 - 0.75) + 2);
				g = 255 * (-4 * Math.Abs(x - 0.25) + 2);
				b = 0;


				return new MCvScalar(b, g, r);
			}
			#endregion
		}
	}
}

