using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_WindII
{
    public class DummyWafer
    {
        public bool ck
        {
            get;
            set;
        }
        public string date
        {
            get;
            set;
        }
        public string no
        {
            get;
            set;
        }
        public string id
        {
            get;
            set;
        }
        public string rcp
        {
            get;
            set;
        }
    }
    public class DummyInsp
    {
        public string no
        {
            get;
            set;
        }
        public string name
        {
            get;
            set;
        }
        public string mode
        {
            get;
            set;
        }
    }
    public class DummyMask
    {
        public string no
        {
            get;
            set;
        }
        public string x
        {
            get;
            set;
        }
        public string y
        {
            get;
            set;
        }
        public string width
        {
            get;
            set;
        }
        public string height
        {
            get;
            set;
        }
        public string color
        {
            get;
            set;
        }
    }
    public class DummyGeneral
    {
        public string insp
        {
            get;set;
        }
        public string test
        {
            get;set;
        }
        public string mode
        {
            get;
            set;
        }
        public string no
        {
            get;
            set;
        }
        public string mask
        {
            get;
            set;
        }
        public string color
        {
            get;
            set;
        }
        public string item
        {
            get;
            set;
        }
    }
    public class DummyResult
    {
        public string no
        {
            get;
            set;
        }
        public string mode
        {
            get;set;
        }
        public string chip
        {
            get;
            set;
        }
        public string xy
        {
            get;
            set;
        }
        public string gv
        {
            get;
            set;
        }
        public string size
        {
            get;
            set;
        }
        public string dc
        {
            get;
            set;
        }
    }


}
