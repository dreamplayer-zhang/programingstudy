using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools_Vision;

namespace Root_WindII
{
    public class WIND2 : ModuleBase
    {
        public TK4SGroup m_tk4s;
        public FFU_Group m_FFUGourp;
        DIO_I di_Door_VSMainPanel;
        DIO_I di_Door_VSAxisPanel;
        DIO_I di_Door_VSRearTop;
        DIO_I di_Door_VSTop;
        DIO_I di_Door_VSBottom;
        DIO_I di_Door_EFEMElecPanel;
        DIO_I di_Door_EFEMTop;
        DIO_I di_EMO;
        DIO_I di_CDA;
        DIO_I di_VAC1;
        DIO_I di_VAC2;
        DIO_I di_MCReset;
        DIO_I di_Ionizer_LP;
        DIO_I di_Ionizer_Edge;
        DIO_I di_Ionizer_VS;
        DIO_I di_ProtectionBar;
        DIO_O do_door_Lock;
        DIO_O do_SERVOON;

        ALID alid_EMS;
        ALID alid_Ionizer;
        ALID alid_CDA;
        ALID alid_VAC1;
        ALID alid_VAC2;
        ALID alid_MCRESET;
        public DIO_Os m_doLamp;
        string[] asLamp = Enum.GetNames(typeof(eLamp));

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetDIO(ref m_doLamp, this, "Tower Lamp", asLamp);
            m_toolBox.Get(ref m_tk4s, this, "FDC", GlobalObjects.Instance.Get<DialogService>());
            m_toolBox.Get(ref m_FFUGourp, this, "FFU", GlobalObjects.Instance.Get<DialogService>());
            p_sInfo = m_toolBox.GetDIO(ref di_Door_VSMainPanel, this, "Vision Main Panel Door");
            p_sInfo = m_toolBox.GetDIO(ref di_Door_VSAxisPanel, this, "Vision Axis Panel Door");
            p_sInfo = m_toolBox.GetDIO(ref di_Door_VSRearTop, this, "Vision Rear Top Door");
            p_sInfo = m_toolBox.GetDIO(ref di_Door_VSTop, this, "Vision Top Door");
            p_sInfo = m_toolBox.GetDIO(ref di_Door_VSBottom, this, "Vision Bottom Door");
            p_sInfo = m_toolBox.GetDIO(ref di_Door_EFEMElecPanel, this, "EFEM Elec PanelDoor");
            p_sInfo = m_toolBox.GetDIO(ref di_Door_EFEMTop, this, "EFEM Top Door");
            
            p_sInfo = m_toolBox.GetDIO(ref di_EMO, this, "Emergency");
            p_sInfo = m_toolBox.GetDIO(ref di_CDA, this, "CDA");
            p_sInfo = m_toolBox.GetDIO(ref di_VAC1, this, "VAC1");
            p_sInfo = m_toolBox.GetDIO(ref di_VAC2, this, "VAC2");
            p_sInfo = m_toolBox.GetDIO(ref di_MCReset, this, "MCRESET");
            p_sInfo = m_toolBox.GetDIO(ref di_Ionizer_LP, this, "Ionizer LP");
            p_sInfo = m_toolBox.GetDIO(ref di_Ionizer_Edge, this, "Ionizer Edge");
            p_sInfo = m_toolBox.GetDIO(ref di_Ionizer_VS, this, "Ionizer VS");
            p_sInfo = m_toolBox.GetDIO(ref di_ProtectionBar, this, "Protection Bar");
            p_sInfo = m_toolBox.GetDIO(ref do_door_Lock, this, "Door Lock");
            p_sInfo = m_toolBox.GetDIO(ref do_SERVOON, this, "Servo On");

