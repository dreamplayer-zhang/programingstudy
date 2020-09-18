using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_WIND2
{
    public class InspectionItem : ObservableObject
    {
        private ObservableCollection<Mask> m_cMask;
        public ObservableCollection<Mask> p_cMask
        {
            get
            {
                return m_cMask;
            }
            set
            {
                SetProperty(ref m_cMask, value);
            }
        }

        private ObservableCollection<InspectionMethod> m_cInspMethod;
        public ObservableCollection<InspectionMethod> p_cInspMethod
        {
            get
            {

                return m_cInspMethod;
            }
            set
            {
                SetProperty(ref m_cInspMethod, value);
            }
        }

        private Mask m_Mask;
        public Mask p_Mask
        {
            get
            {
                return m_Mask;
            }
            set
            {
                SetProperty(ref m_Mask, value);
            }
        }

        private InspectionMethod m_InspMethod;
        public InspectionMethod p_InspMethod
        {
            get
            {
                return m_InspMethod;
            }
            set
            {
                SetProperty(ref m_InspMethod, value);
            }
        }
        
    }
}
