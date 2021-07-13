using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.ParticleCounter;
using RootTools.Trees;
using System;

namespace Root_VEGA_P.Module
{
    public class VEGA_P :ModuleBase
    {
        #region ToolBox
        public TK4SGroup m_tk4s; // 온도, 정전기
        public FFU_Group m_FFU;

        public DIO_I di_EMO;
        public DIO_I di_CDA;
        public DIO_I di_MCReset;
        public DIO_O do_door_Lock;
        public DIO_I di_ProtectionBar;
        
        
        public DIO_I di_door_Machine;
        public DIO_I di_door_AirTop;
        public DIO_I di_door_IO;
        public DIO_I di_door_Elec;

        public override void GetTools(bool bInit)
        {
            
            p_sInfo = m_toolBox.GetDIO(ref do_door_Lock, this, "Door Lock");
            p_sInfo = m_toolBox.GetDIO(ref di_EMO, this, "EMS");
            p_sInfo = m_toolBox.GetDIO(ref di_CDA, this, "CDA");
            p_sInfo = m_toolBox.GetDIO(ref di_MCReset, this, "M/C Reset");
            p_sInfo = m_toolBox.GetDIO(ref di_ProtectionBar, this, "Protection Bar Loadport");

            //m_toolBox.Get(ref m_tk4s, this, "FDC", ((AOP01_Engineer)m_engineer).m_dialogService);
            //m_toolBox.Get(ref m_FFU, this, "FFU", ((AOP01_Engineer)m_engineer).m_dialogService);

            
            p_sInfo = m_toolBox.GetDIO(ref di_door_Machine, this, "Door Machine");
            p_sInfo = m_toolBox.GetDIO(ref di_door_AirTop, this, "Door Air Top");
            p_sInfo = m_toolBox.GetDIO(ref di_door_IO, this, "Door I/O");
            p_sInfo = m_toolBox.GetDIO(ref di_door_Elec, this, "Door Elec");

            p_sInfo = m_flowSensor.GetTools(this);

            if (bInit)
            {
                InitALID();
                m_flowSensor.m_modbus.Connect();
            }
        }
        #endregion

        #region GAF
        ALID alid_EMS;
        ALID alid_MC;
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
            alid_EMS = m_gaf.GetALID(this, "EMS", "EMS ERROR");
            alid_MC = m_gaf.GetALID(this, "M/C Reset", "M/C RESET ERROR");
            alid_CDA = m_gaf.GetALID(this, "CDA", "CDA ERROR");
            alid_Door = m_gaf.GetALID(this, "DOOR", "DOOR ERROR");
            alid_ProtectionBar = m_gaf.GetALID(this, "PROTECTION BAR", "PROTECTION BAR ERROR");
            
            


            //alid_Tape = m_gaf.GetALID(this, "TAPE", "INTERLOCK KEY ERROR");

            //alid_LightCurtain = m_gaf.GetALID(this, "LIGHT CURTAIN", "LIGHT CURTAIN ERROR");

            //alid_Elevator_Protection = m_gaf.GetALID(this, "ELEVATOR", "PROTECTION ERROR");
            //alid_Cartridge_Check = m_gaf.GetALID(this, "CARTRIDGE", "CARTRIDGE ERROR");
            //alid_Wrapper_Check = m_gaf.GetALID(this, "WRAPPER", "WRAPEER ERROR");
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }

        public override void InitMemorys()
        {
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_secPumpDelay = tree.Set(m_secPumpDelay, m_secPumpDelay, "Pump Delay", "Pump Delay (sec)"); 
            m_sample.RunTree(tree.GetTree("Particle Counter"), true);
            m_flowSensor.RunTree(tree.GetTree("Flow Sensor")); 
        }
        #endregion


        protected override void RunThread()
        {
            base.RunThread();
            if (EQ.p_bSimulate)
                return;

            MachineCheck();
            DoorCheck();
        }
        private void MachineCheck()
        {
            if (di_EMO.p_bIn)
            {
                if (di_CDA.p_bIn)
                    alid_EMS.Run(di_EMO.p_bIn, "Please Check " + "EMS");
                else
                    alid_EMS.Run(di_EMO.p_bIn, "Please Check " + "EMO");
            }
            if (di_MCReset.p_bIn)
                alid_MC.Run(di_MCReset.p_bIn, "Please Check " + di_MCReset.m_id);
            if (!di_CDA.p_bIn)
                alid_CDA.Run(!di_CDA.p_bIn, "Please Check " + di_CDA.m_id);
        }

        private void DoorCheck()
        {
            //string str = "";
            if (!di_door_Machine.p_bIn)
                alid_Door.Run(!di_door_Machine.p_bIn, "Please Check " + di_door_Machine.m_id);
            if (!di_door_Elec.p_bIn)
                alid_Door.Run(!di_door_Elec.p_bIn, "Please Check " + di_door_Elec.m_id);
            if (!di_door_AirTop.p_bIn)
                alid_Door.Run(!di_door_AirTop.p_bIn, "Please Check " + di_door_AirTop.m_id);
            if (!di_door_IO.p_bIn)
                alid_Door.Run(!di_door_IO.p_bIn, "Please Check " + di_door_IO.m_id);
        }


        public FlowSensor m_flowSensor;
        public ParticleCounterBase.Sample m_sample;
        public double m_secPumpDelay = 2; 
        public VEGA_P(string id, IEngineer engineer)
        {
            m_flowSensor = new FlowSensor("FlowSensor", this);
            m_sample = new ParticleCounterBase.Sample(); 
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
