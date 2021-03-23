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
		public int m_nImageHeight = 1000;

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
			dst.m_nImageHeight = src.m_nImageHeight;
			return dst;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bReadOnly)
		{
			base.RunTree(tree, bVisible, bReadOnly);
			RunTreeEdge(tree, bVisible, bReadOnly);
		}

		private void RunTreeEdge(Tree tree, bool bVisible, bool bReadOnly)
		{
			m_nImageHeight = tree.Set(m_nImageHeight, m_nImageHeight, "Image Height", "전체 Image Height", bVisible);
		}
	}
}
