using RootTools;
using RootTools.Module;
using Root_AOP01_Inspection.Module;
using System.IO;
using System.Windows;
using Root_EFEM.Module;

namespace Root_AOP01_Inspection.UI._3._RUN
{
    public class ManualJobSchedule : NotifyProperty
    {
        #region Property
        string _sLocID = "";
        public string p_sLocID
        {
            get { return _sLocID; }
            set
            {
                if (_sLocID == value) return;
                _sLocID = value;
                OnPropertyChanged();
            }
        }

        string _sLotID = "";
        public string p_sLotID
        {
            get { return _sLotID; }
            set
            {
                if (_sLotID == value) return;
                _sLotID = value;
                OnPropertyChanged();
            }
        }

        string _sCarrierID = "";
        public string p_sCarrierID
        {
            get { return _sCarrierID; }
            set
            {
                if (_sCarrierID == value) return;
                _sCarrierID = value;
                OnPropertyChanged();
            }
        }

        string _sSlotID = "";
        public string p_sSlotID
        {
            get { return _sSlotID; }
            set
            {
                if (_sSlotID == value) return;
                _sSlotID = value;
                OnPropertyChanged();
            }
        }

        public string m_sRecipe = "";
        public string SetInfoPod()
        {
            m_loadport[0].p_infoCarrier.p_sLocID = p_sLocID;
            m_loadport[0].p_infoCarrier.m_aGemSlot[0].p_sLotID = p_sLotID;
            m_loadport[0].p_infoCarrier.p_sCarrierID = p_sCarrierID;
            m_loadport[0].p_infoCarrier.m_aGemSlot[0].p_sSlotID = p_sSlotID;

            if (m_loadport[0].p_infoCarrier.m_aInfoWafer == null)
            {
                m_loadport[0].m_alidInforeticle.Run(true, "Reticle Info is null");
                return "p_infoWafer == null";
            }
            //m_loadport[0].p_infoCarrier.m_aInfoWafer.Recipe
            //m_loadport.m_infoPod.p_infoReticle.RecipeOpen(m_sRecipe);
            // Vision Recipe Open 코드 추가
            return "OK";
        }
        #endregion

        #region RnR Property
        bool _bRnR = false;
        public bool p_bRnR
        {
            get { return _bRnR; }
            set
            {
                _bRnR = value;
                OnPropertyChanged();
            }
        }

        int _nRnR = 1;
        public int p_nRnR
        {
            get { return _nRnR; }
            set
            {
                _nRnR = value;
                OnPropertyChanged();
            }
        }
        #endregion

        //public string[] p_id { get; set; }
        //Log[] m_log;
        AOP01_Handler m_handler;
        AOP01_Engineer m_engineer;
        public Loadport_Cymechs[] m_loadport = new Loadport_Cymechs[2];
        public Loadport_RND[] m_loadportrnd= new Loadport_RND[2];
        public ManualJobSchedule(Loadport_Cymechs loadport1, Loadport_Cymechs loadport2, AOP01_Engineer engineer)
        {
            m_loadport[0] = loadport1;
            m_loadport[1] = loadport2;
            m_handler = engineer.m_handler;
            //p_id[0] = loadport1.p_id;
            //m_log[0] = loadport1.m_log;
            //p_id[1] = loadport2.p_id;
            //m_log[1] = loadport2.m_log;
        }
        public ManualJobSchedule(Loadport_RND loadport1, Loadport_RND loadport2, AOP01_Engineer engineer)
        {
            m_loadportrnd[0] = loadport1;
            m_loadportrnd[1] = loadport2;
            m_handler = engineer.m_handler;
            //p_id[0] = loadport1.p_id;
            //m_log[0] = loadport1.m_log;
            //p_id[1] = loadport2.p_id;
            //m_log[1] = loadport2.m_log;
        }
        public bool ShowPopup(AOP01_Engineer engineer)
        {
            
            if (Dlg_Start.m_bShow) return false;
            Dlg_Start dlg_Start = new Dlg_Start();
            switch (engineer.m_handler.LoadportType)
            {
                case AOP01_Handler.eLoadport.Cymechs:
                    dlg_Start.Init(engineer.m_handler.m_mainVision, (WTRCleanUnit)engineer.m_handler.m_wtr, (Loadport_Cymechs)engineer.m_handler.m_aLoadport[0], (Loadport_Cymechs)engineer.m_handler.m_aLoadport[1], engineer);
                    break;
                case AOP01_Handler.eLoadport.RND:
                default:
                    dlg_Start.Init(engineer.m_handler.m_mainVision, (WTRCleanUnit)engineer.m_handler.m_wtr, (Loadport_RND)engineer.m_handler.m_aLoadport[0], (Loadport_RND)engineer.m_handler.m_aLoadport[1], engineer);
                    break;
            }
            dlg_Start.Init(this);
            p_bRnR = false;
            Dlg_Start.m_bShow = true;
            dlg_Start.ShowDialog();
            m_handler.m_nRnR = p_bRnR ? p_nRnR : 1;
            return dlg_Start.DialogResult == true;
        }
    }
}
