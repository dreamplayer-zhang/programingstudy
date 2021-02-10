using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
    public class EBR_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        public EBR_ImageViewer_ViewModel()
        {
            this.p_VisibleMenu = System.Windows.Visibility.Collapsed;
        }

        #region [Command]
        public RelayCommand btnOpen
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._openImage();
                });
            }
        }

        public RelayCommand btnSave
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._saveImage();
                });
            }
        }

        public RelayCommand btnClear
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._clearImage();
                });
            }
        }

        public RelayCommand btnClearDefect
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ClearObjects();
                });
            }
        }
        #endregion

        #region [Overrides]
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
        }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            RedrawShapes();
        }

        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {
            base.PreviewMouseUp(sender, e);
        }

        public override void SetRoiRect()
        {
            base.SetRoiRect();
            RedrawShapes();
        }
        #endregion

        public void ClearObjects()
        {
            p_DrawElement.Clear();
        }

        private void RedrawShapes()
        {
        }
    }
}
