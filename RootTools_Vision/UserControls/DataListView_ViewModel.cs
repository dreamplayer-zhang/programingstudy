using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public delegate void DataItemValueChangedHandler(DataItem item);

    public class DataItem
    {
        public event DataItemValueChangedHandler DataItemChanged;
        private string itemName;
        public string Name
        { 
            get => this.itemName;
            set
            {
                this.itemName = value;
            } 
        }

        private object itemValue;
        public object Value
        { 
            get => this.itemValue;
            set
            {
                this.itemValue = value;
                if (this.DataItemChanged != null)
                    DataItemChanged(this);
            }
        }        
    }

    public class DataListView_ViewModel : ObservableObject
    {
        public event DataItemValueChangedHandler DataItemChanged;

        private ObservableCollection<DataItem> items;
        public ObservableCollection<DataItem> Items
        {
            get => this.items;
        }

        public DataListView_ViewModel()
        {
            this.items = new ObservableCollection<DataItem>();
        }

        private object dataObj;
        public object DataObect
        {
            get => this.dataObj;
        }

        private bool isProperties = false;

        public void Init(object obj, bool isProperties = false)
        {
            this.isProperties = isProperties;
            items.Clear();

            if (this.isProperties)
            {
                Type type = obj.GetType();

                foreach (var f in type.GetProperties().Where(f => f.CanRead && f.PropertyType.IsValueType))
                {
                    //if (!f.GetType().IsValueType) continue;
                    DataItem item = new DataItem();

                    item.Name = f.Name;
                    item.Value = f.GetValue(obj);
                    item.DataItemChanged += DataItemValueChanged_Callback;
                    items.Add(item);
                }
            }
            else
            {
                Type type = obj.GetType();

                foreach (var f in type.GetFields().Where(f => f.IsPublic))
                {
                    DataItem item = new DataItem();

                    item.Name = f.Name;
                    item.Value = f.GetValue(obj);
                    item.DataItemChanged += DataItemValueChanged_Callback;
                    items.Add(item);
                }
            }

            this.dataObj = obj;
        }

        public void DataItemValueChanged_Callback(DataItem item)
        {
            if (this.isProperties)
            {
                PropertyInfo propertyInfo = this.DataObect.GetType().GetProperty(item.Name);
                try
                {
                    Convert.ChangeType(item.Value, propertyInfo.PropertyType);
                }
                catch(Exception ex)
                {
                    if ((string)item.Value == "") return;

                    MessageBox.Show("값 형식이 다릅니다.(type:"+ propertyInfo .PropertyType.ToString()+ ")");
                    object obj = propertyInfo.GetValue(this.DataObect);
                    item.Value = obj;
                    return;
                }
                propertyInfo.SetValue(this.DataObect, Convert.ChangeType(item.Value, propertyInfo.PropertyType));
            }
            else
            {
                FieldInfo fieldInfo = this.DataObect.GetType().GetField(item.Name);

                try
                {
                    Convert.ChangeType(item.Value, fieldInfo.FieldType);
                }
                catch (Exception ex)
                {
                    if ((string)item.Value == "") return;

                    MessageBox.Show("값 형식이 다릅니다.(type:" + fieldInfo.FieldType.ToString() + ")");
                    object obj = fieldInfo.GetValue(this.DataObect);
                    item.Value = obj;
                    return;
                }

                fieldInfo.SetValue(this.DataObect, Convert.ChangeType(item.Value, fieldInfo.FieldType));
            }

            if (DataItemChanged != null)
                this.DataItemChanged(item);
        }
    }
}