            alid_EMS = m_gaf.GetALID(this, "EMS", "EMS ERROR");
            alid_Ionizer = m_gaf.GetALID(this, "Ionizer", "Ionizer ERROR");
            alid_CDA = m_gaf.GetALID(this, "CDA", "CDA ERROR");
            alid_VAC1 = m_gaf.GetALID(this, "VAC 1", "VAC 1 Error");
            alid_VAC2 = m_gaf.GetALID(this, "VAC 2", "VAC 2 Error");
            alid_MCRESET = m_gaf.GetALID(this, "MCReset", "MC Reset Error");
        }

        Thread m_threadCheck;
        bool m_bThreadCheck = true;

        public WIND2(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
            m_tk4s.OnDetectLimit += M_tk4s_OnDetectLimit;
            m_FFUGourp.OnDetectLimit += M_FFUGourp_OnDetectLimit;
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadCheck.Start();
        }


        void RunThreadCheck()
        {
            m_bThreadCheck = true;
            Thread.Sleep(1000);
            while (m_bThreadCheck)
            {
                Thread.Sleep(1000);
                LampProcess();
                DoorCheck();
                FanCheck();
                AlarmCheck();
            }
        }

        public enum eLamp
        {
            Red,
            Yellow,
            Green
        }

        StopWatch m_swLamp = new StopWatch();
        private void LampProcess()
        {
            if (EQ.p_bSimulate)
                return;


            switch (EQ.p_eState)
            {
                case EQ.eState.Init:
                    m_doLamp.Write(eLamp.Red);
                    break;
                case EQ.eState.Home:
                    m_doLamp.Write(eLamp.Red);
                    m_doLamp.Write(eLamp.Yellow);
                    m_doLamp.Write(eLamp.Green);
                    break;
                case EQ.eState.Ready:
                    m_doLamp.Write(eLamp.Yellow);
                    break;
                case EQ.eState.Idle:
                    m_doLamp.Write(eLamp.Yellow);
                    break;
                case EQ.eState.Run:
                    do_door_Lock.Write(true);
                    m_doLamp.Write(eLamp.Green);
                    break;
                case EQ.eState.Recovery:
                    //do_door_Lock.Write(true);
                    m_doLamp.Write(eLamp.Red);
                    m_doLamp.Write(eLamp.Yellow);
                    m_doLamp.Write(eLamp.Green);
                    break;
                case EQ.eState.Error:
                    //do_Buzzer.Write(eBuzzer.Buzzer1);
                    m_doLamp.Write(eLamp.Red);
                    break;
            }
        }

        private void AlarmCheck()
        {
            do_SERVOON.Write(true);
            if (!di_EMO.p_bIn)
            {
                this.p_eState = eState.Error;
                EQ.p_eState = EQ.eState.Error;
                EQ.p_bStop = true;
                alid_EMS.Run(!di_EMO.p_bIn, "Please Check the Emergency Buttons");
            }
            else if (!di_EMO.p_bIn)
            {
                this.p_eState = eState.Error;
                EQ.p_eState = EQ.eState.Error;
                EQ.p_bStop = true;
                alid_Ionizer.Run(!di_EMO.p_bIn, "Please Check Ionizer State");
            }
            else if (!di_CDA.p_bIn)
            {
                this.p_eState = eState.Error;
                EQ.p_eState = EQ.eState.Error;
                EQ.p_bStop = true;
                alid_CDA.Run(!di_CDA.p_bIn, "Please Check CDA State");
            }
            else if (!(di_VAC1.p_bIn))
            {
                this.p_eState = eState.Error;
                EQ.p_eState = EQ.eState.Error;
                EQ.p_bStop = true;
                alid_VAC1.Run(!(di_VAC1.p_bIn), "Please Check VAC State");
            }
            else if (!(di_VAC2.p_bIn))
            {
                this.p_eState = eState.Error;
                EQ.p_eState = EQ.eState.Error;
                EQ.p_bStop = true;
                alid_VAC2.Run(!(di_VAC2.p_bIn), "Please Check VAC State");
            }
            else if (!(di_MCReset.p_bIn))
            {
                this.p_eState = eState.Error;
                EQ.p_eState = EQ.eState.Error;
                EQ.p_bStop = true;
                alid_MCRESET.Run(!di_MCReset.p_bIn, "MC Reset Error");
            }
        }

