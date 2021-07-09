using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_VEGA_P_Vision
{
    public class Operating_ViewModel : ObservableObject
    {
        #region Property
        ObservableCollection<UIElement> taskQ;
        public ObservableCollection<UIElement> TaskQ
        {
            get => taskQ;
        }
        int selectedIdx;

        public int SelectedIdx
        {
            get => selectedIdx;
            set => SetProperty(ref selectedIdx, value);
        }
        Operating_Panel Main;
        public NozzleState CoverFrontNozzle { get; set; }
        public NozzleState CoverBackNozzle { get; set; }
        public NozzleState PlateFrontNozzle { get; set; }
        public NozzleState PlateBackNozzle { get; set; }
        public NozzleState EOPDomeNozzle { get; set; }
        public NozzleState EOPDoorNozzle { get; set; }
        MaskRootViewer_ViewModel coverFrontParticle;
        public MaskRootViewer_ViewModel CoverFrontParticle
        {
            get => coverFrontParticle;
            set => SetProperty(ref coverFrontParticle, value);
        }
        #endregion
        public Operating_ViewModel()
        {
            Main = new Operating_Panel();
            Main.DataContext = this;
            selectedIdx = 0;
            taskQ = new ObservableCollection<UIElement>();
            taskQ.Add(new TaskQItem("Docking", true));
            taskQ.Add(new TaskQItem("Task"));
            taskQ.Add(new TaskQItem("Undocking",false,true));

            RecipeCoverFront recipeCoverFront = GlobalObjects.Instance.Get<RecipeCoverFront>();
            RecipeCoverBack recipeCoverBack = GlobalObjects.Instance.Get<RecipeCoverBack>();
            RecipePlateFront recipePlateFront = GlobalObjects.Instance.Get<RecipePlateFront>();
            RecipePlateFront recipePlateBack = GlobalObjects.Instance.Get<RecipePlateFront>();

            coverFrontParticle = new MaskRootViewer_ViewModel("EIP_Cover.Main.Front", null, recipeCoverFront, recipeCoverFront.GetItem<EUVOriginRecipe>().TDIOriginInfo, 0);

/*                            EIPcoverBottom_TDI = new MaskRootViewer_ViewModel("EIP_Cover.Main.Back", recipeMask.MaskTools,
                recipeCoverBack, recipeCoverBack.GetItem<EUVOriginRecipe>().TDIOriginInfo, recipeCoverBack.GetItem<EUVPodSurfaceParameter>().PodTDI.MaskIndex);
*/

            CoverFrontNozzle = new NozzleState(@"D:\03_Projects\Root\Root_VEGA_P_Vision\UI\3. Operation\Resource\CoverFront.png", 1, 9);
            CoverBackNozzle = new NozzleState(@"D:\03_Projects\Root\Root_VEGA_P_Vision\UI\3. Operation\Resource\CoverBack.png", 10, 16);
            PlateFrontNozzle = new NozzleState(@"D:\03_Projects\Root\Root_VEGA_P_Vision\UI\3. Operation\Resource\PlateFront.png", 1, 9);
            PlateBackNozzle = new NozzleState(@"D:\03_Projects\Root\Root_VEGA_P_Vision\UI\3. Operation\Resource\PlateBack.png", 10, 17);
            EOPDomeNozzle = new NozzleState(@"D:\03_Projects\Root\Root_VEGA_P_Vision\UI\3. Operation\Resource\Dome.png", 1, 9);
            EOPDoorNozzle = new NozzleState(@"D:\03_Projects\Root\Root_VEGA_P_Vision\UI\3. Operation\Resource\DoorFront.png", 1, 10);
        }
    }
}
