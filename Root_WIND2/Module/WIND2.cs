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

namespace Root_WIND2
{
    public class WIND2 : ModuleBase
    {
        public TK4SGroup m_tk4s;
        DIO_I di_Door_VSPC;
        DIO_I di_Door_VSTop;
        DIO_I di_Door_VSBTM;
        DIO_I di_Door_VSLoof;
        DIO_I di_Door_EFEMTop;
        DIO_I di_Door_EFEMBtm;
        DIO_I di_Door_EFEMAir;
        DIO_I di_Door_ElecTop;
        DIO_I di_Door_ElecBtm;
        DIO_I di_Door_ElecOptic;
        DIO_I di_Fan_VSPCDoor;
        DIO_I di_Fan_VSTop;
        DIO_I di_Fan_VSBTM;
        DIO_I di_Fan_EFEMPC;
        DIO_I di_Fan_12CH1;
        DIO_I di_Fan_12CH2;
        DIO_I di_Fan_12CH3;
        DIO_I di_Fan_VSPC;
        DIO_I di_Fan_4CH1;
        DIO_I di_Fan_4CH2;
        DIO_I di_EMO;
        DIO_I di_CDA;
        DIO_I di_VAC1;
        DIO_I di_VAC2;
        DIO_I di_MCReset;
        DIO_I di_Ionizer_LP;
        DIO_I di_Ionizer_Edge;
        DIO_I di_Ionizer_VS;
        DIO_I di_ProtectionBar;

        ALID alid_EMS;
        ALID alid_Ionizer;
        ALID alid_CDA;
        ALID alid_VAC1;

        public override void GetTools(bool bInit)
        {
            m_toolBox.Get(ref m_tk4s, this, "FDC", GlobalObjects.Instance.Get<DialogService>());
            p_sInfo = m_toolBox.Get(ref di_Door_VSPC, this, "Vision PC Door");
            p_sInfo = m_toolBox.Get(ref di_Door_VSTop, this, "Vision Top Door");
            p_sInfo = m_toolBox.Get(ref di_Door_VSBTM, this, "Vision Bottom Door");
            p_sInfo = m_toolBox.Get(ref di_Door_VSLoof, this, "Vision LoofDoor");
            p_sInfo = m_toolBox.Get(ref di_Door_EFEMTop, this, "EFEM Top Door");
            p_sInfo = m_toolBox.Get(ref di_Door_EFEMBtm, this, "EFEM Bottom Door");
            p_sInfo = m_toolBox.Get(ref di_Door_EFEMAir, this, "EFEM Air Door");
            p_sInfo = m_toolBox.Get(ref di_Door_ElecTop, this, "Elec Top Door");
            p_sInfo = m_toolBox.Get(ref di_Door_ElecBtm, this, "Elec Bottom Door");
            p_sInfo = m_toolBox.Get(ref di_Door_ElecOptic, this, "Elec Optic Door");
            p_sInfo = m_toolBox.Get(ref di_Fan_VSPCDoor, this, "Vision PC Door Fan");
            p_sInfo = m_toolBox.Get(ref di_Fan_VSTop, this, "Vision Top Fan");
            p_sInfo = m_toolBox.Get(ref di_Fan_VSBTM, this, "Vision Bottom Fan");
            p_sInfo = m_toolBox.Get(ref di_Fan_EFEMPC, this, "EFEM PC Fan");
            p_sInfo = m_toolBox.Get(ref di_Fan_12CH1, this, "12CH 1 Fan");
            p_sInfo = m_toolBox.Get(ref di_Fan_12CH2, this, "12CH 2 Fan");
            p_sInfo = m_toolBox.Get(ref di_Fan_12CH3, this, "12CH 3 Fan");
            p_sInfo = m_toolBox.Get(ref di_Fan_VSPC, this, "Vision PC Fan");
            p_sInfo = m_toolBox.Get(ref di_Fan_4CH1, this, "4CH 1 Fan");
            p_sInfo = m_toolBox.Get(ref di_Fan_4CH2, this, "4CH 2 Fan");
            p_sInfo = m_toolBox.Get(ref di_EMO, this, "Emergency");
            p_sInfo = m_toolBox.Get(ref di_CDA, this, "CDA");
            p_sInfo = m_toolBox.Get(ref di_VAC1, this, "VAC1");
            p_sInfo = m_toolBox.Get(ref di_VAC2, this, "VAC2");
            p_sInfo = m_toolBox.Get(ref di_MCReset, this, "MCRESET");
            p_sInfo = m_toolBox.Get(ref di_Ionizer_LP, this, "Ionizer LP");
            p_sInfo = m_toolBox.Get(ref di_Ionizer_Edge, this, "Ionizer Edge");
            p_sInfo = m_toolBox.Get(ref di_Ionizer_VS, this, "Ionizer VS");
            p_sInfo = m_toolBox.Get(ref di_ProtectionBar, this, "Protection Bar");

            alid_EMS = m_gaf.GetALID(this, "EMS", "EMS ERROR");
            alid_Ionizer = m_gaf.GetALID(this, "Ionizer", "Ionizer ERROR");
            alid_CDA = m_gaf.GetALID(this, "CDA", "CDA ERROR");
            alid_VAC1 = m_gaf.GetALID(this, "VAC1", "VAC Error");
        }

