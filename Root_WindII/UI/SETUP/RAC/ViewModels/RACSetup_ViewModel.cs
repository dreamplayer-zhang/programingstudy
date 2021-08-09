
using RootTools;
using RootTools_Vision;
using Root_WindII.Engineer;
using Root_EFEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Root_WindII
{
    public class RACSetup_ViewModel : MapViewer_ViewModel
    {
        RACSetup_ImageViewer_ViewModel _RACSetupImageViewer_VM;
        public RACSetup_ImageViewer_ViewModel RACSetupImageViewer_VM
        {
            get
            {
                return _RACSetupImageViewer_VM;
            }
            set
            {
                SetProperty(ref _RACSetupImageViewer_VM, value);
            }
        }

        //private MapViewer_ViewModel mapViewer_VM = new MapViewer_ViewModel();
        //public MapViewer_ViewModel MapViewer_VM
        //{
        //    get => this.mapViewer_VM;
        //    set
        //    {
        //        SetProperty<MapViewer_ViewModel>(ref this.mapViewer_VM, value);
        //    }
        //}

        XMLData _XMLData;
        public XMLData XMLData
        {
            get
            {
                return _XMLData;
            }
            set
            {
                SetProperty(ref _XMLData, value);
            }
        }
        public RACSetup_ViewModel()
        {
            RACSetupImageViewer_VM = new RACSetup_ImageViewer_ViewModel();
            RACSetupImageViewer_VM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());

            RACSetupImageViewer_VM.OriginBoxReset += OriginBoxReset_Callback;
            RACSetupImageViewer_VM.OriginPointDone += OriginPointDone_Callback;
            RACSetupImageViewer_VM.OriginBoxDone += OriginBoxDone_Callback;
            RACSetupImageViewer_VM.PitchPointDone += PitchPointDone_Callback;
            RACSetupImageViewer_VM.CenteringPointDone += CenteringPointDone_Callback;
            RACSetupImageViewer_VM.OriginDieChanged += OriginDieChanged_Callback;

            XMLData = GlobalObjects.Instance.Get<XMLData>();
            XMLParser.Parsing += DataUpdate;
            WindII_Handler windII_Handler = (WindII_Handler)GlobalObjects.Instance.Get<WindII_Engineer>().ClassHandler();

            GrabModeFront grabModeFront = windII_Handler.p_VisionFront.GetGrabMode(0);
            //GlobalObjects.Instance.Get<>
        }

        void DataUpdate()
        {
            RACSetupImageViewer_VM.DeviceID = XMLData.Device;
            RACSetupImageViewer_VM.Description = XMLData.Description;
            RACSetupImageViewer_VM.UnitX = XMLData.UnitX;
            RACSetupImageViewer_VM.UnitY = XMLData.UnitY;
            RACSetupImageViewer_VM.DiePitchX = XMLData.DiePitchX;
            RACSetupImageViewer_VM.DiePitchY = XMLData.DiePitchY;
            RACSetupImageViewer_VM.ScribeLaneX = XMLData.ScribeLineX;
            RACSetupImageViewer_VM.ScribeLaneY = XMLData.ScribeLineY;
            RACSetupImageViewer_VM.ShotOffsetX = XMLData.ShotOffsetX;
            RACSetupImageViewer_VM.ShotOffsetY = XMLData.ShotOffsetY;
            RACSetupImageViewer_VM.MapOffsetX = XMLData.MapOffsetX;
            RACSetupImageViewer_VM.MapOffsetY = XMLData.MapOffsetY;
            RACSetupImageViewer_VM.SmiOffsetX = XMLData.SMIOffsetX;
            RACSetupImageViewer_VM.SmiOffsetY = XMLData.SMIOffsetY;
            RACSetupImageViewer_VM.ShotSizeX = XMLData.ShotX;
            RACSetupImageViewer_VM.ShotSizeY = XMLData.ShotY;
            if(XMLData.EvenOdd.ToLower() == "even")
            {
                RACSetupImageViewer_VM.EvenOdd = EVEN_ODD.EVEN;
            }
            else
            {
                RACSetupImageViewer_VM.EvenOdd = EVEN_ODD.ODD;
            }
            RACSetupImageViewer_VM.Rotation = XMLData.Rotation;

            int[] tempMap = XMLData.GetWaferMap();
            RACSetupImageViewer_VM.OriginDieX = XMLData.OriginDieUnit.X;
            RACSetupImageViewer_VM.OriginDieY = XMLData.OriginDieUnit.Y;



            CreateMap((int)XMLData.GetUnitSize().Width, (int)XMLData.GetUnitSize().Height, tempMap.Select(d => (int)d).ToArray());
            SelectPoint = new Point(XMLData.OriginDieUnit.X, XMLData.OriginDieUnit.Y);

            ChipItems[(int)(SelectPoint.Y * MapSizeX + SelectPoint.X)].Fill = Brushes.Chocolate;


            RACSetupImageViewer_VM.DataLoadDone();
            RACSetupImageViewer_VM.IsLoad = true;

            
        }

        public override void Chip_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            if(rect.Fill == Brushes.Chocolate)
                rect.Fill = Brushes.RosyBrown;
            else
                rect.Fill = Brushes.Blue;
        }

        public override void Chip_MouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            CPoint pt = (CPoint)rect.Tag;
            if (SelectPoint.X != pt.X || SelectPoint.Y != pt.Y)
                rect.Fill = Brushes.YellowGreen;
            else
                rect.Fill = Brushes.Chocolate;
        }

        public override void Chip_MouseLeftUp(object sender, MouseEventArgs e)
        {
            Rectangle rect = sender as Rectangle;

            if(SelectPoint.X != -1 && SelectPoint.Y != -1)
                ChipItems[(int)(SelectPoint.Y * MapSizeX + SelectPoint.X)].Fill = Brushes.YellowGreen;
            //ResetMapColor();
            CPoint pt = (CPoint)rect.Tag;
            //SelectPoint = new Point(pt.X, pt.Y);
            rect.Fill = Brushes.Chocolate;

            RACSetupImageViewer_VM.OriginDieX = pt.X;
            RACSetupImageViewer_VM.OriginDieY = pt.Y;

            SelectPoint = new Point(pt.X, pt.Y);
            //ChangeDie();
        }

        public void OriginBoxReset_Callback()
        {
            //XMLData = new XMLData();
            //try
            //{
            //    GeneralFunction.Read(XMLData, @"D:\RACXMLTEST\ProductB.XML");
            //}
            //catch (Exception e)
            //{
            //    string t = e.ToString();
            //}
            ////GeneralFunction.Save(XMLData, @"D:\RACXMLTEST\test.xml");

            Clear();
        }

        public void OriginPointDone_Callback()
        {
            //OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            //XMLData.p_OriginDie = RACSetupImageViewer_VM.p_
            //XMLData.p_OriginDie.X = 
            //this.OriginY = originRecipe.OriginY;
        }

        public void PitchPointDone_Callback()
        {
            //OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            //this.PitchX = originRecipe.DiePitchX;
            //this.PitchY = originRecipe.DiePitchY;

            //WIND2EventManager.OnRecipeUpdated(this, new RecipeEventArgs());

        }

        CPoint CurrentOriginDie { get; set; } = new CPoint();
        void ChangeDie()
        {
            if (ChipItems!=null && RACSetupImageViewer_VM.OriginDieX != -1 && RACSetupImageViewer_VM.OriginDieY != -1 && ChipItems[(int)(RACSetupImageViewer_VM.OriginDieY * MapSizeX + RACSetupImageViewer_VM.OriginDieX)].Fill == Brushes.YellowGreen)
                ChipItems[(int)(RACSetupImageViewer_VM.OriginDieY * MapSizeX + RACSetupImageViewer_VM.OriginDieX)].Fill = Brushes.Chocolate;
        }

        public void OriginDieChanged_Callback(string axis)
        {

            if (ChipItems != null && SelectPoint.X != -1 && SelectPoint.Y != -1 && ChipItems[(int)(SelectPoint.Y * MapSizeX + SelectPoint.X)].Fill == Brushes.Chocolate)
                ChipItems[(int)(SelectPoint.Y * MapSizeX + SelectPoint.X)].Fill = Brushes.YellowGreen;
            SelectPoint = new Point(RACSetupImageViewer_VM.OriginDieX, RACSetupImageViewer_VM.OriginDieY);

            ChangeDie();

        }
        public void CenteringPointDone_Callback()
        {

        }

        public void OriginBoxDone_Callback()
        {
            //this.OriginX = originRecipe.OriginX;
            //this.OriginY = originRecipe.OriginY;

            //this.OriginWidth = originRecipe.OriginWidth;
            //this.OriginHeight = originRecipe.OriginHeight;

            //this.PitchX = originRecipe.DiePitchX;
            //this.PitchY = originRecipe.DiePitchY;

        }

        public void Clear()
        {
            RACSetupImageViewer_VM.ClearObjects(true);

            //this.OriginX = 0;
            //this.OriginY = 0;
            //this.OriginWidth = 0;
            //this.OriginHeight = 0;
            //this.PitchX = 0;
            //this.PitchY = 0;
        }

        public RelayCommand CmdCreate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RACSetupImageViewer_VM.CreateXMLData();
                });
            }
        }
    }
}