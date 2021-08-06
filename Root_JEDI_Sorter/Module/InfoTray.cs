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

    public class InfoChip : NotifyProperty
    {
        public eResult m_eResult; 
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

        #region Chip Count
        public Dictionary<eResult, int> m_aCount = new Dictionary<eResult, int>(); 
        void InitCount()
        {
            foreach (eResult eResult in Enum.GetValues(typeof(eResult))) m_aCount.Add(eResult, 0); 
        }

        public int GetChipCount()
        {
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
            InitCount(); 
        }
    }

    public class Tray : NotifyProperty
    {
        #region Tray
        public RPoint m_sizeTray = new RPoint(135.9, 315);
        public double m_thickTray = 6;
        void RunTreeTray(Tree tree)
        {
            m_sizeTray = tree.Set(m_sizeTray, m_sizeTray, "Size", "Tray Size (mm)");
            m_thickTray = tree.Set(m_thickTray, m_thickTray, "Thickness", "Tray Thickness (mm)"); 
        }
        #endregion

        #region Chip
        public CPoint m_countChip = new CPoint(2, 3);
        RPoint m_distanceChip = new RPoint(1, 1);
        RPoint m_offsetChip = new RPoint(0, 0); 
        public double m_thickChip = 5;
        void RunTreeChip(Tree tree)
        {
            m_countChip = tree.Set(m_countChip, m_countChip, "Count", "Chip Count in Tray");
            m_distanceChip = tree.Set(m_distanceChip, m_distanceChip, "Distance", "Distance between Chip (mm)");
            m_offsetChip = tree.Set(m_offsetChip, m_offsetChip, "Offset", "Chip Position Offset (mm)");
            m_thickChip = tree.Set(m_thickChip, m_thickChip, "Thickness", "Chip Thickness from Base (mm)");
            CalcOffset(); 
        }

        public RPoint GetChipPos(int x, int y)
        {
            double xp = m_fOffset.X + m_offsetChip.X + x * m_distanceChip.X;
            double yp = m_fOffset.Y + m_offsetChip.Y + y * m_distanceChip.Y;
            return new RPoint(xp, yp); 
        }

        RPoint m_fOffset = new RPoint(); 
        void CalcOffset()
        {
            m_fOffset.X = (m_sizeTray.X - (m_distanceChip.X * (m_countChip.X - 1))) / 2;
            m_fOffset.Y = (m_sizeTray.Y - (m_distanceChip.Y * (m_countChip.Y - 1))) / 2;
        }
        #endregion

        public void RunTree(Tree tree)
        {
            RunTreeTray(tree.GetTree("Tray"));
            RunTreeChip(tree.GetTree("Chip"));
        }
    }
}
