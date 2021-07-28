using RootTools;
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
        string dualPodID;
        double weight;

        #region Property
        public string DualPodID
        {
            get => dualPodID;
            set => SetProperty(ref dualPodID, value.Trim());
        }
        public double Weight
        {
            get => weight;
            set => SetProperty(ref weight, value);
        }
        #endregion

        public PodIDInfo() 
        {
            dualPodID = "";
            weight = 0;
            reg = new Registry("PodIDInfo");
        }

        #region Registry
        Registry reg;
        public void WriteReg()
        {
            reg?.Write("DualPodID", DualPodID);
            reg?.Write("Weight", Weight);
        }
        public void ReadReg()
        {
            if (reg == null) return;
            DualPodID = reg.Read("DualPodID", DualPodID);
            Weight = reg.Read("Weight", Weight);
        }
        #endregion
        public bool Load(string dualPodID)
        {
            bool res = true;

            try
            {
                string filePath = App.RecipeRootPath + dualPodID +  "\\PodInfo.xml";
                using(Stream reader = new FileStream(filePath,FileMode.Open))
                {
                    XmlSerializer xml = new XmlSerializer(GetType());
                    PodIDInfo tmp = (PodIDInfo)xml.Deserialize(reader);
                    DualPodID = tmp.DualPodID;
                    Weight = tmp.Weight;
                    WriteReg();
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
            if(dualPodID == null)
            {
                return false;
            }
            string PodInfoPath = App.RecipeRootPath + dualPodID + "\\";
            DirectoryInfo dir = new DirectoryInfo(PodInfoPath); //
            if (!dir.Exists)
                dir.Create();

            try
            {
                using(TextWriter tw =new StreamWriter(PodInfoPath + "PodInfo.xml",false))
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
        public PodIDInfo Clone()
        {
            PodIDInfo podInfo = new PodIDInfo();
            podInfo.dualPodID = dualPodID;
            podInfo.weight = weight;

            return podInfo;
        }
    }
    public class PodInfoRecipe_ViewModel:ObservableObject
    {
        public PodInfoRecipe Main;
        PodIDInfo podIDInfo; //현재 선택중인 하나의 PodInfo를 의미
        public PodIDInfo PodIDInfo
        {
            get => podIDInfo;
            set
            {
                //레지스트리에서 현재 파드 인포 변경하는거임
                value?.WriteReg();
                SetProperty(ref podIDInfo, value); 
            }
        }
        ObservableCollection<UIElement> podList = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> PodList
        {
            get => podList;
            set => SetProperty(ref podList, value);
        }
        int podListIdx = -1;

        public int PodListIdx
        {
            get => podListIdx;
            set
            {
                PodIDInfo.Load(((ListView_TwoTB)PodList.ElementAt(value)).sItem1);
                SetProperty(ref podListIdx, value);
            }
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
            //하위구조의 PodInfo.xml를 읽어서 PodInfo를 읽음
            DirectoryInfo dirInfo = new DirectoryInfo(App.RecipeRootPath);

            if (!dirInfo.Exists)
                return;

            DirectoryInfo[] infos = dirInfo.GetDirectories(); //DualPod ID Directories
            foreach(var info in infos)
            {
                PodIDInfo podinfo = new PodIDInfo();
                podinfo.Load(info.ToString());
                ListView_TwoTB item = new ListView_TwoTB(podinfo.DualPodID.ToString(), podinfo.Weight.ToString());
                podList.Add(item);
            }
        }
        public ICommand btnDelete
        {
            get => new RelayCommand(() => {
                //진짜 삭제할거냐고 물어봐야됨 하위 디렉토리까지 싹다 삭제해버릴꺼니까
                if(MessageBox.Show("진짜 삭제?","",MessageBoxButton.YesNo).Equals(MessageBoxResult.OK))
                {

                }
            });
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
                if (PodIDInfo.Load(item.sItem2))
                    MessageBox.Show("Pod Info was Loaded!!");
                else
                    MessageBox.Show("Pod Info wasn't Loaded!!");
            });
        }
    }
}
