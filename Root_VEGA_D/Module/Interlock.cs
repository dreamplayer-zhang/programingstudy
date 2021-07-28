using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using Root_EFEM.Module;
using Root_VEGA_D.Engineer;
using RootTools.OHT.Semi;
using RootTools.Control.ACS;
using System.Threading;
using System.Collections.Generic;
using System;
using RootTools.OHTNew;

namespace Root_VEGA_D.Module
{
    public class Interlock : ModuleBase
    {
        #region ToolBox
        DIO_I m_diEmergency;
        DIO_I m_diMCReset;
        DIO_I m_diDoorLock;
        DIO_I m_diInterlock_key;
        DIO_I m_diCDA1;
        DIO_I m_diCDA2;
        DIO_I m_diEFEMLeft_Door;
        DIO_I m_diEFEMRight_Door;
        DIO_I m_diActiveIsolator_Alarm;
        DIO_I m_diFP_Isolator;
        public DIO_I p_diFP_Isolator
        {
            get => m_diFP_Isolator;
            set => m_diFP_Isolator = value;
        }
        DIO_I m_diIsolator_VPre;
        public DIO_I p_diIsolator_VPre
        {
            get => m_diIsolator_VPre;
            set => m_diIsolator_VPre = value;
        }
        DIO_I m_diFactory_Air_PadPre;
        public DIO_I p_diFactory_Air_PadPre
        {
            get => m_diFactory_Air_PadPre;
            set => m_diFactory_Air_PadPre = value;
        }
        DIO_I m_diAir_TankPre;
        public DIO_I p_diAir_TankPre
        {
            get => m_diAir_TankPre;
            set => m_diAir_TankPre = value;
        }
        DIO_I m_diX_BottomPre;
        public DIO_I p_diX_BottomPre
        {
            get => m_diX_BottomPre;
            set => m_diX_BottomPre = value;
        }
        DIO_I m_diX_SideMasterPre;
        public DIO_I p_diX_SideMasterPre
        {
            get => m_diX_SideMasterPre;
            set => m_diX_SideMasterPre = value;
        }
        DIO_I m_diX_SideSlavePre;
        public DIO_I p_diX_SideSlavePre
        {
            get => m_diX_SideSlavePre;
            set => m_diX_SideSlavePre = value;
        }
        DIO_I m_diY_BottomPre;
        public DIO_I p_diY_BottomPre
        {
            get => m_diY_BottomPre;
            set => m_diY_BottomPre = value;
        }
        DIO_I m_diY_SideMasterPre;
        public DIO_I p_diY_SideMasterPre
        {
            get => m_diY_SideMasterPre;
            set => m_diY_SideMasterPre = value;
        }
        DIO_I m_diY_SideSlavePre;
        public DIO_I p_diY_SideSlavePre
        {
            get => m_diY_SideSlavePre;
            set => m_diY_SideSlavePre = value;
        }
        DIO_I m_diVisionFFU_Door;
        DIO_I m_diVisionTop_Door;
        DIO_I m_diVisionBtm_Door;
        DIO_I m_diVisionSlide_Door;
        DIO_I m_diPC1Fan_Alarm;
        DIO_I m_diPC2Fan_Alarm;
        DIO_I m_di4ChLEDFan_Alarm;
        DIO_I m_diEFEMFan_Alarm;
        DIO_I m_diELECRackFan_Alarm;
        DIO_I m_diPiezo_Alarm;
        DIO_I m_diLightCurtain;
        DIO_I m_diProtectionbar;

