using RootTools.Gem;
using RootTools.Module;
using RootTools.SQLogs;
using RootTools.Trees;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace RootTools.GAFs
{
    public class ALID : NotifyProperty
    {
        #region Property
        public string p_sModule { get; set; }

        public string p_id { get; set; }

        int _nID = -1;
        public int p_nID
        {
            get { return _nID; }
            set
            {
                if (_nID == value) return;
                _nID = value;
                OnPropertyChanged();
            }
        }

        public string p_sDesc { get; set; }

        string _sMsg = "";
        public string p_sMsg
        {
            get { return _sMsg; }
            set
            {
                if (_sMsg == value) return;
                _sMsg = value;
                OnPropertyChanged();
            }
        }

        bool _bSet = false;
        public bool p_bSet
        {
            get { return _bSet; }
            set
            {
                if (_bSet == value) return;
                _bSet = value;
                if (m_log != null) m_log.Error(m_sID + "." + p_sMsg + " = " + _bSet.ToString());
                if (m_gem != null) m_gem.SetAlarm(this, _bSet);
                p_dateTime = DateTime.Now;
                OnPropertyChanged();
                if (value)
                {
                    m_listALID.SetALID(this);
                    m_sqALID.Insert();
                }
            }
        }

        public void Run(bool bSet, string sMsg)
        {
            p_sMsg = sMsg; 
            p_bSet = bSet; 
        }

        DateTime _dateTime = DateTime.Now;
        public DateTime p_dateTime
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
                OnPropertyChanged();
            }
        }

        string _sImageFile = "";
        public string p_sImageFile 
        { 
            get { return _sImageFile; }
            set
            {
                if (_sImageFile == value) return;
                _sImageFile = value;
                p_image = new BitmapImage(new Uri(value)); 
            }
        }
        #endregion

        BitmapImage _image = null;
        public BitmapImage p_image 
        { 
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged(); 
            }
        }

        ModuleBase m_module;
        ALIDList m_listALID;
        SQTable_ALID m_sqALID; 
        string m_sID;
        Log m_log;
        IGem m_gem;
        public ALID(ModuleBase module, ALIDList listALID, string id, string sDesc)
        {
            m_module = module;
            m_listALID = listALID;
            p_sModule = module.p_id;
            p_id = id;
            m_sID = "ALID." + p_sModule + "." + p_id;
            p_sDesc = sDesc;
            m_gem = module.m_gem;
            m_log = module.m_log;
            p_sImageFile = "";
            m_sqALID = SQLog.Get(this); 
        }

        public void RunTree(Tree treeParent, int nDefaultID)
        {
            Tree tree = treeParent.GetTree(p_id, false);
            p_nID = tree.Set(p_nID, nDefaultID, "Number", "SVID Number");
            p_sImageFile = tree.SetFile(p_sImageFile, "", "jpg", "Image", "Image File Name");
            p_sDesc = tree.Set(p_sDesc, p_sDesc, "Descrition", "ALID Description"); 
        }

        public void Save(StreamWriter sw)
        {
            sw.WriteLine(p_sModule + ", " + p_id + ", " + p_nID.ToString() + ", " + p_sDesc);
        }
    }
}
