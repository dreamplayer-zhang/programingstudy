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
        Operating_Panel Main;
        public NozzleState CoverFrontNozzle { get; set; }
        public NozzleState CoverBackNozzle { get; set; }
        public NozzleState PlateFrontNozzle { get; set; }
        public NozzleState PlateBackNozzle { get; set; }
        public NozzleState EOPDomeNozzle { get; set; }
        public NozzleState EOPDoorNozzle { get; set; }
        MaskRootViewer_ViewModel coverFrontParticle,coverFrontStain,coverBackParticle, coverBackHighRes,coverBackStain,
            plateFrontParticle, plateFrontHighRes,plateFrontStain,plateBackParticle,plateBackStain;
        RecipeSideImageViewers_ViewModel EIPcoverViewers,EIPplateViewers;
        public RecipeSideImageViewers_ViewModel EIPCoverViewers
        {
            get => EIPcoverViewers;
            set => SetProperty(ref EIPcoverViewers, value);
        }
        public RecipeSideImageViewers_ViewModel EIPPlateViewers
        {
            get => EIPplateViewers; 
            set => SetProperty(ref EIPplateViewers, value);
        }
        public MaskRootViewer_ViewModel PlateBackParticle
        {
            get => plateBackParticle;
            set => SetProperty(ref plateBackParticle, value);
        }
        public MaskRootViewer_ViewModel PlateBackStain
        {
            get => plateBackStain;
            set => SetProperty(ref plateBackStain, value);
        }
        public MaskRootViewer_ViewModel PlateFrontParticle
        {
            get => plateFrontParticle;
            set => SetProperty(ref plateFrontParticle, value);
        }
        public MaskRootViewer_ViewModel PlateFrontHighRes
        {
            get => plateFrontHighRes;
            set => SetProperty(ref plateFrontHighRes, value);
        }
        public MaskRootViewer_ViewModel PlateFrontStain
        {
            get => plateFrontStain;
            set => SetProperty(ref plateFrontStain, value);
        }
        public MaskRootViewer_ViewModel CoverFrontParticle
        {
            get => coverFrontParticle;
            set => SetProperty(ref coverFrontParticle, value);
        }
        public MaskRootViewer_ViewModel CoverFrontStain
        {
            get => coverFrontStain;
            set => SetProperty(ref coverFrontStain, value);
        }
        public MaskRootViewer_ViewModel CoverBackParticle
        {
            get => coverBackParticle;
            set => SetProperty(ref coverBackParticle, value);
        }
        public MaskRootViewer_ViewModel CoverBackHighRes
        {
            get => coverBackHighRes;
            set => SetProperty(ref coverBackHighRes, value);
        }
        public MaskRootViewer_ViewModel CoverBackStain
        {
            get => coverBackStain;
            set => SetProperty(ref coverBackStain, value);
        }
        #endregion
        public Operating_ViewModel()
        {
            Main = new Operating_Panel();
            Main.DataContext = this;
            taskQ = new ObservableCollection<UIElement>();
            taskQ.Add(new TaskQItem("Docking", true));
            taskQ.Add(new TaskQItem("Task"));
            taskQ.Add(new TaskQItem("Task"));
            taskQ.Add(new TaskQItem("Task"));
            taskQ.Add(new TaskQItem("Task"));
            taskQ.Add(new TaskQItem("Task"));
            taskQ.Add(new TaskQItem("Task"));
            taskQ.Add(new TaskQItem("Task"));
            taskQ.Add(new TaskQItem("Task"));
            taskQ.Add(new TaskQItem("Undocking",false,true));

            RecipeCoverFront recipeCoverFront = GlobalObjects.Instance.Get<RecipeCoverFront>();
            RecipeCoverBack recipeCoverBack = GlobalObjects.Instance.Get<RecipeCoverBack>();
            RecipePlateFront recipePlateFront = GlobalObjects.Instance.Get<RecipePlateFront>();
            RecipePlateFront recipePlateBack = GlobalObjects.Instance.Get<RecipePlateFront>();

            coverFrontParticle = new MaskRootViewer_ViewModel("EIP_Cover.Main.Front", new MaskTools_ViewModel(), recipeCoverFront, recipeCoverFront.GetItem<EUVOriginRecipe>().TDIOriginInfo, 0);
            coverFrontStain = new MaskRootViewer_ViewModel("EIP_Cover.Stain.Front", new MaskTools_ViewModel(), recipeCoverFront, recipeCoverFront.GetItem<EUVOriginRecipe>().StainOriginInfo, 0);
            EIPcoverViewers = new RecipeSideImageViewers_ViewModel("EIP_Cover");

            coverBackParticle = new MaskRootViewer_ViewModel("EIP_Cover.Main.Back", new MaskTools_ViewModel(), recipeCoverBack, recipeCoverBack.GetItem<EUVOriginRecipe>().TDIOriginInfo, 0);
            coverBackHighRes = new MaskRootViewer_ViewModel("EIP_Cover.Stack.Back", new MaskTools_ViewModel(), recipeCoverBack, recipeCoverBack.GetItem<EUVOriginRecipe>().TDIOriginInfo, 0);
            coverBackStain = new MaskRootViewer_ViewModel("EIP_Cover.Stain.Back", new MaskTools_ViewModel(), recipeCoverBack, recipeCoverBack.GetItem<EUVOriginRecipe>().StainOriginInfo, 0);

            plateFrontStain = new MaskRootViewer_ViewModel("EIP_Plate.Stain.Front", new MaskTools_ViewModel(), recipePlateFront, recipePlateFront.GetItem<EUVOriginRecipe>().StainOriginInfo, 0);
            plateFrontHighRes = new MaskRootViewer_ViewModel("EIP_Plate.Stack.Front", new MaskTools_ViewModel(), recipePlateFront, recipePlateFront.GetItem<EUVOriginRecipe>().TDIOriginInfo, 0);
            plateFrontParticle = new MaskRootViewer_ViewModel("EIP_Plate.Main.Front", new MaskTools_ViewModel(), recipePlateFront, recipePlateFront.GetItem<EUVOriginRecipe>().TDIOriginInfo, 0);
            EIPplateViewers = new RecipeSideImageViewers_ViewModel("EIP_Plate");

            plateBackParticle = new MaskRootViewer_ViewModel("EIP_Plate.Main.Back", new MaskTools_ViewModel(), recipePlateBack, recipePlateBack.GetItem<EUVOriginRecipe>().TDIOriginInfo, 0);
            plateBackStain = new MaskRootViewer_ViewModel("EIP_Plate.Stain.Back", new MaskTools_ViewModel(), recipePlateBack, recipePlateBack.GetItem<EUVOriginRecipe>().StainOriginInfo, 0);

            CoverFrontNozzle = new NozzleState(@"D:\03_Projects\Root\Root_VEGA_P_Vision\UI\3. Operation\Resource\CoverFront.png", 1, 9);
            CoverBackNozzle = new NozzleState(@"D:\03_Projects\Root\Root_VEGA_P_Vision\UI\3. Operation\Resource\CoverBack.png", 10, 16);
            PlateFrontNozzle = new NozzleState(@"D:\03_Projects\Root\Root_VEGA_P_Vision\UI\3. Operation\Resource\PlateFront.png", 1, 9);
            PlateBackNozzle = new NozzleState(@"D:\03_Projects\Root\Root_VEGA_P_Vision\UI\3. Operation\Resource\PlateBack.png", 10, 17);
            EOPDomeNozzle = new NozzleState(@"D:\03_Projects\Root\Root_VEGA_P_Vision\UI\3. Operation\Resource\Dome.png", 1, 9);
            EOPDoorNozzle = new NozzleState(@"D:\03_Projects\Root\Root_VEGA_P_Vision\UI\3. Operation\Resource\DoorFront.png", 1, 10);
        }
    }
}
