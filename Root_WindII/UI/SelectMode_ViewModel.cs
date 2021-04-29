using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_WindII
{

    public delegate void ModeSelectedHandler();

    public class SelectMode_ViewModel : ObservableObject
    {
        #region [Event]
        public event ModeSelectedHandler SetupSelected;
        public event ModeSelectedHandler ReviewSelected;
        public event ModeSelectedHandler OperationSelected;

        public event ModeSelectedHandler EngineerSelected;

        #endregion

        public SelectMode_ViewModel()
        {

        }

        #region [Command]
        public ICommand btnSetupCommand
        {
            get => new RelayCommand(() =>
             {
                 if (this.SetupSelected != null)
                     this.SetupSelected();
             });
        }

        public ICommand btnReviewCommand
        {
            get => new RelayCommand(() =>
            {
                if (this.ReviewSelected != null)
                    this.ReviewSelected();
            });
        }

        public ICommand btnOperationCommand
        {
            get => new RelayCommand(() =>
            {
                if (this.OperationSelected != null)
                    this.OperationSelected();

            });
        }

        public ICommand btnEngineerCommand
        {
            get => new RelayCommand(() =>
            {
                if (this.EngineerSelected != null)
                    this.EngineerSelected();

            });
        }
        #endregion
    }
}
