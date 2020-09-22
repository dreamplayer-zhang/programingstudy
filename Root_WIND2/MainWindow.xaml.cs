using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using RootTools.Memory;
using RootTools;
using RootTools_CLR;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RootTools_Vision;
using System.Drawing;

namespace Root_WIND2
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Window Event
        public MainWindow()
        {
            InitializeComponent();
            if (this.WindowState == WindowState.Maximized)
            {
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                NormalizeButton.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            m_ModeUI.tbDate.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        #endregion

        #region Title Bar
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            NormalizeButton.Visibility = Visibility.Visible;
            MaximizeButton.Visibility = Visibility.Collapsed;
        }
        private void NormalizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            MaximizeButton.Visibility = Visibility.Visible;
            NormalizeButton.Visibility = Visibility.Collapsed;
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    MaximizeButton.Visibility = Visibility.Visible;
                    NormalizeButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    NormalizeButton.Visibility = Visibility.Visible;
                    MaximizeButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                this.DragMove();
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        private void textLastError_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        #region UI
        public Setup m_Setup;
        private Setup_ViewModel m_SetupViewModel;

        public Review m_Review;
        private Review_ViewModel m_ReviewViewModel;

        public Run m_Run;
        private Run_ViewModel m_RunViewModel;
        public SelectMode m_ModeUI;


        #endregion

        WIND2_Engineer m_engineer = new WIND2_Engineer();
        MemoryTool m_memoryTool;
        public ImageData m_Image;
        public IDialogService dialogService;
        string sPool = "pool";
        string sGroup = "groupss";
        string sMem = "memss";
        public int MemWidth = 40000;
        public int MemHeight = 40000;
        Viewer viewer = new Viewer();


        public RecipeManager m_RecipeMGR;
        Recipe m_Recipe;
        RecipeInfo m_RecipeInfo;
        RecipeEditor m_RecipeEditor;


        //
        WIND2_InspectionManager m_InspectionManager;


        void Init()
        {
            dialogService = new DialogService(this);
            dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();

            m_engineer.Init("WIND2");
            m_memoryTool = m_engineer.ClassMemoryTool();
            m_memoryTool.GetPool(sPool, true).p_gbPool = 3;
            m_memoryTool.GetPool(sPool, true).GetGroup(sGroup).CreateMemory(sMem, 1, 1, new CPoint(MemWidth, MemHeight));
            m_memoryTool.GetMemory(sPool, sGroup, sMem);

            m_Image = new ImageData(m_memoryTool.GetMemory(sPool, sGroup, sMem));

            viewer.p_ROI_VM = new MaskTool_ViewModel(m_Image, dialogService);
            panel.DataContext = viewer.p_ROI_VM;

            m_RecipeMGR = new RecipeManager();
            m_Recipe = m_RecipeMGR.GetRecipe();
            m_RecipeEditor = m_Recipe.GetRecipeEditor();
            m_RecipeInfo = m_Recipe.GetRecipeInfo();


            // Inspction Manager
            m_InspectionManager = new WIND2_InspectionManager(m_Image.GetPtr(), m_Image.p_Size.X, m_Image.p_Size.Y);

            m_engineer.InspectionManager = m_InspectionManager; 

            InitUI();
            InitTimer();
            //InitDataSetting();
        }

        //private void InitDataSetting()
        //{
        //    m_SetupViewModel.Recipe = m_Recipe;
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            m_RecipeEditor.ClearPlain();

            var TShapes = viewer.p_ROI_VM.GetListTShape();

            DrawData_Plain temp = new DrawData_Plain(PLAIN_TYPE.ORIGIN, 0, TShapes);
            m_RecipeEditor.PushPlain(temp);

            //Trigger
            m_RecipeEditor.UpdateRecipe();
        }

        void InitUI()
        {
            m_Setup = new Setup();
            m_SetupViewModel = new Setup_ViewModel(this, m_Recipe);
            m_Setup.DataContext = m_SetupViewModel;
            //((Setup_ViewModel)m_Setup.DataContext).init(this);
            //m_SetupViewModel = (Setup_ViewModel)m_Setup.DataContext;

            m_Review = new Review();
            ((Review_ViewModel)m_Review.DataContext).init(this);
            m_ReviewViewModel = (Review_ViewModel)m_Review.DataContext;

            m_Run = new Run();

            ((Run_ViewModel)m_Run.DataContext).init(this);
            m_RunViewModel = (Run_ViewModel)m_Run.DataContext;

            m_ModeUI = new SelectMode();
            m_ModeUI.Init(this);

            Home();
        }
        void Home()
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(m_ModeUI);
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            //RecipeData_Origin rcpOrigin = m_Recipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin;

            //rcpOrigin.OriginX = m_SetupViewModel.Wizard.

            //m_InspectionManager.SetBundles();



            //RecipeData_Origin pOrigin = m_Recipe.GetRecipeData().GetRecipeOrigin();

            //CPoint ptOrigin = pOrigin.GetOriginPoint();
            //CRect rtOrigin = pOrigin.GetOriginRect();
            //byte[] InspectionBuffer = new byte[rtOrigin.Width * rtOrigin.Height];
            //int nStride = (int)m_Image.p_Stride;
            //for (int y = 0; y < rtOrigin.Height; y++)
            //{
            //    Marshal.Copy(
            //    m_Image.GetPtr() + ptOrigin.X + (y + ptOrigin.Y) * nStride, // source
            //    InspectionBuffer,
            //    rtOrigin.Width * y,
            //    rtOrigin.Width
            //    );
            //}
            //CLR_IP.Cpp_Threshold(InspectionBuffer, InspectionBuffer, rtOrigin.Width, rtOrigin.Height, true, 200);



            //m_RecipeMGR.m_Recipe.m_ReicpeData.m_RecipeData_Position.AddImageToBundle();
            //m_RecipeMGR.m_Recipe.m_ReicpeData.m_RecipeData_Position.AddImageToBundle();

            //m_RecipeMGR.SaveRecipe();
            //m_RecipeMGR.SaveGraphicsFile();

            ////m_RecipeMGR.LoadRecipe();
            ////m_RecipeMGR.LoadGraphicsRecipe();

            //m_RecipeMGR.m_Recipe.m_ReicpeData.m_RecipeData_Position.SaveImageBundle();


            //byte[] source = m_RecipeMGR.m_Recipe.m_ReicpeData.m_RecipeData_Position.btyePositionFeature;
            //ImageConverter con = new ImageConverter();
            //System.Drawing.Image img = (System.Drawing.Image)con.ConvertFrom(source);

            //img.Save(@"C:\Wind2\Result.bmp");


            //m_RecipeMGR.LoadGraphicsRecipe();
            //m_RecipeMGR.SaveRecipe();
            //m_RecipeMGR.OpenRecipe();
            //CLR_IP.Cpp_Threshold(InspectionBuffer, InspectionBuffer, nBufferWidth, nBufferHeight, this.parameter.IsDark, this.parameter.Threshold);
        }
    }
    public class Viewer : ObservableObject
    {
        private MaskTool_ViewModel m_ROI_VM;
        public MaskTool_ViewModel p_ROI_VM
        {
            get
            {
                return m_ROI_VM;
            }
            set
            {
                SetProperty(ref m_ROI_VM, value);
            }
        }

        private RootViewer_ViewModel m_RootViewer;
        public RootViewer_ViewModel p_RootViewer
        {
            get
            {
                return m_RootViewer;
            }
            set
            {
                SetProperty(ref m_RootViewer, value);
            }
        }

        private ImageToolViewer_VM m_ImageViewer;
        public ImageToolViewer_VM p_ImageViewer
        {
            get
            {
                return m_ImageViewer;
            }
            set
            {
                SetProperty(ref m_ImageViewer, value);
            }
        }
    }
}