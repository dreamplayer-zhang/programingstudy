using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_WIND2
{
    public class WIND2_Warning : ObservableObject
    {
        ObservableCollection<string> m_WarnList = new ObservableCollection<string>();
        public ObservableCollection<string> p_WarnList
        {
            get
            {
                return m_WarnList;
            }
            set
            {   
                SetProperty(ref m_WarnList, value);
            }
        }

        public WIND2_Warning()
        {
            for (int i = 0; i < 100; i++)
                AddWarning(i.ToString());
        }

        public void AddWarning(string str)
        {
            if (p_WarnList.Count > 10)
                p_WarnList.RemoveAt(0);
            p_WarnList.Add(str);
        }


    }
}
