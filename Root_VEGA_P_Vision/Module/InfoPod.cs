using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision.Module
{
    public class InfoPod : NotifyProperty
    {
        public enum ePod
        {
            EOP_Door,
            EIP_Plate,
            EIP_Cover,
            EOP_Dome
        }
        public ePod p_ePod { get; set; }

        bool _bTurn = false;
        public bool p_bTurn
        {
            get { return _bTurn; }
            set
            {
                _bTurn = value;
                OnPropertyChanged();
            }
        }

        public InfoPod(ePod ePod)
        {
            p_ePod = ePod; 
        }
    }
}
