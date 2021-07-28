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
	public class GrabModeBack : GrabModeBase
	{
		public RPoint m_rpAxisCenter = new RPoint();    // Wafer Center Position
		public int m_nFocusPosZ = 0;                    // Focus Position Z

		public int m_ScanLineNum = 1;
		public int m_ScanStartLine = 0;

		public bool m_bUseBiDirectionScan = false;
		public int m_nReverseOffsetY = 800;

		public bool m_bUseLADS = false;

		public GrabModeBack(string id, CameraSet cameraSet, LightSet lightSet, MemoryPool memoryPool, LensLinearTurret lensTurret = null) : base(id, cameraSet, lightSet, memoryPool, lensTurret)
		{
			p_id = id;
			p_sName = id;
			m_cameraSet = cameraSet;
			m_lightSet = lightSet;
			m_memoryPool = memoryPool;
		}

		public override object Copy(object grabMode)
		{
			GrabModeFront src = (GrabModeFront)grabMode;
			GrabModeFront dst = (GrabModeFront)base.Copy(src);

			dst.m_rpAxisCenter = new RPoint(src.m_rpAxisCenter);
			dst.m_nFocusPosZ = src.m_nFocusPosZ;

			dst.m_ScanLineNum = src.m_ScanLineNum;
			dst.m_ScanStartLine = src.m_ScanStartLine;

			dst.m_bUseBiDirectionScan = src.m_bUseBiDirectionScan;
			dst.m_nReverseOffsetY = src.m_nReverseOffsetY;

			dst.m_bUseLADS = src.m_bUseLADS;
			
			return dst;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bReadOnly)
		{
			base.RunTree(tree, bVisible, bReadOnly);
			RunTreeBack(tree, bVisible, bReadOnly);
			//RunTreeLADS(tree.GetTree("LADS", false), bVisible, bReadOnly);
		}

		public void RunTreeBack(Tree tree, bool bVisible, bool bReadOnly)
		{
			m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
			m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);

			m_ScanLineNum = tree.Set(m_ScanLineNum, m_ScanLineNum, "Scan Line Number", "Scan Line Number");
			m_ScanStartLine = tree.Set(m_ScanStartLine, m_ScanStartLine, "Scan Start Line", "Scan Start Line");

			m_GD.nUserSet = tree.Set(m_GD.nUserSet, m_GD.nUserSet, "UserSet", "UserSet 1~10", bVisible);

			m_bUseBiDirectionScan = tree.Set(m_bUseBiDirectionScan, false, "Use BiDirectionScan", "Bi Direction Scan Use");
			m_nReverseOffsetY = tree.Set(m_nReverseOffsetY, 800, "ReverseOffsetY", "Reverse Scan 동작시 Y 이미지 Offset 설정");
		}

		public void RunTreeLADS(Tree tree, bool bVisible = true, bool bReadOnly = false)
		{
			m_bUseLADS = tree.Set(m_bUseLADS, m_bUseLADS, "Use LADS", "LADS 사용 여부");
		}
	}
}
