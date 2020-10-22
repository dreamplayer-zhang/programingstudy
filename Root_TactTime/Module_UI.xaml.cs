using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Root_TactTime
{
    /// <summary>
    /// Module_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Module_UI : UserControl
    {
        public Module_UI()
        {
            InitializeComponent();
        }

        Module m_module; 
        public void Init(Module module)
        {
            m_module = module;
            DataContext = module;
            Canvas.SetLeft(this, module.m_cpLoc.X);
            Canvas.SetTop(this, module.m_cpLoc.Y); 
        }

        #region Drag & Drop
        bool m_bDrag = false; 
        private void Label_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (m_module.p_sStrip == "") return;
            if (m_module.m_tact.m_qSequence.Count > 0) return; 
            m_bDrag = true; 
        }

        private void Label_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (m_bDrag == false) return;
            m_bDrag = false;
            DataObject obj = new DataObject("Module", m_module);
            System.Windows.DragDrop.DoDragDrop(e.OriginalSource as DependencyObject, obj, DragDropEffects.Move); 
        }

        private void Label_Drop(object sender, DragEventArgs e)
        {
            if (m_module.p_sStrip != "") return; 
            if (e.Data.GetDataPresent("Picker") == false) return;
            Picker picker = (Picker)e.Data.GetData("Picker");
            m_module.MoveFrom(picker, true);
        }
        #endregion
    }
}
