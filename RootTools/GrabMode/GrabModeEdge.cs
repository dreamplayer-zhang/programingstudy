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
	public class GrabModeEdge : GrabModeBase
	{
		public int m_nStartDegree = 0;
		public int m_nScanDegree = 360;
		public int m_nFocusX = 0;
		public int m_nFocusZ = 0;

		public int m_nCameraPositionOffset = 0;		// degree
		public int m_nCameraHeight = 2000;			// Camera 연결 안되어 있을 시, 검사에 필요한 Camera Height
		public int m_nImageHeight = 10000;

		public GrabModeEdge(string id, CameraSet cameraSet, LightSet lightSet, MemoryPool memoryPool, LensLinearTurret lensTurret = null) : base(id, cameraSet, lightSet, memoryPool, lensTurret)
		{
			p_id = id;
			p_sName = id;
			m_cameraSet = cameraSet;
			m_lightSet = lightSet;
			m_memoryPool = memoryPool;
		}

		public override object Copy(object grabMode)
		{
			GrabModeEdge src = (GrabModeEdge)grabMode;
			GrabModeEdge dst = (GrabModeEdge)base.Copy(src);
			dst.m_nStartDegree = src.m_nStartDegree;
			dst.m_nScanDegree = src.m_nScanDegree;
			dst.m_nCameraPositionOffset = src.m_nCameraPositionOffset;
			dst.m_nCameraHeight = src.m_nCameraHeight;
			dst.m_nImageHeight = src.m_nImageHeight;
			return dst;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bReadOnly)
		{
			base.RunTree(tree, bVisible, bReadOnly);
			RunTreeSnap(tree.GetTree("Snap", false), bVisible, bReadOnly);
			RunTreeEdge(tree.GetTree("Edge", false), bVisible, bReadOnly);
		}

		private void RunTreeSnap(Tree tree, bool bVisible, bool bReadOnly)
		{
			m_nStartDegree = tree.Set(m_nStartDegree, m_nStartDegree, "Start Degree", "시작 위치", bVisible);
			m_nScanDegree = tree.Set(m_nScanDegree, m_nScanDegree, "Scan Degree", "스캔 각도", bVisible);
			m_nFocusX = tree.Set(m_nFocusX, m_nFocusX, "Focus X Axis", "Focus X Axis", bVisible);
			m_nFocusZ = tree.Set(m_nFocusZ, m_nFocusZ, "Focus Z Axis", "Focus Z Axis", bVisible);

			//m_GD.nUserSet = tree.Set(m_GD.nUserSet, m_GD.nUserSet, "UserSet", "UserSet 1~10", bVisible);
		}

		private void RunTreeEdge(Tree tree, bool bVisible, bool bReadOnly)
		{
			m_nCameraPositionOffset = tree.Set(m_nCameraPositionOffset, m_nCameraPositionOffset, "Position Offset", "Camera Position Offset(Degree)", bVisible);
			
			m_nCameraHeight = tree.Set(m_nCameraHeight, m_nCameraHeight, "Camera Height", "Camera Height", bVisible);
			m_nImageHeight = tree.Set(m_nImageHeight, m_nImageHeight, "Image Height", "전체 Image Height", bVisible);
		}
	}
}
