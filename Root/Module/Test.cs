using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Lens.LinearTurret;
using RootTools.Light;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;

namespace Root.Module
{
    public class Test : ModuleBase
    {
        #region StringTable
        static string[] m_asStringTable =
        {
            "Gray Value Range (0~255)",
        };
        StringTable.Group m_ST = StringTable.Get("Test", m_asStringTable);
        #endregion

        #region ToolBox
        enum eBuzzer
        {
            Home,
            Error,
            Done,
            Test
        }
        DIO_I m_diTest;
        DIO_O m_doBeep;
        DIO_IO m_dioStart;
        DIO_I2O2 m_dioDoor;
        DIO_Os m_doBuzzer;
        AxisXY m_axisXY;
        Axis m_axisZ;
        NamedPipe m_namePipe;
        //TCPIPServer m_socketServer;
        //TCPIPClient m_socketClient;
        LightSet m_light;
        LensLinearTurret m_lens; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_dioStart, this, "Start");
            p_sInfo = m_toolBox.Get(ref m_dioDoor, this, "Door", "Close", "Open");
            p_sInfo = m_toolBox.Get(ref m_diTest, this, "Test");
            p_sInfo = m_toolBox.Get(ref m_doBeep, this, "Beep");
            p_sInfo = m_toolBox.Get(ref m_doBuzzer, this, "Buzzer", Enum.GetNames(typeof(eBuzzer)));
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "Loader");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "CamZ");
            p_sInfo = m_toolBox.Get(ref m_namePipe, this, "Pipe");
            //p_sInfo = m_toolBox.Get(ref m_socketServer, this, "SocketServer");
            //p_sInfo = m_toolBox.Get(ref m_socketClient, this, "SocketClient");
            p_sInfo = m_toolBox.Get(ref m_light, this);
            p_sInfo = m_toolBox.Get(ref m_lens, this, "LinearTurret"); 
        }
        #endregion

        #region AxisPos
        enum ePosXY
        {
            Left,
            Right,
            Top,
            Bottom
        }
        enum ePosZ
        {
            Ready,
            Done
        }
        void InitAxisPos()
        {
            m_axisXY.AddPos(Enum.GetNames(typeof(ePosXY)));
            m_axisXY.AddPos("Center");
            m_axisZ.AddPos(Enum.GetNames(typeof(ePosZ)));
            m_axisZ.AddPos("First", "Second");
        }
        #endregion

        public Test(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
            InitAxisPos();
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
        }

        public override void Reset()
        {
            base.Reset();
            //
        }

        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Test(this), false, "Desc Test");
            AddModuleRunList(new Run_Run(this), true, "Desc Run");
            AddModuleRunList(new Run_AxisMove(this), false, "Axis Move");
        }

        public class Run_Test : ModuleRunBase
        {
            Test m_module;
            public Run_Test(Test module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bTest = false;
            public override ModuleRunBase Clone()
            {
                Run_Test run = new Run_Test(m_module);
                run.m_bTest = m_bTest;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bTest = tree.Set(m_bTest, false, "Test", "Run Test or Not", bVisible);
            }

            class Data
            {
                string m_sCode;
                public string m_sStrip;
                string m_sXout;
                public string m_sResult = "";
                public string m_sTray = "";

                public void Write(StreamWriter sw)
                {
                    sw.WriteLine(m_sCode + "\t" + m_sXout + "\t" + m_sStrip + "\t" + m_sResult + "\t" + m_sTray);
                }

                public Data(string sCode, string sStrip, string sXout)
                {
                    m_sCode = sCode;
                    m_sStrip = sStrip;
                    m_sXout = sXout; 
                }
            }
            List<Data> m_aData = new List<Data>(); 

            public override string Run()
            {
                //m_module.RunTree(Tree.eMode.Init); 
                /*                m_log.Info(p_id + " : Test Start");
                                Thread.Sleep(2000);
                                m_log.Info(p_id + " : Test End"); */
                int s = "[21.15.08] <Teach0AOI> Strip : ".Length;
                int l = "Q0554510002S 027I".Length; 
                FileStream fs = null;
                StreamReader sr = null;
                try
                {
                    fs = new FileStream("c:\\ASISLog\\LOG\\2020-09-27_Edit.Log", FileMode.Open);
                    sr = new StreamReader(fs);
                    string sLine = ReadLine(sr); 
                    while (sLine != null)
                    {
                        if (sLine.Contains("Strip : Q0554"))
                        {
                            string sCode = sLine.Substring(s, l);
                            string sStrip = ReadStrip(sr);
                            string sXout = ReadITS(sCode);
                            m_aData.Add(new Data(sCode, sStrip, sXout)); 
                        }
                        sLine = ReadLine(sr);
                    }
                }
                finally
                {
                    sr.Close();
                    fs.Close();
                }
                FileStream fw = null;
                StreamWriter sw = null;
                try
                {
                    fw = new FileStream("c:\\ASISLog\\LOG\\ASIS.txt", FileMode.Create);
                    sw = new StreamWriter(fw);
                    foreach (Data data in m_aData) data.Write(sw); 
                }
                finally
                {
                    sw.Close();
                    fw.Close();
                }
                return "OK";
            }

            string ReadStrip(StreamReader sr)
            {
                string sLine = ReadLine(sr); 
                if (sLine.Contains("<Teach0> S="))
                {
                    string[] asStrip0 = sLine.Split('=');
                    string[] asStrip1 = asStrip0[1].Split(' ');
                    return asStrip1[0];
                }
                return ReadStrip(sr); 
            }

            string ReadLine(StreamReader sr)
            {
                string sLine = sr.ReadLine();
                if (sLine == null) return null; 
                if (sLine.Contains("<Work> Strip :"))
                {
                    string[] asLine = sLine.Split(',');
                    string[] asStrip = asLine[0].Split(' ');
                    string sStrip = asStrip[asStrip.Length - 1]; 
                    string[] asXout = asLine[1].Split(' ');
                    string sXout = asXout[asXout.Length - 1]; 
                    string[] asTray = asLine[2].Split(' ');
                    string sTray = asTray[asTray.Length - 1];
                    for (int n = m_aData.Count - 1; n >= 0; n--)
                    {
                        if (m_aData[n].m_sStrip == sStrip)
                        {
                            m_aData[n].m_sResult = sXout;
                            m_aData[n].m_sTray = sTray;
                            return sLine; 
                        }
                    }
                    Data data = new Data("No ITS", sStrip, "");
                    data.m_sResult = sXout;
                    data.m_sTray = sTray;
                    m_aData.Add(data); 
                }
                return sLine; 
            }

            string ReadITS(string sCode)
            {
                int nCount = 0; 
                FileStream fs = null;
                StreamReader sr = null;
                try
                {
                    fs = new FileStream("c:\\ASISLog\\ITS-FILE(SERVER)\\Q0554\\Q0554510002S 0\\" + sCode + ".smk", FileMode.Open);
                    sr = new StreamReader(fs);
                    for (int n = 0; n < 5; n++)
                    {
                        string sLine = sr.ReadLine();
                        foreach (char ch in sLine) if (ch == '1') nCount++; 
                    }
                    return nCount.ToString(); 
                }
                finally
                {
                    sr.Close();
                    fs.Close();
                }
            }
        }

        public class Run_Run : ModuleRunBase
        {
            Test m_module;
            Run_Test m_runTest;
            public Run_Run(Test module)
            {
                m_module = module;
                InitModuleRun(module);
                m_runTest = new Run_Test(module);
            }

            int m_nTry = 3;
            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                run.m_nTry = m_nTry;
                run.m_runTest = (Run_Test)m_runTest.Clone();
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nTry = tree.Set(m_nTry, 3, "Try", "Try Count", bVisible);
                m_runTest.RunTree(tree.GetTree("Test", true, bVisible), bVisible);
            }

            public override string Run()
            {
                return "OK";
            }
        }

        public class Run_AxisMove : ModuleRunBase
        {
            Test m_module;
            public Run_AxisMove(Test module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            string m_sPosXY = "";
            string m_sPosZ = "";
            public override ModuleRunBase Clone()
            {
                Run_AxisMove run = new Run_AxisMove(m_module);
                run.m_sPosXY = m_sPosXY;
                run.m_sPosZ = m_sPosZ;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                if (m_module.m_axisXY == null) return;
                m_sPosXY = tree.Set(m_sPosXY, "", m_module.m_axisXY.m_asPos, "PosXY", "Axis Move Posistion", bVisible);
                m_sPosZ = tree.Set(m_sPosZ, "", m_module.m_axisZ.m_asPos, "PosZ", "Axis Move Posistion", bVisible);
            }

            public override string Run()
            {
                if (m_module.Run(m_module.m_axisXY.StartMove(m_sPosXY))) return p_sInfo;
                if (m_module.Run(m_module.m_axisZ.StartMove(m_sPosZ))) return p_sInfo;
                return "OK";
            }

        }
    }
}
