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
            m_loadport.p_infoCarrier.p_sLocID = p_sLocID;
            m_loadport.p_infoCarrier.m_aGemSlot[0].p_sLotID = p_sLotID;
            m_loadport.p_infoCarrier.p_sCarrierID = p_sCarrierID;
            m_loadport.p_infoCarrier.m_aGemSlot[0].p_sSlotID = p_sSlotID;

            if (m_loadport.p_infoCarrier.m_aInfoWafer == null)
            {
                m_loadport.m_alidInforeticle.Run(true, "Reticle Info is null");
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


        AOP01_Handler m_handler;
        AOP01_Engineer m_engineer;
        public Loadport_Cymechs m_loadport;
        public InfoCarrier m_infoCarrier = null;
        public ManualJobSchedule(Loadport_Cymechs loadport, AOP01_Engineer engineer, InfoCarrier infoCarrier)
        {
            m_infoCarrier = infoCarrier;
            if (m_infoCarrier == null) return;
            m_loadport = loadport;
            m_engineer = engineer;
            m_handler = engineer.m_handler;
        }
        public bool ShowPopup(AOP01_Handler handler)
        {
            
            if (Dlg_Start.m_bShow) return false;
            Dlg_Start dlg_Start = new Dlg_Start(m_infoCarrier);
            dlg_Start.Init(m_engineer);
            dlg_Start.Init(this);
            p_bRnR = false;
            Dlg_Start.m_bShow = true;
            dlg_Start.ShowDialog();
            m_handler.m_nRnR = p_bRnR ? p_nRnR : 1;
            p_nRnR = 1;
            return dlg_Start.DialogResult == true;
        }
    }
}
