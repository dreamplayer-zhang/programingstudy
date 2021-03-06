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
        public event ComboboxItemChanged ComboBoxItemChanged_Channel;

        public event ButtonClicked ButtonClicked_Delete;

        public InspectionItem()
        {
            m_cInspMethod = new ObservableCollection<ParameterBase>();
            m_cInspROI = new ObservableCollection<ItemMask>();

            // Old
            //if(this.m_cInspROI != null) this.m_InspROI = this.m_cInspROI[0];

            //// Method
            //m_cInspMethod = new ObservableCollection<ParameterBase>();
            //p_cInspMethod = ParameterBase.GetFrontSideClass();

            //this.p_InspMethod = this.m_cInspMethod[0]; // Position
            //this.p_InspChannel = IMAGE_CHANNEL.R_GRAY;

            //// Mask
            //m_cInspROI = new ObservableCollection<ItemMask>();
            //MaskRecipe maskRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<MaskRecipe>();
            //for (int i = 0; i < maskRecipe.MaskList.Count; i++)
            //{
            //    ItemMask roi = new ItemMask();
            //    roi.p_Index = i;
            //    roi.p_Size = maskRecipe.MaskList[i].Area;
            //    roi.p_Data = maskRecipe.MaskList[i].ToPointLineList();
            //    roi.p_Color = maskRecipe.MaskList[i].ColorIndex;
            //    this.p_cInspROI.Add(roi);
            //}

            //if (this.p_cInspROI.Count > 0)
            //    this.p_InspROI = this.p_cInspROI[0];
        }

        public InspectionItem(string inspection)
        {
            if (this.m_cInspROI != null) this.m_InspROI = this.m_cInspROI[0];

            // Method
            m_cInspMethod = new ObservableCollection<ParameterBase>();
            p_cInspMethod = ParameterBase.GetParameters(inspection);

            this.p_InspMethod = this.m_cInspMethod[0]; // Position
            this.p_InspChannel = IMAGE_CHANNEL.R_GRAY;

            // Mask
            m_cInspROI = new ObservableCollection<ItemMask>();
            MaskRecipe maskRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<MaskRecipe>();
            for (int i = 0; i < maskRecipe.MaskList.Count; i++)
            {
                ItemMask roi = new ItemMask();
                roi.p_Index = i;
                roi.p_Size = maskRecipe.MaskList[i].Area;
                roi.p_Data = maskRecipe.MaskList[i].ToPointLineList();
                roi.p_Color = maskRecipe.MaskList[i].ColorIndex;
                this.p_cInspROI.Add(roi);
            }

            if (this.p_cInspROI.Count > 0)
                this.p_InspROI = this.p_cInspROI[0];
        }

        public InspectionItem(ParameterBase param)
        {
            // Method
            //m_cInspMethod = new ObservableCollection<ParameterBase>();
            //p_cInspMethod = ParameterBase.GetFrontSideClass();

            this.p_InspMethod = param; // Position
        }

        public int p_Index
        {
            get
            {
                return m_Index;
            }
            set
            {
                SetProperty(ref m_Index, value);
            }

        }
        private int m_Index = 0;

        private ObservableCollection<ItemMask> m_cInspROI;
        public ObservableCollection<ItemMask> p_cInspROI
        {
            get
            {
                return m_cInspROI;
            }
            set
            {
                SetProperty(ref m_cInspROI, value);
            }
        }


        private ItemMask m_InspROI;
        public ItemMask p_InspROI
        {
            get
            {
                return m_InspROI;
            }
            set
            {
                SetProperty(ref m_InspROI, value);
            }
        }





        private IMAGE_CHANNEL m_inspChannel;
        public IMAGE_CHANNEL p_InspChannel
        {
            get => this.m_inspChannel;
            set
            {
                SetProperty(ref this.m_inspChannel, value);
            }
        }


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
        private ObservableCollection<ParameterBase> m_cInspMethod;

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
        private ParameterBase m_InspMethod;

        public ICommand ComboBoxSelectionChanged_MethodItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (ComboBoxItemChanged_Method != null)
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
                        ComboBoxItemChanged_Mask(this, new EventArgs());
                });
            }
        }
        public ICommand ComboBoxSelectionChanged_ChannelItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (ComboBoxItemChanged_Channel != null)
                        ComboBoxItemChanged_Channel(this, new EventArgs());
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
