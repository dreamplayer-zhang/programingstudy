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
    public class PBI_RecipeTeaching_ViewModel : ObservableObject
    {
        public delegate void PBISaveHandler();
        public event PBISaveHandler RecipeSave;

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
        private int featureItemIdx;
        public int FeatureItemIdx
        {
            get
            {
                this.featureItemIdx = (this.featureItemIdx < 0) ? 0 : this.featureItemIdx;
                this.featureItemIdx = (this.featureItemIdx > FeatureListItem.Count - 1) ? FeatureListItem.Count - 1 : featureItemIdx;
                return this.featureItemIdx;
            }
            set
            {
                SetProperty<int>(ref this.featureItemIdx, value);
            }
        }

        private ObservableCollection<UIElement> featureListItem = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> FeatureListItem
        {
            get
            {
                return featureListItem;
            }
            set
            {
                SetProperty(ref featureListItem, value);
            }
        }
        #endregion
        public PBI_RecipeTeaching_ViewModel()
        {

        }

        public void AddFeatureItem()
        {
            PBI_FeatureItem_ViewModel featureItem = new PBI_FeatureItem_ViewModel(this.FeatureListItem.Count + 1);
            this.FeatureListItem.Add(featureItem.Main);
        }

        public void RemoveAllFeatureItem()
        {
            this.FeatureListItem.Clear();
        }

        public void DeleteFeatureItem()
        {
            this.FeatureListItem.RemoveAt(FeatureListItem.Count - 1);
        }

        #region [Command]
        public ICommand DeleteROI
        {
            get
            {
                return new RelayCommand(DeleteFeatureItem);
            }
        }

        public ICommand AddROI
        {
            get
            {
                return new RelayCommand(AddFeatureItem);
            }
        }

        public ICommand FeatureClickedCommand
        {
            get => new RelayCommand(() =>
            {
                this.SelectedItemIdx = FeatureItemIdx;
            });
        }

        public ICommand Save
        {
            get => new RelayCommand(() =>
            {
                this.RecipeSave?.Invoke();
            });
        }
        #endregion
    }
}
