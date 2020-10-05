using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_ASIS.Module
{
    public class Sorter0 : ModuleBase
    {
        #region ToolBox
        Axis m_axisX;
        Axis m_axisZ;
        DIO_I m_diEmg;
        DIO_I m_diSafeZ;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisX, this, "Axis X");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "Axis Z");
            p_sInfo = m_toolBox.Get(ref m_diEmg, this, "Emegency");
            p_sInfo = m_toolBox.Get(ref m_diSafeZ, this, "Safe Z Position");
            m_picker.GetTools(this, bInit);
            if (bInit) InitTools();
        }

        void InitTools()
        {
            m_axisX.AddPos(Enum.GetNames(typeof(ePosX)));
            m_axisZ.AddPos(Enum.GetNames(typeof(ePosZ)));
        }
        #endregion

        #region Picker
        PickerFix m_picker;
        void InitPicker()
        {
            m_picker = new PickerFix(p_id + ".Picker", this);
        }
        #endregion

        #region AxisMoveX
        public enum ePosX
        {
            Cleaner0,
            Cleaner1,
            TrayLeft,
            TrayRight
        }
        string AxisMoveX(ePosX ePosX, double xOffset = 0)
        {
            if (Run(m_axisX.StartMove(ePosX, xOffset))) return p_sInfo; 
            return m_axisX.WaitReady(); 
        }

        double GetTrayOffsetX(int xTray)
        {
            double x0 = m_axisX.GetPosValue(ePosX.TrayLeft);
            double x1 = m_axisX.GetPosValue(ePosX.TrayRight);
            return xTray * (x1 - x0) / (m_trays.p_szTray.X - 1);
        }
        #endregion

        #region AxisMoveZ
        public enum ePosZ
        {
            Cleaner0,
            Cleaner1,
            Ready,
            TrayBottom,
            TrayTop
        }
        string AxisMoveZ(ePosZ ePosZ, double zOffset = 0)
        {
            if (Run(m_axisZ.StartMove(ePosZ, zOffset))) return p_sInfo;
            if (Run(m_axisZ.WaitReady())) return p_sInfo;
            if ((ePosZ != ePosZ.TrayBottom) && (ePosZ != ePosZ.TrayTop)) return "OK";
            return m_diSafeZ.p_bIn ? "OK" : "Safe Z Sensor not Detected";
        }

        double GetTrayOffsetZ(int zTray)
        {
            double z0 = m_axisZ.GetPosValue(ePosZ.TrayBottom); 
            double z1 = m_axisZ.GetPosValue(ePosZ.TrayTop);
            return zTray * (z1 - z0) / (m_trays.p_szTray.Y - 1); 
        }
        #endregion

        #region AxisMoveXZ
        string AxisMoveReady(Cleaner.eCleaner eCleaner)
        {
            ePosX ePosX = (eCleaner == Cleaner.eCleaner.Cleaner0) ? ePosX.Cleaner0 : ePosX.Cleaner1; 
            if (Run(AxisMoveX(ePosX))) return p_sInfo;
            return AxisMoveZ(ePosZ.Ready); 
        }

        string AxisMoveCleaner(Cleaner.eCleaner eCleaner, double zOffset)
        {
            if (Run(AxisMoveReady(eCleaner))) return p_sInfo;
            ePosZ ePosZ = (eCleaner == Cleaner.eCleaner.Cleaner0) ? ePosZ.Cleaner0 : ePosZ.Cleaner1;
            return AxisMoveZ(ePosZ, zOffset); 
        }

        string AxisMoveTray(CPoint cpTray)
        {
            double xOffset = GetTrayOffsetX(cpTray.X);
            double zOffset = GetTrayOffsetZ(cpTray.Y);
            if (Run(AxisMoveZ(ePosZ.TrayBottom, zOffset))) return p_sInfo; 
            return AxisMoveX(ePosX.TrayLeft, xOffset); 
        }
        #endregion

        #region RunLoad
        int m_nTry = 1;
        int m_nShake = 0;
        double m_dzShake = 2;
        string RunLoad(Cleaner.eCleaner eCleaner)
        {
            if (m_trays.p_bFull) return "Run Load Cancel : Tray Full";
            if (m_picker.p_infoStrip != null) return "Run Load Cancel : Picker Already has Strip";
            try
            {
                for (int nTry = 0; nTry < m_nTry; nTry++)
                {
                    if (Run(AxisMoveReady(eCleaner))) return p_sInfo;
                    if (Run(AxisMoveCleaner(eCleaner, 0))) return p_sInfo;
                    if (Run(m_picker.RunVacuum(true))) return p_sInfo; 
                    for (int nShake = 0; nShake < m_nShake; nShake++)
                    {
                        if (Run(AxisMoveCleaner(eCleaner, -m_dzShake))) return p_sInfo;
                        if (Run(AxisMoveCleaner(eCleaner, -m_dzShake / 10))) return p_sInfo;
                    }
                    if (Run(AxisMoveReady(eCleaner))) return p_sInfo;
                    if (m_picker.m_dioVacuum.p_bIn)
                    {
                        m_picker.p_infoStrip = m_aCleaner[eCleaner].p_infoStrip1;
                        m_aCleaner[eCleaner].p_infoStrip1 = null;
                        return "OK"; 
                    }
                }
                return "RunLoad Error"; 
            }
            finally { AxisMoveReady(eCleaner); }
        }

        void RunTreeLoad(Tree tree)
        {
            m_nTry = tree.Set(m_nTry, m_nTry, "Try", "Load Try Count");
            m_nShake = tree.Set(m_nShake, m_nShake, "Shake", "Shake Count");
            m_dzShake = tree.Set(m_dzShake, m_dzShake, "dShake", "Shake Width (unit)"); 
        }
        #endregion

        #region RunUnload
        string RunUnload(CPoint cpTray)
        {
            if (m_trays.p_bFull) return "Run Unload Cancel : Tray Full";
            if (m_picker.p_infoStrip == null) return "Run Unload Cancel : Picker has no Strip";
            if (Run(AxisMoveZ(ePosZ.TrayBottom, GetTrayOffsetZ(cpTray.Y)))) return p_sInfo;
            //forget
            return "OK"; 
        }
        #endregion

        #region Override
        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
//            if (m_picker.m_bLoad)
//            {
//                if (m_boat1.p_bReady) StartRun(m_runUnload);
//            }
//            else
//            {
//                if (m_turnover.p_infoStrip1 != null) StartRun(m_runLoad);
//            }
            return "OK";
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTreeLoad(tree.GetTree("Load", false)); 
            m_picker.RunTree(tree.GetTree("Picker", false));
        }

        public override void Reset()
        {
            AxisMoveReady(Cleaner.eCleaner.Cleaner1);
            base.Reset();
        }
        #endregion

        Dictionary<Cleaner.eCleaner, Cleaner> m_aCleaner;
        Trays m_trays; 
        public Sorter0(string id, IEngineer engineer, Dictionary<Cleaner.eCleaner, Cleaner> aCleaner, Trays trays)
        {
            m_aCleaner = aCleaner;
            m_trays = trays;
            InitPicker();
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            m_picker.ThreadStop();
            base.ThreadStop();
        }

    }
}
