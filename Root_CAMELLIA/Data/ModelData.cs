using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Met = LibSR_Met;
namespace Root_CAMELLIA.Data
{
    public class ModelData
    {
        public ModelData()
        {
            NanoView = App.m_nanoView;
            MetDataManager = Met.DataManager.GetInstance();
        }

        private Met.Nanoview _nanoView;
        public Met.Nanoview NanoView
        {
            get
            {
                return _nanoView;
            }
            private set
            {
                _nanoView = value;
            }
        }
        private Met.DataManager _metDataManager;
        public Met.DataManager MetDataManager
        {
            get
            {
                return _metDataManager;
            }
            private set
            {
                _metDataManager = value;
            }
        }

        private List<string> _materialList = new List<string>();
        public List<string> MaterialList
        {
            get
            {
                return _materialList;
            }
            set
            {
                _materialList = value;
            }
        }
        

    }
}
