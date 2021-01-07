using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class EdgeSurfaceParameter : ParameterBase, IMaskInspection, IColorInspection
	{
		public int MaskIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public IMAGE_CHANNEL IndexChannel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public override object Clone()
		{
			throw new NotImplementedException();
		}
	}
}
