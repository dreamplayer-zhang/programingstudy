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
using RootTools_Vision;

namespace Root_WIND2
{
    public delegate void ButtonClicked(object obj, EventArgs args);
    public delegate void ComboboxItemChanged(object obj, EventArgs args);
    public class InspectionItem : ObservableObject
    {
        public event ComboboxItemChanged ComboBoxItemChanged_Mask;
        public event ComboboxItemChanged ComboBoxItemChanged_Method;
        public event ButtonClicked ButtonClicked_Delete;

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

        private ObservableCollection<ParameterBase> m_cInspMethod;
        public ObservableCollection<ParameterBase> p_cInspMethod
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

        private ParameterBase m_InspMethod;
        public ParameterBase p_InspMethod
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
                    if (ComboBoxItemChanged_Mask != null)
                        ComboBoxItemChanged_Method(p_InspMethod, new EventArgs());
                });
            }
        }

        public ICommand ComboBoxSelectionChanged_MaskItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (ComboBoxItemChanged_Mask != null)
                        ComboBoxItemChanged_Mask(p_Mask, new EventArgs());
                });
            }
        }


        public ICommand btnDeleteInspItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (ButtonClicked_Delete != null)
                        ButtonClicked_Delete(this, new EventArgs());

                });
            }
        }
    }
}
