using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            m_bDrag = true; 
        }

        private void Label_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (m_bDrag == false) return;
            m_bDrag = false;
            DataObject obj = new DataObject("DragDrop", m_module);
            System.Windows.DragDrop.DoDragDrop(e.OriginalSource as DependencyObject, obj, DragDropEffects.Move); 
        }

        private void Label_Drop(object sender, DragEventArgs e)
        {
            if (m_module.p_sStrip != "") return; 
            if (e.Data.GetDataPresent("DragDrop") == false) return;
            Module moduleFrom = (Module)e.Data.GetData("DragDrop");
            m_module.MoveFrom(moduleFrom); 
        }
        #endregion
    }
}