        private void DoorCheck()
        {
            if (true)
            {
                if (EQ.p_eState == EQ.eState.Run)
                    do_door_Lock.Write(true);

                string str = "";

               
                if (!di_Door_VSMainPanel.p_bIn)
                    str = di_Door_VSMainPanel.m_id;
                else if (!di_Door_VSAxisPanel.p_bIn)
                    str = di_Door_VSAxisPanel.m_id;
                else if (!di_Door_VSRearTop.p_bIn)
                    str = di_Door_VSRearTop.m_id;
                else if (!di_Door_VSTop.p_bIn)
                    str = di_Door_VSTop.m_id;
                else if (!di_Door_VSBottom.p_bIn)
                    str = di_Door_VSBottom.m_id;
                else if (!di_Door_EFEMElecPanel.p_bIn)
                    str = di_Door_EFEMElecPanel.m_id;
                else if (!di_Door_EFEMTop.p_bIn)
                    str = di_Door_EFEMTop.m_id;
                else
                {
                    str = GeneralFunction.ReadINIFile("1", "2", @"Z:\1.ini");
                }
                if (str != "" && m_bThreadCheck)
                    GlobalObjects.Instance.Get<WindII_Warning>().AddWarning(str + " Open Detect");

                str = "";

                if (!di_ProtectionBar.p_bIn)
                    str = di_ProtectionBar.m_id;
                if (str != "" && m_bThreadCheck)
                    GlobalObjects.Instance.Get<WindII_Warning>().AddWarning(str + " Up Dectect");
            }
        }

        private void FanCheck()
        {
            if (true)
            {
                string str = "";
                //if (!di_Fan_VSPCDoor.p_bIn)
                //    str = di_Fan_VSPCDoor.m_id;
                //else if (!di_Fan_VSTop.p_bIn)
                //    str = di_Fan_VSTop.m_id;
                //else if (!di_Fan_VSBTM.p_bIn)
                //    str = di_Fan_VSBTM.m_id;
                //else if (!di_Fan_EFEMPC.p_bIn)
                //    str = di_Fan_EFEMPC.m_id;
                //else if (!di_Fan_12CH1.p_bIn)
                //    str = di_Fan_12CH1.m_id;
                //else if (!di_Fan_12CH2.p_bIn)
                //    str = di_Fan_12CH2.m_id;
                //else if (!di_Fan_12CH3.p_bIn)
                //    str = di_Fan_12CH3.m_id;
                //else if (!di_Fan_VSPC.p_bIn)
                //    str = di_Fan_VSPC.m_id;
                //else if (!di_Fan_4CH1.p_bIn)
                //    str = di_Fan_4CH1.m_id;
                //else if (!di_Fan_4CH2.p_bIn)
                //    str = di_Fan_4CH2.m_id;
                if (str != "" && m_bThreadCheck)
                    GlobalObjects.Instance.Get<WindII_Warning>().AddWarning(str + " Fan Off Detect");
            }
        }

        public override void ThreadStop()
        {
            if (m_bThreadCheck)
            {
                m_bThreadCheck = false;
                Thread.Sleep(500);
                m_threadCheck.Join();
            }
            base.ThreadStop();
        }

        private void M_tk4s_OnDetectLimit(string str)
        {
            if (m_bThreadCheck)
                GlobalObjects.Instance.Get<WindII_Warning>().AddWarning(str);
        }

        private void M_FFUGourp_OnDetectLimit(string str)
        {
            if(m_bThreadCheck)
                GlobalObjects.Instance.Get<WindII_Warning>().AddWarning(str);
        }
    }
}
