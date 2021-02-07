using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_AOP01_Packing.Module
{
    public class AOP01_P : ModuleBase
    {
        public bool m_bUseDoorAlarm = true;
        public bool m_bUseCurtainAlarm = true;
        string[] asLamp = Enum.GetNames(typeof(eLamp));
        string[] asBuzzer = Enum.GetNames(typeof(eBuzzer));

        #region ToolBox
        public DIO_Os m_doLamp;
        public TK4SGroup m_tk4s; // 온도, 정전기
        public FFU_Group m_FFU;
        DIO_Os do_Buzzer;
        DIO_O do_door_Lock;
       
        DIO_I di_InterlockKey;
        DIO_I di_LightCurtainKey;
        DIO_I di_EMO;
        DIO_I di_CDA;
        DIO_I di_MCReset;
        DIO_I di_Fan_4CH;
        DIO_I di_Fan_PC;
        DIO_IO dio_TapeLoad;
        DIO_IO dio_TapeUnload;
        DIO_I di_door_Machine;
        DIO_I di_door_AirTop;
        DIO_I di_door_IO;
        DIO_I di_door_Elec;
        DIO_I di_ProtectionBar;        
        DIO_I di_LightCurtain_Load;
        DIO_I di_LightCurtain_Unload;
        DIO_I di_Elevator_Protection1;
        DIO_I di_Elevator_Protection2;
        DIO_I di_Cartridge_Check1;
        DIO_I di_Cartridge_Check2;
        DIO_I di_Wrapper_WrapCheck;
        DIO_I di_Wrapper_WrapLevelCheck;
        #endregion

        #region GAF
        ALID alid_InterlockKey;
        ALID alid_LightCurtainKey;
        ALID alid_EMS;
        ALID alid_CDA;
        ALID alid_Fan;
        ALID alid_Tk4s;
        ALID alid_FFU;
        ALID alid_Door;
        ALID alid_Tape;
        ALID alid_ProtectionBar;
        ALID alid_LightCurtain;
        ALID alid_Elevator_Protection;
        ALID alid_Cartridge_Check;
        ALID alid_Wrapper_Check;
        void InitALID()
        {
            alid_InterlockKey = m_gaf.GetALID(this, "INTERLOCK KEY", "INTERLOCK KEY ERRE");
            alid_LightCurtainKey = m_gaf.GetALID(this, "LIGHT CURTAIN KEY", "LIGHT CURTAIN KEY ERROR");

            alid_EMS = m_gaf.GetALID(this, "EMS", "EMS ERROR");
            alid_CDA = m_gaf.GetALID(this, "CDA", "CDA ERROR");
            alid_Fan = m_gaf.GetALID(this, "FAN", "FAN ERROR");
            alid_Tk4s = m_gaf.GetALID(this, "TK4S", "TK4S ERROR");
            alid_FFU = m_gaf.GetALID(this, "FFU", "FFU ERROR");

            alid_Door = m_gaf.GetALID(this, "DOOR", "DOOR ERROR");

            alid_Tape = m_gaf.GetALID(this, "TAPE", "INTERLOCK KEY ERROR");

            alid_ProtectionBar = m_gaf.GetALID(this, "PROTECTION BAR", "PROTECTION BAR ERROR");
            alid_LightCurtain = m_gaf.GetALID(this, "LIGHT CURTAIN", "LIGHT CURTAIN ERROR");

            alid_Elevator_Protection = m_gaf.GetALID(this, "ELEVATOR", "PROTECTION ERROR");
            alid_Cartridge_Check = m_gaf.GetALID(this, "CARTRIDGE", "CARTRIDGE ERROR");
            alid_Wrapper_Check = m_gaf.GetALID(this, "WRAPPER", "WRAPEER ERROR");
        }
        #endregion

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_doLamp, this, "Tower Lamp", asLamp);
            p_sInfo = m_toolBox.Get(ref do_Buzzer, this, "Buzzer", asBuzzer, true, true);
            p_sInfo = m_toolBox.Get(ref do_door_Lock, this, "Door Lock");

            m_toolBox.Get(ref m_tk4s, this, "FDC", ((AOP01_Engineer)m_engineer).m_dialogService);
            m_toolBox.Get(ref m_FFU, this, "FFU", ((AOP01_Engineer)m_engineer).m_dialogService);
            //m_toolBox.Get(ref m_FFU, this, "FFU");
            p_sInfo = m_toolBox.Get(ref di_InterlockKey, this, "Interlock Key");
            p_sInfo = m_toolBox.Get(ref di_LightCurtainKey, this, "Light Curtian Key");
            p_sInfo = m_toolBox.Get(ref di_EMO, this, "EMS");
            p_sInfo = m_toolBox.Get(ref di_CDA, this, "CDA");
            p_sInfo = m_toolBox.Get(ref di_MCReset, this, "M/C Reset");
            p_sInfo = m_toolBox.Get(ref di_Fan_4CH, this, "4CH Fan");
            p_sInfo = m_toolBox.Get(ref di_Fan_PC, this, "PC Fan");
            p_sInfo = m_toolBox.Get(ref dio_TapeLoad, this, "Tape Load");
            p_sInfo = m_toolBox.Get(ref dio_TapeUnload, this, "Tape Unload");
            p_sInfo = m_toolBox.Get(ref di_door_Machine, this, "Door Machine");
            p_sInfo = m_toolBox.Get(ref di_door_AirTop, this, "Door Air Top");
            p_sInfo = m_toolBox.Get(ref di_door_IO, this, "Door I/O");
            p_sInfo = m_toolBox.Get(ref di_door_Elec, this, "Door Elec");
            p_sInfo = m_toolBox.Get(ref di_ProtectionBar, this, "Protection Bar Loadport");
            p_sInfo = m_toolBox.Get(ref di_LightCurtain_Load, this, "Light Curtain Loadport");
            p_sInfo = m_toolBox.Get(ref di_LightCurtain_Unload, this, "Light Curtain Unloadport");
            p_sInfo = m_toolBox.Get(ref di_Elevator_Protection1, this, "Elevator Protection Sensor Forward");
            p_sInfo = m_toolBox.Get(ref di_Elevator_Protection2, this, "Elevator Protection Sensor Backward");
            p_sInfo = m_toolBox.Get(ref di_Cartridge_Check1, this, "Cartridge Check Left");
            p_sInfo = m_toolBox.Get(ref di_Cartridge_Check2, this, "Cartirdge Check Right");
            p_sInfo = m_toolBox.Get(ref di_Wrapper_WrapCheck, this, "Wrapper Exist Sensor");
            p_sInfo = m_toolBox.Get(ref di_Wrapper_WrapLevelCheck, this, "Wrapper Level Sensor");

            
            if (bInit)
                InitALID();
        }
        public override void RunTree(Tree tree)
        {
            m_bUseDoorAlarm = tree.Set(m_bUseDoorAlarm, m_bUseDoorAlarm, "Door Alarm", "Use Door Alarm");
            m_bUseCurtainAlarm = tree.Set(m_bUseCurtainAlarm, m_bUseCurtainAlarm, "Curtain Alarm", "Use Curtain Alarm");
        }
        protected override void RunThread()
        {
            base.RunThread();
            if (EQ.p_bSimulate)
                return;
            switch (EQ.p_eState)
            {
                case EQ.eState.Init:
                    m_doLamp.Write(eLamp.Yellow);
                    break;
                case EQ.eState.Home:
                    do_door_Lock.Write(true);
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
                    do_door_Lock.Write(true);
                    m_doLamp.Write(eLamp.Yellow);
                    break;
                case EQ.eState.Error:
                    m_doLamp.Write(eLamp.Red);
                    break;
            }

            //KeyCheck();
            //MachineCheck();
            //DoorCheck();
            //TapeloadCheck(); // ??
            InterruptCheck();
            //ModuleCheck();
        }

        private void KeyCheck()
        {
            if (di_InterlockKey.p_bIn) // true:on/false:off
                alid_InterlockKey.Run(!di_InterlockKey.p_bIn, "Please Check " + di_InterlockKey.m_id);
            if (!di_LightCurtainKey.p_bIn)
                alid_LightCurtainKey.Run(!di_LightCurtainKey.p_bIn, "Please Check " + di_LightCurtainKey.m_id);
        }
        private void MachineCheck()
        {
            if (di_EMO.p_bIn)
            {
                if (!di_CDA.p_bIn)
                    alid_EMS.Run(di_EMO.p_bIn, "Please Check " + "EMS");
                else
                    alid_EMS.Run(di_EMO.p_bIn, "Please Check " + "EMO");
            }
            if (di_CDA.p_bIn)
                alid_CDA.Run(di_CDA.p_bIn, "Please Check " + di_CDA.m_id);
            if (di_Fan_4CH.p_bIn)
                alid_Fan.Run(!di_Fan_4CH.p_bIn, "Please Check " + di_Fan_4CH.m_id);
            if (di_Fan_PC.p_bIn)
                alid_Fan.Run(!di_Fan_PC.p_bIn, "Please Check " + di_Fan_PC.m_id);
            //tk4s
            //FFU
            
        }
        private void DoorCheck()
        {
            if (m_bUseDoorAlarm)
            {
                if (!di_door_Machine.p_bIn)
                    alid_Door.Run(!di_door_Machine.p_bIn, "Please Check " + di_door_Machine.m_id);
                if (!di_door_Elec.p_bIn)
                    alid_Door.Run(!di_door_Elec.p_bIn, "Please Check " + di_door_Elec.m_id);
                if (!di_door_AirTop.p_bIn)
                    alid_Door.Run(!di_door_AirTop.p_bIn, "Please Check " + di_door_AirTop.m_id);
                if (!di_door_IO.p_bIn)
                    alid_Door.Run(!di_door_IO.p_bIn, "Please Check " + di_door_IO.m_id);
            }
        }
        private void TapeloadCheck()
        {
            if (dio_TapeLoad.p_bIn)
                alid_Tape.Run(!dio_TapeLoad.p_bIn, "Please Check " + dio_TapeLoad.m_id);
            if (dio_TapeUnload.p_bIn)
                alid_Tape.Run(!dio_TapeUnload.p_bIn, "Please Check " + dio_TapeUnload.m_id);
        }
        private void InterruptCheck()
        {
            if (di_ProtectionBar.p_bIn)
                alid_ProtectionBar.Run(di_ProtectionBar.p_bIn, "Please Check " + di_ProtectionBar.m_id);
            if (di_LightCurtain_Load.p_bIn)
                alid_LightCurtain.Run(di_LightCurtain_Load.p_bIn, "Please Check " + di_LightCurtain_Load.m_id);
            if (di_LightCurtain_Unload.p_bIn)
                alid_LightCurtain.Run(di_LightCurtain_Unload.p_bIn, "Please Check " + di_LightCurtain_Unload.m_id);
        }
        private void ModuleCheck()
        {
            if (di_Elevator_Protection1.p_bIn)
                alid_Elevator_Protection.Run(!di_Elevator_Protection1.p_bIn, "Please Check " + di_Elevator_Protection1.m_id);
            if (di_Elevator_Protection2.p_bIn)
                alid_Elevator_Protection.Run(!di_Elevator_Protection2.p_bIn, "Please Check " + di_Elevator_Protection2.m_id);
            if (di_Cartridge_Check1.p_bIn)
                alid_Cartridge_Check.Run(!di_Cartridge_Check1.p_bIn, "Please Check " + di_Cartridge_Check1.m_id);
            if (di_Cartridge_Check2.p_bIn)
                alid_Cartridge_Check.Run(!di_Cartridge_Check2.p_bIn, "Please Check " + di_Cartridge_Check2.m_id);
            if (di_Wrapper_WrapCheck.p_bIn)
                alid_Wrapper_Check.Run(!di_Wrapper_WrapCheck.p_bIn, "Please Check " + di_Wrapper_WrapCheck.m_id);
            if (di_Wrapper_WrapLevelCheck.p_bIn)
                alid_Wrapper_Check.Run(!di_Wrapper_WrapLevelCheck.p_bIn, "Please Check " + di_Wrapper_WrapLevelCheck.m_id);
        }

        public AOP01_P(string id, IEngineer engineer)
        {
            p_id = id;
            base.InitBase(id, engineer);
        }

        public enum eLamp
        {
            Red,
            Yellow,
            Green
        }
        enum eBuzzer
        {
            Buzzer1,
            Buzzer2,
            Buzzer3,
            Buzzer4,
        }
    }
}
