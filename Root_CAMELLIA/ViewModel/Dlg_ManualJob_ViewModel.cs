using Root_CAMELLIA.Data;
using RootTools;
using RootTools.Gem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_CAMELLIA
{
    public class Dlg_ManualJob_ViewModel : ObservableObject, IDialogRequestClose
    {
        ObservableCollection<WaferSlotData> m_data = new ObservableCollection<WaferSlotData>();
        public ObservableCollection<WaferSlotData> p_data
        {
            get
            {
                return m_data;
            }
            set
            {
                SetProperty(ref m_data, value);
            }
        }

        Visibility m_RnRVisibility = Visibility.Hidden;
        public Visibility p_RnRVisibility
        {
            get
            {
                return m_RnRVisibility;
            }
            set
            {
                SetProperty(ref m_RnRVisibility, value);
            }
        }

        int m_RnR = 1;
        public int p_RnR
        {
            get
            {
                return m_RnR;
            }
            set
            {
                SetProperty(ref m_RnR, value);
            }
        }

        bool m_loadedDone = false;
        public bool p_loadedDone
        {
            get
            {
                return m_loadedDone;
            }
            set
            {
                SetProperty(ref m_loadedDone, value);
            }
        }

        string m_lotID = "";
        public string p_lotID
        {
            get
            {
                return m_lotID;
            }
            set
            {
                SetProperty(ref m_lotID, value);
            }
        }

        string m_carrierID = "";
        public string p_carrierID
        {
            get
            {
                return m_carrierID;
            }
            set
            {
                SetProperty(ref m_carrierID, value);
            }
        }

        bool m_checkRnR = false;
        public bool p_checkRnR
        {
            get
            {
                return m_checkRnR;
            }
            set
            {
                SetProperty(ref m_checkRnR, value);
            }
        }

        List<string> m_recipeList = new List<string>();
        public List<string> p_recipeList
        {
            get
            {
                return m_recipeList;
            }
            set
            {
                SetProperty(ref m_recipeList , value);
            }
        }

        string m_selectRecipe = "";
        public string p_selectRecipe
        {
            get
            {
                return m_selectRecipe;
            }
            set
            {
                SetProperty(ref m_selectRecipe, value);
            }
        }
        InfoCarrier m_infoCarrier;
        public Dlg_ManualJob_ViewModel()
        {
            //InitData();
        }

        public void InitData(InfoCarrier infoCarrier)
        {
            m_infoCarrier = infoCarrier;
            p_data.Clear();
            int slot;
            slot = infoCarrier.p_lWafer;

            p_lotID = infoCarrier.p_sLotID;
            p_carrierID = infoCarrier.p_sCarrierID;

            ObservableCollection<WaferSlotData> temp = new ObservableCollection<WaferSlotData>();
            for (int i = 0; i < slot; i++)
            {
                WaferSlotData waferData;
                if (infoCarrier.GetInfoWafer(i) == null)
                {
                    waferData = new WaferSlotData(i, "Empty", Visibility.Hidden, CmdChecked, CmdUnChecked);
                }
                else
                {
                    waferData = new WaferSlotData(i, "Exist", Visibility.Visible, CmdChecked, CmdUnChecked);
                }

                temp.Insert(0, waferData);
                //p_data.Add(new SlotData(i));
            }

            InitRecipeData();

            p_data = temp;
            p_loadedDone = true;
        }
        
        void InitRecipeData()
        {
            p_recipeList.Clear();
            DirectoryInfo info = new DirectoryInfo(BaseDefine.Dir_SequenceInitialPath);
            List<string> asRecipeFile = new List<string>();
            FileInfo[] fileInfos = info.GetFiles("*." + EQ.m_sModel);
            foreach (var file in fileInfos)
            {
                asRecipeFile.Add(file.Name);
            }

            p_recipeList = new List<string>(asRecipeFile);
        }

        public ICommand CmdTest
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CustomMessageBox.Show("Error", "Error!");
                });
            }
        }

        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_loadedDone = false;
                    CloseRequested(this, new DialogCloseRequestedEventArgs(true));
                });
            }
        }

        public RelayCommandWithParameter CmdChecked
        {
            get
            {
                return new RelayCommandWithParameter(OnChecked);
            }
        }

        public RelayCommandWithParameter CmdUnChecked
        {
            get
            {
                return new RelayCommandWithParameter(OnUnChecked);
            }
        }

        public ICommand CmdSelectAllSlot
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SelectAllSlot(true);
                });
            }
        }

        public ICommand CmdUnSelectAllSlot
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SelectAllSlot(false);
                });
            }
        }

        public ICommand CmdCheckRnR
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CheckRnR(true);
                });
            }
        }
        public ICommand CmdUnCheckRnR
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CheckRnR(false);
                });
            }
        }

        public ICommand CmdCancel
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_checkRnR = false;
                    CloseRequested(this, new DialogCloseRequestedEventArgs(false));
                });
            }
        }

        public ICommand CmdRun
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeRun();
                });
            }
        }

        void RecipeRun()
        {
            if (m_infoCarrier == null) return;

            if(p_selectRecipe == "")
            {
                CustomMessageBox.Show("Error", "Please Select Recipe", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Error);
            }
            //m_infoCarrier.p_sLocID = textboxLocID.Text;
            m_infoCarrier.p_sLotID = p_lotID;
            m_infoCarrier.p_sCarrierID = p_carrierID;
            int nSlot = m_infoCarrier.p_lWafer;

            string recipePath = p_selectRecipe.Replace(Path.GetExtension(p_selectRecipe), "") + "\\" + p_selectRecipe;
            string sequenceRecipePath = BaseDefine.Dir_SequenceInitialPath + p_selectRecipe;
            bool isVisionRecipeOpen = false;

            int firstIdx = -1;
            int lastIdx = -1;
            List<string> moduleRunList = new List<string>();
            MarsLogManager.Instance.m_flowData.ClearData();
            RnRData rnrData = App.m_engineer.ClassHandler().GetRnRData();
            rnrData.ClearData();
            moduleRunList.Add(App.m_engineer.m_handler.m_loadport[EQ.p_nRunLP].p_id);
            rnrData.CarrierID = p_carrierID;
            rnrData.LotID = p_lotID;
            for (int i = 0; i < nSlot; i++)
            {
                m_infoCarrier.m_aGemSlot[i].p_sRecipe = "";
                InfoWafer infoWafer = m_infoCarrier.GetInfoWafer(i);

                if (infoWafer != null)
                {
                    infoWafer.p_eState = (p_data[nSlot - 1 - i].p_state == "Select") ? GemSlotBase.eState.Select : GemSlotBase.eState.Exist;
                    m_infoCarrier.m_aGemSlot[i].p_eState = infoWafer.p_eState;
                    infoWafer.p_sWaferID = p_data[nSlot - 1 - i].p_waferID;
                    infoWafer.p_sLotID = p_lotID;
                    infoWafer.p_sCarrierID = p_carrierID;

                    if (infoWafer.p_eState == GemSlotBase.eState.Select)
                    {
                        rnrData.SelectSlot.Add(i);
                        infoWafer.RecipeOpen(sequenceRecipePath);
                        string visionPath = recipePath.Replace(Path.GetExtension(recipePath), ".aco");
                        m_infoCarrier.m_aGemSlot[i].p_sRecipe = infoWafer.p_sRecipe;
                        ((InfoWafer)m_infoCarrier.m_aGemSlot[i]).p_sWaferID = infoWafer.p_sWaferID;
                        if (!isVisionRecipeOpen && !DataManager.Instance.recipeDM.RecipeLoad(BaseDefine.Dir_SequenceInitialPath + visionPath, false))
                        {
                            CustomMessageBox.Show("Error", "Recipe Not Exist", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Error);
                            return;
                        }
                        isVisionRecipeOpen = true;

                        if (firstIdx == -1)
                        {
                            firstIdx = i;
                            for (int a = 0; a < infoWafer.m_moduleRunList.p_aModuleRun.Count; a++)
                            {
                                bool bFind = false;
                                string module = infoWafer.m_moduleRunList.p_aModuleRun[a].m_moduleBase.p_id;
                                foreach (string str in moduleRunList)
                                {
                                    if(module == str)
                                    {
                                        bFind = true;
                                        break;
                                    }
                                }
                                if (!bFind)
                                    moduleRunList.Add(module);
                            }
                            moduleRunList.Add(App.m_engineer.m_handler.m_loadport[EQ.p_nRunLP].p_id);

                            foreach(string str in moduleRunList)
                            {
                                string module = str;
                                if(module == EQ.m_sModel)
                                {
                                    module = "Vision";
                                }
                                MarsLogManager.Instance.m_flowData.AddData(module);
                            }
                            MarsLogManager.Instance.WriteLEH(EQ.p_nRunLP, moduleRunList[0], SSLNet.LEH_EVENTID.CARRIER_LOAD, MarsLogManager.Instance.m_flowData);
                        }

                        m_infoCarrier.StartProcess(infoWafer.p_id);
 
                        lastIdx = i;
                    }
                }
            }
            //foreach (CAMELLIA_Process.Sequence prc in App.m_engineer.m_handler.p_process.p_qSequence)
            //{
            //    string s = prc.p_moduleRun.p_id;
            //}
            if (firstIdx == lastIdx)
                m_infoCarrier.m_aInfoWafer[firstIdx].p_eWaferOrder = InfoWafer.eWaferOrder.FirstLastWafer;
            else
            {
                m_infoCarrier.m_aInfoWafer[firstIdx].p_eWaferOrder = InfoWafer.eWaferOrder.FirstWafer;
                m_infoCarrier.m_aInfoWafer[lastIdx].p_eWaferOrder = InfoWafer.eWaferOrder.LastWafer;
            }

            MarsLogManager.Instance.m_dataFormatter.ClearData();
            MarsLogManager.Instance.m_dataFormatter.AddData("RecipeID", Path.GetFileNameWithoutExtension(m_infoCarrier.m_aInfoWafer[firstIdx].p_sRecipe));
            m_infoCarrier.SetSelectMapData(m_infoCarrier);
            
            EQ.p_nRnR = p_checkRnR ? p_RnR : 1;

            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
        }

        void CheckRnR(bool check)
        {
            if (check)
            {
                p_RnRVisibility = Visibility.Visible;
            }
            else
            {
                p_RnRVisibility = Visibility.Hidden;
            }
        }

        void SelectAllSlot(bool select)
        {
            int cnt = m_infoCarrier.p_lWafer;
            for(int i = 0; i < cnt; i++)
            {
                if(p_data[i].p_state != "Empty")
                {
                    if (select)
                    {
                        p_data[i].p_isChecked = true;
                        p_data[i].p_isEnable = true;
                    }
                    else
                    {
                        p_data[i].p_isChecked = false;
                        p_data[i].p_isEnable = false;
                    }

                }
            }
        }

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        public void OnChecked(object obj)
        {
            int idx = Convert.ToInt32(obj);
            p_data[25 - idx].p_isEnable = true;
        }

        public void OnUnChecked(object obj)
        {
            int idx = Convert.ToInt32(obj);
            //p_data[idx - 1];
            p_data[25 - idx].p_isEnable = false;
        }
    }

    public class WaferSlotData :ObservableObject
    {
        public WaferSlotData(int index, string content, Visibility visibility, ICommand check, ICommand uncheck)
        {
            p_row = 24 - index;
            p_idx = index + 1;
            p_state = content;
            p_visibility = visibility;
            //this.command = command;
            p_waferID = string.Format("Wafer{00}", (index + 1).ToString());

            CmdChecked = check;
            CmdUnChecked = uncheck;
        }
        public int p_idx { get; set; }
        public int p_row { get; set; }

        string m_state = "";
        public string p_state
        {
            get
            {
                return m_state;
            }
            set
            {
                SetProperty(ref m_state, value);
            }
        }
        public Visibility p_visibility { get; set; } = Visibility.Hidden;

        bool m_isEnable = false;
        public bool p_isEnable
        {
            get
            {
                return m_isEnable;
            }
            set
            {
                SetProperty(ref m_isEnable, value);
                if (value)
                {
                    p_state = "Select";
                }
                else
                {
                    p_state = "Exist";
                }
            }
        }
        bool m_isChecked = false;
        public bool p_isChecked
        {
            get
            {
                return m_isChecked;
            }
            set
            {
                SetProperty(ref m_isChecked, value);
            }
        }
        public string p_waferID { get; set; } = "";
        public ICommand CmdChecked { get; set; }
        public ICommand CmdUnChecked { get; set; }
        //public string content { get; set; }
        //public ICommand command { get; set; }
    }

    //public class StateData
    //{
    //    public StateData(Visibility visibility)
    //    {
    //        this.visibility = visibility;
    //    }
       
    //}
}
