using Emgu.CV;
using Emgu.CV.Structure;
using Root_Wind.Module;
using RootTools;
using RootTools.Camera;
using RootTools.Memory;
using RootTools.Module;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace Root_Wind
{
    class _4_Viewer_ViewModel : ObservableObject
    {
        #region Property
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
        MemoryTool m_MemoryModule;
        public MemoryTool p_MemoryModule
        {
            get
            {
                return m_MemoryModule;
            }
            set
            {
                SetProperty(ref m_MemoryModule, value);
            }
        }
        MemoryPool m_SelectedMemPool;
        public MemoryPool p_SelectedMemPool
        {
            get
            {
                return m_SelectedMemPool;
            }
            set
            {   
                SetProperty(ref m_SelectedMemPool, value);
            }
        }

        MemoryGroup m_SelectedMemGroup;
        public MemoryGroup p_SelectedMemGroup
        {
            get
            {
                return m_SelectedMemGroup;
            }
            set
            {
                SetProperty(ref m_SelectedMemGroup, value);
            }
        }

        MemoryData m_SelectedMemData;
        public MemoryData p_SelectedMemData
        {
            get
            {
                return m_SelectedMemData;
            }
            set
            {
                SetProperty(ref m_SelectedMemData, value);
                if (m_SelectedMemData != null)
                    SetImageData();
            }
        }
        ImageData m_imagedata;
        ICamera m_MainCamera;
        public ICamera p_MainCamera
        {
            get
            {
                return m_MainCamera;
            }
            set
            {
                SetProperty(ref m_MainCamera, value);
            }
        }
        #endregion
        Wind_Engineer m_Engineer;
        private readonly IDialogService m_DialogService;
        public BackgroundWorker Worker_ViewerUpdate = new BackgroundWorker();

        public _4_Viewer_ViewModel(Wind_Engineer engineer, IDialogService dialogService)
        {
            m_Engineer = engineer;
            m_DialogService = dialogService;
            p_MemoryModule = m_Engineer.ClassMemoryTool();
            p_ImageViewer = new ImageViewer_ViewModel(null, dialogService);
            Worker_ViewerUpdate.DoWork += Worker_ViewerUpdate_DoWork;
        }

        public void SetImageData()
        {  
            if(p_SelectedMemData == null)
                return;
            m_imagedata = new ImageData(p_SelectedMemData);
            p_ImageViewer.SetImageData(m_imagedata);
        }


        private void Worker_ViewerUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
            Vision vision = ((Wind_Handler)m_Engineer.ClassHandler()).p_vision;
            while (vision.p_eState == ModuleBase.eState.Run)
            {
                Thread.Sleep(300);
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    p_ImageViewer.SetImageSource();
                });

            }
        }

        public void VisionHome()
        {
            Vision vision = ((Wind_Handler)m_Engineer.ClassHandler()).p_vision;
            vision.p_eState = ModuleBase.eState.Home;
            while(vision.p_eState == ModuleBase.eState.Home)
            {
                Thread.Sleep(100);
            }
        }

        public void Scan()
        {
            Vision vision = ((Wind_Handler)m_Engineer.ClassHandler()).p_vision;
            Vision.Run_Grab Grab = (Vision.Run_Grab)vision.CloneModuleRun("Grab");
            var viewModel = new Dialog_Scan_ViewModel(vision, Grab);
            Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
            if (result.HasValue)
            {
                if (result.Value)
                {
                    p_MainCamera = Grab.m_grabMode.m_camera;
                    vision.StartRun(Grab);
                    vision.p_eState = ModuleBase.eState.Ready;
                    //Worker_ViewerUpdate.RunWorkerAsync();
                    //ModuleList modulelist = ((Wind_Handler)m_Engineer.ClassHandler()).m_moduleList;
                    //ModuleRunList runlist = ((Wind_Handler)m_Engineer.ClassHandler()).m_moduleList.m_moduleRunList;
                    //runlist.Clear();
                    //runlist.Add(vision, Grab);
                    //EQ.p_eState = EQ.eState.Ready;
                    //modulelist.StartModuleRuns(); 
                }
                else
                {
                    // Cancelled
                }
            }
        }
        //int m_TEst = 100000;
        CPoint cpcp = new CPoint(0, 0);

        void TestFunction2()
        {
            m_imagedata.SaveWholeImage();
        }


        void TestFunction()
        {

            string sPool = "pool";
            string sGroup = "group";
            string sMem = "mem";

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            Image<Gray, byte> img = new Image<Gray, byte>(imgWidth, imgHeight, imgWidth, (ptrMem));

            Image<Gray, byte> view = img.Rotate(67,new Gray(0) );


            view.Save(@"D:\Image\67.bmp");

            IntPtr target =  view.Ptr;
            //CvInvoke.Rotate(img, view, RotateFlags.Rotate90Clockwise);


            
            
                //for (int i = 0; i < m_imagedata.p_Size.Y; i++)
                //{
                //    //Emgu.CV.Util.CvToolbox.Memcpy( (IntPtr)((long)ptrMem + i * m_imagedata.p_Size.X)  ,  (IntPtr)((long)target + i * m_imagedata.p_Size.X)  , m_imagedata.p_Size.X);
                //}

            p_ImageViewer.SetImageSource();
            //double nHeightStep = ((double)p_View_Rect.Height - p_View_Rect.Y) / ViewHeight;
            //Image<Gray, byte> view = new Image<Gray, byte>(ViewWidth, ViewHeight);
            //if (nHeightStep < 1) {
            //    Image<Gray, byte> img = new Image<Gray, byte>(imgWidth, imgHeight, imgWidth, (ptrMem));
            //    img.ROI = new Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width, p_View_Rect.Height);
            //    view = img.Copy().Resize(ViewWidth, ViewHeight, Inter.Nearest);
            //}
            //else {
            //    for (int yy = 0; yy < ViewHeight; yy += 1) {
            //        Image<Gray, byte> img = new Image<Gray, byte>(p_View_Rect.Width, 1, p_View_Rect.Width, (ptrMem + (Convert.ToInt32(yy * nHeightStep) + p_View_Rect.Y) * imgWidth + p_View_Rect.X));
            //        Image<Gray, byte> resize = img.Resize(ViewWidth, 1, Inter.Nearest);
            //        view.ROI = new Rectangle(0, yy, ViewWidth, 1);
            //        resize.CopyTo(view);
            //        //CvInvoke.cvCopy(resize, view, (IntPtr)yy); 

            //    }
            //    view.ROI = new Rectangle(0, 0, ViewWidth, ViewHeight);
            //}
        }


        #region RelayCommand
        public RelayCommand CommandVisionHome
        {
            get
            {
                return new RelayCommand(VisionHome);
            }
        }
        public RelayCommand CommandMemorySelect
        {
            get
            {
                return new RelayCommand(SetImageData);
            }
        }
        public RelayCommand CommandScan
        {
            get
            {
                return new RelayCommand(Scan);
            }
        }
        public RelayCommand CommandTest
        {
            get
            {
                return new RelayCommand(TestFunction);
            }
        }
        public RelayCommand CommandTest2
        {
            get
            {
                return new RelayCommand(TestFunction2);
            }
        }
        #endregion
    }
}
