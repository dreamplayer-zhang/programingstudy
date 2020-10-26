using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_TactTime
{
    /// <summary>
    /// Picker_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Picker_UI : UserControl
    {
        public Picker_UI()
        {
            InitializeComponent();
        }

        Picker m_picker;
        public void Init(Picker picker)
        {
            m_picker = picker;
            DataContext = picker;
            Canvas.SetLeft(this, picker.m_cpLoc.X);
            Canvas.SetTop(this, picker.m_cpLoc.Y);
        }

        #region Drag & Drop
        bool m_bDrag = false;
        private void Label_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (m_picker.p_sStrip == "") return;
            if (m_picker.m_tact.m_qSequence.Count > 0) return; 
            m_bDrag = true;
        }

        private void Label_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (m_bDrag == false) return;
            m_bDrag = false;
            DataObject obj = new DataObject("Picker", m_picker);
            System.Windows.DragDrop.DoDragDrop(e.OriginalSource as DependencyObject, obj, DragDropEffects.Move);
        }

        private void Label_Drop(object sender, DragEventArgs e)
        {
            if (m_picker.p_sStrip != "") return;
            Module module = (Module)e.Data.GetData("Module");
            if (module != null)
            {
                m_picker.MoveFrom(module, true);
                return;
            }
            Picker picker = (Picker)e.Data.GetData("Picker");
            if (picker != null)
            {
                m_picker.MoveFrom(picker, true);
                return;
            }
        }
        #endregion
    }
}
