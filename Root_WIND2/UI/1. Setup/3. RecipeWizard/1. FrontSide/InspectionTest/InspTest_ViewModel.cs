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
using RootTools.Database;
using Root_WIND2.Module;
using RootTools.Module;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Root_WIND2
{
    class InspTest_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        Recipe m_Recipe;
        private readonly IDialogService m_DialogService;

        private MapControl_ViewModel m_MapControl_VM;
        public MapControl_ViewModel p_MapControl_VM
        {
            get
            {
                return m_MapControl_VM;
            }
            set
            {
                SetProperty(ref m_MapControl_VM, value);
            }
        }

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

        private Database_DataView_VM m_DataViewer_VM = new Database_DataView_VM();
        public DispatcherTimer timer;

        public Database_DataView_VM p_DataViewer_VM
        {
            get { return this.m_DataViewer_VM; }
            set { SetProperty(ref m_DataViewer_VM, value); }
        }


        public InspTest_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            m_Recipe = setup.Recipe;

            init(setup);
            m_DialogService = ProgramManager.Instance.DialogService;
             
        }

        public void init(Setup_ViewModel setup)
        { 
            Main = new InspTestPanel();
            InspTest = new InspTestPage();

            p_MapControl_VM = new MapControl_ViewModel(m_Setup.InspectionVision);
            p_DrawTool_VM = new DrawTool_ViewModel();
            p_DrawTool_VM.DrawDone += DrawDone_Callback;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromTicks(100000);
            timer.Tick += new EventHandler(timer_Tick);
            //zDatabaseManager.Instance.SetDatabase(1);

        }

        private void DrawDone_Callback(CPoint leftTop, CPoint rightBottom)
        {
            p_DrawTool_VM.Clear();
            this.m_DrawTool_VM.DrawRect(leftTop, rightBottom, DrawTool_ViewModel.ColorType.FeatureMatchingFail);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //DatabaseManager.Instance.SelectData();
            ////m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;
        }

        public void SetPage(UserControl page)
        {
            RecipeType_WaferMap waferMap = m_Recipe.WaferMap;

            if (waferMap.Data != null)
            {
                int nMapX = waferMap.MapSizeX;
                int nMapY = waferMap.MapSizeY;

                p_MapControl_VM.SetMap(waferMap.Data, new CPoint(nMapX, nMapY));
            }
            else
            {
                p_MapControl_VM.SetMap(m_Setup.InspectionVision.mapdata, new CPoint(14, 14));
            }


            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);

            p_DrawTool_VM.Clear();
        }
        

        public ICommand btnInspTestStart
        {
            get
            {
                return new RelayCommand(_btnTest);
            }
        }
        public ICommand btnInspTestSnap
        {
            get
            {
                return new RelayCommand(_btnSnap);
            }
        }
        public ICommand btnInspTestStop
        {
            get
            {
                return new RelayCommand(_btnStop);
            }
        }
        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(m_Setup.SetWizardFrontSide);
            }
        }

        public void DrawRectMasterFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text , bool bSuccess)
        {
            p_DrawTool_VM.DrawRect(ptOldStart, ptOldEnd, DrawTool_ViewModel.ColorType.MasterFeature);
            p_DrawTool_VM.DrawRect(ptNewStart, ptNewEnd, bSuccess?DrawTool_ViewModel.ColorType.FeatureMatching : DrawTool_ViewModel.ColorType.FeatureMatchingFail, text);
        }

        public void DrawRectShotFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text)
        {
            p_DrawTool_VM.DrawRect(ptOldStart, ptOldEnd, DrawTool_ViewModel.ColorType.ShotFeature);
            p_DrawTool_VM.DrawRect(ptNewStart, ptNewEnd, DrawTool_ViewModel.ColorType.FeatureMatching, text);
        }

        public void DrawRectChipFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
        {
            p_DrawTool_VM.DrawRect(ptOldStart, ptOldEnd, DrawTool_ViewModel.ColorType.ChipFeature);
            p_DrawTool_VM.DrawRect(ptNewStart, ptNewEnd, bSuccess ? DrawTool_ViewModel.ColorType.FeatureMatching : DrawTool_ViewModel.ColorType.FeatureMatchingFail, text);
        }
        public void DrawRectDefect(List<CRect> rectList, List<String> text, bool reDraw = false)
        {
            if(reDraw)
                p_DrawTool_VM.Clear();

            p_DrawTool_VM.DrawRect(rectList, DrawTool_ViewModel.ColorType.Defect, text);
        }

        public void UpdateDataGrid()
        {
            //DatabaseManager.Instance.SelectData();
            //m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;
        }
        private void _btnStop()
        {
            timer.Stop();
            m_Setup.InspectionVision.Stop();
            DatabaseManager.Instance.SelectData();
            m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;
        }


        unsafe private void saveWholeWaferImage(string Path, CRect rect, int byteCnt, int temp = 0)
        {
            // Width가 4의 배수가 아닐 경우 에러남...
            // 예외처리 필요 if (rect.Width % 4 != 0) >> 처리가 잘못되는듯...

            //int byteCnt = p_DrawTool_VM.p_ImageData.p_nByte;
            int _width = p_DrawTool_VM.p_ImageData.p_Size.X;
            int _height = p_DrawTool_VM.p_ImageData.p_Size.X;

            FileStream fs = new FileStream(Path, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(Convert.ToUInt16(0x4d42));//ushort bfType = br.ReadUInt16();
            if (byteCnt == 1)
            {
                if ((Int64)rect.Width * (Int64)rect.Height > Int32.MaxValue) bw.Write(Convert.ToUInt32(54 + 1024 + byteCnt * 1000 * 1000));
                else bw.Write(Convert.ToUInt32(54 + 1024 + byteCnt * (Int64)rect.Width * (Int64)rect.Height));
            }
            else if (byteCnt == 3)
            {
                if ((Int64)rect.Width * (Int64)rect.Height > Int32.MaxValue) bw.Write(Convert.ToUInt32(54 + byteCnt * 1000 * 1000));//uint bfSize = br.ReadUInt32();
                else bw.Write(Convert.ToUInt32(54 + byteCnt * (Int64)rect.Width * (Int64)rect.Height));//uint bfSize = br.ReadUInt32();
            }

            //image 크기 bw.Write();   bmfh.bfSize = sizeof(14byte) + nSizeHdr + rect.right * rect.bottom;
            bw.Write(Convert.ToUInt16(0));   //reserved // br.ReadUInt16();
            bw.Write(Convert.ToUInt16(0));   //reserved //br.ReadUInt16();
            if (byteCnt == 1)
                bw.Write(Convert.ToUInt32(1078));
            else if (byteCnt == 3)
                bw.Write(Convert.ToUInt32(54));//uint bfOffBits = br.ReadUInt32();

            bw.Write(Convert.ToUInt32(40));// uint biSize = br.ReadUInt32();
            bw.Write(Convert.ToInt32(rect.Width));// nWidth = br.ReadInt32();
            bw.Write(Convert.ToInt32(rect.Height));// nHeight = br.ReadInt32();
            bw.Write(Convert.ToUInt16(1));// a = br.ReadUInt16();
            bw.Write(Convert.ToUInt16(8 * byteCnt));     //byte       // nByte = br.ReadUInt16() / 8;                
            bw.Write(Convert.ToUInt32(0));      //compress //b = br.ReadUInt32();
            if ((Int64)rect.Width * (Int64)rect.Height > Int32.MaxValue) bw.Write(Convert.ToUInt32(1000 * 1000));// b = br.ReadUInt32();
            else bw.Write(Convert.ToUInt32((Int64)rect.Width * (Int64)rect.Height));// b = br.ReadUInt32();
            bw.Write(Convert.ToInt32(0));//a = br.ReadInt32();
            bw.Write(Convert.ToInt32(0));// a = br.ReadInt32();
            bw.Write(Convert.ToUInt32(256));      //color //b = br.ReadUInt32();
            bw.Write(Convert.ToUInt32(256));      //import // b = br.ReadUInt32();
            if (byteCnt == 1)
            {
                for (int i = 0; i < 256; i++)
                {
                    bw.Write(Convert.ToByte(i));
                    bw.Write(Convert.ToByte(i));
                    bw.Write(Convert.ToByte(i));
                    bw.Write(Convert.ToByte(255));
                }
            }
            if (rect.Width % 4 != 0)
            {
                rect.Right += 4 - rect.Width % 4;
            }

            if(byteCnt == 1)
            {
                unsafe
                {
                    byte[] aBuf = new byte[byteCnt * rect.Width];

                    byte* ptr = (byte*)p_DrawTool_VM.p_ImageData.GetPtr(temp);

                    for (int i = 0; i < rect.Bottom; i++)
                    {
                        ptr += p_DrawTool_VM.p_ImageData.p_Size.X;
                    }

                    for (int i = 0; i < rect.Height; i++)
                    {
                        for (int j = 0; j < rect.Width; j++)
                        {
                            aBuf[j * byteCnt + 0] = *(ptr + j + rect.Left);


                        }

                        ptr -= p_DrawTool_VM.p_ImageData.p_Size.X;

                        bw.Write(aBuf);
                    }
                }
            }
            else
            {
                unsafe
                {
                    byte* ptrR = (byte*)p_DrawTool_VM.p_ImageData.GetPtr(0);
                    byte* ptrG = (byte*)p_DrawTool_VM.p_ImageData.GetPtr(1);
                    byte* ptrB = (byte*)p_DrawTool_VM.p_ImageData.GetPtr(2);

                    byte[] aBuf = new byte[byteCnt * rect.Width];

                    for (int i = 0; i < rect.Bottom; i++)
                    {
                        ptrR += p_DrawTool_VM.p_ImageData.p_Size.X;
                        ptrG += p_DrawTool_VM.p_ImageData.p_Size.X;
                        ptrB += p_DrawTool_VM.p_ImageData.p_Size.X;
                    }

                    for (int i = 0; i < rect.Height; i++)
                    {
                        for (int j = 0; j < rect.Width; j++)
                        {
                            aBuf[j * byteCnt + 0] = *(ptrB + j + rect.Left);
                            aBuf[j * byteCnt + 1] = *(ptrG + j + rect.Left);
                            aBuf[j * byteCnt + 2] = *(ptrR + j + rect.Left);

                        }

                        ptrR -= p_DrawTool_VM.p_ImageData.p_Size.X;
                        ptrG -= p_DrawTool_VM.p_ImageData.p_Size.X;
                        ptrB -= p_DrawTool_VM.p_ImageData.p_Size.X;

                        bw.Write(aBuf);
                    }
                }
            }

        }

        public void _btnSnap()
        {
            EQ.p_bStop = false;
            Vision vision = ((WIND2_Handler)ProgramManager.Instance.Engineer.ClassHandler()).m_vision;
            if (vision.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Run_GrabLineScan Grab = (Run_GrabLineScan)vision.CloneModuleRun("GrabLineScan");
            var viewModel = new Dialog_Scan_ViewModel(vision, Grab);
            Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
            if (result.HasValue)
            {
                if (result.Value)
                {
                    vision.StartRun(Grab);
                }
                else
                {

                }
            }
        }

        public virtual void _btnTest()
        {
            //Temp
            timer.Start();

            //if (DatabaseManager.Instance.GetConnectionStatus())
            //{
            //    string sTableName = "wind2.defect";
            //    DatabaseManager.Instance.ClearTableData(sTableName);
            //}

            //m_Setup.InspectionManager.Recipe.GetParameter().Save();

            p_DrawTool_VM.Clear();
            
            IntPtr SharedBuf = new IntPtr();
            if (p_DrawTool_VM.p_ImageData.p_nByte == 3)
            {
                if (p_DrawTool_VM.p_eColorViewMode != RootViewer_ViewModel.eColorViewMode.All)
                    SharedBuf = p_DrawTool_VM.p_ImageData.GetPtr((int)p_DrawTool_VM.p_eColorViewMode - 1);
                else // All 일때는 R채널로...
                    SharedBuf = p_DrawTool_VM.p_ImageData.GetPtr(0);

                m_Setup.InspectionVision.SetWorkplaceBuffer(SharedBuf, p_DrawTool_VM.p_ImageData.GetPtr(0), p_DrawTool_VM.p_ImageData.GetPtr(1), p_DrawTool_VM.p_ImageData.GetPtr(2));
            }
            else
            { 
                SharedBuf = p_DrawTool_VM.p_ImageData.GetPtr();
                m_Setup.InspectionVision.SharedBuffer = SharedBuf;
            }
            
            m_Setup.InspectionVision.SharedBufferByteCnt = p_DrawTool_VM.p_ImageData.p_nByte;
            
            if(m_Setup.InspectionVision.CreateInspection() == false)
            {
                return;
            }
            m_Setup.InspectionVision.Start(false);

        }
    }
}
