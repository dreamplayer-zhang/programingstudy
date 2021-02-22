using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
    class FrontsideSummary_ViewModel : ObservableObject, IPage
    {
        public FrontsideSummary_ViewModel()
        {

        }

        public void LoadRecipe()
        {

        }

        #region [Command]
        public ICommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {
                LoadRecipe();
            });
        }

        #endregion

    }
}
