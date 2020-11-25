using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.IO;

namespace Root.Module
{
    public class ReadExcel : ModuleBase
    {
        string[] m_asGroup =
        {
            "응용그룹",
            "영업그룹",
            "선행그룹",
            "생산그룹",
            "바이오파트",
            "기술그룹",
            "계측그룹",
            "개발그룹",
            "SBU그룹",
            "QM파트"
        };
        int GetIndex(string sID)
        {
            for (int n = 0; n < c_lData; n++)
            {
                if (sID.Contains(m_asGroup[n])) return n; 
            }
            p_sInfo = sID; 
            return -1; 
        }

        public class Data
        {
            double m_fW = 0;
            double m_fScore = 0; 

            public void Clear()
            {
                m_fW = 0;
                m_fScore = 0; 
            }

            public void Add(double fW, double fS)
            {
                m_fW += fW;
                m_fScore += (fW * fS); 
            }

            public double Result()
            {
                return (m_fW == 0) ? 50 : m_fScore / m_fW; 
            }
        }
        int c_lData = 10;
        Data[,] m_aData = null; 
        void ClearData()
        {
            for (int y = 0; y < c_lData; y++)
            {
                for (int x = 0; x < c_lData; x++)
                {
                    if (m_aData[x, y] == null) m_aData[x, y] = new Data(); 
                    m_aData[x, y].Clear();
                }
            }
        }

        void CalcData(string sFrom, string sTo, double fW, double fS)
        {
            int nFrom = GetIndex(sFrom);
            if (nFrom < 0) return;
            int nTo = GetIndex(sTo);
            if (nTo < 0) return;
            m_aData[nFrom, nTo].Add(fW, fS); 
        }

        void SaveResult()
        {
            FileStream fs = new FileStream("c:\\Log\\상호평가.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            for (int y = 0; y < c_lData; y++)
            {
                for (int x = 0; x < c_lData; x++)
                {
                    sw.WriteLine(m_asGroup[y] + "\t" + m_asGroup[x] + "\t" + m_aData[y, x].Result() + "\t" + m_aData[x, y].Result()); 
                }
            }
            sw.Close();
            fs.Close();
        }

        public void RunExcel()
        {
            ClearData();
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Excel Files (*.xlsx)|*.xlsx";
            if (dlg.ShowDialog() == false) return;
            Application ap = new Application();
            Workbook wb = ap.Workbooks.Open(dlg.FileName);
            Worksheet ws = wb.Sheets[1];
            Range range = ws.UsedRange;
            for (int y = 3; y <= 332; y++)
            {
                string sF = GetString(range.Cells[y, 2].Value2);
                string sT = GetString(range.Cells[y, 3].Value2); 
                double fW = GetNumber(range.Cells[y, 4].Value2);
                double fS = GetNumber(range.Cells[y, 5].Value2);
                CalcData(sF, sT, fW, fS);
            }
            SaveResult();
        }

        string GetString(dynamic value)
        {
            if (value == null) return "null";
            if (value.GetType() == typeof(string)) return (string)value;
            return "Invalid";
        }

        double GetNumber(dynamic value)
        {
            if (value == null) return 0;
            if (value.GetType() == typeof(double)) return (double)value;
            return 0; 
        }

        public ReadExcel(string id, IEngineer engineer)
        {
            m_aData = new Data[c_lData, c_lData];
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Test(this), false, "Desc Test");
        }

        public class Run_Test : ModuleRunBase
        {
            ReadExcel m_module;
            public Run_Test(ReadExcel module)
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
            }

            public override string Run()
            {
                m_module.RunExcel(); 
                return "OK";
            }
        }
    }
}
