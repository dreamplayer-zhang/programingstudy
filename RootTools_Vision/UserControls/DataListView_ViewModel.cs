using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{

    public class DataItem
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }

    public class DataListView_ViewModel
    {
        private ObservableCollection<DataItem> items;
        public ObservableCollection<DataItem> Items
        {
            get => this.items;
        }

        public DataListView_ViewModel()
        {
            this.items = new ObservableCollection<DataItem>();
        }

        public void Init(object obj)
        {
            items.Clear();

            Type type = obj.GetType();

            foreach (var f in type.GetFields().Where(f => f.IsPublic))
            {
                DataItem item = new DataItem();
                item.Name = f.Name;
                item.Value = f.GetValue(obj).ToString();
                items.Add(item);
            }
        }
    }
}
