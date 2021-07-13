using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Root_VEGA_P_Vision
{
    /// <summary>
    /// TaskQItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TaskQItem : UserControl
    {
        public string Task { get; set; }
        public bool IsDocking { get; set; }
        public bool IsUnDocking { get; set; }
        public TaskQItem(string Task, bool IsDocking= false, bool IsUnDocking=false)
        {
            this.Task = Task;
            this.IsDocking = IsDocking;
            this.IsUnDocking = IsUnDocking;

            InitializeComponent();
        }
    }
}
