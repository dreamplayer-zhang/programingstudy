using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Root_WIND2
{
    class FrontsideSpec_ViewModel : ObservableObject
    {


        public Frontside_ViewModel m_front;
        public Recipe m_Recipe;
        public void init(Frontside_ViewModel front, Recipe recipe)
        {
            m_Recipe = recipe;
            m_front = front;    
            m_cMask = new ObservableCollection<Mask>();
            m_cInspMethod = new ObservableCollection<ParameterBase>();
            m_cInspItem = new ObservableCollection<InspectionItem>();

            p_cInspMethod = ParameterBase.GetChildClass();
            p_selectedMethodItem = ParameterBase.CreateChildInstance(p_cInspMethod[0]);
        }


        #region Property
        private ObservableCollection<Mask> m_cMask;
        public ObservableCollection<Mask> p_cMask
        {
            get
            {
                return m_cMask;
            }
            set
            {
                SetProperty(ref m_cMask, p_cMask);
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

        private ObservableCollection<InspectionItem> m_cInspItem;
        public ObservableCollection<InspectionItem> p_cInspItem
        {
            get
            {
                return m_cInspItem;
            }
            set
            {
                SetProperty(ref m_cInspItem, value);
            }
        }

        private Mask m_selectedMask;
        public Mask p_selectedMask
        {
            get
            {
                return m_selectedMask;
            }
            set
            {
                if (p_selectedInspItem == null)
                    return;
                m_selectedInspItem.p_Mask = value;
                SetProperty(ref m_selectedMask, value);
            }
        }

        private InspectionItem m_selectedInspItem;
        public InspectionItem p_selectedInspItem
        {
            get
            {
                return m_selectedInspItem;
            }
            set
            {
                SetProperty(ref m_selectedInspItem, value);
                p_selectedMethodItem = m_selectedInspItem.p_cInspMethod[0];
            }
        }

        private ParameterBase m_selectedMethodItem;
        public ParameterBase p_selectedMethodItem
        {
            get
            {
                return m_selectedMethodItem;
            }
            set
            {
                SetProperty(ref m_selectedMethodItem, value);
            }
        }
        #endregion


        public ObservableCollection<ParameterBase> CloneMethod()
        {
            ObservableCollection<ParameterBase> method = new ObservableCollection<ParameterBase>();
            foreach(ParameterBase param in p_cInspMethod)
            {
                method.Add((ParameterBase)param.Clone());
            }
            return method;
        }

        public void ComboBoxItemChanged_Mask_Callback(object obj, EventArgs args)
        {
            Mask mask = (Mask)obj;
        }

        public void ComboBoxItemChanged_Method_Callback(object obj, EventArgs args)
        {
            ParameterBase param = obj as ParameterBase;
            p_selectedMethodItem = ParameterBase.CreateChildInstance(param);
            SetParameter();
        }



        public void SetParameter()
        {
            List<ParameterBase> paramList = new List<ParameterBase>();
            foreach(InspectionItem item in p_cInspItem)
            {
                paramList.Add(item.p_InspMethod);
            }

            this.m_Recipe.ParameterItemList = paramList;
        }

        #region ICommand
        public ICommand btnAddInspItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    InspectionItem item = new InspectionItem();
                    item.p_cMask = p_cMask;
                    item.p_cInspMethod = CloneMethod();
                    item.ComboBoxItemChanged_Mask += ComboBoxItemChanged_Mask_Callback;
                    item.ComboBoxItemChanged_Method += ComboBoxItemChanged_Method_Callback;

                    p_cInspItem.Add(item);
                    SetParameter();
                    var asdf = m_front.p_ROI_VM;
                });
            }
        }
        public ICommand btnDeleteInspItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_cInspItem.Remove(p_selectedInspItem);
                    SetParameter();

                });
            }
        }
        public ICommand ComboBoxSelectionChanged_MethodItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_selectedInspItem.p_InspMethod = p_cInspMethod[0];
                    SetParameter();
                });
            }
        }
        #endregion
    }
}
