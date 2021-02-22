using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Module
{
    public class Interlock : ModuleBase
    {
        #region ToolBox
        DIO_I m_diEmergency;
        DIO_I m_diCDALow;
        DIO_I m_diVacLow;
        DIO_I m_diDoorLock;
        DIO_I m_diMCReset;
        DIO_I m_diInterlock_key;
        DIO_I m_diEFEMElec_Door;
        DIO_I m_diEFEMAir_Door;
        DIO_I m_diEFEMPC_Door;
        DIO_I m_diEFEMWTR_Door;
        DIO_I m_diVisionPC_Door;
        DIO_I m_diVisionTop_Door;
        DIO_I m_diVisionBtm_Door;
        DIO_I m_diVisionLoof_Door;
        DIO_I m_diEFEMElec_FanAlarm;
        DIO_I m_diEFEMPC_FanAlarm;
        DIO_I m_diEFEMWTR_FanAlarm;
        DIO_I m_diVision4Ch_FanAlarm;
        DIO_I m_diVisionPC_FanAlarm;
        DIO_I m_diVisionTop_FanAlarm;
        DIO_I m_diVisionBtm_FanAlarm;
        DIO_I m_diLPIonizerAlarm;
        DIO_I m_diALIonizerAlarm;
        DIO_I m_diVSIonizerAlarm;

        DIO_O m_doDoorLock;
        DIO_O m_doIonizer;
        DIO_O m_doServoOn;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_diEmergency, this, "Emergency");
            p_sInfo = m_toolBox.Get(ref m_diCDALow, this, "CDA Low");
            p_sInfo = m_toolBox.Get(ref m_diVacLow, this, "VAC Low");
            p_sInfo = m_toolBox.Get(ref m_diDoorLock, this, "Door Lock");
            p_sInfo = m_toolBox.Get(ref m_diMCReset, this, "MC Reset");
            p_sInfo = m_toolBox.Get(ref m_diInterlock_key, this, "Interlock Key");
            p_sInfo = m_toolBox.Get(ref m_diEFEMElec_Door, this, "EFEM Elec Door");
            p_sInfo = m_toolBox.Get(ref m_diEFEMAir_Door, this, "EFEM Air Door");
            p_sInfo = m_toolBox.Get(ref m_diEFEMPC_Door, this, "EFEM PC Door");
            p_sInfo = m_toolBox.Get(ref m_diEFEMWTR_Door, this, "EFEM WTR Door");
            p_sInfo = m_toolBox.Get(ref m_diVisionPC_Door, this, "Vision PC Door");
            p_sInfo = m_toolBox.Get(ref m_diVisionTop_Door, this, "Vision Top Door");
            p_sInfo = m_toolBox.Get(ref m_diVisionBtm_Door, this, "Vision Bottom Door");
            p_sInfo = m_toolBox.Get(ref m_diVisionLoof_Door, this, "Vision Loof Door");
            p_sInfo = m_toolBox.Get(ref m_diEFEMElec_FanAlarm, this, "EFEM Elec Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_diEFEMPC_FanAlarm, this, "EFEM PC Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_diEFEMWTR_FanAlarm, this, "EFEM WTR Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_diVision4Ch_FanAlarm, this, "Vision 4Ch Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_diVisionPC_FanAlarm, this, "Vision PC Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_diVisionTop_FanAlarm, this, "Vision Top Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_diVisionBtm_FanAlarm, this, "Vision Bottom Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_diLPIonizerAlarm, this, "Loadport Ionizer Alarm");
            p_sInfo = m_toolBox.Get(ref m_diALIonizerAlarm, this, "Aligner Ionizer Alarm");
            p_sInfo = m_toolBox.Get(ref m_diVSIonizerAlarm, this, "Vision Ionizer Alarm");

            p_sInfo = m_toolBox.Get(ref m_doDoorLock, this, "Door Lock On/Off");
            p_sInfo = m_toolBox.Get(ref m_doIonizer, this, "Ionizer On/Off");
            p_sInfo = m_toolBox.Get(ref m_doServoOn, this, "Servo On/Off");

            if (bInit)
            {
                InitALID();
                m_doServoOn.Write(true);
            }
        }
        #endregion

        #region GAF
        ALID m_alidEmergency;
        ALID m_alidCDALow;
        ALID m_alidVacLow;
        ALID m_alidDoorLock;
        ALID m_alidMCReset;
        ALID m_alidLPIonizerAlarm;
        ALID m_alidALIonizerAlarm;
        ALID m_alidVSIonizerAlarm;
        void InitALID()
        {
            m_alidEmergency = m_gaf.GetALID(this, "Emergency", "Emergency Error");
            m_alidCDALow = m_gaf.GetALID(this, "CDA Low", "CDA Low Error");
            m_alidVacLow = m_gaf.GetALID(this, "Vac Low", "Vacuum Low Error");
            m_alidDoorLock = m_gaf.GetALID(this, "Door Lock", "Door Lock Error");
            m_alidMCReset = m_gaf.GetALID(this, "MC Reset", "MC Reset Error");
            m_alidLPIonizerAlarm = m_gaf.GetALID(this, "Loadport Ionizer", "Loadport Ionizer Error");
            m_alidALIonizerAlarm = m_gaf.GetALID(this, "Aligner Ionizer", "Aligner Ionizer Error");
            m_alidVSIonizerAlarm = m_gaf.GetALID(this, "Vision Ionizer", "Vision Ionizer Error");
        }

        #endregion

        #region Thread
        public EQ.eState m_eState = EQ.eState.Init;
        protected override void RunThread()
        {
            base.RunThread();
            m_alidEmergency.Run(!m_diEmergency.p_bIn, "Please Check Emergency Sensor");
            m_alidCDALow.Run(!m_diCDALow.p_bIn, "Please Check CDA Low Sensor");
            m_alidVacLow.Run(!m_diVacLow.p_bIn, "Please Check Vac Low Sensor");
            if (m_diInterlock_key.p_bIn)
            {
                m_alidDoorLock.Run(!m_diDoorLock.p_bIn, "Please Check Door Lock");
            }
            m_alidMCReset.Run(!m_diMCReset.p_bIn, "Please Check M/C Reset");
            if (m_bIonizer_Use)
            {
                m_alidLPIonizerAlarm.Run(!m_diLPIonizerAlarm.p_bIn, "Please Check Loadport Ionizer");
                m_alidALIonizerAlarm.Run(!m_diALIonizerAlarm.p_bIn, "Please Check Aligner Ionizer");
                m_alidVSIonizerAlarm.Run(!m_diVSIonizerAlarm.p_bIn, "Please Check Vision Ionizer");
            }
        }
        #endregion

        #region Tree
        bool m_bIonizer_Use = false;
        public override void RunTree(Tree tree)
        {
            RunTreeInterLock(tree.GetTree("Option", false));
            base.RunTree(tree);
        }
        void RunTreeInterLock(Tree tree)
        {
            m_bIonizer_Use = tree.Set(m_bIonizer_Use, m_bIonizer_Use, "Ionizer Use", "Ionizer Use");
        }
        #endregion

        public Interlock(string id, IEngineer engineer)
        {
            p_id = id;
            base.InitBase(id, engineer);
        }
    }
}