        Thread m_threadCheck;
        bool m_bThreadCheck = true;

        public WIND2(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
            m_tk4s.OnDetectLimit += M_tk4s_OnDetectLimit;
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
                //DoorCheck();
                //FanCheck();
                //AlarmCheck();
                //if (_diEMS.p_bIn)
                //{
                //	this.p_eState = eState.Error;
                //	EQ.p_bStop = true;
                //	_alid_EMS.Run(!_diEMS.p_bIn, "Please Check the Emergency Buttons");
                //}
                //for (int i = 0; i < _diDoorLock.Length; i++)
                //{
                //	if (_diDoorLock[i].p_bIn)
                //	{
                //		this.p_eState = eState.Error;
                //		EQ.p_bStop = true;
                //		string strDoor = " ";
                //		if (i == 0)
                //			strDoor = "Main Panel Top Left Door";
                //		if (i == 1)
                //			strDoor = "Main Panel Bottom Left Door";
                //		if (i == 2)
                //			strDoor = "Main Panel Door";

                //		_alid_DoorOpen.Run(!_diDoorLock[i].p_bIn, strDoor + "Opened");
                //	}
                //}
                //if (m_bUseCurtain)
                //{
                //	for (int i = 0; i < _diLightCurtain.Length; i++)
                //	{
                //		if (_diLightCurtain[i].p_bIn)
                //		{
                //			this.p_eState = eState.Error;
                //			EQ.p_bStop = true;
                //			string strCurtain = " ";
                //			if (i == 0)
                //				strCurtain = "Unloadport Light Curtain Error";
                //			if (i == 1)
                //				strCurtain = "Loadport Light Curatin Error";

                //			_alid_LightCurtain.Run(!_diLightCurtain[i].p_bIn, strCurtain);

                //		}
                //	}
                //}
                //if (_diProtectionBar.p_bIn)
                //{
                //	this.p_eState = eState.Error;
                //	EQ.p_bStop = true;
                //	_alid_EMS.Run(!_diProtectionBar.p_bIn, "Protection Bar Error");
                //}
            }
        }

