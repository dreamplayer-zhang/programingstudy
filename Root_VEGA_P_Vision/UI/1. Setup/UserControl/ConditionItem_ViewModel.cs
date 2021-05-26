using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class ConditionItem_ViewModel:ObservableObject
    {
        int conditioncnt;
        string defectCode, defectName;
        bool isEnable;
        string memstr;
        public ConditionItem Main;
        #region Property
        public int ConditionCnt
        {
            get => conditioncnt;
            set => SetProperty(ref conditioncnt, value);
        }
        public bool IsEnable
        {
            get => isEnable;
            set => SetProperty(ref isEnable, value);
        }
        public string DefectCode
        {
            get => defectCode;
            set => SetProperty(ref defectCode, value);
        }
        public string DefectName
        {
            get => defectName;
            set => SetProperty(ref defectName, value);
        }
        #endregion

        public ConditionItem_ViewModel(int Num,string memstr)
        {
            Main = new ConditionItem();
            Main.DataContext = this;
            ConditionCnt = Num;
            this.memstr = memstr;
        }
        public ICommand btnImagenROI
        {
            get => new RelayCommand(() => {
                VegaPEventManager.OnImageROIBtnClicked(this, new ImageROIEventArgs(memstr));
            });
        }
    }
}
