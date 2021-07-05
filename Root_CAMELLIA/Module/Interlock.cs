using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        DIO_I m_diProtectionbar;

        DIO_O m_doDoorLock;
        DIO_O m_doIonizer;
        DIO_O m_doServoOn;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetDIO(ref m_diEmergency, this, "Emergency");
            p_sInfo = m_toolBox.GetDIO(ref m_diCDALow, this, "CDA Low");
            p_sInfo = m_toolBox.GetDIO(ref m_diVacLow, this, "VAC Low");
            p_sInfo = m_toolBox.GetDIO(ref m_diDoorLock, this, "Door Lock");
            p_sInfo = m_toolBox.GetDIO(ref m_diMCReset, this, "MC Reset");
            p_sInfo = m_toolBox.GetDIO(ref m_diInterlock_key, this, "Interlock Key");
            p_sInfo = m_toolBox.GetDIO(ref m_diEFEMElec_Door, this, "EFEM Elec Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diEFEMAir_Door, this, "EFEM Air Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diEFEMPC_Door, this, "EFEM PC Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diEFEMWTR_Door, this, "EFEM WTR Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diVisionPC_Door, this, "Vision PC Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diVisionTop_Door, this, "Vision Top Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diVisionBtm_Door, this, "Vision Bottom Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diVisionLoof_Door, this, "Vision Loof Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diEFEMElec_FanAlarm, this, "EFEM Elec Fan Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diEFEMPC_FanAlarm, this, "EFEM PC Fan Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diEFEMWTR_FanAlarm, this, "EFEM WTR Fan Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diVision4Ch_FanAlarm, this, "Vision 4Ch Fan Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diVisionPC_FanAlarm, this, "Vision PC Fan Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diVisionTop_FanAlarm, this, "Vision Top Fan Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diVisionBtm_FanAlarm, this, "Vision Bottom Fan Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diLPIonizerAlarm, this, "Loadport Ionizer Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diALIonizerAlarm, this, "Aligner Ionizer Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diVSIonizerAlarm, this, "Vision Ionizer Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diProtectionbar, this, "Protection bar");

            p_sInfo = m_toolBox.GetDIO(ref m_doDoorLock, this, "Door Lock On/Off");
            p_sInfo = m_toolBox.GetDIO(ref m_doIonizer, this, "Ionizer On/Off");
            p_sInfo = m_toolBox.GetDIO(ref m_doServoOn, this, "Servo On/Off");

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
        ALID m_alidMCReset_EMS;
        ALID m_alidMCReset_EMO;
        ALID m_alidLPIonizerAlarm;
        ALID m_alidALIonizerAlarm;
        ALID m_alidVSIonizerAlarm;
        ALID m_alidEFEMElec_Door;
        ALID m_alidEFEMAir_Door;
        ALID m_alidEFEMPC_Door;
        ALID m_alidEFEMWTR_Door;
        ALID m_alidVisionPC_Door;
        ALID m_alidVisionTop_Door;
        ALID m_alidVisionBtm_Door;
        ALID m_alidVisionLoof_Door;
        ALID m_alidEFEMElec_FanAlarm;
        ALID m_alidEFEMPC_FanAlarm;
        ALID m_alidEFEMWTR_FanAlarm;
        ALID m_alidVision4Ch_FanAlarm;
        ALID m_alidVisionPC_FanAlarm;
        ALID m_alidVisionTop_FanAlarm;
        ALID m_alidVisionBtm_FanAlarm;
        ALID m_alidProtectionbar;
        void InitALID()
        {
            m_alidEmergency = m_gaf.GetALID(this, "Emergency", "Emergency Error");
            m_alidCDALow = m_gaf.GetALID(this, "CDA Low", "CDA Low Error");
            m_alidVacLow = m_gaf.GetALID(this, "Vac Low", "Vacuum Low Error");
            m_alidDoorLock = m_gaf.GetALID(this, "Door Lock", "Door Lock Error");
            m_alidMCReset_EMS = m_gaf.GetALID(this, "MC Reset (EMS)", "MC Reset Error");
            m_alidMCReset_EMO = m_gaf.GetALID(this, "MC Reset (EMO)", "MC Reset Error");
            m_alidLPIonizerAlarm = m_gaf.GetALID(this, "Loadport Ionizer", "Loadport Ionizer Error");
            m_alidALIonizerAlarm = m_gaf.GetALID(this, "Aligner Ionizer", "Aligner Ionizer Error");
            m_alidVSIonizerAlarm = m_gaf.GetALID(this, "Vision Ionizer", "Vision Ionizer Error");

            m_alidEFEMElec_Door = m_gaf.GetALID(this, "EFEM Elec Door", "EFEM Elec Door Open");
            m_alidEFEMAir_Door = m_gaf.GetALID(this, "EFEM Air Door", "EFEM Air Door Open");
            m_alidEFEMPC_Door = m_gaf.GetALID(this, "EFEM PC Door", "EFEM PC Door Open");
            m_alidEFEMWTR_Door = m_gaf.GetALID(this, "EFEM WTR Door", "EFEM WTR Door Open");
            m_alidVisionPC_Door = m_gaf.GetALID(this, "Vision PC Door", "Vision PC Door Open");
            m_alidVisionTop_Door = m_gaf.GetALID(this, "Vision Top Door", "Vision Top Door Open");
            m_alidVisionBtm_Door = m_gaf.GetALID(this, "Vision Btm Door", "Vision Bottom Door Open");
            m_alidVisionLoof_Door = m_gaf.GetALID(this, "Vision Loof Door", "Vision Loof Door Open");
            m_alidEFEMElec_FanAlarm = m_gaf.GetALID(this, "EFEM Elec Fan", "EFEM Elec Fan Alarm");
            m_alidEFEMPC_FanAlarm = m_gaf.GetALID(this, "EFEM PC Fan", "EFEM PC Fan Alarm");
            m_alidEFEMWTR_FanAlarm = m_gaf.GetALID(this, "EFEM WTR Fan", "EFEM WTR Fan Alarm");
            m_alidVision4Ch_FanAlarm = m_gaf.GetALID(this, "Vision 4Ch Fan", "Vision 4Ch Fan Alarm");
            m_alidVisionPC_FanAlarm = m_gaf.GetALID(this, "Vision PC Door Fan", "Vision PC Door Fan Alarm");
            m_alidVisionTop_FanAlarm = m_gaf.GetALID(this, "Vision Top Fan", "Vision Top Fan Alarm");
            m_alidVisionBtm_FanAlarm = m_gaf.GetALID(this, "Vision Btm Fan", "Vision Bottom Fan Alarm");

            m_alidProtectionbar = m_gaf.GetALID(this, "Protectionbar", "Protection Bar");
        }

        #endregion

        #region Thread
        protected override void RunThread()
        {
            base.RunThread();
            m_alidEmergency.Run(!m_diEmergency.p_bIn, "Please Check Emergency Sensor");
            m_alidCDALow.Run(!m_diCDALow.p_bIn, "Please Check CDA Low Sensor");
            m_alidVacLow.Run(!m_diVacLow.p_bIn, "Please Check Vac Low Sensor");
            if (m_diInterlock_key.p_bIn)
            {
                if (!m_doDoorLock.p_bOut)
                    m_doDoorLock.Write(true);
            }
            else
            {
                m_alidDoorLock.Run(!m_diDoorLock.p_bIn, "Please Check Door Lock");
            }
            if (!m_diMCReset.p_bIn)
            {
                Thread.Sleep(100);
                if (!m_diCDALow.p_bIn) m_alidMCReset_EMO.Run(!m_diMCReset.p_bIn, "Please Check M/C Reset (EMO)");
                else m_alidMCReset_EMS.Run(!m_diMCReset.p_bIn, "Please Check M/C Reset (EMS)");
            }
            if (m_bIonizer_Use)
            {
                m_alidLPIonizerAlarm.Run(!m_diLPIonizerAlarm.p_bIn, "Please Check Loadport Ionizer");
                m_alidALIonizerAlarm.Run(!m_diALIonizerAlarm.p_bIn, "Please Check Aligner Ionizer");
                m_alidVSIonizerAlarm.Run(!m_diVSIonizerAlarm.p_bIn, "Please Check Vision Ionizer");
            }
            if (m_bProtectionbar_Use)
            {
                m_alidProtectionbar.Run(!m_diProtectionbar.p_bIn, "Protectionbar Check");
            }

            //Door
            m_alidEFEMElec_Door.Run(!m_diEFEMElec_Door.p_bIn, "EFEM Elec Door Open");
            m_alidEFEMAir_Door.Run(!m_diEFEMAir_Door.p_bIn, "EFEM Air Door Open");
            m_alidEFEMPC_Door.Run(!m_diEFEMPC_Door.p_bIn, "EFEM PC Door Open");
            m_alidEFEMWTR_Door.Run(!m_diEFEMWTR_Door.p_bIn, "EFEM WTR Door Open");
            m_alidVisionPC_Door.Run(!m_diVisionPC_Door.p_bIn, "Vision PC Door Open");
            m_alidVisionTop_Door.Run(!m_diVisionTop_Door.p_bIn, "Vision Top Door Open");
            m_alidVisionBtm_Door.Run(!m_diVisionBtm_Door.p_bIn, "Vision Bottom Door Open");
            m_alidVisionLoof_Door.Run(!m_diVisionLoof_Door.p_bIn, "Vision Loof Door Open");
            m_alidEFEMElec_FanAlarm.Run(!m_diEFEMElec_FanAlarm.p_bIn, "EFEM Elec Fan Alarm");
            m_alidEFEMPC_FanAlarm.Run(!m_diEFEMPC_FanAlarm.p_bIn, "EFEM PC Fan Alarm");
            m_alidEFEMWTR_FanAlarm.Run(!m_diEFEMWTR_FanAlarm.p_bIn, "EFEM WTR Fan Alarm");
            m_alidVision4Ch_FanAlarm.Run(!m_diVision4Ch_FanAlarm.p_bIn, "Vision 4Ch Fan Alarm");
            m_alidVisionPC_FanAlarm.Run(!m_diVisionPC_FanAlarm.p_bIn, "Vision PC Fan Alarm");
            m_alidVisionTop_FanAlarm.Run(!m_diVisionTop_FanAlarm.p_bIn, "Vision Top Fan Alarm");
            m_alidVisionBtm_FanAlarm.Run(!m_diVisionBtm_FanAlarm.p_bIn, "Vision Bottom Fan Alarm");
        }
        #endregion

        #region Tree
        bool m_bIonizer_Use = false;
        bool m_bProtectionbar_Use = false;
        public override void RunTree(Tree tree)
        {
            RunTreeInterLock(tree.GetTree("Option", false));
            base.RunTree(tree);
        }
        void RunTreeInterLock(Tree tree)
        {
            m_bIonizer_Use = tree.Set(m_bIonizer_Use, m_bIonizer_Use, "Ionizer Use", "Ionizer Use");
            m_bProtectionbar_Use = tree.Set(m_bProtectionbar_Use, m_bProtectionbar_Use, "Protectionbar Use", "Protectionbar Use");
        }
        #endregion

        public Interlock(string id, IEngineer engineer)
        {
            p_id = id;
            base.InitBase(id, engineer);
        }
    }
}