        DIO_O m_doDoorLock;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetDIO(ref m_diEmergency, this, "Emergency");
            p_sInfo = m_toolBox.GetDIO(ref m_diMCReset, this, "MC Reset");
            p_sInfo = m_toolBox.GetDIO(ref m_diDoorLock, this, "Door Lock");
            p_sInfo = m_toolBox.GetDIO(ref m_diInterlock_key, this, "Interlock Key");
            p_sInfo = m_toolBox.GetDIO(ref m_diCDA1, this, "CDA1 Pressure");
            p_sInfo = m_toolBox.GetDIO(ref m_diCDA2, this, "CDA2 Pressure");
            p_sInfo = m_toolBox.GetDIO(ref m_diEFEMLeft_Door, this, "EFEM Left Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diEFEMRight_Door, this, "EFEM Right Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diActiveIsolator_Alarm, this, "Active Isolator Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diFP_Isolator, this, "Factory Pressure Isolator Indicator");
            p_sInfo = m_toolBox.GetDIO(ref m_diIsolator_VPre, this, "Isolator V Pressure Indicator");
            p_sInfo = m_toolBox.GetDIO(ref m_diFactory_Air_PadPre, this, "Factory Air Pad Pressure Indicator");
            p_sInfo = m_toolBox.GetDIO(ref m_diAir_TankPre, this, "Air Tank Pressure Indicator");
            p_sInfo = m_toolBox.GetDIO(ref m_diX_BottomPre, this, "X Bottom Pressure Indicator");
            p_sInfo = m_toolBox.GetDIO(ref m_diX_SideMasterPre, this, "X Side Master Pressure Indicator");
            p_sInfo = m_toolBox.GetDIO(ref m_diX_SideSlavePre, this, "X Side Slave Pressure Indicator");
            p_sInfo = m_toolBox.GetDIO(ref m_diY_BottomPre, this, "Y Bottom Pressure Indicator");
            p_sInfo = m_toolBox.GetDIO(ref m_diY_SideMasterPre, this, "Y Side Master Pressure Indicator");
            p_sInfo = m_toolBox.GetDIO(ref m_diY_SideSlavePre, this, "Y Side Slave Pressure Indicator");
            p_sInfo = m_toolBox.GetDIO(ref m_diVisionFFU_Door, this, "Vision FFU Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diVisionTop_Door, this, "Vision Top Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diVisionBtm_Door, this, "Vision Bottom Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diVisionSlide_Door, this, "Vision Slide Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diPC1Fan_Alarm, this, "PC1 Fan Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diPC2Fan_Alarm, this, "PC2 Fan Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_di4ChLEDFan_Alarm, this, "4Ch LED Fan Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diEFEMFan_Alarm, this, "EFEM Fan Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diELECRackFan_Alarm, this, "Elec Rack Fan Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diPiezo_Alarm, this, "Piezo Alarm");
            p_sInfo = m_toolBox.GetDIO(ref m_diLightCurtain, this, "Light Curtain");
            p_sInfo = m_toolBox.GetDIO(ref m_diProtectionbar, this, "Protectionbar Sensor");
            p_sInfo = m_toolBox.GetDIO(ref m_doDoorLock, this, "Door Lock On/Off");

            if (bInit)
            {
                InitALID();
            }
        }
        #endregion

        #region GAF
        ALID m_alidEmergency;
        ALID m_alidMCReset_EMS;
        ALID m_alidMCReset_EMO;
        ALID m_alidDoorLock;
        ALID m_alidCDA1_digital;
        ALID m_alidCDA1_Low;
        ALID m_alidCDA1_High;
        ALID m_alidCDA2_digital;
        ALID m_alidCDA2_Low;
        ALID m_alidCDA2_High;
        ALID m_alidEFEMLeft_Door;
        ALID m_alidEFEMRight_Door;
        ALID m_alidActiveIsolator_Alarm;
        ALID m_alidFP_Isolator;
        ALID m_alidIsolator_VPre;
        ALID m_alidFactory_Air_PadPre;
        ALID m_alidAir_TankPre;
        ALID m_alidX_BottomPre;
        ALID m_alidX_SideMasterPre;
        ALID m_alidX_SideSlavePre;
        ALID m_alidY_BottomPre;
        ALID m_alidY_SideMasterPre;
        ALID m_alidY_SideSlavePre;
        ALID m_alidVisionFFU_Door;
        ALID m_alidVisionTop_Door;
        ALID m_alidVisionBtm_Door;
        ALID m_alidVisionSlide_Door;
        ALID m_alidPC1Fan_Alarm;
        ALID m_alidPC2Fan_Alarm;
        ALID m_alid4ChLEDFan_Alarm;
        ALID m_alidEFEMFan_Alarm;
        ALID m_alidElecRackFan_Alarm;
        ALID m_alidPiezo_Alarm;
        ALID m_alidLightCurtain;
        ALID m_alidProtectionbar;
        void InitALID()
        {
            m_alidEmergency = m_gaf.GetALID(this, "Emergency", "Emergency Error");
            m_alidMCReset_EMS = m_gaf.GetALID(this, "MC Reset (EMS)", "MC Reset Error");
            m_alidMCReset_EMO = m_gaf.GetALID(this, "MC Reset (EMO)", "MC Reset Error");
            m_alidDoorLock = m_gaf.GetALID(this, "Door Lock", "Door Lock Error");
            m_alidCDA1_digital = m_gaf.GetALID(this, "CDA1 Pressure Digital", "CDA1 Pressure Error(Digital)");
            m_alidCDA1_Low = m_gaf.GetALID(this, "CDA1 Pressure Low Alarm", "CDA1 Pressure Low Error");
            m_alidCDA1_High = m_gaf.GetALID(this, "CDA1 Pressure Low Alarm", "CDA1 Pressure Low Error");
            m_alidCDA2_digital = m_gaf.GetALID(this, "CDA2 Pressure Digital", "CDA2 Pressure Error(Digital)");
            m_alidCDA2_Low = m_gaf.GetALID(this, "CDA2 Pressure", "CDA2 Pressure Low Error");
            m_alidCDA2_High = m_gaf.GetALID(this, "CDA2 Pressure", "CDA2 Pressure High Error");
            m_alidEFEMLeft_Door = m_gaf.GetALID(this, "EFEM Left Door", "EFEM Left Door Open");
            m_alidEFEMRight_Door = m_gaf.GetALID(this, "EFEM Right Door", "EFEM Right Door Open");
            m_alidVisionFFU_Door = m_gaf.GetALID(this, "Vision FFU Door", "Vision FFU Door Open");
            m_alidVisionTop_Door = m_gaf.GetALID(this, "Vision Top Door", "Vision Top Door Open");
            m_alidFP_Isolator = m_gaf.GetALID(this, "Factory Pressure Isolator Error", "Factory Pressure Isolator Error");
            m_alidIsolator_VPre = m_gaf.GetALID(this, "Isolator V Pressure Error", "Isolator V Pressure Error");
            m_alidFactory_Air_PadPre = m_gaf.GetALID(this, "Factory Air PAd Pressure Error", "Factory Air Pad Pressure Error");
            m_alidAir_TankPre = m_gaf.GetALID(this, "Air Tank Pressure Error", "Air Tank Pressure Error");
            m_alidX_BottomPre = m_gaf.GetALID(this, "X Bottom Pressure Error", "X Bottom Pressure Error");
            m_alidX_SideMasterPre = m_gaf.GetALID(this, "X Side Master Pressure Error", "X Side Master Pressure Error");
            m_alidX_SideSlavePre = m_gaf.GetALID(this, "X Side Slave Pressure Error", "X Side Slave Pressure Error");
            m_alidY_BottomPre = m_gaf.GetALID(this, "Y Bottom Pressure Error", "Y Bottom Pressure Error");
            m_alidY_SideMasterPre = m_gaf.GetALID(this, "Y Side Master Pressure Error", "Y Side Master Pressure Error");
            m_alidY_SideSlavePre = m_gaf.GetALID(this, "Y Side Slave Pressure Error", "Y Side Slave Pressure Error");
            m_alidVisionBtm_Door = m_gaf.GetALID(this, "Vision Btm Door", "Vision Bottom Door Open");
            m_alidVisionSlide_Door = m_gaf.GetALID(this, "Vision Slide Door", "Vision Slide Door Open");
            m_alidActiveIsolator_Alarm = m_gaf.GetALID(this, "Active Isolator", "Active Isolator Alarm");
            m_alidPC1Fan_Alarm = m_gaf.GetALID(this, "PC1 Fan", "PC1 Fan Alarm");
            m_alidPC2Fan_Alarm = m_gaf.GetALID(this, "PC2 Fan", "PC2 Fan Alarm");
            m_alid4ChLEDFan_Alarm = m_gaf.GetALID(this, "4Ch LED Fan", "4Ch LED Fan Alarm");
            m_alidEFEMFan_Alarm = m_gaf.GetALID(this, "EFEM Fan", "EFEM Fan Alarm");
            m_alidElecRackFan_Alarm = m_gaf.GetALID(this, "Elec Rack Fan", "Elec Rack Fan Alarm");
            m_alidPiezo_Alarm = m_gaf.GetALID(this, "Piezo", "Piezo Alarm");
            m_alidLightCurtain = m_gaf.GetALID(this, "Light Curtain", "Light Curtain Check");
            m_alidProtectionbar = m_gaf.GetALID(this, "Protectionbar", "Protectionbar Check Error");
        }
        #endregion
        #region FDC
        enum eAnalog_CDA
        {
            CDA1 =0,
            CDA2
        }

        public double m_CDA1_Value;
        public double p_CDA1_Value
        {
            get
            {
                return m_CDA1_Value;
            }
            set
            {
               // if (Math.Abs(m_CDA1_Value - value) < 0.05) return;
                m_CDA1_Value = value;
                OnPropertyChanged();
            }
        }
        public double m_CDA2_Value;
        public double p_CDA2_Value
        {
            get
            {
                return m_CDA2_Value;
            }
            set
            {
               // if (Math.Abs(m_CDA2_Value - value) < 0.05) return;
                m_CDA2_Value = value;
                OnPropertyChanged();
            }
        }
        public void CheckFDC()
		{
			Thread.Sleep(10);
			if (m_ACS.p_id != null)
			{
				try
				{
                    p_CDA1_Value = Math.Truncate(m_ACS.GetAnalogData((int)eAnalog_CDA.CDA1) / 32768 * 1000) / 1000;
                    p_CDA2_Value = Math.Truncate(m_ACS.GetAnalogData((int)eAnalog_CDA.CDA2) / 32768 * 1000) / 1000;
                }
				catch (Exception e) { m_log.Info("FDC Error " + e.Message); }
				if (p_CDA1_Value < m_mmLimitCDA1.X)
				{ m_alidCDA1_Low.Run(true, "CDA1_Value Pressure Lower than Limit"); }
				if (p_CDA1_Value > m_mmLimitCDA1.Y)
				{ m_alidCDA1_High.Run(true, "CDA1_Value Pressure Higher than Limit"); }
				if (p_CDA2_Value < m_mmLimitCDA2.X)
				{ m_alidCDA2_Low.Run(true, "CDA2_Value Pressure Lower than Limit"); }
				if (p_CDA2_Value > m_mmLimitCDA2.Y)
				{ m_alidCDA2_High.Run(true, "CDA2_Value Pressure Higher than Limit"); }
			}
		}
        #endregion
        #region Thread
        EQ.eState m_eStateLast;
        protected override void RunThread()
        {
            base.RunThread();
            //m_alidEmergency.Run(!m_diEmergency.p_bIn, "Please Check Emergency Sensor");
            if (!m_diMCReset.p_bIn && !m_diEmergency.p_bIn)
            {
                m_alidEmergency.Run(true, "Please Check Emergency Sensor");
                Thread.Sleep(100);
                if (m_ACS.p_bConnect == false) m_alidMCReset_EMO.Run(!m_diMCReset.p_bIn, "Please Check M/C Reset (EMO)");
                else m_alidMCReset_EMS.Run(!m_diMCReset.p_bIn, "Please Check M/C Reset (EMS)");
            }
            if (m_diInterlock_key.p_bIn)
            {
                m_alidDoorLock.Run(!m_diDoorLock.p_bIn, "Please Check Door Lock");
            }
            CheckFDC();

            m_alidCDA1_digital.Run(!m_diCDA1.p_bIn, "Please Check CDA1 Pressure Sensor");
            m_alidCDA2_digital.Run(!m_diCDA2.p_bIn, "Please Check CDA2 Pressure Sensor");

            m_alidEFEMLeft_Door.Run(!m_diEFEMLeft_Door.p_bIn, "EFEM Left Door Open");
            m_alidEFEMRight_Door.Run(!m_diEFEMRight_Door.p_bIn, "EFEM Right Door Open");
            m_alidVisionFFU_Door.Run(!m_diVisionFFU_Door.p_bIn, "Vision FFU Door Open");
            m_alidVisionTop_Door.Run(!m_diVisionTop_Door.p_bIn, "Vision Top Door Open");
            m_alidVisionBtm_Door.Run(!m_diVisionBtm_Door.p_bIn, "Vision Bottom Door Open");
            m_alidVisionSlide_Door.Run(m_diVisionSlide_Door.p_bIn, "Vision Slide Door Open");

            m_alidFP_Isolator.Run(!m_diFP_Isolator.p_bIn, "Factory Pressure Isolator Alarm");
            m_alidIsolator_VPre.Run(!m_diIsolator_VPre.p_bIn, "Isolator V Alarm");
            m_alidFactory_Air_PadPre.Run(!m_diFactory_Air_PadPre.p_bIn, "Factory Air Pad Pressure Alarm");
            m_alidAir_TankPre.Run(!m_diAir_TankPre.p_bIn, "Air Tank Alarm");
            m_alidX_BottomPre.Run(!m_diX_BottomPre.p_bIn, "X Bottom Pressure Alarm");
            m_alidX_SideMasterPre.Run(!m_diX_SideMasterPre.p_bIn, "X Side Master Pressure Alarm");
            m_alidX_SideSlavePre.Run(!m_diX_SideSlavePre.p_bIn, "X Side Slave Pressure Alarm");
            m_alidY_BottomPre.Run(!m_diY_BottomPre.p_bIn, "Y Bottom Pressure Alarm");
            m_alidY_SideMasterPre.Run(!m_diY_SideMasterPre.p_bIn, "Y Side Master Pressure Alarm");
            m_alidY_SideSlavePre.Run(!m_diY_SideSlavePre.p_bIn, "Y Side Slave Pressure Alarm");
            m_alidActiveIsolator_Alarm.Run(!m_diActiveIsolator_Alarm.p_bIn, "Active Isolator Alarm");
            m_alidPC1Fan_Alarm.Run(!m_diPC1Fan_Alarm.p_bIn, "PC1 Fan Alarm");
            m_alidPC2Fan_Alarm.Run(!m_diPC2Fan_Alarm.p_bIn, "PC2 Fan Alarm");
            m_alid4ChLEDFan_Alarm.Run(!m_di4ChLEDFan_Alarm.p_bIn, "4Ch LEF Fan Alarm");
            m_alidEFEMFan_Alarm.Run(!m_diEFEMFan_Alarm.p_bIn, "EFEM Fan Alarm");
            m_alidElecRackFan_Alarm.Run(!m_diELECRackFan_Alarm.p_bIn, "Elec Rack Fan Alarm");
            m_alidPiezo_Alarm.Run(!m_diPiezo_Alarm.p_bIn, "Piezo Alarm");

            m_eStateLast = EQ.p_eState;
            if (m_bDoorlock_Use == true && (m_eStateLast != EQ.p_eState))
            {
                if (EQ.p_eState == EQ.eState.Run) m_doDoorLock.Write(true);
                else m_doDoorLock.Write(false);
            }

            if (m_bLightCurtain_Use)
            {
                m_alidLightCurtain.Run(!m_diLightCurtain.p_bIn, "Light Curtain Detect Error"); 
            }
            if (m_bProtectionbar_Use)
            {
                m_alidProtectionbar.Run(!m_diProtectionbar.p_bIn, "Protectionbar Check Error");
            }
            foreach (OHT_Semi OHT in p_aOHT)
            {
                OHT.p_bLightCurtain = !m_diLightCurtain.p_bIn;
                OHT.P_bProtectionBar = !m_diProtectionbar.p_bIn;
            }
        }
        #endregion

        #region OHT
        List<OHT_Semi> p_aOHT
        {
            get
            {
                List<OHT_Semi> aOHT = new List<OHT_Semi>();
                VEGA_D_Handler handler = (VEGA_D_Handler)m_engineer.ClassHandler();
                foreach (ILoadport loadport in handler.m_aLoadport)
                {
                    aOHT.Add(((Loadport_Cymechs)loadport).m_OHT);
                }
                return aOHT;
            }
        }
        #endregion
        #region Tree
        bool m_bDoorlock_Use = false;
        bool m_bLightCurtain_Use = false;
        bool m_bProtectionbar_Use = false;
        public override void RunTree(Tree tree)
        {
            RunTreeInterLock(tree.GetTree("Option", false));
            RunTreeFDC(tree.GetTree("FDC Module", false));
            base.RunTree(tree);
        }
        void RunTreeFDC(Tree tree)
        {
            m_bDoorlock_Use = tree.Set(m_bDoorlock_Use, m_bDoorlock_Use, "Doorlock Use", "Doorlock Use");
            m_mmLimitCDA1 = tree.Set(m_mmLimitCDA1, m_mmLimitCDA1, "Limit", "FDC CDA1 Lower & Upper Limit");
            m_mmLimitCDA2 = tree.Set(m_mmLimitCDA2, m_mmLimitCDA2, "Limit", "FDC CDA2 Lower & Upper Limit");
        }

        void RunTreeInterLock(Tree tree)
        {
            m_bLightCurtain_Use = tree.Set(m_bLightCurtain_Use, m_bLightCurtain_Use, "LightCurtain Use", "LightCurtain Use");
            m_bProtectionbar_Use = tree.Set(m_bProtectionbar_Use, m_bProtectionbar_Use, "Protectionbar Use", "Protectionbar Use");
        }
        #endregion
        public RPoint m_mmLimitCDA1 = new RPoint();//EFEM
        public RPoint m_mmLimitCDA2 = new RPoint();//INSPECT
        ALID[] m_alid = new ALID[2] { null, null };
        ACS m_ACS;
        public Interlock(string id, IEngineer engineer,ACS acs)
        {
            p_id = id;
            m_ACS = acs;
            base.InitBase(id, engineer);
        }
    }
}
