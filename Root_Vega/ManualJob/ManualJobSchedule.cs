using Root_Vega.Module;
using RootTools;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;

namespace Root_Vega.ManualJob
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
            m_loadport.m_infoPod.p_sLocID = p_sLocID;
            m_loadport.m_infoPod.m_aGemSlot[0].p_sLotID = p_sLotID;
            m_loadport.m_infoPod.p_sCarrierID = p_sCarrierID;
            m_loadport.m_infoPod.m_aGemSlot[0].p_sSlotID = p_sSlotID;

            if (m_loadport.m_infoPod.p_infoReticle == null) return "p_infoReticle == null";
            m_loadport.m_infoPod.p_infoReticle.m_sManualRecipe = m_sRecipe;
            m_loadport.m_infoPod.p_infoReticle.RecipeOpen(m_sRecipe);
            // Vision Recipe Open 코드 추가
            string strFileName = Path.GetFileNameWithoutExtension(m_sRecipe);
            string strVisionRecipeDirectoryPath = Path.GetDirectoryName(m_sRecipe) + "\\" + strFileName;
            string strVisionRecipeFullPath = strVisionRecipeDirectoryPath + "\\" + "Parameter.VegaVision";

            if (Directory.Exists(strVisionRecipeDirectoryPath) == false) Directory.CreateDirectory(strVisionRecipeDirectoryPath);
            if (File.Exists(strVisionRecipeFullPath) == false)
            {
                string strMessage = string.Format("\"Parameter.VegaVision\" Recipe is not Exist in the \"{0}\"", strVisionRecipeDirectoryPath);
                MessageBox.Show(strMessage);
                //comboRecipeID.SelectedValue = null;
            }
            else
            {
                App.m_engineer.m_recipe.Load(strVisionRecipeFullPath);
            }
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

        public string p_id { get; set; }
        Log m_log;
        Vega_Handler m_handler; 
        public Loadport m_loadport; 
        public ManualJobSchedule(Loadport loadport, Vega_Handler handler)
        {
            m_loadport = loadport;
            m_handler = handler; 
            p_id = loadport.p_id;
            m_log = loadport.m_log;
        }

        public bool ShowPopup()
        {
            if (ManualJobSchedule_UI.m_bShow) return false;
            ManualJobSchedule_UI jobschedulePopup = new ManualJobSchedule_UI();
            jobschedulePopup.Init(this);
            p_bRnR = false; 
            jobschedulePopup.ShowDialog();
            m_handler.m_nRnR = p_bRnR ? p_nRnR : 1; 
            return jobschedulePopup.DialogResult == true;
        }
    }
}
