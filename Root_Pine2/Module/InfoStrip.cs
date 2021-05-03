using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_Pine2.Module
{
    public class InfoStrip : NotifyProperty
    {
        public string p_id { get; set; }
        public InfoStrip(string id)
        {
            p_id = id; 
        }
    }
}
