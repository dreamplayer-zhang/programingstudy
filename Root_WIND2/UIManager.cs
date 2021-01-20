using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Root_WIND2
{
    public class UIManager
    {
        private UIManager()
        {
        }

        private static readonly Lazy<UIManager> instance = new Lazy<UIManager>(() => new UIManager());

        public static UIManager Instance
        {
            get
            {
                return instance.Value;
            }
        }

        public SelectMode ModeWindow { get => modeWindow; set => modeWindow = value; }
        public Setup SetupWindow { get => setupWindow; set => setupWindow = value; }
        public Review ReviewWindow { get => reviewWindow; set => reviewWindow = value; }
        public Run RunWindow { get => runWindow; set => runWindow = value; }
        public Grid MainPanel { get => mainPanel; set => mainPanel = value; }
        public Setup_ViewModel SetupViewModel { get => setupViewModel; set => setupViewModel = value; }
        internal Review_ViewModel ReviewViewModel { get => reviewViewModel; set => reviewViewModel = value; }
        internal Run_ViewModel RunViewModel { get => runViewModel; set => runViewModel = value; }
        public SettingDialog SettingDialog { get => settingDialog; set => settingDialog = value; }
        public SettingDialog_ViewModel SettingDialogViewModel { get => settingDialogViewModel; set => settingDialogViewModel = value; }



        #region WPF member
        private Grid mainPanel;
        #endregion

        #region UI
        private SelectMode modeWindow;
        private Setup setupWindow;
        private Review reviewWindow;
        private Run runWindow;

        private SettingDialog settingDialog;
        #endregion

        #region ViewModel
        private Setup_ViewModel setupViewModel;
        private Review_ViewModel reviewViewModel;
        private Run_ViewModel runViewModel;

        private SettingDialog_ViewModel settingDialogViewModel;
        #endregion

        public bool Initialize()
        {
            // Main UI
            InitModeSelect();
            InitSetupMode();
            InitReviewMode();
            InitRunMode();

            // 기타 UI
            InitSettingDialog();

            return true;
        }

        void InitModeSelect()
        {
            modeWindow = new SelectMode();
            modeWindow.Init();
        }
        void InitSetupMode()
        {
            setupWindow = new Setup();
            setupViewModel = new Setup_ViewModel();
            setupWindow.DataContext = SetupViewModel;
        }
        void InitReviewMode()
        {
            reviewWindow = new Review();
            reviewViewModel = new Review_ViewModel(reviewWindow);
            reviewWindow.DataContext = ReviewViewModel;
        }
        void InitRunMode()
        {
            runWindow = new Run();
            runViewModel = new Run_ViewModel(setupViewModel);
            runWindow.DataContext = runViewModel;
        }

        void InitSettingDialog()
        {
            settingDialog = new SettingDialog();
            settingDialogViewModel = new SettingDialog_ViewModel();
            settingDialog.DataContext = settingDialogViewModel;
        }

        public void ChangeMainUI(UIElement window)
        {
            if (window == null) return;

            MainPanel.Children.Clear();
            MainPanel.Children.Add((UIElement)window);
        }

        public void ChangeUIMode()
        {
            ChangeMainUI((UIElement)modeWindow);
        }

        public void ChangUISetup()
        {
            ChangeMainUI((UIElement)setupWindow);
        }

        public void ChangUIReview()
        {
            ChangeMainUI((UIElement)reviewWindow);
        }

        public void ChangUIRun()
        {
            ChangeMainUI((UIElement)runWindow);
        }


        #region [Recipe Method]
        //public void NewRecipe()
        //{
        //    try
        //    {

        //        if (MessageBox.Show("작성 중인 레시피(Recipe)를 저장하시겠습니까?", "YesOrNo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        //        {
        //            if (this.Recipe.RecipePath == "")
        //            {
        //                ShowDialogSaveRecipe();
        //            }
        //            else
        //            {
        //                this.Recipe.Save(this.Recipe.RecipePath);
        //            }
        //        }

        //        this.Recipe.Clear();
        //        ShowDialogSaveRecipe(true);

        //        WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Save Recipe : " + ex.Message);
        //    }
        //}

        //public void ShowDialogSaveRecipe(bool bNew = false)
        //{
        //    try
        //    {
        //        SaveFileDialog dlg = new SaveFileDialog();
        //        dlg.InitialDirectory = recipeFolderPath;

        //        if (bNew == true) dlg.Title = "새로 만들기";

        //        dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
        //        if (dlg.ShowDialog() == true)
        //        {
        //            string sFilePath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
        //            string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
        //            string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
        //            string sFolderPath = Path.Combine(sFilePath, sFileNameNoExt); // 레시피 이름으로 된 폴더
        //            string sResultFileName = Path.Combine(sFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

        //            DirectoryInfo dir = new DirectoryInfo(sFolderPath);
        //            if (!dir.Exists)
        //                dir.Create();

        //            this.SaveRecipe(sResultFileName);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Save Recipe : " + ex.Message);
        //    }
        //}

        //public void SaveRecipe(string recipePath)
        //{
        //    string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        //    using (StreamWriter writer = new StreamWriter(recipePath, true))
        //    {
        //        writer.WriteLine(time + " - SaveRecipe()");
        //    }


        //    WIND2EventManager.OnBeforeRecipeSave(recipe, new RecipeEventArgs());

        //    if (recipePath.IndexOf(".rcp") == -1) recipePath += ".rcp";
        //    recipe.Save(recipePath);

        //    WIND2EventManager.OnAfterRecipeSave(recipe, new RecipeEventArgs());
        //    //this.Load(sFilePath); //?
        //    //WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());
        //}

        //public void ShowDialogLoadRecipe()
        //{
        //    try
        //    {
        //        OpenFileDialog dlg = new OpenFileDialog();
        //        dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
        //        if (dlg.ShowDialog() == true)
        //        {

        //            // 레시피 전/후처리 이벤트 Recipe 클래스 안쪽에 넣어야할 수 도?
        //            WIND2EventManager.OnBeforeRecipeRead(recipe, new RecipeEventArgs());

        //            this.LoadRecipe(dlg.FileName);
        //            WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());

        //            WIND2EventManager.OnAfterRecipeRead(recipe, new RecipeEventArgs());

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Load Recipe : " + ex.Message);
        //    }
        //}
        //public void LoadRecipe(string recipePath)
        //{
        //    string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        //    using (StreamWriter writer = new StreamWriter(recipePath, true))
        //    {
        //        writer.WriteLine(time + " - LoadRecipe()");
        //    }

        //    this.recipe.Read(recipePath);
        //}
        #endregion
    }
}
