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
using System.Data;
using System.Windows;

namespace Root_CAMELLIA
{
    public class PM_Reflectance_ViewModel : ObservableObject
    {
        public PM_Reflectance_ViewModel()
        {
            ResultGraph();
            InitData();
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

        public void DrawPMGraph(int GraphCount, Met.PMDatas PMData)
        {
            
            double[] nRepeatCount = new double[PMData.nSensorTiltRepeatNum];
            for (int i = 0; i < PMData.nSensorTiltRepeatNum; i++)
            {
                nRepeatCount[i] = i + 1;
            }
            if(GraphCount==0)
            {
                p_PMReflectance500.DrawReviewGraph(PMData.arrCheckWavelength[GraphCount].ToString() +" [nm]", "Repeat Count", "Diff [%]", nRepeatCount, PMData.m_CalPMReflectance[GraphCount].dDiffReflectance);
            }
            if (GraphCount == 1)
            {
                p_PMReflectance740.DrawReviewGraph(PMData.arrCheckWavelength[GraphCount].ToString() + " [nm]", "Repeat Count", "Diff [%]", nRepeatCount, PMData.m_CalPMReflectance[GraphCount].dDiffReflectance);
            }
            if (GraphCount == 2)
            {
                p_PMReflectance1100.DrawReviewGraph(PMData.arrCheckWavelength[GraphCount].ToString() + " [nm]", "Repeat Count", "Diff [%]", nRepeatCount, PMData.m_CalPMReflectance[GraphCount].dDiffReflectance);
            }

        }
        private DataTable _PMReflectanceResultData = new DataTable();
        public DataTable PMReflectanceResult
        {
            get
            {
                return _PMReflectanceResultData;
            }
            set
            {
                SetProperty(ref _PMReflectanceResultData, value);
            }
        }

        public void InitData()
        {
            PMReflectanceResult.Columns.Add(new DataColumn("Wavelength"));
            PMReflectanceResult.Columns.Add(new DataColumn("Min"));
            PMReflectanceResult.Columns.Add(new DataColumn("Max"));
            PMReflectanceResult.Columns.Add(new DataColumn("Cop"));
            PMReflectanceResult.Columns.Add(new DataColumn("Average"));
            PMReflectanceResult.Columns.Add(new DataColumn("STD"));
        }
        public void PMResultDataGrid(int DataCount, Met.PMDatas PMData)
        {
            DataRow row;
            if (DataCount == 0)
            {
                PMReflectanceResult.Clear();
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                row = PMReflectanceResult.NewRow();
                if (DataCount == PMData.arrCheckWavelength.Length)
                {
                    row["Wavelength"] = "Total";
                }
                else
                {
                    row["Wavelength"] = PMData.m_CalPMReflectance[DataCount].dWavelength.ToString();
                }
                row["Min"] = PMData.m_CalPMReflectance[DataCount].dMin.ToString();
                row["Max"] = PMData.m_CalPMReflectance[DataCount].dMax.ToString();
                row["Cop"] = PMData.m_CalPMReflectance[DataCount].dCop.ToString();
                row["Average"] = PMData.m_CalPMReflectance[DataCount].dAvg.ToString();
                row["STD"] = PMData.m_CalPMReflectance[DataCount].dSTD.ToString();
                PMReflectanceResult.Rows.Add(row);
            });
        }
       
    }
}
