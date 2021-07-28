using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RootTools_Vision
{
    public class VerticalWire_RecipeTeaching_ViewModel : ObservableObject
    {
        public int ROIItemOffset = 1000;

        #region [EVENT]
        public delegate void MoveEventHandler();
        public delegate void VerticalWireSaveHandler();

        public event MoveEventHandler CollectionChanged;
        public event VerticalWireSaveHandler RecipeSave;
        #endregion

        #region [Properties]
        private int selectedItemIdx;

        public int SelectedItemIdx
        {
            get => this.selectedItemIdx;
            set
            {
                SetProperty<int>(ref this.selectedItemIdx, value);
            }
        }

        private int refItemIdx;
        public int RefItemIdx
        {
            get => this.refItemIdx;
            set
            {
                SetProperty<int>(ref this.refItemIdx, value);
            }
        }

        private int roiItemIdx;
        public int ROIItemIdx
        {
            get => this.roiItemIdx;
            set
            {
                SetProperty<int>(ref this.roiItemIdx, value);
            }
        }

        private int chipNuminOrigin = 0;
        public int ChipNuminOrigin
        {
            get => this.chipNuminOrigin;
            set
            {
                SetProperty<int>(ref this.chipNuminOrigin, value);
            }
        }

        private ObservableCollection<UIElement> refCoordListItem = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> RefCoordListItem
        {
            get
            {
                return refCoordListItem;
            }
            set
            {
                SetProperty(ref refCoordListItem, value);
            }
        }

        private ObservableCollection<UIElement> roiListItem = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> ROIListItem
        {
            get
            {
                return roiListItem;
            }
            set
            {
                SetProperty(ref roiListItem, value);
            }
        }
        #endregion

        public VerticalWire_RecipeTeaching_ViewModel()
        {
            this.ChipNuminOrigin = 1;
            VerticalWire_RefCoordItem_ViewModel refItem = new VerticalWire_RefCoordItem_ViewModel(this.ChipNuminOrigin);
            RefCoordListItem.Add(refItem.Main);

            VerticalWire_ROIItem_ViewModel roiItem = new VerticalWire_ROIItem_ViewModel(this.ROIListItem.Count + 1);
            roiItem.CollectionChanged += InspROIItem_CollectionChanged;
            this.ROIListItem.Add(roiItem.Main);
        }

        public void AddRefItem()
        {
            VerticalWire_RefCoordItem_ViewModel item = new VerticalWire_RefCoordItem_ViewModel(this.ChipNuminOrigin);
            RefCoordListItem.Add(item.Main);

            for (int i = 0; i < ROIListItem.Count; i++)
            {
                VerticalWire_ROIItem roiItem = ROIListItem[i] as VerticalWire_ROIItem;
                VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;
                roiItemItem_ViewModel.RefCoordNum = chipNuminOrigin;
            }
        }

        public void AddROIItem()
        {
            VerticalWire_ROIItem_ViewModel roiItem = new VerticalWire_ROIItem_ViewModel(this.ROIListItem.Count + 1);
            roiItem.CollectionChanged += InspROIItem_CollectionChanged;
            ROIListItem.Add(roiItem.Main);            
        
        }

        public void RemoveRefItem()
        {
            RefCoordListItem.RemoveAt(RefCoordListItem.Count - 1);

            for (int i = 0; i < ROIListItem.Count; i++)
            {
                VerticalWire_ROIItem roiItem = ROIListItem[i] as VerticalWire_ROIItem;
                VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;
                roiItemItem_ViewModel.RefCoordNum = chipNuminOrigin;
            }
        }

        public void RemoveROIItem() 
        {
            ROIListItem.RemoveAt(ROIListItem.Count - 1);
        }

        private void _CreateROI()
        {
            AddROIItem();
        }
        private void _DeleteROI()
        {
            RemoveROIItem();
        }

        #region [Command]

        void RecipeLoad()
        {
            VerticalWireRecipe recipe = GlobalObjects.Instance.Get<VerticalWireRecipe>();
            
            //recipe.Clear();

            //ROIListItem.Clear();
            //for(int i = 0; i < recipe.InspROI.Count; i++)
            //{
            //    VerticalWire_ROIItem_ViewModel roiItem = new VerticalWire_ROIItem_ViewModel(this.ROIListItem.Count + 1);
            //    roiItem.CollectionChanged += InspROIItem_CollectionChanged;
            //    this.ROIListItem.Add(roiItem.Main);
            //}
            //RefCoordListItem.Clear();
            //for (int i = 0; i < recipe.RefCoord.Count; i++)
            //{
            //    VerticalWire_RefCoordItem_ViewModel refItem = new VerticalWire_RefCoordItem_ViewModel(this.ChipNuminOrigin);
            //    RefCoordListItem.Add(refItem.Main);
            //}

            //foreach (UIElement item in ROIListItem)
            //{
            //    VerticalWire_ROIItem roiItem = item as VerticalWire_ROIItem;
            //    VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

            //    recipe.InspROI.Add(new VerticalWire_InspROI_Info(roiItemItem_ViewModel.WireRectList, roiItemItem_ViewModel.SelectedRefCoord));
            //}

            //foreach (UIElement item in RefCoordListItem)
            //{
            //    VerticalWire_RefCoordItem refItem = item as VerticalWire_RefCoordItem;
            //    VerticalWire_RefCoordItem_ViewModel refItem_ViewModel = refItem.DataContext as VerticalWire_RefCoordItem_ViewModel;

            //    recipe.RefCoord.Add(new VerticalWire_RefCoord_Info(refItem_ViewModel.RefX, refItem_ViewModel.RefY, refItem_ViewModel.RefW, refItem_ViewModel.RefH));
            //}
        }

        public ICommand LoadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeLoad();
                });
            }
        }
        
        public ICommand btnChipNumPlusCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(this.ChipNuminOrigin + 1 <= 5)
                    {
                        this.ChipNuminOrigin = this.ChipNuminOrigin + 1;
                        AddRefItem();
                    }
                });
            }
        }

        public ICommand btnChipNumMinusCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(this.ChipNuminOrigin - 1 >= 0)
                    {
                        this.ChipNuminOrigin = this.ChipNuminOrigin - 1;
                        RemoveRefItem();
                    }
                });
            }
        }

        public ICommand DeleteROI
        {
            get
            {
                return new RelayCommand(_DeleteROI);
            }
        }

        public ICommand CreateROI
        {
            get
            {
                return new RelayCommand(_CreateROI);
            }
        }

        public ICommand RefCoordClickedCommand
        {
            get => new RelayCommand(() =>
            {
                SelectedItemIdx = refItemIdx;
            });
        }

        public ICommand ROIClickedCommand
        {
            get => new RelayCommand(() =>
            {
                SelectedItemIdx = ROIItemOffset + roiItemIdx;
            });
        }

        public ICommand ROIUp
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(SelectedItemIdx >= ROIItemOffset)
                    {
                        int itemIdx = SelectedItemIdx - ROIItemOffset;

                        VerticalWire_ROIItem roiItem = ROIListItem[itemIdx] as VerticalWire_ROIItem;
                        VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

                        foreach(TRect rt in roiItemItem_ViewModel.WireRectList)
                        {
                            rt.MemoryRect.Top -= 1;
                            rt.MemoryRect.Bottom -= 1;
                        }
                        this.CollectionChanged?.Invoke();
                    }
                });
            }
        }

        public ICommand ROIDown
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedItemIdx >= ROIItemOffset)
                    {
                        int itemIdx = SelectedItemIdx - ROIItemOffset;

                        VerticalWire_ROIItem roiItem = ROIListItem[itemIdx] as VerticalWire_ROIItem;
                        VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

                        foreach (TRect rt in roiItemItem_ViewModel.WireRectList)
                        {
                            rt.MemoryRect.Top += 1;
                            rt.MemoryRect.Bottom += 1;
                        }
                        this.CollectionChanged?.Invoke();
                    }
                });
            }
        }

        public ICommand ROILeft
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedItemIdx >= ROIItemOffset)
                    {
                        int itemIdx = SelectedItemIdx - ROIItemOffset;

                        VerticalWire_ROIItem roiItem = ROIListItem[itemIdx] as VerticalWire_ROIItem;
                        VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

                        foreach (TRect rt in roiItemItem_ViewModel.WireRectList)
                        {
                            rt.MemoryRect.Left -= 1;
                            rt.MemoryRect.Right -= 1;
                        }
                        this.CollectionChanged?.Invoke();
                    }
                });
            }
        }

        public ICommand ROIRight
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedItemIdx >= ROIItemOffset)
                    {
                        int itemIdx = SelectedItemIdx - ROIItemOffset;

                        VerticalWire_ROIItem roiItem = ROIListItem[itemIdx] as VerticalWire_ROIItem;
                        VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

                        foreach (TRect rt in roiItemItem_ViewModel.WireRectList)
                        {
                            rt.MemoryRect.Left += 1;
                            rt.MemoryRect.Right += 1;
                        }
                        this.CollectionChanged?.Invoke();
                    }
                });
            }
        }

        public ICommand Save
        {
            get => new RelayCommand(() =>
            {
                this.RecipeSave?.Invoke();
            });
        }
        #endregion

        #region [Callback]
        void InspROIItem_CollectionChanged()
        {
            this.CollectionChanged?.Invoke();
        }
        #endregion
    }
}
