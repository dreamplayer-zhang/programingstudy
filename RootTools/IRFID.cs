using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools
{
    public interface IRFID
    {
        string p_id { get; set; }
        //string ReadRFID(ref string sID);
        string ReadRFID();
        string m_sReadID { get; set; }
        bool m_bReadID { get; set; }
        ModuleRunBase m_runReadID { get; set; }
    }
}
