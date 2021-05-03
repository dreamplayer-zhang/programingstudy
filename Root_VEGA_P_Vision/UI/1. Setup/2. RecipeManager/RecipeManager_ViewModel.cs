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
        public RecipeSetting_ViewModel recipeSettingVM;

        public RecipeManager_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            Init();
        }
        private void Init()
        {
            Main = new RecipeManagerPanel();
            recipeSettingVM = new RecipeSetting_ViewModel(this);
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
            get => new RelayCommand(()=> {
                m_Setup.SetRecipeSetting();
                recipeSettingVM.SetStain();
            });
        }
        public ICommand btn6um
        {
            get => new RelayCommand(()=> {
                m_Setup.SetRecipeSetting();
                recipeSettingVM.Set6um();
            });
        }
        public ICommand btn1um
        {
            get => new RelayCommand(()=> {
                m_Setup.SetRecipeSetting();
                recipeSettingVM.Set1um();
            });
        }
        public ICommand btnSide
        {
            get => new RelayCommand(()=>
            {
                m_Setup.SetRecipeSetting();
                recipeSettingVM.SetSide();
            });
        }
        public ICommand btnBack
        {
            get => new RelayCommand(() => { m_Setup.SetHome(); });
        }
        #endregion
    }
}
