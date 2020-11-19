using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

        private ObservableCollection<Type> m_cInspMethod;
        public ObservableCollection<Type> p_cInspMethod
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

        private Type m_InspMethod;
        public Type p_InspMethod
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

        public ICommand ComboBoxSelectionChanged_MethodItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MessageBox.Show("DDD");
                });
            }
        }

    }
}
