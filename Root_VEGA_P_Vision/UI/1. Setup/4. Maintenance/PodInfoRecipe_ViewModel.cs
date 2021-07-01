using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Root_VEGA_P_Vision
{
    [Serializable]
    public class PodIDInfo:ObservableObject
    {
        string rfid,dualPodID,domeID,coverID,basePlateID,podName;
        double weight;
        bool padLT, padRT, padLB, padRB;

        #region Property
        public string PodName
        {
            get => podName;
            set => SetProperty(ref podName, value.Trim());
        }
        public string RFID
        {
            get => rfid;
            set => SetProperty(ref rfid, value.Trim());
        }
        public string DualPodID
        {
            get => dualPodID;
            set => SetProperty(ref dualPodID, value.Trim());
        }
        public string DomeID
        {
            get => domeID;
            set => SetProperty(ref domeID, value.Trim());
        }
        public string CoverID
        {
            get => coverID;
            set => SetProperty(ref coverID, value.Trim());
        }
        public string BasePlateID
        {
            get => basePlateID;
            set => SetProperty(ref basePlateID, value.Trim());
        }
        public double Weight
        {
            get => weight;
            set => SetProperty(ref weight, value);
        }
        public bool PadLT
        {
            get => padLT;
            set => SetProperty(ref padLT, value);
        }
        public bool PadRT
        {
            get => padRT;
            set => SetProperty(ref padRT, value);
        }
        public bool PadLB
        {
            get => padLB;
            set => SetProperty(ref padLB, value);
        }
        public bool PadRB
        {
            get => padRB; 
            set => SetProperty(ref padRB, value);
        }
        #endregion

        public PodIDInfo() { }
        
        public bool Load(string PodName,string dualPodID)
        {
            bool res = true;

            try
            {
                string filePath = App.RecipeRootPath + dualPodID + "\\" + PodName + "\\PodInfo.xml";
                using(Stream reader = new FileStream(filePath,FileMode.Open))
                {
                    XmlSerializer xml = new XmlSerializer(GetType());
                    PodIDInfo tmp = (PodIDInfo)xml.Deserialize(reader);
                    RFID = tmp.RFID;
                    DualPodID = tmp.DualPodID;
                    DomeID = tmp.DomeID;
                    CoverID = tmp.CoverID;
                    BasePlateID = tmp.BasePlateID;
                    this.PodName = tmp.PodName;
                    Weight = tmp.Weight;
                    PadLT = tmp.padLT;
                    PadRT = tmp.PadRT;
                    PadLB = tmp.PadLB;
                    PadRB = tmp.PadRB;
                }
            }
            catch(Exception ex)
            {
                res = false;
                MessageBox.Show("Pod ID Info Load Error!!\nDetail : " + ex.Message);
            }

            return res;
        }
        public bool Save()
        {
            bool res = true;
            string PodInfoPath = App.RecipeRootPath + dualPodID + "\\";
            DirectoryInfo dir = new DirectoryInfo(PodInfoPath); //
            if (!dir.Exists)
                dir.Create();

            PodInfoPath += PodName + "\\";
            dir = new DirectoryInfo(PodInfoPath); //
            if (!dir.Exists)
                dir.Create();

            try
            {
                using(TextWriter tw =new StreamWriter(PodInfoPath+"PodInfo.xml",false))
                {
                    XmlSerializer xml = new XmlSerializer(GetType());
                    xml.Serialize(tw, this);
                }
            }
            catch(Exception ex)
            {
                res = false;
                MessageBox.Show("Pod ID Info Save Error!!\nDetail : " + ex.Message);
            }
            return res;
        }
    }
    public class PodInfoRecipe_ViewModel:ObservableObject
    {
        public PodInfoRecipe Main;
        PodIDInfo podIDInfo;
        public PodIDInfo PodIDInfo
        {
            get => podIDInfo;
            set => SetProperty(ref podIDInfo, value);
        }
        private ObservableCollection<UIElement> podList = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> PodList
        {
            get => podList;
            set => SetProperty(ref podList, value);
        }
        int podListIdx = -1;

        public int PodListIdx
        {
            get => podListIdx;
            set => SetProperty(ref podListIdx, value);
        }

        public PodInfoRecipe_ViewModel()
        {
            Main = new PodInfoRecipe();
            Main.DataContext = this;
            podIDInfo = GlobalObjects.Instance.Get<PodIDInfo>();
            InitPodList();
        }

        void InitPodList()
        {
            /*
             Dir 구조
            RootPath -> DualPodID -> PodName -> RecipeName -> 하위 Recipe 항목들
             */
            DirectoryInfo dirInfo = new DirectoryInfo(App.RecipeRootPath);

            if (!dirInfo.Exists)
                return;

            DirectoryInfo[] infos = dirInfo.GetDirectories(); //DualPod ID Directories
            foreach(var info in infos)
            {
                DirectoryInfo[] Names = info.GetDirectories();
                foreach(var name in Names)
                {
                    ListView_TwoTB item = new ListView_TwoTB(info.ToString(), name.ToString());
                    podList.Add(item);
                }
            }
        }
        public ICommand btnSave
        {
            get => new RelayCommand(() => {
                if (PodIDInfo.Save())
                    MessageBox.Show("Pod Info was Saved!!");
                else
                    MessageBox.Show("Pod Info wasn't Saved!!");

                podList.Clear();
                InitPodList();
            });
        }
        public ICommand btnLoad
        {
            get => new RelayCommand(() => {
                ListView_TwoTB item = (ListView_TwoTB)PodList.ElementAt(PodListIdx);
                if (PodIDInfo.Load(item.sItem1,item.sItem2))
                    MessageBox.Show("Pod Info was Loaded!!");
                else
                    MessageBox.Show("Pod Info wasn't Loaded!!");
            });
        }
    }
}
