using RootTools.Camera;
using RootTools.Lens.LinearTurret;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools
{
	public class GrabModeFront : GrabModeBase
	{
		public RPoint m_rpAxisCenter = new RPoint();    // Wafer Center Position
		public RPoint m_ptXYAlignData = new RPoint(0, 0);
		public int m_nFocusPosZ = 0;                    // Focus Position Z

		public int m_ScanLineNum = 1;
		public int m_ScanStartLine = 0;

		public bool m_bUseBiDirectionScan = false;
		public int m_nReverseOffsetY = 800;

		public double m_dTDIToVRSOffsetX = 0;
		public double m_dTDIToVRSOffsetY = 0;
		public double m_dTDIToVRSOffsetZ = 0;
		public double m_dVRSFocusPos = 0;
		
		public bool m_bUseLADS = false;

		public LensLinearTurret m_lens = null;
		public string m_sLens = "";
		bool m_bUseRADS = false;
		public bool pUseRADS
		{
			get
			{
				return m_bUseRADS;
			}
			set
			{
				m_bUseRADS = value;
			}
		}

		public GrabModeFront(string id, CameraSet cameraSet, LightSet lightSet, MemoryPool memoryPool, LensLinearTurret lensTurret = null) : base(id, cameraSet, lightSet, memoryPool, lensTurret)
		{
			p_id = id;
			p_sName = id;
			m_cameraSet = cameraSet;
			m_lightSet = lightSet;
			m_memoryPool = memoryPool;
			m_lens = lensTurret;
		}

		public override object Copy(object grabMode)
		{
			GrabModeFront src = (GrabModeFront)grabMode;
			GrabModeFront dst = (GrabModeFront)base.Copy(src);
			
			dst.m_rpAxisCenter = new RPoint(src.m_rpAxisCenter);
			dst.m_ptXYAlignData = new RPoint(src.m_ptXYAlignData);
			dst.m_nFocusPosZ = src.m_nFocusPosZ;

			dst.m_ScanLineNum = src.m_ScanLineNum;
			dst.m_ScanStartLine = src.m_ScanStartLine;

			dst.m_bUseBiDirectionScan = src.m_bUseBiDirectionScan;
			dst.m_nReverseOffsetY = src.m_nReverseOffsetY;

			dst.m_dTDIToVRSOffsetX = src.m_dTDIToVRSOffsetX;
			dst.m_dTDIToVRSOffsetY = src.m_dTDIToVRSOffsetY;
			dst.m_dTDIToVRSOffsetZ = src.m_dTDIToVRSOffsetZ;
			dst.m_dVRSFocusPos = src.m_dVRSFocusPos;

			dst.m_bUseLADS = src.m_bUseLADS;
			dst.m_lens = src.m_lens;
			dst.m_sLens = src.m_sLens;

			return dst;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bReadOnly)
		{
			base.RunTree(tree, bVisible, bReadOnly);
			RunTreeFront(tree, bVisible, bReadOnly);
			RunTreeVRS(tree.GetTree("VRS", false), bVisible, bReadOnly);
			RunTreeLADS(tree.GetTree("LADS", false), bVisible, bReadOnly);
			RunTreeLens(tree.GetTree("Lens", false), bVisible, bReadOnly);
		}

		private void RunTreeFront(Tree tree, bool bVisible, bool bReadOnly)
		{
			m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
			m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);

			m_ScanLineNum = tree.Set(m_ScanLineNum, m_ScanLineNum, "Scan Line Number", "Scan Line Number");
			m_ScanStartLine = tree.Set(m_ScanStartLine, m_ScanStartLine, "Scan Start Line", "Scan Start Line");

			m_GD.nUserSet = tree.Set(m_GD.nUserSet, m_GD.nUserSet, "UserSet", "UserSet 1~10", bVisible);
			m_bUseBiDirectionScan = tree.Set(m_bUseBiDirectionScan, false, "Use BiDirectionScan", "Bi Direction Scan Use");
			m_nReverseOffsetY = tree.Set(m_nReverseOffsetY, 800, "ReverseOffsetY", "Reverse Scan 동작시 Y 이미지 Offset 설정");

			m_ptXYAlignData = tree.Set(m_ptXYAlignData, m_ptXYAlignData, "XY Align Data", "XY Align Data", bVisible, true);
		}

		private void RunTreeVRS(Tree tree, bool bVisible, bool bReadOnly)
		{
			m_dTDIToVRSOffsetX = tree.Set(m_dTDIToVRSOffsetX, m_dTDIToVRSOffsetX, "TDI To VRS Offset X", "TDI To VRS Offset X");
			m_dTDIToVRSOffsetY = tree.Set(m_dTDIToVRSOffsetY, m_dTDIToVRSOffsetY, "TDI To VRS Offset Y", "TDI To VRS Offset Y");
			m_dTDIToVRSOffsetZ = tree.Set(m_dTDIToVRSOffsetZ, m_dTDIToVRSOffsetZ, "TDI To VRS Offset Z", "TDI To VRS Offset Z");
			m_dVRSFocusPos = tree.Set(m_dVRSFocusPos, m_dVRSFocusPos, "VRS Focus Z", "VRS Focus Z", bVisible, true);
		}

		private void RunTreeLADS(Tree tree, bool bVisible, bool bReadOnly)
		{
			m_bUseLADS = tree.Set(m_bUseLADS, m_bUseLADS, "Use LADS", "LADS 사용 여부");
			m_bUseRADS = tree.Set(m_bUseRADS, m_bUseRADS, "Use RADS", "RADS 사용 여부");
		}

		private void RunTreeLens(Tree tree, bool bVisible, bool bReadOnly)
		{
			if (m_lens == null)
				return;
			m_sLens = tree.Set(m_sLens, m_sLens, m_lens.p_asPos, "Lens Turret", "Turret", bVisible);
		}

		public void SetLens()
		{
			if (m_lens != null)
			{
				m_lens.StartHome();
				m_lens.WaitReady();
				System.Threading.Thread.Sleep(2000);
				m_lens.ChangePos(m_sLens);
				m_lens.WaitReady();
			}
		}
	}
}
