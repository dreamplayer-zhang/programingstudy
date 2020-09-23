﻿using System;
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
        TRect Box;
        //private Origin_ViewModel m_Origin_VM;
        //public Origin_ViewModel p_Origin_VM
        //{
        //    get
        //    {
        //        return m_Origin_VM;
        //    }
        //    set
        //    {
        //        SetProperty(ref m_Origin_VM, value);
        //    }
        //}
        private DrawTool_ViewModel m_DrawTool_VM;
        public DrawTool_ViewModel p_DrawTool_VM
        {
            get
            {
                return m_DrawTool_VM;
            }
            set
            {
                SetProperty(ref m_DrawTool_VM, value);
            }
        }
        public InspTestPanel Main;
        public InspTestPage InspTest;

        private MapViewer_ViewModel m_MapViewer_VM;

        public MapViewer_ViewModel p_MapViewer_VM
        {
            get { return this.m_MapViewer_VM; }
            set { SetProperty(ref m_MapViewer_VM, value); }
        }

        public InspTest_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;

            init();
            ViewerInit(setup);
        }
        public void ViewerInit(Setup_ViewModel setup)
        {
            p_DrawTool_VM = new DrawTool_ViewModel(setup.m_MainWindow.m_Image, setup.m_MainWindow.dialogService);
            p_DrawTool_VM.BoxDone += P_BOX_VM_BoxDone;
        }

        public void init()
        {
            Main = new InspTestPanel();
            InspTest = new InspTestPage();
        }
     
        public void SetPage(UserControl page)
        {

            m_MapViewer_VM = new MapViewer_ViewModel();

            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
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
        System.Windows.Media.Color ConvertColor(System.Drawing.Color color)
        {
            System.Windows.Media.Color c = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
            return c;
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

        public void DrawRect(Point ptStart, Point ptEnd)
        {
            
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

            p_DrawTool_VM.DrawRect(new CPoint(L + nPosX, T + nPosY), new CPoint(L + nPosX + 100, T + nPosY + 100));
        }
        private void P_BOX_VM_BoxDone(object e)
        {
            Box = e as TRect;

            ImageData BoxImageData = new ImageData(Box.MemoryRect.Width, Box.MemoryRect.Height);
            BoxImageData.m_eMode = ImageData.eMode.ImageBuffer;
            BoxImageData.SetData(p_DrawTool_VM.p_ImageData.GetPtr(), Box.MemoryRect, (int)p_DrawTool_VM.p_ImageData.p_Stride);
        }
    }
}
