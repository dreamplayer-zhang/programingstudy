using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools
{
    public class RnRData
    {
        public List<int> SelectSlot { get; set; } = new List<int>();
        public string CarrierID { get; set; } = "";
        public string LotID { get; set; } = "";

        public void ClearData()
        {
            SelectSlot.Clear();
            CarrierID = "";
            LotID = "";
        }
    }
}