        private void AlarmCheck()
        {
            if (!di_EMO.p_bIn)
            {
                this.p_eState = eState.Error;
                EQ.p_bStop = true;
                alid_EMS.Run(!di_EMO.p_bIn, "Please Check the Emergency Buttons");
            }
            else if (!di_EMO.p_bIn)
            {
                this.p_eState = eState.Error;
                EQ.p_bStop = true;
                alid_Ionizer.Run(!di_EMO.p_bIn, "Please Check Ionizer State");
            }
            else if (!di_CDA.p_bIn)
            {
                this.p_eState = eState.Error;
                EQ.p_bStop = true;
                alid_CDA.Run(!di_CDA.p_bIn, "Please Check CDA State");
            }
            else if (!(di_VAC1.p_bIn && di_VAC2.p_bIn))
            {
                this.p_eState = eState.Error;
                EQ.p_bStop = true;
                alid_VAC1.Run(!(di_VAC1.p_bIn && di_VAC2.p_bIn), "Please Check VAC State");
            }
        }

        private void DoorCheck()
        {
            if (true)
            {
                string str = "";
                if (!di_Door_VSPC.p_bIn)
                    str = di_Door_VSPC.m_id;
                else if (!di_Door_VSTop.p_bIn)
                    str = di_Door_VSTop.m_id;
                else if (!di_Door_VSBTM.p_bIn)
                    str = di_Door_VSBTM.m_id;
                else if (!di_Door_VSLoof.p_bIn)
                    str = di_Door_VSLoof.m_id;
                else if (!di_Door_EFEMTop.p_bIn)
                    str = di_Door_EFEMTop.m_id;
                else if (!di_Door_EFEMBtm.p_bIn)
                    str = di_Door_EFEMBtm.m_id;
                else if (!di_Door_EFEMAir.p_bIn)
                    str = di_Door_EFEMAir.m_id;
                else if (!di_Door_ElecTop.p_bIn)
                    str = di_Door_ElecTop.m_id;
                else if (!di_Door_ElecBtm.p_bIn)
                    str = di_Door_ElecBtm.m_id;
                else if (!di_Door_ElecOptic.p_bIn)
                    str = di_Door_ElecOptic.m_id;
                if (str != "")
                    GlobalObjects.Instance.Get<WIND2_Warning>().AddWarning(str + " Open Detect");

                if (!di_ProtectionBar.p_bIn)
                    str = di_ProtectionBar.m_id;
                if (str != "")
                    GlobalObjects.Instance.Get<WIND2_Warning>().AddWarning(str + " Up Dectect");
            }
        }

        private void FanCheck()
        {
            if (true)
            {
                string str = "";
                if (!di_Fan_VSPCDoor.p_bIn)
                    str = di_Fan_VSPCDoor.m_id;
                else if (!di_Fan_VSTop.p_bIn)
                    str = di_Fan_VSTop.m_id;
                else if (!di_Fan_VSBTM.p_bIn)
                    str = di_Fan_VSBTM.m_id;
                else if (!di_Fan_EFEMPC.p_bIn)
                    str = di_Fan_EFEMPC.m_id;
                else if (!di_Fan_12CH1.p_bIn)
                    str = di_Fan_12CH1.m_id;
                else if (!di_Fan_12CH2.p_bIn)
                    str = di_Fan_12CH2.m_id;
                else if (!di_Fan_12CH3.p_bIn)
                    str = di_Fan_12CH3.m_id;
                else if (!di_Fan_VSPC.p_bIn)
                    str = di_Fan_VSPC.m_id;
                else if (!di_Fan_4CH1.p_bIn)
                    str = di_Fan_4CH1.m_id;
                else if (!di_Fan_4CH2.p_bIn)
                    str = di_Fan_4CH2.m_id;
                if (str != "")
                    GlobalObjects.Instance.Get<WIND2_Warning>().AddWarning(str + " Fan Off Detect");
            }
        }

        public override void ThreadStop()
        {
            if (m_bThreadCheck)
            {
                m_bThreadCheck = false;
                m_threadCheck.Join();
            }
            base.ThreadStop();
        }

        private void M_tk4s_OnDetectLimit(string str)
        {
            GlobalObjects.Instance.Get<WIND2_Warning>().AddWarning(str);
        }
    }
}
