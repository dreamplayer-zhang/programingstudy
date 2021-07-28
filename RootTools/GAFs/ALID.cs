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
                if (p_nID < 0) return; 
                _bSet = value;
                p_dateTime = DateTime.Now;
				OnPropertyChanged();
				if (value && p_bEQError)
                {
						m_log?.Error(m_sID + "." + p_sMsg + " = " + _bSet.ToString());
						m_gem?.SetAlarm(this, _bSet);
						m_listALID.SetALID(this);
						m_sqALID.Insert();
						EQ.p_eState = EQ.eState.Error;
						EQ.p_bStop = true;
				}
				else
				{
					m_log?.Info(m_sID + "." + p_sMsg + " = " + _bSet.ToString());
				}
			}
		}

		bool _bEQError = true;
        public bool p_bEQError
        {
            get { return _bEQError; }
            set
            {
                if (_bEQError == value) return;
                _bEQError = value;
                OnPropertyChanged(); 
            }
        }
        int _nErrorLevel = 0;
        int m_nmaxlevel = 5;
        public int p_nErrorLevel
        {
            get { return _nErrorLevel; }
            set
            {
                if (_nErrorLevel == value) return;
                if (value > m_nmaxlevel) value = 5;
                _nErrorLevel = value;
                OnPropertyChanged();
            }
        }
        public void Run(bool bSet, string sMsg)
        {
            p_sMsg = sMsg; 
            p_bSet = bSet;
            if (bSet && p_bEQError)
            {
                EQ.p_eState = EQ.eState.Error;
                EQ.p_bStop = true; 
            }
        }

        DateTime _dateTime = DateTime.Now;
        public DateTime p_dateTime
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
                OnPropertyChanged();
                p_sTime = value.ToString("HH:mm:ss"); 
            }
        }

        string _sTime = "";
        public string p_sTime
        {
            get { return _sTime; }
            set
            {
                _sTime = value;
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
        public string m_sID;
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
            p_bEQError = tree.Set(p_bEQError, p_bEQError, "EQ Error", "EQ Error when Set");
            p_nErrorLevel = tree.Set(p_nErrorLevel, p_nErrorLevel, "Error Level", "Error Level");
        }

        public void Save(StreamWriter sw)
        {
            sw.WriteLine(p_sModule + ", " + p_id + ", " + p_nID.ToString() + ", " + p_sDesc);
        }
    }
}
