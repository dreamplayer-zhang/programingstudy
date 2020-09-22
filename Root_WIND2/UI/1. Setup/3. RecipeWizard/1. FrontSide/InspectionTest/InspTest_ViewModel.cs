using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.InteropServices;
using RootTools;
using RootTools_CLR;
using RootTools_Vision;
using System.Windows.Threading;

namespace Root_WIND2
{
    class InspTest_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;

        public InspTestPanel Main;
        public InspTestPage InspTest;
   
        private RootViewer_ViewModel m_ROOT_VM;
        public RootViewer_ViewModel p_ROOT_VM
        {
            get
            {
                return m_ROOT_VM;
            }
            set
            {
                SetProperty(ref m_ROOT_VM, value);
            }
        }

        private MapViewer_ViewModel m_MapViewer_VM;

        public MapViewer_ViewModel p_MapViewer_VM
        {
            get { return this.m_MapViewer_VM; }
            set { SetProperty(ref m_MapViewer_VM, value); }
        }

        public InspTest_ViewModel(Setup_ViewModel setup)
        {
            init();
            m_Setup = setup;
            ViewerInit();
        }
        
        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public void init()
        {
            Main = new InspTestPanel();
            InspTest = new InspTestPage();
        }
        private ImageViewer_ViewModel m_ImageViewer;
        public ImageViewer_ViewModel p_ImageViewer
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
        
        private void ViewerInit()
        {
            p_ROOT_VM = new RootViewer_ViewModel();
            p_ROOT_VM.init(m_Setup.m_MainWindow.m_Image, m_Setup.m_MainWindow.dialogService);

            m_MapViewer_VM = new MapViewer_ViewModel();
            //m_MapViewer_VM.MapSize = new Point(10, 10);

        }
   
        public ICommand btnInspTestStart
        {
            get
            {
                return new RelayCommand(_btnTest);
            }
        }
        //public ICommand btnInspTestSnap
        //{
        //    get
        //    {
        //        return new RelayCommand();
        //    }
        //}
        //public ICommand btnInspTestStop
        //{
        //    get
        //    {
        //        return new RelayCommand();
        //    }
        //}
        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(m_Setup.SetWizardFrontSide);
            }
        }


        SolidColorBrush brushSnap = System.Windows.Media.Brushes.LightSkyBlue;
        SolidColorBrush brushPosition = System.Windows.Media.Brushes.SkyBlue;
        SolidColorBrush brushPreInspection = System.Windows.Media.Brushes.Cornsilk;
        SolidColorBrush brushInspection = System.Windows.Media.Brushes.Gold;
        SolidColorBrush brushMeasurement = System.Windows.Media.Brushes.CornflowerBlue;
        SolidColorBrush brushComplete = System.Windows.Media.Brushes.YellowGreen;

        object lockObj = new object();
        private void MapStateChanged_Callback(int mapPosX, int mapPosY, WORKPLACE_STATE state)
        {
            lock(lockObj)
            {
                
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    int index = (int)(mapPosX + mapPosY * m_MapViewer_VM.MapSize.X);
                    if (index > this.m_MapViewer_VM.CellItems.Count - 1) return;

                    TextBox tb = (TextBox)this.m_MapViewer_VM.CellItems[index];

                    switch (state)
                    {
                        case WORKPLACE_STATE.NONE:
                            //tb.Background = brushPosition;
                            break;
                        case WORKPLACE_STATE.SNAP:
                            tb.Background = brushPreInspection;
                            break;
                        case WORKPLACE_STATE.READY:
                            tb.Background = brushPosition;
                            break;
                        case WORKPLACE_STATE.INSPECTION:
                            tb.Background = brushInspection;
                            break;
                        case WORKPLACE_STATE.DEFECTPROCESS:
                            tb.Background = brushComplete;
                            break;
                    }
                }));
            }
        }

        private void _btnTest()
        {
            //m_MapViewer_VM.MapSize = new Point(10, 10);

            //m_MapViewer_VM.MapSize = new Point(14, 14);

            ////m_MapViewer_VM.CellItems = new ObservableCollection<UIElement>();

            //int nSizeX = 14;
            //int nSizeY = 14;
            //for(int y = 0; y < nSizeY; y++)
            //{
            //    for(int x= 0; x < nSizeX; x++)
            //    {
            //        TextBox tb = new TextBox();
            //        tb.Background = Brushes.LightGray;
            //        m_MapViewer_VM.CellItems.Add(tb);

            //        Grid.SetRow(tb, y);
            //        Grid.SetColumn(tb, x);
            //    }
            //}
            
            //((ObservableCollection<UIElement>)items.ItemsSource).Add(tb);

           

            m_Setup.InspectionManager.MapStateChanged += MapStateChanged_Callback;
            m_Setup.InspectionManager.CreateInspecion();
            m_Setup.InspectionManager.Start();

            // TestCode
            //ImageData Image = m_Setup.m_MainWindow.m_Image;
            //int T = 7213;
            //int L = 640;
            //int nImgWsz = 796; //int imgWsz = 2560;
            //int nImgHsz = 683; //int imgHsz = 2786;
            //byte[] arrCopyImg = new byte[nImgHsz * nImgWsz];
            //int nStride = (int)Image.p_Stride;
            //int idx = 0;

            //for (int cnt = T+50; cnt < T+150; cnt++, idx++)
            //    Marshal.Copy(Image.m_ptrImg + (L + 200 + cnt * nStride), arrCopyImg, 100 * idx, 100);

            //float score;
            //int nPosX;
            //int nPosY;

            //unsafe
            //{
            //    score = CLR_IP.Cpp_TemplateMatching((byte*)Image.m_ptrImg.ToPointer(), arrCopyImg, &nPosX, &nPosY, Image.p_Size.X, Image.p_Size.Y, 100, 100, L, T, L+nImgWsz, T+nImgHsz, 3);
            //}
        }
    }
}
