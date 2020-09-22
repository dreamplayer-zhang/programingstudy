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

        private void _btnTest()
        {
            // TestCode
            ImageData Image = m_Setup.m_MainWindow.m_Image;
            int T = 7213;
            int L = 640;
            int nImgWsz = 796; //int imgWsz = 2560;
            int nImgHsz = 683; //int imgHsz = 2786;
            byte[] arrCopyImg = new byte[nImgHsz * nImgWsz];
            int nStride = (int)Image.p_Stride;
            int idx = 0;

            for (int cnt = T+50; cnt < T+150; cnt++, idx++)
                Marshal.Copy(Image.m_ptrImg + (L + 200 + cnt * nStride), arrCopyImg, 100 * idx, 100);

            float score;
            int nPosX;
            int nPosY;

            unsafe
            {
                score = CLR_IP.Cpp_TemplateMatching((byte*)Image.m_ptrImg.ToPointer(), arrCopyImg, &nPosX, &nPosY, Image.p_Size.X, Image.p_Size.Y, 100, 100, L, T, L+nImgWsz, T+nImgHsz, 3);
            }
        }
    }
}
