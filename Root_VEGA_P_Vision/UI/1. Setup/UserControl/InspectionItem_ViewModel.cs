using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_VEGA_P_Vision
{
    public class InspectionItem_ViewModel:ObservableObject
    {
        public InspectionItem Main;
        InspectionOneItem_ViewModel particle, highres, stain, side;
        bool visible, bhighRes, bside;
        #region Property
        public bool bHighRes
        {
            get => bhighRes;
            set => SetProperty(ref bhighRes, value);
        }
        public bool bSide
        {
            get => bside;
            set => SetProperty(ref bside, value);
        }
        public bool Visible
        {
            get => visible;
            set => SetProperty(ref visible, value);
        }
        public InspectionOneItem_ViewModel ParticleItem
        {
            get => particle;
            set => SetProperty(ref particle, value);
        }
        public InspectionOneItem_ViewModel HighResItem
        {
            get => highres;
            set => SetProperty(ref highres, value);
        }
        public InspectionOneItem_ViewModel StainItem
        {
            get => stain;
            set => SetProperty(ref stain, value);
        }
        public InspectionOneItem_ViewModel SideItem
        {
            get => side;
            set => SetProperty(ref side, value);
        }
        #endregion
        public InspectionItem_ViewModel(string memstr, bool bhighRes = true, bool bside = true)
        {
            Main = new InspectionItem();
            Main.DataContext = this;

            string[] PartSide = memstr.Split('.');
            bHighRes = bhighRes;
            bSide = bside;
            particle = new InspectionOneItem_ViewModel("Particle",PartSide[0]+".Main."+PartSide[1]);
            highres = new InspectionOneItem_ViewModel("HighRes", PartSide[0] + ".Stack." + PartSide[1]);
            stain = new InspectionOneItem_ViewModel("Stain", PartSide[0] + ".Stain." + PartSide[1]);
            side = new InspectionOneItem_ViewModel("Side", PartSide[0] + ".Side." + PartSide[1]);
        }
    }
}
