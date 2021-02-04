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
    class FrontsideSpec_ViewModel : ObservableObject, IRecipeUILoadable
    {
        public Frontside_ViewModel m_front;
        public void init(Frontside_ViewModel front)
        {
            m_front = front;
            ViewerInit();
           
            m_cInspItem = new ObservableCollection<InspectionItem>();

            p_selectedMethodItem = null;

            WIND2EventManager.BeforeRecipeSave += BeforeRecipeSave_Callback;
        }

        private void ViewerInit()
        {
            p_ROI_Viewer = new FrontsideMask_ViewModel();
            p_ROI_Viewer.p_VisibleMenu = Visibility.Collapsed;
            p_ROI_Viewer.SetBackGroundWorker();
            p_ROI_Viewer.p_ImageData = m_front.p_ROI_VM.p_ImageData;
            p_ROI_Viewer.p_ROILayer = m_front.p_ROI_VM.p_ROILayer;
            p_ROI_Viewer.p_cInspROI = m_front.p_ROI_VM.p_cInspROI;
            p_ROI_Viewer.SetRoiRect();
        }

        #region Property
        private FrontsideMask_ViewModel m_ROI_Viewer;
        public FrontsideMask_ViewModel p_ROI_Viewer
        {
            get
            {
                m_ROI_Viewer.p_ImageData = m_front.p_ROI_VM.p_ImageData;
                m_ROI_Viewer.SetImageSource();
                if(m_ROI_Viewer.p_SelectedROI != null)
                    m_ROI_Viewer._ReadROI();
                return m_ROI_Viewer;
            }
            set
            {
                SetProperty(ref m_ROI_Viewer, value);
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

        private InspectionItem m_selectedInspItem;
        public InspectionItem p_selectedInspItem
        {
            get
            {
                return m_selectedInspItem;
            }
            set
            {
                var asdf = p_ROI_Viewer.p_ImgSource;
                SetProperty(ref m_selectedInspItem, value);
                if(m_selectedInspItem != null)
                    p_selectedMethodItem = m_selectedInspItem.p_InspMethod;
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


        public void ComboBoxItemChanged_Mask_Callback(object obj, EventArgs args)
        {
            InspectionROI mask = (InspectionROI)obj;
        }

        public void ComboBoxItemChanged_Method_Callback(object obj, EventArgs args)
        {
            ParameterBase param = obj as ParameterBase;
            p_selectedMethodItem = param;
            SetParameter();
        }

        public void ButtonClicked_Delete_Callback(object obj, EventArgs args)
        {
            InspectionItem item = obj as InspectionItem;

            p_cInspItem.Remove(item);
            for (int i = 0; i < p_cInspItem.Count; i++)
            {
                p_cInspItem[i].p_Index = i;
            }
            SetParameter();
        }


        private void BeforeRecipeSave_Callback(object obj, RecipeEventArgs args)
        {
            SetParameter();
        }

        public void LoadSpec()
        {
            p_cInspItem.Clear();

            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();

            foreach(ParameterBase parameterBase in recipe.ParameterItemList)
            {
                InspectionItem item = new InspectionItem();
                item.p_cInspROI = p_ROI_Viewer.p_cInspROI;

                int selectMethod = 0;
                for (int i = 0; i < item.p_cInspMethod.Count; i++)
                {
                    if (item.p_cInspMethod[i].InspectionType.Name == parameterBase.InspectionType.Name)
                    {
                        item.p_cInspMethod[i] = (ParameterBase)parameterBase.Clone();
                        selectMethod = i;
                        break;
                    }
                }

                item.ComboBoxItemChanged_Mask += ComboBoxItemChanged_Mask_Callback;
                item.ComboBoxItemChanged_Method += ComboBoxItemChanged_Method_Callback;
                item.ButtonClicked_Delete += ButtonClicked_Delete_Callback;

                p_cInspItem.Add(item);

                item.p_InspMethod = item.p_cInspMethod[selectMethod];
            }

            if(p_cInspItem.Count > 0)
                p_selectedInspItem = p_cInspItem[0];

            SetParameter();
        }

        public void SetParameter()
        {
            List<ParameterBase> paramList = new List<ParameterBase>();
            foreach(InspectionItem item in p_cInspItem)
            {
                if (item.p_InspMethod is IMaskInspection)
                {
                    if (item.p_InspROI != null)
                    {
                        ((IMaskInspection)item.p_InspMethod).MaskIndex = item.p_InspROI.p_Index;
                    }
                }

                if(item.p_InspMethod is IColorInspection)
                {
                    ((IColorInspection)item.p_InspMethod).IndexChannel = item.p_InspChannel;
                }

                paramList.Add(item.p_InspMethod);
            }

            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            recipe.ParameterItemList = paramList;
        }

        public void Load()
        {
            LoadSpec();
        }

        #region ICommand
        public ICommand btnAddInspItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    InspectionItem item = new InspectionItem();
                    item.p_cInspROI = p_ROI_Viewer.p_cInspROI;
                    item.p_Index = p_cInspItem.Count();
                    item.ComboBoxItemChanged_Mask += ComboBoxItemChanged_Mask_Callback;
                    item.ComboBoxItemChanged_Method += ComboBoxItemChanged_Method_Callback;
                    item.ButtonClicked_Delete += ButtonClicked_Delete_Callback;

                    p_cInspItem.Add(item);
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
                    m_selectedInspItem.p_InspMethod = p_selectedInspItem.p_InspMethod;
                    SetParameter();
                });
            }
        }
        #endregion

    }
}
