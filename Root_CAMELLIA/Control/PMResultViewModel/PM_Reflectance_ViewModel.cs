using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using System.Collections.ObjectModel;
using Met = Root_CAMELLIA.LibSR_Met;

namespace Root_CAMELLIA
{
    public class PM_Reflectance_ViewModel : ObservableObject
    {
        public PM_Reflectance_ViewModel()
        {
            ResultGraph();

        }
        ObservableCollection<ReviewGraph> PMResultGraph500 = new ObservableCollection<ReviewGraph>();
        public ObservableCollection<ReviewGraph> p_PMResultGraph500
        {
            get
            {
                return PMResultGraph500;
            }
            set
            {
                SetProperty(ref PMResultGraph500, value);
            }
        }
        ObservableCollection<ReviewGraph> PMResultGraph740 = new ObservableCollection<ReviewGraph>();
        public ObservableCollection<ReviewGraph> p_PMResultGraph740
        {
            get
            {
                return PMResultGraph740;
            }
            set
            {
                SetProperty(ref PMResultGraph740, value);
            }
        }
        ObservableCollection<ReviewGraph> PMResultGraph1100 = new ObservableCollection<ReviewGraph>();
        public ObservableCollection<ReviewGraph> p_PMResultGraph1100
        {
            get
            {
                return PMResultGraph1100;
            }
            set
            {
                SetProperty(ref PMResultGraph1100, value);
            }
        }

        ReviewGraph PMReflectance500 = new ReviewGraph();
        public ReviewGraph p_PMReflectance500
        {
            get
            {
                return PMReflectance500;
            }
            set
            {
                SetProperty(ref PMReflectance500, value);
            }
        }
        ReviewGraph PMReflectance740 = new ReviewGraph();
        public ReviewGraph p_PMReflectance740
        {
            get
            {
                return PMReflectance740;
            }
            set
            {
                SetProperty(ref PMReflectance740, value);
            }
        }
        ReviewGraph PMReflectance1100 = new ReviewGraph();
        public ReviewGraph p_PMReflectance1100
        {
            get
            {
                return PMReflectance1100;
            }
            set
            {
                SetProperty(ref PMReflectance1100, value);
            }
        }

        public void ResultGraph()
        {
            PMResultGraph500.Add(PMReflectance500);
            PMResultGraph740.Add(PMReflectance740);
            PMResultGraph1100.Add(PMReflectance1100);
            
        }

        public void DrawPMGraph()
        {
            Met.PMDatas m_PMData = new Met.PMDatas();
            double [] nRepeatCount = new double[m_PMData.nSensorTiltRepeatNum];
            for (int i = 0; i < m_PMData.nSensorTiltRepeatNum; i++)
            {
                nRepeatCount[i] = i+1;
            }
            //p_PMReflectance500.DrawReviewGraph("500", "Count","Diff [%]", nRepeatCount, m_PMData.DiffReflectnace500);

            //p_PMReflectance740.DrawReviewGraph("740", "Count", "Diff [%]",  nRepeatCount, m_PMData.DiffReflectnace740);
            
            //p_PMReflectance1100.DrawReviewGraph("1100", "Count", "Diff [%]", nRepeatCount, m_PMData.DiffReflectnace1100);
        }
    }
}
