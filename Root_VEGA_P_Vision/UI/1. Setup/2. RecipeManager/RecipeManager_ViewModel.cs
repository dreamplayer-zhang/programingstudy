using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class RecipeManager_ViewModel:ObservableObject
    {
        public Setup_ViewModel m_Setup;
        public RecipeManagerPanel Main;

        public RecipeManager_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            Init();
        }
        private void Init()
        {
            Main = new RecipeManagerPanel();

            //SetPage(Main);
        }
        public enum ModifyType
        {
            None,
            LineStart,
            LineEnd,
            ScrollAll,
            Left,
            Right,
            Top,
            Bottom,
            LeftTop,
            RightTop,
            LeftBottom,
            RightBottom,
        }

        #region [RelayCommand]
        public ICommand btnStain
        {
            get => new RelayCommand(()=> { m_Setup.SetStain(); });
        }
        public ICommand btnBack
        {
            get => new RelayCommand(() => { m_Setup.SetHome(); });
        }
        #endregion
    }
}
