using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using RootTools.Module;

namespace Root_WIND2
{
    public class WIND2: ModuleBase
    {
		public TK4SGroup m_tk4s;

		public override void GetTools(bool bInit)
		{
			m_toolBox.Get(ref m_tk4s, this, "FDC", ProgramManager.Instance.DialogService);
		}

		public WIND2(string id, IEngineer engineer)
		{
			base.InitBase(id, engineer);
		}
	}
}
