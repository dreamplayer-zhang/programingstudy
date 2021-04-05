using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Root_CAMELLIA.TemplateSelector
{
    public class FFUTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Fan { get; set; }
        public DataTemplate Humidity { get; set; }
        public DataTemplate Temperature { get; set; }
        public DataTemplate Pressure { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(((FFUListItem)item).Unit is Module.Module_FFU.Unit.Fan)
            {
                return Fan;
            }
            else if(((FFUListItem)item).Unit is Module.Module_FFU.Unit.Humidity)
            {
                return Humidity;
            }
            else if(((FFUListItem)item).Unit is Module.Module_FFU.Unit.Temp)
            {
                return Temperature;
            }
            else
            {
                //?
            }
            return base.SelectTemplate(item, container);
        }
    }
}
