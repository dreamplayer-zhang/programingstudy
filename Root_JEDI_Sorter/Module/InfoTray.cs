using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_JEDI_Sorter.Module
{
    public enum eVision
    {
        CCS,
        Top3D,
        Top2D,
        Bottom,
    }

    public enum eResult
    {
        Good,
        Reject,
        Rework,
        Empty
    }

    public class InfoTray : NotifyProperty
    {
        #region On Inspect
        Dictionary<eVision, bool> m_bInspect = new Dictionary<eVision, bool>();
        void InitInspect()
        {
            foreach (eVision eVision in Enum.GetValues(typeof(eVision))) m_bInspect.Add(eVision, false);
        }

        public void StartInspect(eVision eVision)
        {
            m_bInspect[eVision] = true;
        }

        public bool p_bInspect
        {
            get 
            {
                foreach (eVision eVision in Enum.GetValues(typeof(eVision)))
                {
                    if (m_bInspect[eVision]) return true; 
                }
                return false; 
            }
        }
        #endregion

        #region Chip
        public List<List<eResult>> m_aChip = new List<List<eResult>>(); 
        void InitChip()
        {
            while (m_aChip.Count > Tray.m_countChip.Y) m_aChip.RemoveAt(m_aChip.Count - 1);
            while (m_aChip.Count < Tray.m_countChip.Y) m_aChip.Add(new List<eResult>()); 
            for (int y = 0; y < Tray.m_countChip.Y; y++)
            {
                while (m_aChip[y].Count > Tray.m_countChip.X) m_aChip[y].RemoveAt(m_aChip[y].Count - 1);
                while (m_aChip[y].Count > Tray.m_countChip.X) m_aChip[y].Add(eResult.Good); 
            }
        }

        public void FlipTray()
        {
            InitChip();
            for (int y = 0; y < Tray.m_countChip.Y; y++)
            {
                for (int x0 = 0, x1 = Tray.m_countChip.X - 1; x0 < Tray.m_countChip.X / 2; x0++, x1--)
                {
                    eResult eResult = m_aChip[y][x0];
                    m_aChip[y][x0] = m_aChip[y][x1];
                    m_aChip[y][x1] = eResult; 
                }
            }
        }

        public CPoint FindChip(eResult eResult)
        {
            for (int y = 0; y < Tray.m_countChip.Y; y++)
            {
                for (int x = 0; x < Tray.m_countChip.X; x++)
                {
                    if (m_aChip[y][x] == eResult) return new CPoint(x, y); 
                }
            }
            return new CPoint(-1, -1); 
        }
        #endregion

        #region Chip Count
        public Dictionary<eResult, int> m_aCount = new Dictionary<eResult, int>(); 
        void InitCount()
        {
            foreach (eResult eResult in Enum.GetValues(typeof(eResult))) m_aCount.Add(eResult, 0); 
        }

        public InfoTray CalcCount()
        {
            foreach (eResult eResult in Enum.GetValues(typeof(eResult))) m_aCount[eResult] = 0;
            for (int y = 0; y < Tray.m_countChip.Y; y++)
            {
                foreach (eResult eResult in m_aChip[y]) m_aCount[eResult]++; 
            }
            return this; 
        }

        public int GetChipCount()
        {
            CalcCount(); 
            return m_aCount[eResult.Good] + m_aCount[eResult.Reject] + m_aCount[eResult.Rework]; 
        }
        #endregion

        public string m_sTrayIn = "";
        public string m_sTrayOut = "";
        public string p_id { get; set; }
        public InfoTray(string id)
        {
            p_id = id;
            m_sTrayIn = id; 
            InitInspect();
            InitChip(); 
            InitCount(); 
        }
    }

    public static class Tray
    {
        #region Tray
        public static RPoint m_sizeTray = new RPoint(135.9, 315);
        public static double m_thickTray = 6;
        static void RunTreeTray(Tree tree)
        {
            m_sizeTray = tree.Set(m_sizeTray, m_sizeTray, "Size", "Tray Size (mm)");
            m_thickTray = tree.Set(m_thickTray, m_thickTray, "Thickness", "Tray Thickness (mm)"); 
        }
        #endregion

        #region Chip
        public static CPoint m_countChip = new CPoint(2, 3);
        public static RPoint m_distanceChip = new RPoint(1, 1);
        static RPoint m_offsetChip = new RPoint(0, 0); 
        public static double m_thickChip = 5;
        static void RunTreeChip(Tree tree)
        {
            m_countChip = tree.Set(m_countChip, m_countChip, "Count", "Chip Count in Tray");
            m_distanceChip = tree.Set(m_distanceChip, m_distanceChip, "Distance", "Distance between Chip (mm)");
            m_offsetChip = tree.Set(m_offsetChip, m_offsetChip, "Offset", "Chip Position Offset (mm)");
            m_thickChip = tree.Set(m_thickChip, m_thickChip, "Thickness", "Chip Thickness from Base (mm)");
            CalcOffset(); 
        }

        public static double GetChipPosX(int x)
        {
            return m_fOffset.X + m_offsetChip.X + x * m_distanceChip.X;
        }

        public static double GetChipPosY(int y)
        {
            return m_fOffset.Y + m_offsetChip.Y + y * m_distanceChip.Y;
        }

        static RPoint m_fOffset = new RPoint();
        static void CalcOffset()
        {
            m_fOffset.X = (m_sizeTray.X - (m_distanceChip.X * (m_countChip.X - 1))) / 2;
            m_fOffset.Y = (m_sizeTray.Y - (m_distanceChip.Y * (m_countChip.Y - 1))) / 2;
        }
        #endregion

        public static void RunTree(Tree tree)
        {
            RunTreeTray(tree.GetTree("Tray"));
            RunTreeChip(tree.GetTree("Chip"));
        }
    }
}
