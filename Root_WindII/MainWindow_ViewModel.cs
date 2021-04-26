using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WindII
{
    public class MainWindow_ViewModel : ObservableObject
    {
        #region [Event]

        #endregion


        #region [ViewModel]

        private int selectedModeIndex = 0;
        public int SelectedModeIndex
        {
            get => this.selectedModeIndex;
            set
            {
                SetProperty(ref this.selectedModeIndex, value);
            }
        }

        private SelectMode_ViewModel selectModeVM;
        public SelectMode_ViewModel SelectModeVM
        {
            get => this.selectModeVM;
            set
            {
                SetProperty(ref this.selectModeVM, value);
            }
        }

        private Setup_ViewModel setupVM;
        public Setup_ViewModel SetupVM
        {
            get => this.setupVM;
            set
            {
                SetProperty(ref this.setupVM, value);
            }
        }

        #endregion

        public MainWindow_ViewModel()
        {
            this.SelectModeVM = new SelectMode_ViewModel();
            this.selectModeVM.SetupSelected += SetupSelected_Callback;
            this.selectModeVM.ReviewSelected += ReviewSelected_Callback;
            this.selectModeVM.OperationSelected += OperationSelected_Callback;

            this.SetupVM = new Setup_ViewModel();
        }

        private void SetupSelected_Callback()
        {
            this.SelectedModeIndex = 1;
        }

        private void ReviewSelected_Callback()
        {
            this.SelectedModeIndex = 2;
        }

        private void OperationSelected_Callback()
        {
            this.SelectedModeIndex = 3;
        }
    }
}
