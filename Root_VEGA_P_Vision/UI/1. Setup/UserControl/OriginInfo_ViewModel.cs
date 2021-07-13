using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class OriginInfo_ViewModel:ObservableObject
    {
        public OriginInfo_UI Main;
        string header;
        OriginInfo originInfo;
        public string Header
        {
            get => header;
            set => SetProperty(ref header, value);
        }
        public OriginInfo OriginInfo
        {
            get => originInfo;
            set => SetProperty(ref originInfo, value);
        }
        public OriginInfo_ViewModel(OriginInfo OriginInfo, string header)
        {
            this.OriginInfo = OriginInfo;
            Main = new OriginInfo_UI();
            Main.DataContext = this;
            this.header = header;
        }

    }
}
