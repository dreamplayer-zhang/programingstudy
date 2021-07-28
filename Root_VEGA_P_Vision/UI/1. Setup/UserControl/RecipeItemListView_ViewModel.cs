using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_VEGA_P_Vision
{
    public delegate void RecipeItemValueChangedHandler(RecipeItem item);
    public class RecipeItem
    {
        public event RecipeItemValueChangedHandler RecipeItemChanged;
        string itemName;
        public string Name
        {
            get => itemName;
            set => itemName = value;
        }
        object itemValue;
        public object Value
        {
            get => itemValue;
            set
            {
                itemValue = value;
                if (RecipeItemChanged != null)
                    RecipeItemChanged(this);
            }
        }
    }
    public class RecipeItemListView_ViewModel:ObservableObject
    {
        public event RecipeItemValueChangedHandler RecipeItemChanged;

        ObservableCollection<RecipeItem> items;
        public ObservableCollection<RecipeItem> Items
        {
            get => items;
        }
        public RecipeItemListView_ViewModel()
        {
            items = new ObservableCollection<RecipeItem>();
        }
        object recipeObj;
        public object RecipeObj
        {
            get => recipeObj;
        }
        bool isProperties = false;
        public void Init(object obj, bool isProperties=false)
        {
            this.isProperties = isProperties;
            items.Clear();

            if(isProperties)
            {
                Type type = obj.GetType();
                foreach(var f in type.GetProperties().Where(f=>f.CanRead && f.PropertyType.IsValueType))
                {
                    RecipeItem item = new RecipeItem();
                    item.Name = f.Name;
                    item.Value = f.GetValue(obj);
                    item.RecipeItemChanged += Item_RecipeItemChanged;
                    items.Add(item);
                }
            }
            else
            {
                Type type = obj.GetType();
                foreach(var f in type.GetFields().Where(f=>f.IsPublic))
                {
                    RecipeItem item = new RecipeItem();
                    item.Name = f.Name;
                    item.Value = f.GetValue(obj);
                    item.RecipeItemChanged += Item_RecipeItemChanged;
                    items.Add(item);
                }
            }
            recipeObj = obj;
        }

        private void Item_RecipeItemChanged(RecipeItem item)
        {
            if (this.isProperties)
            {
                PropertyInfo propertyInfo = RecipeObj.GetType().GetProperty(item.Name);
                try
                {
                    Convert.ChangeType(item.Value, propertyInfo.PropertyType);
                }
                catch (Exception)
                {
                    if ((string)item.Value == "") return;

                    MessageBox.Show("값 형식이 다릅니다.(type:" + propertyInfo.PropertyType.ToString() + ")");
                    object obj = propertyInfo.GetValue(RecipeObj);
                    item.Value = obj;
                    return;
                }
                propertyInfo.SetValue(RecipeObj, Convert.ChangeType(item.Value, propertyInfo.PropertyType));
            }
            else
            {
                FieldInfo fieldInfo = RecipeObj.GetType().GetField(item.Name);

                try
                {
                    Convert.ChangeType(item.Value, fieldInfo.FieldType);
                }
                catch (Exception)
                {
                    if ((string)item.Value == "") return;

                    MessageBox.Show("값 형식이 다릅니다.(type:" + fieldInfo.FieldType.ToString() + ")");
                    object obj = fieldInfo.GetValue(RecipeObj);
                    item.Value = obj;
                    return;
                }

                fieldInfo.SetValue(RecipeObj, Convert.ChangeType(item.Value, fieldInfo.FieldType));
            }

            if (RecipeItemChanged != null)
                RecipeItemChanged(item);
        }
    }
}
