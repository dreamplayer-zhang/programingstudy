using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2
{
    class FrontsideSpecTool_ViewModel: RootViewer_ViewModel
    {
        Recipe m_Recipe;

        public Recipe Recipe 
        { 
            get => m_Recipe;
            set
            {
                SetProperty(ref m_Recipe, value);
            }
        }

        public FrontsideSpecTool_ViewModel()
        {
        }

        public void init(Setup_ViewModel setup, Recipe recipe)
        {
            base.init(setup.m_MainWindow.m_Image, setup.m_MainWindow.dialogService);
            p_VisibleMenu = System.Windows.Visibility.Visible;

            m_Recipe = recipe;
        }

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
        }
        public override void SetRoiRect()
        {
            base.SetRoiRect();
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
        }
    }
}
